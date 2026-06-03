using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElectronBotExpressionController;

internal sealed class NotificationSettings
{
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "notification-settings.json");

	public List<string> AudioTargets { get; set; } = DefaultAudioTargets();
	public int SoundThresholdPercent { get; set; } = 2;
	public int FrameIntervalMs { get; set; } = 100;
	public bool EnableServo { get; set; } = true;
	public int CooldownSeconds { get; set; } = 5;
	public string DefaultFacePath { get; set; } = "";
	public List<NotificationProfile> Profiles { get; set; } = DefaultProfiles();
	public List<ExpressionAction> NotificationFrames { get; set; } = NotificationMotionFrames.NodAndWaveLeft().ToList();
	public List<ScheduledAction> ScheduledActions { get; set; } = DefaultScheduledActions();

	[JsonIgnore]
	public string? LoadWarning { get; set; }

	public static NotificationSettings LoadOrCreate()
	{
		if (!File.Exists(SettingsPath))
		{
			NotificationSettings settings = CreateDefault();
			settings.Save();
			return settings;
		}
		try
		{
			NotificationSettings settings = JsonSerializer.Deserialize<NotificationSettings>(File.ReadAllText(SettingsPath, Encoding.UTF8), JsonOptions) ?? CreateDefault();
			settings.Sanitize();
			return settings;
		}
		catch (Exception ex)
		{
			NotificationSettings settings = CreateDefault();
			settings.LoadWarning = "配置读取失败，已临时使用默认值：" + ex.Message;
			return settings;
		}
	}

	public static NotificationSettings CreateDefault()
	{
		return new NotificationSettings
		{
			AudioTargets = DefaultAudioTargets(),
			Profiles = DefaultProfiles(),
			NotificationFrames = NotificationMotionFrames.NodAndWaveLeft().ToList(),
			ScheduledActions = DefaultScheduledActions()
		};
	}

		public static List<string> DefaultAudioTargets()
		{
			return new List<string> { "WeChat", "Weixin", "微信", "QQ", "Tim" };
		}

	public void Save()
	{
		Sanitize();
		Directory.CreateDirectory(AppContext.BaseDirectory);
		File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	public NotificationPlan ResolvePlan(string signal, string fallbackFacePath)
	{
		Sanitize();
		NotificationProfile? profile = Profiles.FirstOrDefault(item => item.Matches(signal));
		string facePath = ResolveExistingPath(profile?.FacePath ?? DefaultFacePath, fallbackFacePath);
		List<ExpressionAction> frames = ResolveFrames(profile, NotificationFrames);
		if (frames.Count == 0)
		{
			frames = NotificationMotionFrames.NodAndWaveLeft().ToList();
		}
		return new NotificationPlan(profile?.Name ?? "默认提醒", facePath, CloneFrames(frames));
	}

	public void SyncAudioTargetsFromProfiles()
	{
		AudioTargets = Profiles.Where(profile => profile.Enabled)
			.SelectMany(profile => profile.Keywords ?? new List<string>())
			.Where(keyword => !string.IsNullOrWhiteSpace(keyword))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
		if (AudioTargets.Count == 0)
		{
			AudioTargets = DefaultAudioTargets();
		}
	}

	private void Sanitize()
	{
		AudioTargets = CleanList(AudioTargets);
		SoundThresholdPercent = Math.Clamp(SoundThresholdPercent, 1, 100);
		FrameIntervalMs = Math.Clamp(FrameIntervalMs, 20, 1000);
		CooldownSeconds = Math.Clamp(CooldownSeconds, 0, 60);
		DefaultFacePath = DefaultFacePath?.Trim() ?? "";
		Profiles = Profiles?.Where(profile => profile != null).ToList() ?? DefaultProfiles();
		if (Profiles.Count == 0)
		{
			Profiles = DefaultProfiles();
		}
		foreach (NotificationProfile profile in Profiles)
		{
			profile.Sanitize();
		}
		NotificationFrames = CloneFrames(NotificationFrames);
		if (NotificationFrames.Count == 0)
		{
			NotificationFrames = NotificationMotionFrames.NodAndWaveLeft().ToList();
		}
		ScheduledActions = ScheduledActions?.Where(action => action != null).ToList() ?? DefaultScheduledActions();
		if (ScheduledActions.Count == 0)
		{
			ScheduledActions = DefaultScheduledActions();
		}
		foreach (ScheduledAction action in ScheduledActions)
		{
			action.Sanitize();
		}
		SyncAudioTargetsFromProfiles();
	}

	private static List<NotificationProfile> DefaultProfiles()
	{
		return new List<NotificationProfile>
		{
			new NotificationProfile
			{
				Name = "微信",
				Keywords = new List<string> { "WeChat", "Weixin", "微信" },
				FacePath = "Assets/Notify/wechat_notify.png"
			},
			new NotificationProfile
				{
					Name = "QQ",
					Keywords = new List<string> { "QQ", "Tim" },
					FacePath = "Assets/Emoji/excited.png"
				}
			};
		}

	private static List<ScheduledAction> DefaultScheduledActions()
	{
		return new List<ScheduledAction>
		{
			new ScheduledAction { Time = "08:30", Name = "上班", ActionKey = "schedule-work-start", FacePath = "Assets/Scheduled/work_start.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "09:00", Name = "努力工作", ActionKey = "schedule-work-hard", FacePath = "Assets/Scheduled/work_hard.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "11:00", Name = "累了", ActionKey = "schedule-tired", FacePath = "Assets/Scheduled/tired.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "11:30", Name = "吃饭", ActionKey = "schedule-lunch", FacePath = "Assets/Scheduled/lunch.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "12:00", Name = "睡觉", ActionKey = "schedule-sleep", FacePath = "Assets/Scheduled/sleep.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "13:30", Name = "回到工作", ActionKey = "schedule-back", FacePath = "Assets/Scheduled/back_to_work.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "15:00", Name = "休息倒水", ActionKey = "schedule-water", FacePath = "Assets/Scheduled/water.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "16:50", Name = "准备收拾", ActionKey = "schedule-off", FacePath = "Assets/Scheduled/off_work.png", ActionPath = "Assets/Emoji/normal.json" },
			new ScheduledAction { Time = "17:00", Name = "准备下班", ActionKey = "schedule-off", FacePath = "Assets/Scheduled/off_work.png", ActionPath = "Assets/Emoji/normal.json" }
		};
	}

	private static List<string> CleanList(IEnumerable<string>? values)
	{
		return values?.Where(value => !string.IsNullOrWhiteSpace(value))
			.Select(value => value.Trim())
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList() ?? new List<string>();
	}

	private static List<ExpressionAction> CloneFrames(IEnumerable<ExpressionAction>? frames)
	{
		return frames?.Where(frame => frame != null)
			.Select(frame => new ExpressionAction
			{
				J1 = frame.J1,
				J2 = frame.J2,
				J3 = frame.J3,
				J4 = frame.J4,
				J5 = frame.J5,
				J6 = frame.J6
			})
			.ToList() ?? new List<ExpressionAction>();
	}

	private static List<ExpressionAction> ResolveFrames(NotificationProfile? profile, IEnumerable<ExpressionAction> fallbackFrames)
	{
		if (profile?.Frames?.Count > 0)
		{
			return profile.Frames;
		}
		List<ExpressionAction> actionFrames = LoadFrames(profile?.ActionPath);
		if (actionFrames.Count > 0)
		{
			return actionFrames;
		}
		return CloneFrames(fallbackFrames);
	}

	private static List<ExpressionAction> LoadFrames(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return new List<ExpressionAction>();
		}
		string fullPath = ResolveFullPath(path);
		if (!File.Exists(fullPath))
		{
			return new List<ExpressionAction>();
		}
		try
		{
			return ExpressionAction.Load(fullPath);
		}
		catch
		{
			return new List<ExpressionAction>();
		}
	}

	private static string ResolveExistingPath(string? path, string fallbackPath)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return fallbackPath;
		}
		string fullPath = ResolveFullPath(path);
		return File.Exists(fullPath) ? fullPath : fallbackPath;
	}

	private static string ResolveFullPath(string path)
	{
		return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
	}
}
