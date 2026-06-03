using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;

namespace ElectronBotExpressionController;

internal sealed record AudioSessionInfo(string Key, string Name, int ProcessId, float Peak, bool Muted, float Volume)
{
	public bool Matches(IEnumerable<string> targets)
	{
		return targets.Any((string target) => Name.Contains(target, StringComparison.OrdinalIgnoreCase));
	}

	public static AudioSessionInfo From(AudioSessionControl session, float peak)
	{
		int num = 0;
		string text = "";
		string text2 = "";
		bool muted = false;
		float volume = 0f;
		try
		{
			num = (int)session.GetProcessID;
			if (num > 0)
			{
				using Process process = Process.GetProcessById(num);
				text = process.ProcessName;
			}
		}
		catch
		{
		}
		try
		{
			text2 = session.DisplayName ?? "";
		}
		catch
		{
		}
		try
		{
			muted = session.SimpleAudioVolume.Mute;
			volume = session.SimpleAudioVolume.Volume;
		}
		catch
		{
		}
		string text3 = string.Join(" ", new string[2] { text, text2 }.Where((string x) => !string.IsNullOrWhiteSpace(x))).Trim();
		if (string.IsNullOrWhiteSpace(text3))
		{
			text3 = $"pid:{num}";
		}
		return new AudioSessionInfo($"{num}|{text3}", text3, num, peak, muted, volume);
	}
}
