using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public class NpcState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;    // 初始官职 (如：大将军、十常侍、议郎)
    public int TitleTier { get; set; } = 0;              // 0-4级官阶
    public int BirthYear { get; set; } = 150;            // 出生年份
    public int StashedWealth { get; set; } = 50;         // 私蓄赃款 (万钱)
    public int Favorability { get; set; } = 50;          // 对天子好感 (0-100)
    public int Power { get; set; } = 15;                 // 朝堂政治权势 (0-100)
    public int Corruption { get; set; } = 20;            // 贪腐度 (0-100)

    // “藏锋于词” 专属文学词汇特征
    public System.Collections.Generic.List<string> Traits { get; set; } = new(); // 经天纬地、孔武有力、老谋深算、贪得无厌
    public string Personality { get; set; } = "中庸";     // 性格简述 (如：阴险、刚直、谄媚)
    public string Style { get; set; } = "明哲保身";       // 处事风格 (如：结党营私、雷厉风行、拥兵自重)
    public string Faction { get; set; } = "清流派";       // 派系归属 (清流派/外戚派/阉党派/割据军阀)

    // 生存与生命周期控制
    public int Health { get; set; } = 100;               // 健康值 (0-100)，归 0 则病逝
    public int BaseLongevity { get; set; } = 65;         // 期望寿命上限
    public bool IsActive { get; set; } = true;            // 是否活跃于朝堂
    public string DeathReason { get; set; } = string.Empty; // 死亡/退场因由

    // === 五维基本属性 (0-100) ===
    public int Martial    { get; set; } = 40;  // 武力
    public int Leadership { get; set; } = 40;  // 统帅
    public int Politics   { get; set; } = 40;  // 政治
    public int Charisma   { get; set; } = 40;  // 魅力
    public int Ambition   { get; set; } = 40;  // 野心

    // === 地方治理 ===
    public string? GovernedProvinceId { get; set; } = null; // 正在治理的郡 ID

    // === 历史预设与登场控制 ===
    public string InitialLocation { get; set; } = "洛阳朝堂";  // 洛阳朝堂/地方州郡/在野/边军/敌对势力
    public string EntryCondition { get; set; } = "开局";       // 开局/事件触发/年月触发/冷备
    public string HistoricalRole { get; set; } = string.Empty;  // 简短史料定位
    public bool IsHostile { get; set; } = false;                // 敌对首领不进入任官/平叛/招安候选
    public int? HistoricalDeathYear { get; set; } = null;       // 史实/传统说法卒年，仅作参考，不强制死亡
    public string SourceNote { get; set; } = string.Empty;      // 生卒年与传记来源说明；生年不详者标明游戏估算
}

public class ArmyState
{
    public int Size { get; set; } = 8000;         // 西园军人数
    public int BasePayPerTurn { get; set; } = 120; // 单回合基础军饷 (万钱)
    public int Morale { get; set; } = 55;          // 士气 (0-100)
    public int Loyalty { get; set; } = 50;         // 对天子的绝对忠诚度 (0-100)
}

public class GameState
{
    public int ImperialPower { get; set; } = 25; // 皇权 (0-100) - 初始极弱，政令不出宫门
    public int Treasury { get; set; } = 8000;    // 朝廷国库 (万钱) - 初始开支窘迫
    public int PrivateTreasury { get; set; } = 1200; // 西园天子私库 (万钱) - 内库告急
    public int PopularSupport { get; set; } = 28;  // 天下民心 (0-100) - 跌破活命红线，黄巾蠢动
    public int Health { get; set; } = 35;        // 皇帝健康 (0-100) - 龙体极其虚弱，沉迷享乐濒危
    public string ReignTitle { get; set; } = "光和"; // 年号
    public int ReignYear { get; set; } = 7;       // 年份
    
    public string CurrentLocation { get; set; } = "宣政殿";

    public System.Collections.Generic.Dictionary<string, NpcState> Npcs { get; set; } = new();
    public System.Collections.Generic.List<NpcRelation> NpcRelations { get; set; } = new();
    public ArmyState WestGardenArmy { get; set; } = new(); // 西园八校尉新军
    public System.Collections.Generic.Dictionary<string, Province> Provinces { get; set; } = new();
    
    public List<string> Chronicle { get; set; } = new();

    // 纪元时间系统（旬：1-3，每旬十天，三旬为一月）
    public int Year { get; set; } = 184; // 中平元年
    public int Month { get; set; } = 4;  // 孟春/仲春
    public int Xun { get; set; } = 1;    // 1: 上旬, 2: 中旬, 3: 下旬

