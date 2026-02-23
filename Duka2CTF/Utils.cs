using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Windows.Forms;

namespace Duka2CTF;

public static class Utils
{
    private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    private static decimal[] DEC_DIVS = new decimal[7] { 1m, 0.1m, 0.01m, 0.001m, 0.0001m, 0.00001m, 0.000001m };

    private static double[] DBL_DIVS = new double[7] { 1.0, 0.1, 0.01, 0.001, 0.0001, 1E-05, 1E-06 };

    private static decimal[] DEC_MULTS = new decimal[7] { 1m, 10m, 100m, 1000m, 10000m, 100000m, 1000000m };

    private static double[] DBL_MULTS = new double[7] { 1.0, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0 };

    public static int DAY_SECONDS = 86400;

    public static int MAX_DAYS = 50000;

    private static int[] timeZoneShifts = new int[MAX_DAYS];

    private static List<string> profilingLogs = new List<string>(4000);

    private static Stopwatch stopwatch = new Stopwatch();

    // private static int GWL_HWNDPARENT = -8;

    // ĐÃ SỬA: Loại bỏ phụ thuộc vào Simulation.Instance
    public static void PrepareTimeZoneShifts()
    {
        for (int j = 0; j < MAX_DAYS; j++)
        {
            timeZoneShifts[j] = 0; // Mặc định dùng UTC cho Tool Converter
        }
    }

    public static long ToUnixTime(DateTime t)
    {
        return (long)(t - epoch).TotalSeconds;
    }

    public static DateTime FromUnixTime(long t)
    {
        return epoch.AddSeconds(t);
    }

    public static int DateToDayNumber(int day, int month, int year)
    {
        return (int)(ToUnixTime(new DateTime(year, month, day)) / DAY_SECONDS);
    }

    public static int TimeToDayNumber(long time)
    {
        return (int)(time / DAY_SECONDS);
    }

    public static long DayStartingTime(int day)
    {
        return (long)day * (long)DAY_SECONDS;
    }

    public static long DaysToSeconds(int days)
    {
        return (long)days * (long)DAY_SECONDS;
    }

    public static DateTime DayNumberToDate(int day)
    {
        return FromUnixTime(DayStartingTime(day));
    }

    public static long AdjustTimeZone(long gmt)
    {
        return gmt + timeZoneShifts[gmt / DAY_SECONDS];
    }

    public static float TimeShiftInHours(long time)
    {
        return (float)timeZoneShifts[time / DAY_SECONDS] / 3600f;
    }

    // ĐÃ SỬA: Dùng InvariantCulture thay cho GlobalSettings
    public static string DayRangeToString(int fromDay, int toDay)
    {
        DateTime dt1 = DayNumberToDate(fromDay);
        DateTime dt2 = DayNumberToDate(toDay);
        return dt1.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " --> " + dt2.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public static double FastRoundToTwoDigits(double x)
    {
        if (x >= 0.0)
        {
            return (double)(long)(x * 100.0 + 0.5) * 0.01;
        }
        return (double)(long)(x * 100.0 - 0.5) * 0.01;
    }

    public static int FastRound(double x)
    {
        if (x >= 0.0)
        {
            return (int)(x + 0.5);
        }
        return (int)(x - 0.5);
    }

    public static string HashSha1(string input)
    {
        using SHA1 sha1 = SHA1.Create();
        byte[] array = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder sb = new StringBuilder(array.Length * 2);
        byte[] array2 = array;
        foreach (byte b in array2)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    public static decimal ToDecimal(int price, int digits)
    {
        return (decimal)price * DEC_DIVS[digits];
    }

    public static double PriceAsDouble(int price, int digits)
    {
        return (double)price * DBL_DIVS[digits];
    }

    public static int PriceToInt(decimal price, int digits)
    {
        return (int)(price * DEC_MULTS[digits]);
    }

    public static int PriceToInt(double price, int digits)
    {
        return (int)Math.Round(price * DBL_MULTS[digits]);
    }

    // ĐÃ SỬA: Dùng InvariantCulture thay cho GlobalSettings
    public static string PriceAsString(int price, int digits)
    {
        return ((double)price * DBL_DIVS[digits]).ToString("N" + digits, CultureInfo.InvariantCulture);
    }

    public static string MoneyToStr(double x)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", x);
    }

    public static string LotsToStr(int lots)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", (double)lots * 0.01);
    }

