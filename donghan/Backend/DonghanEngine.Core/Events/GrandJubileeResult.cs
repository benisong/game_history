namespace DonghanEngine.Core.Events;

public enum GrandJubileeOutcome
{
    GreatRestorationGrandJubilee,  // 大汉中兴·七旬大圆满：皇权>=60，国库>=50000，西园禁军>=10000。曹丕、诸葛亮、孙权齐聚洛阳九龙殿朝觐九宾之礼，天下大一统尊奉汉室正朔，皇权达成100巅峰，起居注终极铭刻
    ProsperousAutonomousTributes,  // 诸侯奉朔·盛世同庆：皇权>=45，朝廷保持天下共主超然地位，天下诸侯各贡重赋，国库+10000，皇权+15
    FactionalFissuresAtJubilee     // 藩镇异心·寿辰难圆：皇权弱势，诸侯仅遣微使应付，朝廷威严有限
}

public sealed record GrandJubileeResult(
    GrandJubileeOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoPiLoyaltyDelta,
    int ZhugeLiangLoyaltyDelta,
    int SunQuanLoyaltyDelta,
    int WestGardenMoraleBonus,
    bool GrandRestorationAchieved);
