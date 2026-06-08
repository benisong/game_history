namespace DonghanEngine.Core;

public partial class GameEngine
{
    private static string BuildInsufficientDrillFundsStory()
    {
        return "【发饷失败】\n\n天子私库空虚，不足以支撑此次开支！群情哗然！\n\n[color=red]● 士气: -5[/color]";
    }

    private static string BuildWestGardenArmyFullStory(int maxSize)
    {
        return $"【募兵暂止】\n\n西园校尉跪奏：西园新军已满 {maxSize} 人，营垒粮械俱已逼仄。若再强征丁壮，只会扰乱京畿民生。\n\n[color=yellow]● 西园军：已达上限[/color]";
    }

    private static string BuildInsufficientRecruitFundsStory(int troops, int cost)
    {
        return $"【募兵失败】\n\n大司农叩首急奏：欲新募 {troops} 名丁壮，需国库 {cost} 万钱置办甲械粮饷。如今国帑不足，诏书虽下，州县不敢奉行。\n\n[color=red]● 国库不足：需要 {cost} 万钱[/color]";
    }

    private static string BuildRaiseWestGardenTroopsChronicle(int troops, int cost, int supportDelta)
    {
        return $"【西园募兵】天子下诏扩充西园新军，征发丁壮 {troops} 人，国库支出 {cost} 万钱，天下民心 {supportDelta}。";
    }

    private static string BuildRaiseWestGardenTroopsStory(int troops, int cost, int supportDelta, int newArmySize, int maxSize)
    {
        return $"【西园募兵 · 补充新军】\n\n天子密诏州郡，选募骁勇丁壮入洛阳西园。校场之上，旌旗重列，甲械新发，西园新军重新有了可供征战的兵额。\n\n然而募兵并非无本之举：置办兵甲、转运粮草、安置军户，皆由国库支应；州县抽丁也令百姓怨声渐起。\n\n[color=green]● 西园军人数：+{troops}（当前 {newArmySize}/{maxSize}）[/color]\n[color=red]● 朝廷国库：-{cost} 万钱[/color]\n[color=red]● 天下民心：{supportDelta}[/color]\n[color=yellow]● 西园士气：-{troops / 1000}（新卒入营，军阵磨合）[/color]";
    }

    private static string BuildDrillArmyChronicle(DrillArmySettlement settlement)
    {
        return $"【西园】天子命【{settlement.Officer.Name}】犒赏禁军，拨发 {settlement.PaidAmount} 万钱。被侵吞漂没 {settlement.SiphonedAmount} 万钱！实际到手 {settlement.ActualReceivedAmount} 万钱。军心士气变化: {settlement.FinalMoraleDelta}。";
    }

