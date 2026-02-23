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
        public static readonly HashSet<string> ValidSymbols = new(StringComparer.OrdinalIgnoreCase)
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
        };

        private static readonly Dictionary<string, double> SymbolPointDividers = new()
        {
            ["XAUUSD"] = 1000.0,
            ["XAGUSD"] = 1000.0,
            ["BTCUSD"] = 1.0,
            ["ETHUSD"] = 1.0
        };

        // Học từ dukascopy-node: retry nhiều hơn, backoff exponential + jitter mạnh
        private static readonly TimeSpan[] BackoffDelays =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(16)
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

            double divider = SymbolPointDividers.TryGetValue(symbol, out var d) ? d : 100000.0;

            string url = $"https://datafeed.dukascopy.com/datafeed/{symbol}/{day.Year}/{(day.Month - 1):D2}/{day.Day:D2}/{hour:D2}h_ticks.bi5";

            const int MAX_RETRIES = 5; // học dukascopy-node: 5 lần retry

            int retry = 0;

            while (retry <= MAX_RETRIES)
            {
                using var client = CreateHttpClient(); // Tạo mới mỗi retry → tránh pending read
                Stream? rawStream = null;
                bool setupSuccess = false;

                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(60)); // tăng timeout lên 60s

                    using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            yield break;

                        retry++;
                        if (retry <= MAX_RETRIES)
                        {
                            var delay = BackoffDelays[Math.Min(retry - 1, BackoffDelays.Length - 1)];
                            delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1500)); // jitter lớn hơn
                            await Task.Delay(delay, ct);
                        }
                        continue;
                    }

                    rawStream = await response.Content.ReadAsStreamAsync(ct);

                    byte[] props = new byte[5];
                    int read = await rawStream.ReadAsync(props, 0, 5, ct);
                    if (read < 5) yield break;

                    var decoder = new Decoder();
                    decoder.SetDecoderProperties(props);

                    setupSuccess = true;
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Hour {hour} setup error (retry {retry}/{MAX_RETRIES}): {ex.Message}");
                    retry++;
                    if (retry <= MAX_RETRIES)
                    {
                        var delay = BackoffDelays[Math.Min(retry - 1, BackoffDelays.Length - 1)];
                        delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1500));
                        await Task.Delay(delay, ct);
                    }
                }
                finally
                {
                    if (!setupSuccess)
                    {
                        rawStream?.Dispose();
                    }
                }

                if (setupSuccess && rawStream != null)
                {
                    try
                    {
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
                    }
                    finally
                    {
                        rawStream.Dispose();
                        client?.Dispose(); //
                    }

                    yield break; // Thành công
                }
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.Zero, // Tắt pool lifetime → mỗi client độc lập
                MaxConnectionsPerServer = 6,
                PooledConnectionIdleTimeout = TimeSpan.Zero // Tắt idle timeout
            };

            return new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public static async Task<List<Tick>> DownloadHourTicksAsync(
            string symbol, DateTime day, int hour, int digits, CancellationToken ct = default)
        {
            var list = new List<Tick>(50000);
            await foreach (var tick in DownloadHourTicksStreamingAsync(symbol, day, hour, digits, ct))
            {
                list.Add(tick);
            }
            return list;
        }
    }
}