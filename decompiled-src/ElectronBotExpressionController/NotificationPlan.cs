using System.Collections.Generic;

namespace ElectronBotExpressionController;

internal sealed record NotificationPlan(string Name, string FacePath, IReadOnlyList<ExpressionAction> Frames);