    private static string BuildDrillArmyStory(DrillArmySettlement settlement)
    {
        string corruptionNarrative = settlement.SiphonedAmount > 0
            ? $"[color=red]经办官【{settlement.Officer.Name}】从中层层中饱私囊，天子虽拨下 {settlement.PaidAmount} 万钱，但将士最终仅分得 {settlement.ActualReceivedAmount} 万钱（其中被贪污漂没 {settlement.SiphonedAmount} 万钱，滚入其私蓄中）！[/color]\n\n"
            : "";

        string narrativeFeedback = settlement.Outcome switch
        {
            DrillArmyOutcome.Underpaid =>
                $"将士们看到手里的那几枚铜钱，个个咬牙切齿。西园新军愤怒地瞪着在台上督办的【{settlement.Officer.Name}】，低沉的骂声在军营中蔓延：\"昏君奸臣！拨了这么多钱，到我们手里就剩这点！\"",
            DrillArmyOutcome.BarelyFunded =>
                "士兵们领到本月军饷，虽足额，但对官吏层层抽水的贪婪行径极度不满。军营中弥漫着压抑的冷漠，将士们领赏后只是木然行礼。",
            DrillArmyOutcome.RewardedTrusted =>
                $"陛下朱批重赏！即使【{settlement.Officer.Name}】漂没了部分赏钱，但剩下的钱依然让士卒们喜笑颜开。校场上战鼓齐鸣，三军山呼万岁，忠于天子的军心大振！",
            _ =>
                "虽然天子重赏，但将士们对官吏吃拿卡要的作风深恶痛绝。将士们领过钱财，脸上并无感激，纷纷在私下讥笑朝廷的无能与贪墨。"
        };

        return $"【西园大阅 · 发放犒赏】\n\n天子拨出：{settlement.PaidAmount} 万钱\n{corruptionNarrative}{narrativeFeedback}\n\n[color=yellow]● 士气变化：{(settlement.FinalMoraleDelta >= 0 ? "+" : "")}{settlement.FinalMoraleDelta}[/color]\n[color=yellow]● 忠诚变化：{(settlement.BaseLoyaltyDelta >= 0 ? "+" : "")}{settlement.BaseLoyaltyDelta}[/color]\n[color=green]● 皇权变化：{(settlement.ImperialPowerDelta >= 0 ? "+" : "")}{settlement.ImperialPowerDelta}[/color]\n[color=red]● 【{settlement.Officer.Name}】搜刮中饱：+{settlement.SiphonedAmount} 万钱 (滚入赃款私蓄，朝堂权势 +2)[/color]";
    }

    private static string BuildInsufficientReliefFundsStory()
    {
        return "【赈灾中止】\n\n大司农跪奏：国库空空如也，连一万钱也拿不出来！赈灾下旨流产，天下大哗，灾民流离失所！\n\n[color=red]● 天下民心: -10[/color]";
    }

    private static string BuildDisasterReliefChronicle(DisasterReliefSettlement settlement)
    {
        return $"【朝会】天子命【{settlement.Officer.Name}】开仓赈灾，拨发国库 {settlement.ReliefAmount} 万钱。被侵吞扣下 {settlement.SiphonedAmount} 万钱！灾民实际得 {settlement.ActualReliefReceived} 万钱。天下民心变化: {settlement.FinalSupportDelta}。";
    }

    private static string BuildDisasterReliefStory(DisasterReliefSettlement settlement)
    {
        string corruptionNarrative = settlement.SiphonedAmount > 0
            ? $"[color=red]钦差大臣【{settlement.Officer.Name}】领御旨赈灾，从中雁过拔毛，侵吞扣下了 {settlement.SiphonedAmount} 万钱并存入个人赃款私蓄！实际运抵灾区的粮草仅折合 {settlement.ActualReliefReceived} 万钱。[/color]\n\n"
            : "";

        string narrativeFeedback = settlement.Outcome == DisasterReliefOutcome.Starved
            ? $"【{settlement.Officer.Name}】贪婪成性，层层盘剥，运往灾区的米粮多是掺了沙子的陈米米糠。灾民食之腹胀而死，白骨露于野，千里无鸡鸣。天下百姓指天痛骂昏君奸臣。"
            : $"【{settlement.Officer.Name}】总算将一车车饱满的谷物运抵灾区，设粥厂、施医药。数万嗷嗷待哺的灾民得保一命。百姓无不面向京洛叩首高呼万岁，盛赞当今天子圣明。";

        return $"【大朝会 · 开仓赈灾】\n\n朝廷拨出：{settlement.ReliefAmount} 万钱\n{corruptionNarrative}{narrativeFeedback}\n\n[color=yellow]● 民心指数：{(settlement.FinalSupportDelta >= 0 ? "+" : "")}{settlement.FinalSupportDelta}[/color]\n[color=green]● 朝廷皇权：{(settlement.ImperialPowerDelta >= 0 ? "+" : "")}{settlement.ImperialPowerDelta}[/color]\n[color=red]● 【{settlement.Officer.Name}】中饱私囊：+{settlement.SiphonedAmount} 万钱 (领旨钦差，朝堂权势 +5)[/color]";
    }

