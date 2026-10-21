using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 决策前瞻定性提示词库服务接口 (支持通过 Init 注入热调，面向接口解耦)
/// </summary>
public interface IDecisionPromptLibraryService
{
    /// <summary>
    /// 判定 NPC 的政治性格原型
    /// </summary>
    NpcArchetype ClassifyNpcArchetype(NpcState npc);

    /// <summary>
    /// 根据操作类型与目标 NPC 属性匹配生成定性后果
    /// </summary>
    DecisionConsequenceTemplate MatchConsequences(
        DecisionActionType actionType, 
        NpcState? targetNpc = null, 
        string? targetLocation = null,
        string? customActionLabel = null);
}

/// <summary>
/// 提示词库默认领域实现类
/// </summary>
public sealed class DecisionPromptLibraryService : IDecisionPromptLibraryService
{
    public NpcArchetype ClassifyNpcArchetype(NpcState npc)
    {
        if (npc == null) return NpcArchetype.CivilBureaucrat;

        if (npc.Faction == FactionCatalog.EunuchFaction)
            return NpcArchetype.CourtEunuch;

        if (npc.Faction == FactionCatalog.ImperialClan)
            return NpcArchetype.ImperialConsortLord;

        if (npc.Faction == FactionCatalog.PureStream || npc.Traits.Contains(TraitNames.QingZhengLianJie))
            return NpcArchetype.PureStreamScholar;

        if (npc.Faction == FactionCatalog.WesternGarden)
            return NpcArchetype.WesternGardenGeneral;

        if (npc.Ambition >= 65 || npc.Faction == FactionCatalog.Warlord || npc.Traits.Contains(TraitNames.YongBingZiZhong))
            return NpcArchetype.AmbitiousWarlord;

        return NpcArchetype.CivilBureaucrat;
    }

