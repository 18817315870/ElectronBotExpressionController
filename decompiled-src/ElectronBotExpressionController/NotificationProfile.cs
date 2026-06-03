using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectronBotExpressionController;

internal sealed class NotificationProfile
{
	public bool Enabled { get; set; } = true;

	public string Name { get; set; } = "";

	public List<string> Keywords { get; set; } = new List<string>();

	public string FacePath { get; set; } = "";

	public string ActionKey { get; set; } = "";

	public string ActionPath { get; set; } = "";

	public List<ExpressionAction> Frames { get; set; } = new List<ExpressionAction>();

	public bool Matches(string signal)
	{
		return Enabled && Keywords.Any(keyword => signal.Contains(keyword, StringComparison.OrdinalIgnoreCase));
	}

	public void Sanitize()
	{
		Name = Name?.Trim() ?? "";
		FacePath = FacePath?.Trim() ?? "";
		ActionKey = ActionKey?.Trim() ?? "";
		ActionPath = ActionPath?.Trim() ?? "";
		Keywords = Keywords?.Where(keyword => !string.IsNullOrWhiteSpace(keyword))
			.Select(keyword => keyword.Trim())
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList() ?? new List<string>();
		Frames = Frames?.Where(frame => frame != null)
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
}
