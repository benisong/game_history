using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 玩家对 NPC 或朝政执行的核心操作类别
/// </summary>
public enum DecisionActionType
{
    AppointGovernor,      // 外任太守 / 出牧一方
    RecallGovernor,       // 召还回京
    ConfiscateToTreasury, // 廷审抄没 · 充入国库
    ConfiscateToPrivate,  // 密诏查抄 · 充入私库
    ArmyDrillWithOfficer, // 命其统兵 / 犒赏三军
    MilitaryReorganization,// 准其整军备寇 (如准何进整北军)
    AuditFinances,        // 令其清核三军军费
    AppeaseOfficer,       // 圣眷安抚 / 嘉奖重赏
    ReprimandOfficer,     // 当朝申饬 / 训诫敲打
    RaiseTribute,         // 命其筹措内帑资财
    DisasterRelief,       // 开仓赈济灾民
    ImpeachAndPunish      // 弹劾定罪 / 罢黜惩办
}

/// <summary>
/// NPC 政治性格/阶级原型分类 (用于定性后果匹配)
/// </summary>
public enum NpcArchetype
{
    PureStreamScholar,   // 清流名士 / 太学领袖 (如 蔡邕、卢植、荀爽、皇甫嵩)
    ImperialConsortLord, // 外戚宗勋 / 禁军巨头 (如 何进)
    CourtEunuch,         // 阉党近侍 / 十常侍 (如 张让、赵忠)
    WesternGardenGeneral,// 西园新贵 / 天子心腹 (如 蹇硕、曹操早期)
    AmbitiousWarlord,    // 异志枭雄 / 割据军阀 (如 董卓、袁绍、袁术)
    CivilBureaucrat      // 循吏文官 / 朝堂干吏 (普通官僚)
}

/// <summary>
/// 前瞻提示条目
/// </summary>
public sealed record DecisionConsequenceTemplate(
    string ActionTitle,
    string NarrativeDescription,
    IReadOnlyList<string> ConsequenceBullets
);
