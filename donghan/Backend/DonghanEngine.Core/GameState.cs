using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public class NpcState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;    // 初始官职 (如：大将军、十常侍、议郎)
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
    public ArmyState WestGardenArmy { get; set; } = new(); // 西园八校尉新军
    
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
    public System.Collections.Generic.List<string> ActiveEdicts { get; set; } = new();  // 待批阅的地方奏折、政务

    // 异步朝局辩论缓冲区
    public System.Collections.Generic.Queue<CourtSpeech> CourtDebateQueue { get; set; } = new();

    public GameState()
    {
        // 大将军何进：外戚权臣，初始私蓄 1500。何进权势 80，好感 35。性格：平庸。Traits：[“拥兵自重”]
        Npcs["he_jin"] = new NpcState { 
            Id = "he_jin", Name = "何进", Title = "大将军", 
            Favorability = 35, Power = 80, Corruption = 45, StashedWealth = 1500, BirthYear = 135,
            Traits = new() { "拥兵自重" }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派"
        };
        
        // 十常侍张让：历史极度贪婪，擅权夺利。初始私蓄 6000！张让权势 75，好感 65。Traits：[“贪得无厌”]
        Npcs["zhang_rang"] = new NpcState { 
            Id = "zhang_rang", Name = "张让", Title = "十常侍之首", 
            Favorability = 65, Power = 75, Corruption = 90, StashedWealth = 6000, BirthYear = 130,
            Traits = new() { "贪得无厌" }, Personality = "阴险", Style = "谄媚专权", Faction = "阉党派"
        };
        
        // 青年曹操：廉洁。初始私蓄 50。曹操权势为 15，好感 45。Traits：[“经天纬地”, “老谋深算”]
        Npcs["cao_cao"] = new NpcState { 
            Id = "cao_cao", Name = "曹操", Title = "议郎/典军校尉", 
            Favorability = 45, Power = 15, Corruption = 5, StashedWealth = 50, BirthYear = 155,
            Traits = new() { "经天纬地", "老谋深算" }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派"
        };
        
        // 蹇硕：天子亲信。初始私蓄 300。蹇硕权势 30，好感 80。Traits：[“孔武有力”]
        Npcs["jian_shuo"] = new NpcState { 
            Id = "jian_shuo", Name = "蹇硕", Title = "西园上军校尉", 
            Favorability = 80, Power = 30, Corruption = 25, StashedWealth = 300, BirthYear = 145,
            Traits = new() { "孔武有力" }, Personality = "刚直", Style = "保皇尽忠", Faction = "阉党派"
        };
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
