using System.Collections.Generic;

namespace DonghanEngine.Core;

/// <summary>
/// 东汉十三州全局地理拓扑与基础数据定义（纯领域不可变目录）
/// </summary>
public static class ProvinceCatalog
{
    public const string Sili = "sili";         // 司隶校尉部（京畿）
    public const string Jizhou = "jizhou";     // 冀州
    public const string Bingzhou = "bingzhou"; // 并州
    public const string Yanzhou = "yanzhou";   // 兖州
    public const string Yuzhou = "yuzhou";     // 豫州
    public const string Jingzhou = "jingzhou"; // 荆州
    public const string Qingzhou = "qingzhou"; // 青州
    public const string Xuzhou = "xuzhou";     // 徐州
    public const string Yangzhou = "yangzhou"; // 扬州
    public const string Youzhou = "youzhou";   // 幽州
    public const string Liangzhou = "liangzhou";// 凉州
    public const string Yizhou = "yizhou";     // 益州
    public const string Jiaozhou = "jiaozhou"; // 交州

    public static readonly IReadOnlyList<string> AllThirteenProvinceIds = new List<string>
    {
        Sili, Jizhou, Bingzhou, Yanzhou, Yuzhou, Jingzhou,
        Qingzhou, Xuzhou, Yangzhou, Youzhou, Liangzhou, Yizhou, Jiaozhou
    };

    /// <summary>
    /// 获取全国十三州标准开局定义
    /// </summary>
    public static IReadOnlyList<Province> CreateInitialProvinces()
    {
        return new List<Province>
        {
            // 核心 6 郡（经典原案开局数值保持兼容）
            new() { Id = Sili, Name = "司隶", Distance = 0, LocalSupport = 50, Garrison = 5000, Wealth = 5000, DefenseLevel = 80,
                Neighbors = new List<string> { Jizhou, Yanzhou, Yuzhou, Bingzhou, Liangzhou } },

            new() { Id = Jizhou, Name = "冀州", Distance = 3, LocalSupport = 28, Garrison = 2000, Wealth = 3000, DefenseLevel = 30,
                Neighbors = new List<string> { Sili, Yanzhou, Bingzhou, Youzhou, Qingzhou } }, // +10 from 18 → 28 (桥玄任太守加成)

            new() { Id = Bingzhou, Name = "并州", Distance = 4, LocalSupport = 40, Garrison = 3000, Wealth = 2500, DefenseLevel = 40,
                Neighbors = new List<string> { Sili, Jizhou, Liangzhou, Youzhou } }, // +10 from 30 → 40 (黄甫嵩任太守加成)

            new() { Id = Yanzhou, Name = "兖州", Distance = 2, LocalSupport = 35, Garrison = 2500, Wealth = 3500, DefenseLevel = 35,
                Neighbors = new List<string> { Sili, Jizhou, Yuzhou, Qingzhou, Xuzhou } },

            new() { Id = Yuzhou, Name = "豫州", Distance = 1, LocalSupport = 55, Garrison = 2000, Wealth = 4000, DefenseLevel = 40,
                Neighbors = new List<string> { Sili, Yanzhou, Jingzhou, Xuzhou, Yangzhou } }, // +10 from 45 → 55 (卢植任太守加成)

            new() { Id = Jingzhou, Name = "荆州", Distance = 5, LocalSupport = 50, Garrison = 3000, Wealth = 6000, DefenseLevel = 50,
                Neighbors = new List<string> { Yuzhou, Yangzhou, Yizhou, Jiaozhou } },

            // 扩展 7 州
            new() { Id = Qingzhou, Name = "青州", Distance = 4, LocalSupport = 32, Garrison = 2000, Wealth = 2800, DefenseLevel = 35,
                Neighbors = new List<string> { Jizhou, Yanzhou, Xuzhou } },

            new() { Id = Xuzhou, Name = "徐州", Distance = 3, LocalSupport = 48, Garrison = 2200, Wealth = 4500, DefenseLevel = 45,
                Neighbors = new List<string> { Yanzhou, Yuzhou, Qingzhou, Yangzhou } },

            new() { Id = Yangzhou, Name = "扬州", Distance = 6, LocalSupport = 45, Garrison = 2500, Wealth = 5200, DefenseLevel = 40,
                Neighbors = new List<string> { Yuzhou, Jingzhou, Xuzhou, Jiaozhou } },

            new() { Id = Youzhou, Name = "幽州", Distance = 6, LocalSupport = 38, Garrison = 3500, Wealth = 2200, DefenseLevel = 50,
                Neighbors = new List<string> { Jizhou, Bingzhou } },

            new() { Id = Liangzhou, Name = "凉州", Distance = 6, LocalSupport = 25, Garrison = 4000, Wealth = 1800, DefenseLevel = 55,
                Neighbors = new List<string> { Sili, Bingzhou, Yizhou } },

            new() { Id = Yizhou, Name = "益州", Distance = 7, LocalSupport = 42, Garrison = 3000, Wealth = 4800, DefenseLevel = 60,
                Neighbors = new List<string> { Liangzhou, Jingzhou } },

            new() { Id = Jiaozhou, Name = "交州", Distance = 9, LocalSupport = 40, Garrison = 1500, Wealth = 2000, DefenseLevel = 30,
                Neighbors = new List<string> { Jingzhou, Yangzhou } }
        };
    }
}
