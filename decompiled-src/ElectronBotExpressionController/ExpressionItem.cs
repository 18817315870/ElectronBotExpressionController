namespace ElectronBotExpressionController;

internal sealed record ExpressionItem(string Key, string Title, string ImagePath, string ActionPath)
{
	public string Category { get; init; } = "动作";
}
