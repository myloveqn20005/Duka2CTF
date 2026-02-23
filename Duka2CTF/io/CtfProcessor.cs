using Duka2CTF.model;
using System;
using System.Collections.Generic;
using System.IO;

namespace Duka2CTF.io;

public static class CtfProcessor
{
    // Hàm nén danh sách Tick của một ngày thành mảng byte
    public static byte[] EncodeDay(List<Tick> ticks, ref long lastTime, ref int lastBid, ref int lastAsk)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        foreach (var tick in ticks)
        {
            // Tính toán chênh lệch (Delta)
            long deltaTime = tick.Utc - lastTime;
            int deltaBid = tick.Bid - lastBid;
            int deltaAsk = tick.Ask - lastAsk;

            // 1. Xác định bits thời gian (TimeBits)
            byte tBits = (byte)(deltaTime <= 2 ? deltaTime : 3);

            // 2. Xác định bits giá Bid/Ask (-3 đến +3 points thì nén được)
            byte bBits = (byte)((deltaBid >= -3 && deltaBid <= 3) ? (deltaBid + 3) : 7);
            byte aBits = (byte)((deltaAsk >= -3 && deltaAsk <= 3) ? (deltaAsk + 3) : 7);

            // 3. Đóng gói vào Control Byte (1 byte)
            // Cấu trúc: [TimeBits(2bit)][BidBits(3bit)][AskBits(3bit)]
            byte controlByte = (byte)((tBits << 6) | (bBits << 3) | aBits);
            writer.Write(controlByte);

            // 4. Ghi dữ liệu bổ sung nếu vượt ngưỡng nén
            if (tBits == 3) writer.Write((uint)tick.Utc);
            if (bBits == 7) writer.Write(tick.Bid);
            if (aBits == 7) writer.Write(tick.Ask);

            // Cập nhật trạng thái cuối để so sánh với Tick kế tiếp
            lastTime = tick.Utc;
            lastBid = tick.Bid;
            lastAsk = tick.Ask;
        }
        return ms.ToArray();
    }
}