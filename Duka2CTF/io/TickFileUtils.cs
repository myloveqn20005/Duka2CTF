using System;
using System.Collections.Generic;
using System.IO;
using Duka2CTF.model;

namespace Duka2CTF.io;

public class TickFileUtils
{
	public const string EXTENSION = ".ctf";

	public const string DUKASCOPY_PREFIX = "sim_ticks_";

	public const string TRUEFX_PREFIX = "sim_truefx_ticks_";

	public const string HISTDATACOM_PREFIX = "sim_histdata_ticks_";

	public const string METATRADER_PREFIX = "sim_mt4_ticks_";

	public static TICK_PROVIDER[] GetProviders()
	{
		return (TICK_PROVIDER[])Enum.GetValues(typeof(TICK_PROVIDER));
	}

	public static TICK_PROVIDER GetProviderFromFileName(string file)
	{
		if (file == null)
		{
			return TICK_PROVIDER.Dukascopy;
		}
		string fileName = Path.GetFileName(file);
		if (fileName.StartsWith("sim_truefx_ticks_"))
		{
			return TICK_PROVIDER.TrueFX;
		}
		if (fileName.StartsWith("sim_ticks_"))
		{
			return TICK_PROVIDER.Dukascopy;
		}
		return TICK_PROVIDER.HistDataCom;
	}

	public static string[] GetProviderNames()
	{
		return Enum.GetNames(typeof(TICK_PROVIDER));
	}

	private static string GetPrefix(TICK_PROVIDER provider)
	{
		return provider switch
		{
			TICK_PROVIDER.Dukascopy => "sim_ticks_", 
			TICK_PROVIDER.TrueFX => "sim_truefx_ticks_", 
			TICK_PROVIDER.HistDataCom => "sim_histdata_ticks_", 
			_ => throw new NotSupportedException(), 
		};
	}

	public static IProviderInfo GetProviderInfo(TICK_PROVIDER provider)
	{
		return provider switch
		{
			TICK_PROVIDER.Dukascopy => new DukascopyInfo(), 
			TICK_PROVIDER.TrueFX => new TrueFxInfo(), 
			TICK_PROVIDER.HistDataCom => new HistDataComInfo(), 
			_ => throw new NotSupportedException(), 
		};
	}

	public static List<string> ListTickFiles(string directory, TICK_PROVIDER provider, bool asSymbols)
	{
		string[] array = GetProviderInfo(provider).AvailableSymbolNames();
		List<string> results = new List<string>();
		string[] array2 = array;
		foreach (string s in array2)
		{
			try
			{
				string fullPath = Path.Combine(directory, SymbolToFileName(s, provider));
				if (File.Exists(fullPath) && new FileInfo(fullPath).Length >= 240064)
				{
					results.Add(asSymbols ? s : fullPath);
				}
			}
			catch
			{
			}
		}
		return results;
	}

	public static string SymbolFromFileName(string fullPath, TICK_PROVIDER provider)
	{
		return Path.GetFileNameWithoutExtension(fullPath).Substring(GetPrefix(provider).Length).ToUpper();
	}

	public static string SymbolToFileName(string symbol, TICK_PROVIDER provider)
	{
		return GetPrefix(provider) + symbol.ToUpper() + ".ctf";
	}

    public static string FullCtfFilePath(string symbol, TICK_PROVIDER provider)
    {
        // Chỉ đơn giản là trả về tên file trong thư mục chạy phần mềm
        return SymbolToFileName(symbol, provider);
    }
}
