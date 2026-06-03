using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ElectronBotExpressionController;

internal static class ExpressionCatalog
{
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	private static string CustomCatalogPath => Path.Combine(AppContext.BaseDirectory, "expression-catalog.json");

	public static List<ExpressionItem> Load()
	{
		string emojiRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Emoji");
		string scheduledRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Scheduled");
		string normalAction = Path.Combine(emojiRoot, "normal.json");
		List<ExpressionItem> items = new List<ExpressionItem>
		{
			Create(emojiRoot, "left", "左转", "left.png", "left.json", "动作"),
			Create(emojiRoot, "right", "右转", "right.png", "right.json", "动作"),
			Create(emojiRoot, "normal", "静态", "normal.png", "normal.json", "动作"),
			Create(emojiRoot, "look", "观察", "normal.png", "look.json", "动作"),
			Create(emojiRoot, "anger", "愤怒", "anger.png", "anger.json", "情绪"),
			Create(emojiRoot, "disdain", "不屑", "disdain.png", "disdain.json", "情绪"),
			Create(emojiRoot, "excited", "兴奋", "excited.png", "excited.json", "情绪"),
			Create(emojiRoot, "fear", "惊恐", "fear.png", "fear.json", "情绪"),
			Create(emojiRoot, "sad", "难过", "sad.png", "sad.json", "情绪"),
			Create(emojiRoot, "hello", "主人你好呀", "hello.jpg", "hello.json", "语音互动"),
			Create(emojiRoot, "goodbye", "主人再见呀", "goodbye.jpg", "goodbye.json", "语音互动"),
			new ExpressionItem("schedule-work-start", "上班", Path.Combine(scheduledRoot, "work_start.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-work-hard", "努力工作", Path.Combine(scheduledRoot, "work_hard.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-tired", "累了", Path.Combine(scheduledRoot, "tired.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-lunch", "吃饭", Path.Combine(scheduledRoot, "lunch.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-sleep", "睡觉", Path.Combine(scheduledRoot, "sleep.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-back", "回到工作", Path.Combine(scheduledRoot, "back_to_work.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-water", "倒水", Path.Combine(scheduledRoot, "water.png"), normalAction) { Category = "通知提醒" },
			new ExpressionItem("schedule-off", "准备下班", Path.Combine(scheduledRoot, "off_work.png"), normalAction) { Category = "通知提醒" }
		};
		foreach (ExpressionItem item in LoadCustomItems())
		{
			if (!items.Any(existing => existing.Key.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
			{
				items.Add(item);
			}
		}
		return items;
	}

	public static ExpressionItem AddCustom(string title, string category, string imagePath, string actionPath)
	{
		List<CatalogEntry> entries = LoadEntries();
		string key = GenerateKey(entries);
		string savedImagePath = CopyToCustomAssets(imagePath, key + Path.GetExtension(imagePath));
		string savedActionPath = CopyToCustomAssets(actionPath, key + Path.GetExtension(actionPath));
		CatalogEntry entry = new CatalogEntry
		{
			Key = key,
			Title = title.Trim(),
			ImagePath = MakeRelativeIfUnderBaseDirectory(savedImagePath),
			ActionPath = MakeRelativeIfUnderBaseDirectory(savedActionPath),
			Category = string.IsNullOrWhiteSpace(category) ? "动作" : category.Trim()
		};
		entries.Add(entry);
		SaveEntries(entries);
		return ToItem(entry);
	}

	private static ExpressionItem Create(string root, string key, string title, string image, string action, string category)
	{
		return new ExpressionItem(key, title, Path.Combine(root, image), Path.Combine(root, action)) { Category = category };
	}

	private static List<ExpressionItem> LoadCustomItems()
	{
		return LoadEntries()
			.Select(ToItem)
			.Where(item => File.Exists(item.ImagePath) && File.Exists(item.ActionPath))
			.ToList();
	}

	private static List<CatalogEntry> LoadEntries()
	{
		if (!File.Exists(CustomCatalogPath))
		{
			return new List<CatalogEntry>();
		}
		try
		{
			return JsonSerializer.Deserialize<List<CatalogEntry>>(File.ReadAllText(CustomCatalogPath, Encoding.UTF8), JsonOptions)?
				.Where(entry => entry != null)
				.Select(Sanitize)
				.Where(entry => !string.IsNullOrWhiteSpace(entry.Key)
					&& !string.IsNullOrWhiteSpace(entry.Title)
					&& !string.IsNullOrWhiteSpace(entry.ImagePath)
					&& !string.IsNullOrWhiteSpace(entry.ActionPath))
				.ToList() ?? new List<CatalogEntry>();
		}
		catch
		{
			return new List<CatalogEntry>();
		}
	}

	private static void SaveEntries(List<CatalogEntry> entries)
	{
		File.WriteAllText(CustomCatalogPath, JsonSerializer.Serialize(entries.Select(Sanitize).ToList(), JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	private static CatalogEntry Sanitize(CatalogEntry entry)
	{
		entry.Key = entry.Key?.Trim() ?? "";
		entry.Title = entry.Title?.Trim() ?? "";
		entry.ImagePath = entry.ImagePath?.Trim() ?? "";
		entry.ActionPath = entry.ActionPath?.Trim() ?? "";
		entry.Category = string.IsNullOrWhiteSpace(entry.Category) ? "动作" : entry.Category.Trim();
		return entry;
	}

	private static ExpressionItem ToItem(CatalogEntry entry)
	{
		entry = Sanitize(entry);
		return new ExpressionItem(entry.Key, entry.Title, ResolvePath(entry.ImagePath), ResolvePath(entry.ActionPath)) { Category = entry.Category };
	}

	private static string CopyToCustomAssets(string sourcePath, string fileName)
	{
		string sourceFullPath = ResolvePath(sourcePath);
		string customRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Custom");
		Directory.CreateDirectory(customRoot);
		string targetPath = Path.Combine(customRoot, SanitizeFileName(fileName));
		if (!Path.GetFullPath(sourceFullPath).Equals(Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
		{
			File.Copy(sourceFullPath, targetPath, overwrite: false);
		}
		return targetPath;
	}

	private static string GenerateKey(List<CatalogEntry> entries)
	{
		string key;
		do
		{
			key = "custom-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
		}
		while (entries.Any(entry => entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase)));
		return key;
	}

	private static string SanitizeFileName(string fileName)
	{
		foreach (char invalid in Path.GetInvalidFileNameChars())
		{
			fileName = fileName.Replace(invalid, '-');
		}
		return fileName;
	}

	private static string MakeRelativeIfUnderBaseDirectory(string path)
	{
		string baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		string fullPath = Path.GetFullPath(path);
		if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
		{
			return fullPath[baseDir.Length..].Replace(Path.DirectorySeparatorChar, '/');
		}
		return path;
	}

	private static string ResolvePath(string path)
	{
		return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
	}

	private sealed class CatalogEntry
	{
		public string Key { get; set; } = "";

		public string Title { get; set; } = "";

		public string ImagePath { get; set; } = "";

		public string ActionPath { get; set; } = "";

		public string Category { get; set; } = "动作";
	}
}
