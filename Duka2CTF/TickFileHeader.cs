using System;
using System.IO;

namespace Duka2CTF;

public class TickFileHeader
{
	public const int HEADER_MAX_DAYS = 30000;

	public const int HEADER_SIZE = 64;

	public const int DAY_TABLE_SIZE = 240000;

	public int firstDay;

	public int lastDay;

	public int digits;

	public static TickFileHeader? ReadHeader(string file)
	{
		using BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
		return ReadHeader(reader);
	}

	public static TickFileHeader? ReadHeader(BinaryReader reader)
	{
		if (reader.BaseStream.Length < 64)
		{
			return null;
		}
		TickFileHeader tickFileHeader = new TickFileHeader();
		reader.BaseStream.Seek(0L, SeekOrigin.Begin);
		tickFileHeader.firstDay = reader.ReadInt32();
		tickFileHeader.lastDay = reader.ReadInt32();
		tickFileHeader.digits = reader.ReadInt32();
		return tickFileHeader;
	}

	public static TickFileHeader ReadHeader(byte[] data)
	{
		if (data.Length < 64)
		{
			return null;
		}
		return new TickFileHeader
		{
			firstDay = BitConverter.ToInt32(data, 0),
			lastDay = BitConverter.ToInt32(data, 4),
			digits = BitConverter.ToInt32(data, 8)
		};
	}

	public static long[] ReadDayTable(BinaryReader reader)
	{
		if (reader.BaseStream.Length < 240064)
		{
			return null;
		}
		reader.BaseStream.Seek(64L, SeekOrigin.Begin);
		long[] dayTable = new long[30000];
		for (int i = 0; i < 30000; i++)
		{
			dayTable[i] = reader.ReadInt64();
		}
		return dayTable;
	}

	public static long[] ReadDayTable(byte[] data)
	{
		if (data.Length < 240064)
		{
			return null;
		}
		long[] dayTable = new long[30000];
		for (int i = 0; i < 30000; i++)
		{
			dayTable[i] = BitConverter.ToInt64(data, 64 + i * 8);
		}
		return dayTable;
	}
}
