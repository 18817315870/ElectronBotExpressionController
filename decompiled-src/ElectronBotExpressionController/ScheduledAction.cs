using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElectronBotExpressionController;

internal sealed class ScheduledAction
{
	public string Time { get; set; } = "";

	public string Name { get; set; } = "";

	public string ActionKey { get; set; } = "";

	public string FacePath { get; set; } = "";

	public string ActionPath { get; set; } = "";

	public bool Enabled { get; set; } = true;

	public void Sanitize()
	{
		Time = Time?.Trim() ?? "";
		Name = Name?.Trim() ?? "";
		ActionKey = ActionKey?.Trim() ?? "";
		FacePath = FacePath?.Trim() ?? "";
		ActionPath = ActionPath?.Trim() ?? "";
	}

	public bool IsDue(DateTime now)
	{
		if (!Enabled || string.IsNullOrWhiteSpace(Time))
		{
			return false;
		}
		if (!TimeSpan.TryParse(Time, out TimeSpan scheduledTime))
		{
			return false;
		}
		return now.Hour == scheduledTime.Hours && now.Minute == scheduledTime.Minutes;
	}

	public string HitKey(DateTime now)
	{
		return $"{now:yyyy-MM-dd}|{Time}|{Name}|{ActionKey}|{FacePath}|{ActionPath}";
	}

	public ExpressionItem? ResolveExpression(IReadOnlyList<ExpressionItem> catalog)
	{
		ExpressionItem? catalogItem = null;
		if (!string.IsNullOrWhiteSpace(ActionKey))
		{
			catalogItem = catalog.FirstOrDefault((ExpressionItem item) => item.Key.Equals(ActionKey, StringComparison.OrdinalIgnoreCase));
		}
		string imagePath = ResolveExistingPath(FacePath, catalogItem?.ImagePath ?? "");
		string actionPath = ResolveExistingPath(ActionPath, catalogItem?.ActionPath ?? "");
		if (string.IsNullOrWhiteSpace(imagePath) || string.IsNullOrWhiteSpace(actionPath))
		{
			return catalogItem;
		}
		string title = string.IsNullOrWhiteSpace(Name) ? (catalogItem?.Title ?? Time) : Name;
		string key = string.IsNullOrWhiteSpace(ActionKey) ? ("schedule-" + Time.Replace(":", "")) : ActionKey;
		return new ExpressionItem(key, title, imagePath, actionPath);
	}

	private static string ResolveExistingPath(string path, string fallbackPath)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return fallbackPath;
		}
		string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
		return File.Exists(fullPath) ? fullPath : fallbackPath;
	}
}
