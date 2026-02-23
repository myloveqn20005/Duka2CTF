using Duka2CTF.io; // TickFileBuilder, CtfProcessor, DukascopyDownloader
using Duka2CTF.model; // Tick
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duka2CTF
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource? _cts;

        public MainForm()
        {
            InitializeComponent();
            cmbSymbol.Items.AddRange(new string[]
            {
                "EURUSD","GBPUSD","USDJPY","AUDUSD","USDCAD","USDCHF","NZDUSD",
                "XAUUSD","XAGUSD","BTCUSD","ETHUSD","US30","DE40","SPX500","NAS100"
            });
            cmbSymbol.SelectedIndex = 0;
            cmbSymbol.SelectedIndexChanged += CmbSymbol_SelectedIndexChanged;
            dtpFrom.Value = DateTime.Today.AddYears(-1);
            dtpTo.Value = DateTime.Today;
        }

        // =========================================================
        // SYMBOL → AUTO SET DIGITS
        // =========================================================
        private void CmbSymbol_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbSymbol.SelectedItem == null) return;
            string symbol = cmbSymbol.SelectedItem.ToString()!.ToUpper();
            if (symbol.Contains("XAU") || symbol.Contains("XAG") ||
                symbol.Contains("JPY") || symbol.Contains("US30") ||
                symbol.Contains("DE40") || symbol.Contains("SPX") ||
                symbol.Contains("NAS"))
            {
                numDigitsDownload.Value = 2;
            }
            else
            {
                numDigitsDownload.Value = 5;
            }
        }

        private bool IsValidSymbol(string symbol)
        {
            return DukascopyDownloader.ValidSymbols.Contains(symbol.ToUpperInvariant());
        }

        // =========================================================
        // CSV → CTF
        // =========================================================
        private async void btnStart_Click(object sender, EventArgs e)
        {
            string input = txtInputPath.Text;
            int digits = (int)numDigits.Value;
            string output = input.Replace(".csv", ".ctf");

            if (!File.Exists(input))
            {
                txtLogs.AppendText("❌ Không tìm thấy file CSV đầu vào!\r\n");
                return;
            }

            btnStart.Enabled = false;
            try
            {
                await Task.Run(() => StartConversion(input, output, digits));
                txtLogs.AppendText("✅ Hoàn thành file: " + output + "\r\n");
            }
            catch (Exception ex)
            {
                txtLogs.AppendText($"❌ Lỗi khi chuyển đổi CSV → CTF: {ex.Message}\r\n");
            }
            finally
            {
                btnStart.Enabled = true;
            }
        }

        private void StartConversion(string input, string output, int digits)
        {
            using var builder = new TickFileBuilder(output, digits);
            try
            {
                builder.Create();

                var groups = new Dictionary<int, List<Tick>>();
                foreach (var line in File.ReadLines(input).Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var p = line.Split(',');
                    if (p.Length < 3) continue;

                    if (!DateTime.TryParse(p[0], out DateTime dt)) continue;

                    long utc = Utils.ToUnixTime(dt);
                    double askRaw = double.Parse(p[1], CultureInfo.InvariantCulture);
                    double bidRaw = double.Parse(p[2], CultureInfo.InvariantCulture);

                    int ask = Utils.PriceToInt(askRaw, digits);
                    int bid = Utils.PriceToInt(bidRaw, digits);

                    int day = Utils.TimeToDayNumber(utc);

                    if (!groups.ContainsKey(day))
                        groups[day] = new List<Tick>();

                    groups[day].Add(new Tick(bid, ask, utc, utc, 0));
                }

                foreach (var dayEntry in groups.OrderBy(x => x.Key))
                {
                    var dayTicks = dayEntry.Value.OrderBy(t => t.Utc).ToList();
                    long lastTime = 0;
                    int lastBid = 0, lastAsk = 0;
                    byte[] data = CtfProcessor.EncodeDay(dayTicks, ref lastTime, ref lastBid, ref lastAsk);
                    builder.AddDay(data, dayEntry.Key);
                }
            }
            finally
            {
                builder.Close();
            }
        }

        // =========================================================
        // DOWNLOAD → CTF (STREAMING VERSION)
        // =========================================================
        private async void btnDownloadConvert_Click(object sender, EventArgs e)
        {
            string? symbol = cmbSymbol.SelectedItem?.ToString()?.ToUpper();
            if (string.IsNullOrEmpty(symbol))
            {
                MessageBox.Show("Vui lòng chọn symbol.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidSymbol(symbol))
            {
                MessageBox.Show($"Symbol '{symbol}' không được hỗ trợ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;
            string outputFolder = txtOutputFolder.Text;
            string outputPath = Path.Combine(outputFolder, $"sim_ticks_{symbol}.ctf");
            int digits = (int)numDigitsDownload.Value;

            btnDownloadConvert.Enabled = false;
            btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();

            try
            {
                await DownloadAndConvertAsync(symbol, from, to, digits, outputPath, _cts.Token);
                txtLogs.AppendText("✅ Hoàn thành!\r\n");
            }
            catch (OperationCanceledException)
            {
                txtLogs.AppendText("→ Đã hủy quá trình download.\r\n");
            }
            catch (Exception ex)
            {
                txtLogs.AppendText($"❌ Lỗi download/convert: {ex.Message}\r\n");
            }
            finally
            {
                btnDownloadConvert.Enabled = true;
                btnCancel.Enabled = false;
                progressBarDownload.Value = 0;
            }
        }

        private async Task DownloadAndConvertAsync(
            string symbol,
            DateTime from,
            DateTime to,
            int digits,
            string outputPath,
            CancellationToken ct)
        {
            using var builder = new TickFileBuilder(outputPath, digits);
            try
            {
                if (File.Exists(outputPath))
                {
                    try
                    {
                        builder.OpenForUpdate();
                    }
                    catch (Exception ex)
                    {
                        txtLogs.AppendText($"Không mở được file .ctf để update: {ex.Message}\r\n");
                        builder.Create(); // fallback tạo mới
                    }
                }
                else
                {
                    builder.Create();
                }

                int totalDays = (int)(to - from).TotalDays + 1;
                int processedDays = 0;
                DateTime current = from;

                while (current <= to && !ct.IsCancellationRequested)
                {
                    if (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday)
                    {
                        txtLogs.AppendText($"Ngày nghỉ: {current:dd/MM/yyyy} (bỏ qua)\r\n");
                        processedDays++;
                        current = current.AddDays(1);
                        progressBarDownload.Value = Math.Min((processedDays * 100) / totalDays, 100);
                        continue;
                    }

                    long lastTime = 0;
                    int lastBid = 0;
                    int lastAsk = 0;
                    bool hasData = false;

                    for (int h = 0; h < 24; h++)
                    {
                        if (ct.IsCancellationRequested) break;

                        var hourTicks = await DukascopyDownloader.DownloadHourTicksAsync(
                            symbol, current, h, digits, ct);

                        if (hourTicks.Count > 0)
                        {
                            hasData = true;
                            hourTicks = hourTicks.OrderBy(t => t.Utc).ToList();

                            byte[] encoded = CtfProcessor.EncodeDay(
                                hourTicks,
                                ref lastTime,
                                ref lastBid,
                                ref lastAsk);

                            int dayNum = Utils.TimeToDayNumber(hourTicks[0].Utc);
                            builder.AddDay(encoded, dayNum);

                            hourTicks.Clear();
                        }
                    }

                    if (hasData)
                        txtLogs.AppendText($"Xong ngày {current:dd/MM/yyyy}\r\n");
                    else
                        txtLogs.AppendText($"Ngày {current:dd/MM/yyyy}: Không có dữ liệu\r\n");

                    processedDays++;
                    progressBarDownload.Value = Math.Min((processedDays * 100) / totalDays, 100);
                    current = current.AddDays(1);
                }
            }
            finally
            {
                try
                {
                    builder.Close();
                }
                catch (Exception ex)
                {
                    txtLogs.AppendText($"Lỗi khi đóng file .ctf: {ex.Message}\r\n");
                }
            }
        }

        // =========================================================
        // CSV EXPORT (streaming – tương tự CTF)
        // =========================================================
        private async void btnTestToCSV_Click(object sender, EventArgs e)
        {
            string symbol = cmbSymbol.Text.Trim().ToUpperInvariant();
            string outputFolder = txtOutputFolder.Text.Trim();
            int digits = (int)numDigitsDownload.Value;

            if (string.IsNullOrWhiteSpace(symbol) || !Directory.Exists(outputFolder))
            {
                MessageBox.Show("Vui lòng chọn symbol và thư mục đầu ra hợp lệ.",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            if (!IsValidSymbol(symbol))
            {
                MessageBox.Show($"Symbol '{symbol}' không được hỗ trợ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnTestToCSV.Enabled = false;
            btnDownloadConvert.Enabled = false;
            btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();
            txtLogs.Clear();

            txtLogs.AppendText($"🚀 Bắt đầu xuất CSV: {symbol} – digits: {digits}\r\n");
            txtLogs.AppendText($"Thư mục: {outputFolder}\r\n");

            try
            {
                DateTime current = dtpFrom.Value.Date;
                DateTime endDate = dtpTo.Value.Date;

                if (current > endDate)
                {
                    txtLogs.AppendText("Ngày bắt đầu lớn hơn ngày kết thúc, dừng lại.\r\n");
                    return;
                }

                double factor = Math.Pow(10, digits);
                string formatStr = "F" + digits;
                DateTime epoch = DateTime.UnixEpoch;
                int totalDays = (int)(endDate - current).TotalDays + 1;
                int processed = 0;

                const int FLUSH_EVERY = 10000; // tăng lên để giảm I/O overhead

                while (current <= endDate && !_cts.Token.IsCancellationRequested)
                {
                    if (current.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    {
                        current = current.AddDays(1);
                        continue;
                    }

                    processed++;
                    string dateStr = current.ToString("dd/MM/yyyy");
                    txtLogs.AppendText($"[{processed}/{totalDays}] Đang xử lý {dateStr} ...\r\n");

                    string csvPath = Path.Combine(outputFolder, $"{symbol}_{current:yyyyMMdd}.csv");
                    int dailyTickCount = 0;
                    int validHourCount = 0;
                    bool hasSampleLogged = false;

                    await using (var writer = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8, 8192))
                    {
                        await writer.WriteLineAsync("timestamp,askPrice,bidPrice");

                        for (int h = 0; h < 24; h++)
                        {
                            _cts.Token.ThrowIfCancellationRequested();

                            int hourTickCount = 0;

                            try
                            {
                                await foreach (var t in DukascopyDownloader.DownloadHourTicksStreamingAsync(
                                    symbol, current, h, digits, _cts.Token))
                                {
                                    DateTime dt = epoch.AddMilliseconds(t.Utc);
                                    string askStr = (t.Ask / factor).ToString(formatStr, CultureInfo.InvariantCulture);
                                    string bidStr = (t.Bid / factor).ToString(formatStr, CultureInfo.InvariantCulture);
                                    string line = $"{dt:yyyy.MM.dd HH:mm:ss.fff},{askStr},{bidStr}";

                                    await writer.WriteLineAsync(line);
                                    dailyTickCount++;
                                    hourTickCount++;

                                    if (!hasSampleLogged && hourTickCount == 1)
                                    {
                                        txtLogs.AppendText(
                                            $" → Giờ {h:00}h: ... ticks | mẫu: Ask={askStr}, Bid={bidStr}\r\n");
                                        hasSampleLogged = true;
                                    }

                                    if (hourTickCount % FLUSH_EVERY == 0)
                                    {
                                        await writer.FlushAsync();
                                    }
                                }
                            }
                            catch (Exception exHour)
                            {
                                txtLogs.AppendText($" ! Lỗi tải giờ {h:00}h: {exHour.Message}\r\n");
                                continue;
                            }

                            if (hourTickCount > 0)
                            {
                                validHourCount++;
                                await writer.FlushAsync();
                            }

                            // Chỉ gọi GC khi số tick lớn (giảm overhead)
                            if (hourTickCount > 50000)
                            {
                                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                            }
                        }

                        await writer.FlushAsync();
                    }

                    if (dailyTickCount > 0)
                    {
                        txtLogs.AppendText(
                            $" → Hoàn thành {dateStr}: {dailyTickCount:N0} ticks ({validHourCount}/24 giờ có dữ liệu)\r\n");
                    }
                    else
                    {
                        txtLogs.AppendText($" → {dateStr}: Không có dữ liệu\r\n");
                        try { File.Delete(csvPath); } catch { }
                    }

                    current = current.AddDays(1);
                }

                txtLogs.AppendText("\r\nHoàn tất quá trình xuất CSV.\r\n");
            }
            catch (OperationCanceledException)
            {
                txtLogs.AppendText("\r\n→ Đã hủy theo yêu cầu.\r\n");
            }
            catch (Exception ex)
            {
                txtLogs.AppendText($"\r\n❌ Lỗi nghiêm trọng: {ex.Message}\r\n{ex.StackTrace}\r\n");
            }
            finally
            {
                btnTestToCSV.Enabled = true;
                btnDownloadConvert.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        // =========================================================
        // CANCEL
        // =========================================================
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            txtLogs.AppendText("Yêu cầu hủy... đang dừng...\r\n");
        }

        // =========================================================
        // BROWSE
        // =========================================================
        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
                txtOutputFolder.Text = fbd.SelectedPath;
        }
    }
}