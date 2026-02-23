namespace Duka2CTF;

// Định nghĩa các loại nhà cung cấp dữ liệu
public enum TICK_PROVIDER
{
    Dukascopy,
    TrueFX,
    HistDataCom,
    Other
}

// Định nghĩa mức độ ảnh hưởng tin tức
public enum IMPACT
{
    LOW,
    MEDIUM,
    HIGH
}

// Các thành phần phụ trợ để TickFileUtils.cs không báo lỗi
public interface IProviderInfo { string[] AvailableSymbolNames(); }
public class DukascopyInfo : IProviderInfo { public string[] AvailableSymbolNames() => new[] { "XAUUSD" }; }
public class TrueFxInfo : IProviderInfo { public string[] AvailableSymbolNames() => new[] { "EURUSD" }; }
public class HistDataComInfo : IProviderInfo { public string[] AvailableSymbolNames() => new[] { "GBPUSD" }; }