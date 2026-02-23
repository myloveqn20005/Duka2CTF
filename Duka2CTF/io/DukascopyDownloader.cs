using Duka2CTF.model;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Duka2CTF.io
{
    public static class DukascopyDownloader
    {
        private static readonly HashSet<string> ValidSymbols = new(StringComparer.OrdinalIgnoreCase)
        {
            "EURUSD", "GBPUSD", "USDJPY", "AUDUSD", "USDCAD", "USDCHF", "NZDUSD",
            "XAUUSD", "XAGUSD", "BTCUSD", "ETHUSD", "US30", "DE40", "SPX500", "NAS100"
        };

        private static readonly Dictionary<string, (double MinPrice, double MaxPrice)> PriceRanges = new()
        {
            ["XAUUSD"] = (300, 5000),
            ["XAGUSD"] = (5, 100),
            ["BTCUSD"] = (1000, 200000),
            ["ETHUSD"] = (50, 20000),
            // có thể mở rộng thêm nếu cần
        };

        private static readonly Dictionary<string, double> SymbolPointDividers = new()
        {
            ["XAUUSD"] = 1000.0,
            ["XAGUSD"] = 1000.0,
            ["BTCUSD"] = 1.0,
            ["ETHUSD"] = 1.0
        };

        private static readonly TimeSpan[] BackoffDelays =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(10)
        };

        private static int ReadInt32BE(Span<byte> data, int pos)
        {
            return (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
        }

        public static async IAsyncEnumerable<Tick> DownloadHourTicksStreamingAsync(
            string symbol,
            DateTime day,
            int hour,
            int digits,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            symbol = symbol.ToUpperInvariant();

            if (!ValidSymbols.Contains(symbol))
            {
                System.Diagnostics.Debug.WriteLine($"Symbol không hợp lệ: {symbol}");
                yield break;
            }

            if (!SymbolPointDividers.TryGetValue(symbol, out double divider))
                divider = 100000.0;

            string url = $"https://datafeed.dukascopy.com/datafeed/{symbol}/{day.Year}/{(day.Month - 1):D2}/{day.Day:D2}/{hour:D2}h_ticks.bi5";

            const int MAX_RETRIES = 3;
            int retry = 0;

            while (retry <= MAX_RETRIES)
            {
                HttpClient? client = null;
                try
                {
                    client = CreateHttpClient();

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(45)); // timeout mỗi request

                    using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            yield break;

                        retry++;
                        if (retry <= MAX_RETRIES)
                        {
                            var delay = BackoffDelays[Math.Min(retry - 1, BackoffDelays.Length - 1)];
                            // Thêm jitter nhỏ
                            delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                            await Task.Delay(delay, ct);
                        }
                        continue;
                    }

                    await using var rawStream = await response.Content.ReadAsStreamAsync(ct);

                    byte[] props = new byte[5];
                    int read = await rawStream.ReadAsync(props, 0, 5, ct);
                    if (read < 5) yield break;

                    var decoder = new Decoder();
                    try
                    {
                        decoder.SetDecoderProperties(props);
                    }
                    catch
                    {
                        // header LZMA corrupt → retry
                        retry++;
                        if (retry <= MAX_RETRIES)
                        {
                            await Task.Delay(BackoffDelays[Math.Min(retry - 1, BackoffDelays.Length - 1)], ct);
                        }
                        continue;
                    }

                    const int BUFFER_SIZE = 16384;
                    byte[] buffer = new byte[BUFFER_SIZE];
                    byte[] tickBuffer = new byte[20];
                    int tickPos = 0;

                    while (!ct.IsCancellationRequested)
                    {
                        int bytesRead = await rawStream.ReadAsync(buffer, 0, BUFFER_SIZE, ct);
                        if (bytesRead == 0) break;

                        for (int j = 0; j < bytesRead; j++)
                        {
                            tickBuffer[tickPos++] = buffer[j];

                            if (tickPos == 20)
                            {
                                int ms = ReadInt32BE(tickBuffer, 0);
                                int askPoints = ReadInt32BE(tickBuffer, 4);
                                int bidPoints = ReadInt32BE(tickBuffer, 8);

                                if (ms < 0 || ms >= 3600000 || askPoints <= 0 || bidPoints <= 0)
                                {
                                    tickPos = 0;
                                    continue;
                                }

                                double askPrice = askPoints / divider;
                                double bidPrice = bidPoints / divider;

                                // Validation giá theo symbol
                                if (PriceRanges.TryGetValue(symbol, out var range))
                                {
                                    if (askPrice < range.MinPrice || askPrice > range.MaxPrice ||
                                        bidPrice < range.MinPrice || bidPrice > range.MaxPrice)
                                    {
                                        tickPos = 0;
                                        continue;
                                    }
                                }

                                DateTime dt = day.Date.AddHours(hour).AddMilliseconds(ms);
                                long utcMs = (long)(dt - DateTime.UnixEpoch).TotalMilliseconds;

                                int askScaled = (int)Math.Round(askPrice * Math.Pow(10, digits));
                                int bidScaled = (int)Math.Round(bidPrice * Math.Pow(10, digits));

                                yield return new Tick(bidScaled, askScaled, utcMs);

                                tickPos = 0;
                            }
                        }
                    }

                    yield break;
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Hour {hour} error (retry {retry}/{MAX_RETRIES}): {ex.Message}");
                    retry++;
                    if (retry <= MAX_RETRIES)
                    {
                        var delay = BackoffDelays[Math.Min(retry - 1, BackoffDelays.Length - 1)];
                        await Task.Delay(delay, ct);
                    }
                }
                finally
                {
                    client?.Dispose();
                }
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                MaxConnectionsPerServer = 6,
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
            };

            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan // dùng CancellationToken thay vì global timeout
            };
        }

        public static async Task<List<Tick>> DownloadHourTicksAsync(
            string symbol, DateTime day, int hour, int digits, CancellationToken ct = default)
        {
            var list = new List<Tick>(capacity: 50000);
            await foreach (var tick in DownloadHourTicksStreamingAsync(symbol, day, hour, digits, ct))
            {
                list.Add(tick);
            }
            return list;
        }
    }
}