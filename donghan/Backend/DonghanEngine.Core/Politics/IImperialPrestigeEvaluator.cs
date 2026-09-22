using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public interface IImperialPrestigeEvaluator
{
    PrestigeEvaluationResult Evaluate(int currentPrestige);
    
    // 评估朝野官员在高威望下的转嫁压迫反应
    IReadOnlyList<string> EvaluateOppressionTransferOfficials(GameState state, PrestigeEvaluationResult prestigeResult);
}
