using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElectronBotExpressionController;

internal sealed class ExpressionAction
{
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public float J1 { get; set; }

	public float J2 { get; set; }

	public float J3 { get; set; }

	public float J4 { get; set; }

	public float J5 { get; set; }

	public float J6 { get; set; }

	public static List<ExpressionAction> Load(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return JsonSerializer.Deserialize<List<ExpressionAction>>(stream, JsonOptions) ?? new List<ExpressionAction>();
	}

	public static async Task<List<ExpressionAction>> LoadAsync(string path)
	{
		List<ExpressionAction> result;
		await using (FileStream stream = File.OpenRead(path))
		{
			result = (await JsonSerializer.DeserializeAsync<List<ExpressionAction>>(stream, JsonOptions)) ?? new List<ExpressionAction>();
		}
		return result;
	}
}