    // 记录上一次进行 NPC 衰老病退物理结算的时间戳 (格式：Year * 1000 + Month * 10 + Xun)
    // 用于防御在同一旬内，调度师指令频繁触发或者开发回溯导致的 NPC 年龄暴涨等边界问题
    public int LastNpcProcessedTimestamp { get; set; } = 0;

    // 主界面四大操作数据结构缓存
    public System.Collections.Generic.List<string> IntelReports { get; set; } = new(); // 已收集的百官密录与天下异动
    public System.Collections.Generic.List<ImperialEdict> ActiveEdicts { get; set; } = new();  // 待批阅的地方奏折、政务

    // 异步朝局辩论缓冲区
    public System.Collections.Generic.Queue<CourtSpeech> CourtDebateQueue { get; set; } = new();

    public GameState()
    {
        // 大将军何进：外戚权臣，初始私蓄 1500。何进权势 80，好感 35。性格：平庸。Traits：[“拥兵自重”]
        Npcs["he_jin"] = new NpcState { 
            Id = "he_jin", Name = "何进", Title = "大将军", TitleTier = 4,
            Favorability = 35, Power = 80, Corruption = 45, StashedWealth = 1500, BirthYear = 135, BaseLongevity = 44,
            Traits = new() { TraitNames.YongBingZiZhong, TraitNames.ShouXiaYouBing }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派",
            Martial = 40, Leadership = 35, Politics = 30, Charisma = 40, Ambition = 60,
            InitialLocation = "洛阳朝堂", EntryCondition = "开局", HistoricalRole = "外戚权臣，何皇后之兄，掌中央军权"
        };
        
        // 十常侍张让：历史极度贪婪，擅权夺利。初始私蓄 6000！张让权势 75，好感 65。Traits：[“贪得无厌”]
        Npcs["zhang_rang"] = new NpcState { 
            Id = "zhang_rang", Name = "张让", Title = "十常侍之首", TitleTier = 3,
            Favorability = 65, Power = 75, Corruption = 90, StashedWealth = 6000, BirthYear = 130, BaseLongevity = 60,
            Traits = new() { TraitNames.TanDeWuYan, TraitNames.ChanMeiZhuanQuan }, Personality = "阴险", Style = TraitNames.ChanMeiZhuanQuan, Faction = "阉党派",
            Martial = 10, Leadership = 15, Politics = 55, Charisma = 60, Ambition = 85,
            InitialLocation = "洛阳宫中", EntryCondition = "开局", HistoricalRole = "十常侍核心，灵帝宠宦，内廷卖官与诏令枢纽"
        };
        
        // 青年曹操：廉洁。初始私蓄 50。曹操权势为 15，好感 45。Traits：[“经天纬地”, “老谋深算”]
        Npcs["cao_cao"] = new NpcState { 
            Id = "cao_cao", Name = "曹操", Title = "议郎/典军校尉", TitleTier = 1,
            Favorability = 45, Power = 15, Corruption = 5, StashedWealth = 50, BirthYear = 155, BaseLongevity = 65,
            Traits = new() { TraitNames.JingTianWeiDi, TraitNames.LaoMouShenSuan }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派",
            Martial = 72, Leadership = 90, Politics = 85, Charisma = 80, Ambition = 75,
            InitialLocation = "洛阳朝堂", EntryCondition = "开局", HistoricalRole = "青年能臣，西园八校尉之一，未来乱世枭雄"
        };
        
        // 蹇硕：天子亲信。初始私蓄 300。蹇硕权势 30，好感 80。Traits：[“孔武有力”]
        Npcs["jian_shuo"] = new NpcState { 
            Id = "jian_shuo", Name = "蹇硕", Title = "西园上军校尉", TitleTier = 2,
            Favorability = 80, Power = 30, Corruption = 25, StashedWealth = 300, BirthYear = 145, BaseLongevity = 50,
            Traits = new() { TraitNames.KongWuYouLi }, Personality = "刚直", Style = "保皇尽忠", Faction = "西园亲军",
            Martial = 65, Leadership = 45, Politics = 20, Charisma = 30, Ambition = 40,
            InitialLocation = "洛阳西园", EntryCondition = "开局", HistoricalRole = "灵帝亲信宦官，西园军上军校尉"
        };

        // 史实关系网：静态关系边先入库，规则与 UI 按需读取；关系目标可指向冷备人物。
        NpcRelations = HistoricalNpcRelations.All;

        // 扩展开局洛阳群臣：只部署高频朝会/党争人物；地方、在野、敌对人物仍留在冷备池。
        foreach (var id in new[]
        {
            "yuan_shao", "yuan_shu", "wang_yun", "lu_zhi", "huangfu_song", "zhu_jun",
            "zhao_zhong", "duan_gui", "bi_lan", "he_miao", "yang_biao", "ma_ridi", "cai_yong",
            "yuan_wei", "zhang_wen", "cui_lie", "qiao_xuan", "xun_shuang", "chen_song",
            "xia_yun", "guo_sheng", "song_dian", "han_kui"
        })
        {
            if (HistoricalNpcPresets.All.Find(n => n.Id == id) is { } preset)
            {
                Npcs[id] = HistoricalNpcPresets.Clone(preset);
            }
        }

        // === 大汉十三州（当前开放 6 郡）===
        Provinces["sili"] = new Province { Id = "sili", Name = "司隶", Distance = 0, LocalSupport = 50, Garrison = 5000, Wealth = 5000, DefenseLevel = 80, Neighbors = new() { "jizhou", "yanzhou", "yuzhou" } };
        Provinces["jizhou"] = new Province { Id = "jizhou", Name = "冀州", Distance = 3, LocalSupport = 18, Garrison = 2000, Wealth = 3000, DefenseLevel = 30, Neighbors = new() { "sili", "yanzhou", "bingzhou" } };
        Provinces["bingzhou"] = new Province { Id = "bingzhou", Name = "并州", Distance = 4, LocalSupport = 30, Garrison = 3000, Wealth = 2500, DefenseLevel = 40, Neighbors = new() { "jizhou", "yanzhou" } };
        Provinces["yanzhou"] = new Province { Id = "yanzhou", Name = "兖州", Distance = 2, LocalSupport = 35, Garrison = 2500, Wealth = 3500, DefenseLevel = 35, Neighbors = new() { "sili", "jizhou", "yuzhou" } };
        Provinces["yuzhou"] = new Province { Id = "yuzhou", Name = "豫州", Distance = 1, LocalSupport = 45, Garrison = 2000, Wealth = 4000, DefenseLevel = 40, Neighbors = new() { "sili", "yanzhou" } };
        Provinces["jingzhou"] = new Province { Id = "jingzhou", Name = "荆州", Distance = 5, LocalSupport = 50, Garrison = 3000, Wealth = 6000, DefenseLevel = 50, Neighbors = new() { "yuzhou" } };
    }

