namespace DonghanEngine.Core;

/// <summary>
/// 所有 NPC 特质的字符串常量。
/// 在整个代码库中统一使用这些常量，避免硬编码字符串导致的拼写错误。
/// </summary>
public static class TraitNames
{
    // === 赈灾提振（正面） ===
    public const string JingTianWeiDi = "经天纬地";       // 1.20x
    public const string ShanChangMinZheng = "擅长民政";    // 1.08x
    public const string AiMinRuZi = "爱民如子";           // 1.15x
    public const string QinMinWenHe = "亲民温和";         // 1.05x

    // === 赈灾提振（负面） ===
    public const string HaoSheWuDu = "豪奢无度";          // 0.75x
    public const string PuZhangLangFei = "铺张浪费";      // 0.90x
    public const string BuXueWuShu = "不学无术";          // 0.80x
    public const string CaiShuXueQian = "才疏学浅";       // 0.90x

    // === 阅兵士气 ===
    public const string KongWuYouLi = "孔武有力";         // 1.30x
    public const string YouXieLiQi = "有些力气";          // 1.10x
    public const string ZhiJunYanZheng = "治军严整";      // 1.25x
    public const string DongDianBingFa = "懂点兵法";       // 1.10x

    // === 阅兵忠诚 ===
    public const string AiBingRuZi = "爱兵如子";           // 1.20x
    public const string TiXuShiZu = "体恤士卒";           // 1.08x

    // === 贪污漂没 ===
    public const string QingZhengLianJie = "清正廉洁";     // 0%
    public const string BuNaGongKuan = "不拿公款";         // 50%
    public const string TanDeWuYan = "贪得无厌";           // 150%
    public const string YouXieShouZang = "有些手脏";       // 120%

    // === 抄家钦差反噬折减 ===
    public const string GangZhiBuE = "刚直不阿";           // bypass
    public const string LaoMouShenSuan = "老谋深算";       // 0.70x
    public const string YouXieXinJi = "有些心计";          // 0.90x
    public const string ShuoHuaZhiLv = "说话直率";          // 0.60x

    // === 抄家目标反噬加重 ===
    public const string YongBingZiZhong = "拥兵自重";      // +5
    public const string ShouXiaYouBing = "手下有兵";        // +2
    public const string MenFaShiJia = "门阀世家";           // +8
    public const string ChuShenMingMen = "出身名门";        // +3

    // === 后宫随驾 ===
    public const string ChanMeiZhuanQuan = "谄媚专权";     // 健康+5, 好感+15, 权势+5
    public const string HuiPaiMaPi = "会拍马屁";           // 健康+2, 好感+6, 权势+2
    public const string YiShuGaoMing = "医术高明";         // 健康+8
    public const string DongDianYiLi = "懂点医理";         // 健康+3
    public const string XiHaoQingTan = "喜好清谈";         // 健康+2, 皇权-1
}
