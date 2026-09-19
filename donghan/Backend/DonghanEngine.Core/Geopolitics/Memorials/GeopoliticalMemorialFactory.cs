using System;
using System.Collections.Generic;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Memorials;

/// <summary>
/// 纯领域地缘奏折生成工厂：根据地缘战役结果、战后讨封、围城求援与岁贡生成标准朝廷奏折（单一职责）
/// </summary>
public sealed class GeopoliticalMemorialFactory : IGeopoliticalMemorialFactory
{
    public GeopoliticalMemorial CreatePetitionMemorial(WarlordFaction victor, string provinceId, Province province)
    {
        if (victor == null) throw new ArgumentNullException(nameof(victor));
        string provName = province?.Name ?? provinceId;

        var options = new List<MemorialOption>
        {
            new(
                "opt_ratify",
                "【准奏受封】敕命加封，征收谢恩钱",
                $"承认其领有{provName}既成事实，加封为{provName}刺史，令其进奉谢恩助军钱2000万",
                new GeopoliticalImperialEdict(GeopoliticalEdictType.RatifyAndReward, victor.FactionId, string.Empty, provinceId, 2000, $"天子诏：加封{victor.FactionName}为{provName}刺史，着其进贡助军。")),
            new(
                "opt_denounce",
                "【明斥暗谋】严旨斥责，密诏强邻背刺",
                $"下旨斥责其擅动刀兵，并密下朱谕于邻近诸侯，许诺平定后赏其地盘，驱虎吞狼！",
                new GeopoliticalImperialEdict(GeopoliticalEdictType.DenounceAndProvoke, victor.FactionId, string.Empty, provinceId, 0, $"天子明诏严斥{victor.FactionName}擅杀同僚！密令天下共讨之！"))
        };

        return new GeopoliticalMemorial(
            Guid.NewGuid().ToString("N"),
            GeopoliticalMemorialType.PetitionRank,
            victor.FactionId,
            provinceId,
            $"【奏折 · 战后讨封】{victor.FactionName}克定{provName}，伏乞天子明旨加封！",
            $"臣闻{provName}盗贼滋蔓，地方不靖。臣不得已兴仁义之师荡平凶丑，今{provName}粗安，伏乞陛下顺应天心，恩赐符节，领{provName}刺史事！谨奉黄金千两以助军国！",
            SuggestedTributeGold: 2000,
            options);
    }

    public GeopoliticalMemorial CreateAppealMemorial(WarlordFaction besieged, WarlordFaction attacker, string provinceId, Province province)
    {
        if (besieged == null) throw new ArgumentNullException(nameof(besieged));
        string provName = province?.Name ?? provinceId;
        string attName = attacker?.FactionName ?? "强藩";

        var options = new List<MemorialOption>
        {
            new(
                "opt_mediate",
                "【持节调停】遣使敕令罢兵，各收贡赋",
                $"派光禄大夫持天子节仗调停，勒令{attName}解围退兵，双方各输贡金1000万",
                new GeopoliticalImperialEdict(GeopoliticalEdictType.MediateTruce, besieged.FactionId, attacker?.FactionId ?? string.Empty, provinceId, 1000, $"天子敕令：天下同僚不得私斗，{attName}即刻退兵，两家罢战息民！")),
            new(
                "opt_deploy_loyal",
                "【奉旨征调】命皇甫嵩等忠良军团驰援",
                $"发朝廷天子符节，调动邻近忠义藩屏出兵击其侧翼，解{provName}之围",
                new GeopoliticalImperialEdict(GeopoliticalEdictType.DeployLoyalTroops, besieged.FactionId, attacker?.FactionId ?? string.Empty, provinceId, 500, $"天子密诏：命忠义之师星夜兼程驰援{provName}，夹击叛逆！"))
        };

        return new GeopoliticalMemorial(
            Guid.NewGuid().ToString("N"),
            GeopoliticalMemorialType.AppealForHelp,
            besieged.FactionId,
            provinceId,
            $"【绝笔求援】{besieged.FactionName}困守{provName}孤城，泣血乞朝廷发兵解救！",
            $"逆臣{attName}无故兴兵犯境，攻陷城邑，屠戮生灵！臣孤军困守{provName}残垣，粮尽援绝，死在旦夕！叩请陛下隆恩，遣使退敌或调天兵来援，臣粉身碎骨难报天恩！",
            SuggestedTributeGold: 0,
            options);
    }

    public GeopoliticalMemorial CreateTributeMemorial(WarlordFaction loyalist, int tributeAmount)
    {
        if (loyalist == null) throw new ArgumentNullException(nameof(loyalist));

        var options = new List<MemorialOption>
        {
            new(
                "opt_accept_tribute",
                "【欣然嘉纳】纳金入库，颁赐御酒锦袍",
                $"准奏纳贡，国库入账{tributeAmount}万钱，赏赐天子玺书抚慰，忠诚大幅提升",
                new GeopoliticalImperialEdict(GeopoliticalEdictType.AcceptTribute, loyalist.FactionId, string.Empty, string.Empty, tributeAmount, $"天子嘉勉：{loyalist.FactionName}忠贞克笃，输贡济国，特赐金印紫绶！"))
        };

        return new GeopoliticalMemorial(
            Guid.NewGuid().ToString("N"),
            GeopoliticalMemorialType.TributeOffered,
            loyalist.FactionId,
            string.Empty,
            $"【岁贡呈进】{loyalist.FactionName}进奉岁赋黄金{tributeAmount}万钱以充度支！",
            $"臣闻国家度支维艰，西园修防需费。臣谨遵藩臣之职，搜括封内盐铁羡余，谨呈黄金{tributeAmount}万钱，伏乞圣鉴，愿吾皇万岁万万岁！",
            SuggestedTributeGold: tributeAmount,
            options);
    }
}