    public void ApplyNumericalDelta(int imperialPowerDelta, int treasuryDelta, int healthDelta)
    {
        ImperialPower = Math.Clamp(ImperialPower + imperialPowerDelta, 0, 100);
        Treasury = Math.Clamp(Treasury + treasuryDelta, 0, 999999);
        Health = Math.Clamp(Health + healthDelta, 0, 100);
    }

    public void AddToChronicle(string text)
    {
        Chronicle.Add($"【{ReignTitle}{GetYearString(ReignYear)}】: {text}");
        if (Chronicle.Count > 100)
        {
            Chronicle.RemoveAt(0);
        }
    }

    private string GetYearString(int year)
    {
        if (year == 1) return "元年";
        return $"{year}年";
    }
}

public class CourtSpeech
{
    public string MinisterId { get; set; } = string.Empty;
    public string MinisterName { get; set; } = string.Empty;
    public string SpeechText { get; set; } = string.Empty;
    public string Stance { get; set; } = "OPPOSE"; // AGREED / OPPOSE / RETALIATE (迎合 / 反对 / 党羽弹劾反噬)
    public int ExpectedFavorabilityChange { get; set; }
    public int ExpectedPowerChange { get; set; }
}

public class AIOrchestrationResult
{
    public string PrimaryIntent { get; set; } = "UNKNOWN"; // POLITICS / POLICY / PERSONAL / SACRIFICE
    public System.Collections.Generic.List<CourtSpeech> Speeches { get; set; } = new();
    public string NarrativeResponse { get; set; } = string.Empty;
}

public class RitualStageInfo
{
    public int StageIndex { get; set; } // 1, 2, 3
    public string Title { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
}

public class Province
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GovernorId { get; set; } = null;
    public int Distance { get; set; } = 2;
    public List<string> Neighbors { get; set; } = new();

    public bool IsRebelling { get; set; } = false;
    public int RebellionMonths { get; set; } = 0;
    public string RebelFaction { get; set; } = string.Empty;

    public int LocalSupport { get; set; } = 30;
    public int Wealth { get; set; } = 2000;
    public int Garrison { get; set; } = 2000;
    public int DefenseLevel { get; set; } = 30;

    public int LowSupportStreakMonths { get; set; } = 0;
}
