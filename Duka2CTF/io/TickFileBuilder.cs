using System;
using System.IO;

namespace Duka2CTF.io;

public class TickFileBuilder : IDisposable
{
	private string fileName;

	private BinaryWriter? writer;

	private int digits;

	public TickFileBuilder(string file, int digits)
	{
		fileName = file;
		this.digits = digits;
	}

	public void Create()
	{
		FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
		writer = new BinaryWriter(fs);
	}

	public void OpenForUpdate()
	{
		FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Write);
		writer = new BinaryWriter(fs);
	}

	public void Close()
	{
		if (writer != null)
		{
			writer.Close();
			writer = null;
		}
	}

	public void TryRemoveBrokenFile()
	{
		if (writer == null)
		{
			return;
		}
		try
		{
			if (writer.BaseStream.Length < 64)
			{
				File.Delete(fileName);
			}
		}
		catch
		{
		}
	}

	public void AddDay(byte[] data, int day)
	{
		if (writer.BaseStream.Length == 0L)
		{
			writer.Write(day);
			writer.Write(day);
			writer.Write(digits);
		}
		else
		{
			writer.Seek(4, SeekOrigin.Begin);
			writer.Write(day);
		}
		long end = Math.Max(240064L, writer.BaseStream.Length);
		writer.Seek(64 + day * 8, SeekOrigin.Begin);
		writer.Write(end);
		if (data.Length != 0)
		{
			if (240064 >= writer.BaseStream.Length)
			{
				writer.Seek(240064, SeekOrigin.Begin);
			}
			else
			{
				writer.Seek(0, SeekOrigin.End);
			}
			writer.Write(data);
		}
	}

    public void Dispose()
    {
        Close();
    }
}