    private static string BuildFailedConfiscationStory(NpcState target)
    {
        return $"【抄家受阻】\n\n天子正欲罗织罪名籍没[b]【{target.Name}】[/b]，然而环顾宣政殿，台下群臣面面相觑，竟无一人出列附和弹劾！\n\n[color=red]大将军和十常侍党羽冷笑连连，朝堂空气冰冷。天子深感孤立无援，御口一塞，只得作罢。[/color]\n\n[color=red]● 朝廷皇权：-5 (威信受损，政令不出宫门)[/color]";
    }

    private static string BuildConfiscationChronicle(ConfiscationSettlement settlement)
    {
        if (settlement.Outcome == ConfiscationOutcome.CliqueBacklash)
        {
            return $"【政潮】近臣【{settlement.Framer.Name}】诬陷弹劾【{settlement.Target.Name}】。虽遭其党羽抵制，最终成功抄其家。民心变动: {settlement.NetSupportDelta}。";
        }

        return $"【抄家】近臣【{settlement.Framer.Name}】罗织罪名诬陷【{settlement.Target.Name}】并籍没家产。天下民心大振 +{settlement.NetSupportDelta}。";
    }

    private static string BuildConfiscationStory(ConfiscationSettlement settlement)
    {
        string corruptionLossText = settlement.FramerEmbezzled > 0
            ? $"[color=red]● 经办贪腐：抄家钦差【{settlement.Framer.Name}】上下抽水，中饱私囊扣下了 {settlement.FramerEmbezzled} 万钱！[/color]\n"
            : "";

        string splitLootText = $"[color=green]● 籍没分配：朝廷国库进账 +{settlement.AmountToTreasury} 万钱 (占70%)，天子西园私库进账 +{settlement.AmountToPrivate} 万钱 (占30%)[/color]";

        if (settlement.Outcome == ConfiscationOutcome.CliqueBacklash)
        {
            return $"【🚨 朝堂风暴 · 强行抄家】\n\n宣政殿上，近臣[b]【{settlement.Framer.Name}】[/b]突然出列弹劾[b]【{settlement.Target.Name}】[/b]暗通外藩、意图谋反。\n\n由于其权势滔天，十常侍和大将军党羽人人自危，在宣政殿上发起疯狂弹劾抵制：\"陛下诬陷开国元勋，老臣不服！\"天子强令御林军缉拿，查封府邸。\n\n{corruptionLossText}{splitLootText}\n\n[color=red]● 朝堂反噬：政令招致官吏阶层大恐慌，朝廷皇权暴跌 -{settlement.FinalPowerLoss}[/color]\n[color=yellow]● 百姓拍手：铲除朝廷巨蠹，天下民心净变动 {(settlement.NetSupportDelta >= 0 ? "+" : "")}{settlement.NetSupportDelta}[/color]\n[color=green]● 【{settlement.Framer.Name}】执掌抄家：中饱漂没 +{settlement.FramerEmbezzled} 万钱，其朝堂权势稳步上升 +3[/color]";
        }

        return $"【🏛️ 帝王心术 · 诬陷籍没】\n\n大朝会上，近臣[b]【{settlement.Framer.Name}】[/b]心领神会，高声揭发[b]【{settlement.Target.Name}】[/b]结党营私、收受贿赂。陛下朱批定罪，御林军重甲封锁其府邸查抄！\n\n由于对方毫无党羽，满朝群臣噤若寒蝉。查抄所得大快人心！\n\n{corruptionLossText}{splitLootText}\n\n[color=green]● 皇权震慑：巧妙施展手腕清除异己，皇权提升 +8[/color]\n[color=green]● 惩奸除恶：惩治巨贪，天下百姓无不额手称庆，天下民心大幅攀升 +{settlement.NetSupportDelta}[/color]\n[color=green]● 钦差办案：经办官【{settlement.Framer.Name}】中饱漂没 +{settlement.FramerEmbezzled} 万钱，朝堂权势稳步上升 +3[/color]";
    }
}
