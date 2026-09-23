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
            // 司隶（洛阳京畿）：初始土地 120,000，国家控制 80,000（占 67%），世家 40,000
            new() { Id = Sili, Name = "司隶", Distance = 0, LocalSupport = 50, Garrison = 5000, Wealth = 5000, DefenseLevel = 80,
                LandCarryingCapacity = 120000, StateControlledLand = 80000, GentryControlledLand = 40000,
                Neighbors = new List<string> { Jizhou, Yanzhou, Yuzhou, Bingzhou, Liangzhou } },

            // 冀州：土地 150,000，国家仅控制 25,000（占 17%），世家 125,000
            new() { Id = Jizhou, Name = "冀州", Distance = 3, LocalSupport = 18, Garrison = 2000, Wealth = 3000, DefenseLevel = 30,
                LandCarryingCapacity = 150000, StateControlledLand = 25000, GentryControlledLand = 125000,
                Neighbors = new List<string> { Sili, Yanzhou, Bingzhou, Youzhou, Qingzhou } },

            // 并州：土地 80,000，国家 15,000，世家 65,000
            new() { Id = Bingzhou, Name = "并州", Distance = 4, LocalSupport = 30, Garrison = 3000, Wealth = 2500, DefenseLevel = 40,
                LandCarryingCapacity = 80000, StateControlledLand = 15000, GentryControlledLand = 65000,
                Neighbors = new List<string> { Sili, Jizhou, Liangzhou, Youzhou } },

            // 兖州：土地 120,000，国家 20,000，世家 100,000
            new() { Id = Yanzhou, Name = "兖州", Distance = 2, LocalSupport = 35, Garrison = 2500, Wealth = 3500, DefenseLevel = 35,
                LandCarryingCapacity = 120000, StateControlledLand = 20000, GentryControlledLand = 100000,
                Neighbors = new List<string> { Sili, Jizhou, Yuzhou, Qingzhou, Xuzhou } },

            // 豫州：土地 140,000，国家 25,000，世家 115,000（汝南袁氏/颍川荀氏大族根基）
            new() { Id = Yuzhou, Name = "豫州", Distance = 1, LocalSupport = 45, Garrison = 2000, Wealth = 4000, DefenseLevel = 40,
                LandCarryingCapacity = 140000, StateControlledLand = 25000, GentryControlledLand = 115000,
                Neighbors = new List<string> { Sili, Yanzhou, Jingzhou, Xuzhou, Yangzhou } },

            // 荆州：土地 160,000，国家 25,000，世家 135,000（荆襄名门蒯蔡庞黄）
            new() { Id = Jingzhou, Name = "荆州", Distance = 5, LocalSupport = 50, Garrison = 3000, Wealth = 6000, DefenseLevel = 50,
                LandCarryingCapacity = 160000, StateControlledLand = 25000, GentryControlledLand = 135000,
                Neighbors = new List<string> { Yuzhou, Yangzhou, Yizhou, Jiaozhou } },

            // 扩展 7 州
            // 青州：土地 110,000，国家 20,000，世家 90,000
            new() { Id = Qingzhou, Name = "青州", Distance = 4, LocalSupport = 32, Garrison = 2000, Wealth = 2800, DefenseLevel = 35,
                LandCarryingCapacity = 110000, StateControlledLand = 20000, GentryControlledLand = 90000,
                Neighbors = new List<string> { Jizhou, Yanzhou, Xuzhou } },

            // 徐州：土地 120,000，国家 20,000，世家 100,000（徐州陈氏陈珪陈登）
            new() { Id = Xuzhou, Name = "徐州", Distance = 3, LocalSupport = 48, Garrison = 2200, Wealth = 4500, DefenseLevel = 45,
                LandCarryingCapacity = 120000, StateControlledLand = 20000, GentryControlledLand = 100000,
                Neighbors = new List<string> { Yanzhou, Yuzhou, Qingzhou, Yangzhou } },

            // 扬州：土地 140,000，国家 20,000，世家 120,000（江南四姓顾陆朱张）
            new() { Id = Yangzhou, Name = "扬州", Distance = 6, LocalSupport = 45, Garrison = 2500, Wealth = 5200, DefenseLevel = 40,
                LandCarryingCapacity = 140000, StateControlledLand = 20000, GentryControlledLand = 120000,
                Neighbors = new List<string> { Yuzhou, Jingzhou, Xuzhou, Jiaozhou } },

            // 幽州：土地 90,000，国家 15,000，世家 75,000
            new() { Id = Youzhou, Name = "幽州", Distance = 6, LocalSupport = 38, Garrison = 3500, Wealth = 2200, DefenseLevel = 50,
                LandCarryingCapacity = 90000, StateControlledLand = 15000, GentryControlledLand = 75000,
                Neighbors = new List<string> { Jizhou, Bingzhou } },

            // 凉州：土地 90,000，国家 15,000，世家 75,000
            new() { Id = Liangzhou, Name = "凉州", Distance = 6, LocalSupport = 25, Garrison = 4000, Wealth = 1800, DefenseLevel = 55,
                LandCarryingCapacity = 90000, StateControlledLand = 15000, GentryControlledLand = 75000,
                Neighbors = new List<string> { Sili, Bingzhou, Yizhou } },

            // 益州：土地 150,000，国家 20,000，世家 130,000（益州豪强势族）
            new() { Id = Yizhou, Name = "益州", Distance = 7, LocalSupport = 42, Garrison = 3000, Wealth = 4800, DefenseLevel = 60,
                LandCarryingCapacity = 150000, StateControlledLand = 20000, GentryControlledLand = 130000,
                Neighbors = new List<string> { Liangzhou, Jingzhou } },

            // 交州：土地 70,000，国家 10,000，世家 60,000（士燮家族）
            new() { Id = Jiaozhou, Name = "交州", Distance = 9, LocalSupport = 40, Garrison = 1500, Wealth = 2000, DefenseLevel = 30,
                LandCarryingCapacity = 70000, StateControlledLand = 10000, GentryControlledLand = 60000,
                Neighbors = new List<string> { Jingzhou, Yangzhou } }
        };
    }
}
