using System.Collections.Generic;

namespace ElectronBotExpressionController;

internal static class NotificationMotionFrames
{
	public static IReadOnlyList<ExpressionAction> NodAndWaveLeft()
	{
		return new ExpressionAction[9]
		{
			Frame(0f, 0f, 0f, 0f, 0f, 0f),
			Frame(8f, 16f, 28f, 0f, 0f, -18f),
			Frame(14f, 30f, 64f, 0f, 0f, 18f),
			Frame(6f, 14f, 105f, 0f, 0f, -26f),
			Frame(13f, 28f, 58f, 0f, 0f, 26f),
			Frame(5f, 12f, 112f, 0f, 0f, -18f),
			Frame(10f, 24f, 54f, 0f, 0f, 18f),
			Frame(3f, 8f, 28f, 0f, 0f, -8f),
			Frame(0f, 0f, 0f, 0f, 0f, 0f)
		};
	}

	private static ExpressionAction Frame(float j1, float j2, float j3, float j4, float j5, float j6)
	{
		return new ExpressionAction
		{
			J1 = j1,
			J2 = j2,
			J3 = j3,
			J4 = j4,
			J5 = j5,
			J6 = j6
		};
	}
}