    public DecisionConsequenceTemplate MatchConsequences(
        DecisionActionType actionType,
        NpcState? targetNpc = null,
        string? targetLocation = null,
        string? customActionLabel = null)
    {
        string npcName = targetNpc?.Name ?? "朝臣";
        string locationName = targetLocation ?? "地方州郡";
        var archetype = targetNpc != null ? ClassifyNpcArchetype(targetNpc) : NpcArchetype.CivilBureaucrat;

        var bullets = new List<string>();
        string title = customActionLabel ?? actionType.ToString();
        string description = $"天子下旨裁夺：【{title}】。";

        switch (actionType)
        {
            case DecisionActionType.AppointGovernor:
                title = $"出牧授印 · {npcName}";
                description = $"敕拜【{npcName}】为【{locationName}】太守，持节出京治理一方政务。";
                bullets.Add($"预计安定【{locationName}】民心，地方治理成效取决于其政术才能");
                bullets.Add($"【{npcName}】离京赴任，其在京畿朝堂的直接权势将暂时削减");
                if (archetype == NpcArchetype.AmbitiousWarlord || (targetNpc != null && targetNpc.Ambition >= 60))
                {
                    bullets.Add($"⚠️ 警示：【{npcName}】野心炽盛，若地方民心过低或天下大乱，恐有据地割据自雄之风险！");
                }
                else if (archetype == NpcArchetype.PureStreamScholar)
                {
                    bullets.Add($"【{npcName}】秉持清流风骨，赴任后预计深得当地士民拥戴，不易滋生反心");
                }
                else
                {
                    bullets.Add("该臣性情相对恭谨安分，暂无自立割据之忧");
                }
                break;

            case DecisionActionType.RecallGovernor:
                title = $"召还太守 · {npcName}";
                description = $"敕令【{npcName}】卸任【{locationName}】太守一职，解印回京述职。";
                bullets.Add($"【{locationName}】将暂时处于无主官镇抚状态，地方民心预计每月缓慢流失");
                bullets.Add($"【{npcName}】重归京畿中枢，其在朝堂上的派系权势将有所回升");
                bullets.Add("可空出太守职位，以便重新选派能臣清剿盗贼、整饬度田");
                break;

            case DecisionActionType.ConfiscateToTreasury:
            case DecisionActionType.ConfiscateToPrivate:
                bool toTreasury = actionType == DecisionActionType.ConfiscateToTreasury;
                title = $"抄没私财 · {npcName}";
                description = $"降旨籍没【{npcName}】名下田宅、私蓄与家资，明正典刑。";
                bullets.Add(toTreasury 
                    ? "抄家所得家私将充实大司农国库，用于平叛赈灾、补足军饷" 
                    : "抄家所得家私将悉数密入西园私库，归天子内帑调度");

                switch (archetype)
                {
                    case NpcArchetype.PureStreamScholar:
                        bullets.Add("⚠️ 警示：目标在清流名士与天下太学生中名望极高，籍没恐引发朝臣死谏、清流反感与民怨沸腾！");
                        break;
                    case NpcArchetype.ImperialConsortLord:
                        bullets.Add("⚠️ 警示：目标为外戚重臣骨干，籍没恐激化外戚大将军派系矛盾，导致内廷与外朝关系骤紧！");
                        break;
                    case NpcArchetype.CourtEunuch:
                        bullets.Add("预计受到外廷清流公卿一致赞扬称颂，但内廷宦官近侍人人自危、心怀猜忌");
                        break;
                    default:
                        bullets.Add("籍没将触动朝堂世家豪强之利益底线，恐招致朝臣暗中抵制与非议");
                        break;
                }
                bullets.Add("被抄没官员将被革职下狱，其名下党羽势力将受重创");
                break;

            case DecisionActionType.MilitaryReorganization:
                title = $"准其整军 · {npcName}";
                description = $"命【{npcName}】整饬京畿北军防务，拨付兵饷并下放临机调兵之权。";
                bullets.Add($"预计提升【{npcName}】忠诚与所属外戚集团之朝局威望");
                bullets.Add("京畿北军防务与备战态势增强，但会引发内廷中官强烈猜忌与制衡");
                bullets.Add("预计自太仓国库支拨军费粮饷");
                break;

            case DecisionActionType.ArmyDrillWithOfficer:
                title = $"命其整军 · {npcName}";
                description = $"命【{npcName}】统领西园禁卫新军，点验将士并重整武备。";
                bullets.Add($"预计提升【{npcName}】忠诚与立功建勋之志，西园军锐气大振");
                bullets.Add($"【{npcName}】在军中威望渐长，外戚大将军派系或心存防备");
                break;

            case DecisionActionType.ReprimandOfficer:
                title = $"当朝申饬 · {npcName}";
                description = $"当朝严辞申饬【{npcName}】骄横弄权，稍抑嚣张气焰。";
                bullets.Add("预计抚慰外廷清流士大夫之心，天下风评略有好转");
                bullets.Add($"【{npcName}】及所属派系忠诚恐受挫折，心生怨怼与防范");
                break;

            case DecisionActionType.AppeaseOfficer:
                title = $"降旨安抚 · {npcName}";
                description = $"降旨加赏【{npcName}】，以天子圣眷平息内外攻讦。";
                bullets.Add($"预计大幅稳固【{npcName}】忠诚与派系爪牙之向心力");
                bullets.Add("预计引起清流名士与外朝公卿强烈反感，指责天子偏袒姑息");
                break;

            case DecisionActionType.RaiseTribute:
                title = $"令其筹措 · {npcName}";
                description = $"敕令【{npcName}】清核三军粮饷并多方筹措内帑资财。";
                bullets.Add("预计依靠其专有手段快速筹得钱粮以备急用");
                bullets.Add("预计引起清流士族反感与弹劾，指责其侵夺大司农与少府职权");
                break;

            case DecisionActionType.DisasterRelief:
                title = "开仓赈济灾民";
                description = "开常平仓与太仓粮秣，拨发救济银两赈济受灾饥民。";
                bullets.Add("预计动支太仓国库银两与常平仓粮秣，平抑地方饥馑");
                bullets.Add("预计安抚灾区饥民、大幅提振天下民心，遏制民变滋生");
                bullets.Add("若经办官吏贪腐，部分赈粮恐被沿途豪强剥削克扣");
                break;

            case DecisionActionType.ImpeachAndPunish:
                title = "三法司廷审定罪";
                description = "由御史中丞与司隶校尉联名上奏，将涉案渎职官员锁拿归案。";
                bullets.Add("预计彰显天子法度威严，申饬官场奢靡渎职之风");
                bullets.Add("涉案官员及其党羽派系将心生怨恨，朝堂风声鹤唳");
                bullets.Add("预计受到清流名士赞赏，士林公论略见好转");
                break;

            default:
                bullets.Add("直接影响朝堂派系平衡与地方军政态势");
                bullets.Add("请陛下明辨利害，乾纲独断");
                break;
        }

        return new DecisionConsequenceTemplate(title, description, bullets);
    }
}
