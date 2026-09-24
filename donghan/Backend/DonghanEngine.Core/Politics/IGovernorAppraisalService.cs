using System.Collections.Generic;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

public interface IGovernorAppraisalService
{
    // 1. 年末尚书台考课各州刺史太守
    AnnualAppraisalReport EvaluateAnnualAppraisal(GameState state);

    // 2. 朝廷征召绩效优异的刺史/太守内调入朝拜九卿/三公 (受野心与朝廷威望制约)
    GovernorPromotionResolutionResult PromoteGovernorToCourt(GameState state, string governorId, string targetCourtTitle);
}