    public static string PointsToPipsString(int points, int pipSizePts)
    {
        return ((decimal)points / (decimal)pipSizePts).ToString("N" + (pipSizePts.ToString().Length - 1), CultureInfo.InvariantCulture);
    }

    public static decimal PointsToPips(int points, int pipSizePts)
    {
        return (decimal)points / (decimal)pipSizePts;
    }

    public static string TimeZoneToStr(TimeZoneInfo z)
    {
        return z.DisplayName + (z.SupportsDaylightSavingTime ? " [ uses DST ]" : " [ no DST ]");
    }

    public static string GetExecutableDirectory()
    {
        return Path.GetDirectoryName(AppContext.BaseDirectory);
    }

    public static void AddLog(string s)
    {
        try
        {
            File.AppendAllText(Path.Combine(GetExecutableDirectory(), "duka2ctf_errors.log"), DateTime.Now.ToString() + "  " + s + Environment.NewLine);
        }
        catch
        {
        }
    }

    private static void ShowMessage(Form owner, string text, TaskDialogIcon icon)
    {
        TaskDialogPage page = new TaskDialogPage
        {
            SizeToContent = true,
            Text = text,
            Icon = icon,
            Heading = null,
            Caption = "Duka2CTF"
        };
        if (owner == null)
        {
            TaskDialog.ShowDialog(page);
        }
        else
        {
            TaskDialog.ShowDialog(owner, page);
        }
    }

    public static void ShowInformationMessage(Form owner, string text)
    {
        ShowMessage(owner, text, TaskDialogIcon.Information);
    }

    public static void ShowWarningMessage(Form owner, string text)
    {
        ShowMessage(owner, text, TaskDialogIcon.Warning);
    }

    public static void ShowNeutralMessage(Form owner, string text)
    {
        ShowMessage(owner, text, TaskDialogIcon.None);
    }

    public static void ShowErrorMessage(Form owner, string text, Exception ex = null)
    {
        StringBuilder sb = new StringBuilder(text);
        if (ex != null)
        {
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(ex.ToString());
        }
        ShowMessage(owner, sb.ToString(), TaskDialogIcon.Error);
    }

    public static bool ShowQuestionMessage(Form owner, string text)
    {
        return TaskDialogButton.Yes == TaskDialog.ShowDialog(owner, new TaskDialogPage
        {
            SizeToContent = true,
            Text = text,
            Icon = TaskDialogIcon.None,
            Heading = null,
            Caption = "Xác nhận",
            Buttons =
            {
                TaskDialogButton.Yes,
                TaskDialogButton.No
            }
        });
    }

    public static void StartProfiling()
    {
        stopwatch.Restart();
    }

    public static void EndProfiling(string s)
    {
        stopwatch.Stop();
        profilingLogs.Add(s + " " + stopwatch.Elapsed.TotalMicroseconds);
    }

    public static void PrintProfilingLogs()
    {
        foreach (string profilingLog in profilingLogs)
        {
            AddLog(profilingLog);
        }
    }

    // ĐÃ SỬA: Loại bỏ phụ thuộc vào GlobalSettings.Instance
    public static void SetFormOnTop(Form form, bool allowTopMost = true)
    {
        try
        {
            if (form != null)
            {
                form.TopMost = allowTopMost;
            }
        }
        catch
        {
        }
    }

    public static void SetGridOptions(this DataGridView dgv)
    {
        dgv.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgv, true, null);
        dgv.DefaultCellStyle.Font = null;
        dgv.AlternatingRowsDefaultCellStyle.Font = null;
        dgv.ColumnHeadersDefaultCellStyle.Font = null;
        dgv.RowHeadersDefaultCellStyle.Font = null;
    }

    public static void SetDoubleBuffered(this Panel dgv)
    {
        dgv.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dgv, true, null);
    }

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }
        return new IntPtr(SetWindowLong32(hWnd, nIndex, ((IntPtr)dwNewLong).ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);
}