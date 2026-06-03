using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.CoreAudioApi;

namespace ElectronBotExpressionController;

internal sealed class AudioSignalDetector : IDisposable
{
	private readonly MMDeviceEnumerator _enumerator = new MMDeviceEnumerator();

	private readonly Dictionary<string, DateTime> _lastHits = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

	private DateTime _baselineUntil = DateTime.MinValue;

	public void ResetBaseline()
	{
		ResetBaseline(TimeSpan.FromSeconds(1.2));
	}

	public void ResetBaseline(TimeSpan duration)
	{
		_baselineUntil = DateTime.Now.Add(duration);
		_lastHits.Clear();
	}

	public IEnumerable<string> Scan(IEnumerable<string> targets, float threshold, int cooldownSeconds)
	{
		using MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
		SessionCollection sessions = device.AudioSessionManager.Sessions;
		List<string> targetList = targets.Where((string x) => !string.IsNullOrWhiteSpace(x)).ToList();
		for (int i = 0; i < sessions.Count; i++)
		{
			using AudioSessionControl session = sessions[i];
			float masterPeakValue = session.AudioMeterInformation.MasterPeakValue;
			AudioSessionInfo audioSessionInfo = AudioSessionInfo.From(session, masterPeakValue);
			AppendAudioLog(audioSessionInfo);
			if (audioSessionInfo.Matches(targetList) && !(DateTime.Now < _baselineUntil) && !(masterPeakValue < threshold) && (!_lastHits.TryGetValue(audioSessionInfo.Key, out var value) || !(DateTime.Now - value < TimeSpan.FromSeconds(Math.Max(0, cooldownSeconds)))))
			{
				_lastHits[audioSessionInfo.Key] = DateTime.Now;
				yield return $"{audioSessionInfo.Name} peak={masterPeakValue:0.000}";
			}
		}
	}

	public IEnumerable<AudioSessionInfo> CaptureActiveSessions(float minPeak)
	{
		using MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
		SessionCollection sessions = device.AudioSessionManager.Sessions;
		for (int i = 0; i < sessions.Count; i++)
		{
			using AudioSessionControl session = sessions[i];
			float masterPeakValue = session.AudioMeterInformation.MasterPeakValue;
			AudioSessionInfo audioSessionInfo = AudioSessionInfo.From(session, masterPeakValue);
			AppendAudioLog(audioSessionInfo);
			if (masterPeakValue >= minPeak)
			{
				yield return audioSessionInfo;
			}
		}
	}

	private static void AppendAudioLog(AudioSessionInfo info)
	{
		if (info.Peak < 0.005f)
		{
			return;
		}
		try
		{
			File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "audio-sessions.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {info.Name} pid={info.ProcessId} peak={info.Peak:0.000} muted={info.Muted} volume={info.Volume:0.00}{Environment.NewLine}", Encoding.UTF8);
		}
		catch
		{
		}
	}

	public void Dispose()
	{
		_enumerator.Dispose();
	}
}
