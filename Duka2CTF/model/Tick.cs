using System;
using System.Text.Json.Serialization;

namespace Duka2CTF.model
{
    /// <summary>
    /// Đại diện cho một tick giá (Bid/Ask) từ nguồn dữ liệu tài chính.
    /// Sử dụng struct để tối ưu bộ nhớ khi xử lý số lượng lớn tick.
    /// </summary>
    public struct Tick : IComparable<Tick>
    {
        [JsonInclude]
        public int Bid { get; set; }

        [JsonInclude]
        public int Ask { get; set; }

        [JsonInclude]
        public long Time { get; set; }

        [JsonInclude]
        public long Utc { get; set; }

        [JsonInclude]
        public int SymbolId { get; set; }

        // Constructor mặc định (cho JSON hoặc khởi tạo rỗng)
        public Tick()
        {
        }

        // Constructor đầy đủ
        public Tick(int bid, int ask, long utc, long time, int symbolId)
        {
            Bid = bid;
            Ask = ask;
            Utc = utc;
            Time = time;
            SymbolId = symbolId;
        }

        // Constructor tiện lợi khi parse từ Dukascopy (chỉ cần Bid, Ask, Utc)
        public Tick(int bid, int ask, long utc)
            : this(bid, ask, utc, utc, 0)
        {
        }

        public bool IsBefore(Tick other)
        {
            if (Time != other.Time)
            {
                return Time < other.Time;
            }

            return SymbolId < other.SymbolId;
        }

        public int CompareTo(Tick other)
        {
            int timeCompare = Time.CompareTo(other.Time);
            if (timeCompare != 0)
            {
                return timeCompare;
            }

            return SymbolId.CompareTo(other.SymbolId);
        }

        public override string ToString()
        {
            var utcDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(Utc);

            return $"Tick [Bid={Bid}, Ask={Ask}, " +
                   $"Utc={utcDate:yyyy-MM-dd HH:mm:ss.fff}, " +
                   $"SymbolId={SymbolId}]";
        }
    }
}