using System;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Economy;

public interface ILandOwnershipService
{
    // 1. 战乱平定与焦土结算 (平定黄巾或军阀兼并胜利)
    PostWarLandResolutionResult ResolveWarLandScorching(GameState state, string provinceId, string victorFactionId, bool isImperialDirectArmy);

    // 2. 焦土月度自然恢复时序演进 (半年/6个月不产粮，期满后焦土转为国家有效官田)
    void AdvanceScorchedLandMonthly(GameState state);

    // 3. 世家出资向朝廷赎买无主官田
    LandRepurchaseResult RepurchaseStateLandByGentry(GameState state, string provinceId, int purchaseAmount);

    // 4. 计算并征收各州季度赋税 (国家控制土地全额纳税，世家私田少收40%)
    int CollectQuarterlyLandTax(GameState state, bool applyToTreasury = true);
}
