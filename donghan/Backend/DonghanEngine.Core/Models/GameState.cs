using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

// P0-2 结局枚举
//   Playing   - 游戏中
//   ZhongXing - 中兴之治：灵帝活到 40 + 皇权 ≥ 60 + 民心 ≥ 50 + 0 叛郡
//   XuMing    - 续命成功：灵帝活到 40 (其他条件不满足)
//   Collapse  - 崩殂：Health ≤ 0
//   Vanquished- 亡国：PopularSupport ≤ 5 (黄巾入洛)
public enum GameOutcome
{
    Playing,
    ZhongXing,
    XuMing,
    Collapse,
    Vanquished
}

public class GameState
{
    private int _imperialPower = 25;
    private int _treasury = 8000;
    private int _privateTreasury = 1200;
    private int _popularSupport = 28;
    private int _health = 35;
    private int _reignYear = 7;
    private int _year = 184;
    private int _month = 4;
    private int _xun = 1;

    public int ImperialPower
    {
        get => _imperialPower;
        set => _imperialPower = Math.Clamp(value, 0, 100);
    }
    
    public int Treasury
    {
        get => _treasury;
        set => _treasury = Math.Max(0, value);
    }
    
    public int PrivateTreasury
    {
        get => _privateTreasury;
        set => _privateTreasury = Math.Max(0, value);
    }
    
    public int PopularSupport
    {
        get => _popularSupport;
        set => _popularSupport = Math.Clamp(value, 0, 100);
    }
    
    public int Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, 100);
    }
    
    public string ReignTitle { get; set; } = "光和"; // 年号
    public int ReignYear
    {
        get => _reignYear;
        set => _reignYear = Math.Max(1, value);
    }

    public string CurrentLocation { get; set; } = "宣政殿";

    private readonly Dictionary<string, NpcState> _npcs = new();
    private readonly List<NpcRelation> _npcRelations = new();
    private readonly Dictionary<string, Province> _provinces = new();
    private readonly List<string> _chronicle = new();
    private readonly List<string> _intelReports = new();
    private readonly List<ImperialEdict> _activeEdicts = new();
    private readonly Queue<CourtSpeech> _courtDebateQueue = new();

    public IReadOnlyDictionary<string, NpcState> Npcs => _npcs;
    public IReadOnlyList<NpcRelation> NpcRelations => _npcRelations;
    public ArmyState WestGardenArmy { get; set; } = new(); // 西园八校尉新军
    public IReadOnlyDictionary<string, Province> Provinces => _provinces;
    public IReadOnlyList<string> Chronicle => _chronicle;
    public IReadOnlyList<string> IntelReports => _intelReports;
    public IReadOnlyList<ImperialEdict> ActiveEdicts => _activeEdicts;
    public Queue<CourtSpeech> CourtDebateQueue => _courtDebateQueue;

    // === 内部受控集合操作领域方法 ===
    public void RegisterNpc(NpcState npc)
    {
        if (npc != null && !string.IsNullOrWhiteSpace(npc.Id))
            _npcs[npc.Id] = npc;
    }

    public bool RemoveNpc(string npcId) => _npcs.Remove(npcId);

    public void RegisterProvince(Province province)
    {
        if (province != null && !string.IsNullOrWhiteSpace(province.Id))
            _provinces[province.Id] = province;
    }

    public void AddActiveEdict(ImperialEdict edict)
    {
        if (edict != null && !_activeEdicts.Contains(edict))
            _activeEdicts.Add(edict);
    }

    public bool RemoveActiveEdict(ImperialEdict edict) => _activeEdicts.Remove(edict);
    public bool RemoveActiveEdict(string edictId) => _activeEdicts.RemoveAll(e => e.Id == edictId) > 0;

    public void AddIntelReport(string report)
    {
        if (!string.IsNullOrWhiteSpace(report))
            _intelReports.Add(report);
    }

    public void SetNpcRelations(IEnumerable<NpcRelation> relations)
    {
        _npcRelations.Clear();
        if (relations != null) _npcRelations.AddRange(relations);
    }

    // 纪元时间系统（旬：1-3，每旬十天，三旬为一月）
    public int Year
    {
        get => _year;
        set => _year = value;
    }
    
    public int Month
    {
        get => _month;
        set => _month = Math.Clamp(value, 1, 12);
    }
    
    public int Xun
    {
        get => _xun;
        set => _xun = Math.Clamp(value, 1, 3);
    }

    // 记录上一次进行 NPC 衰老病退物理结算的时间戳 (格式：Year * 1000 + Month * 10 + Xun)
    // 用于防御在同一旬内，调度师指令频繁触发或者开发回溯导致的 NPC 年龄暴涨等边界问题
    public int LastNpcProcessedTimestamp { get; set; } = 0;

    // === P0-2 结局系统 ===
    // 历史灵帝刘宏生于 156 年，崩于 189 年（光和七年/中平元年 33 岁）。
    // 游戏开局灵帝 28 岁；"中兴/续命"以 40 岁为达成门槛（= 12 年后，144 旬）。
    public int EmperorBirthYear { get; set; } = 156;
    public GameOutcome Outcome { get; set; } = GameOutcome.Playing;

    public int GetEmperorAge() => Year - EmperorBirthYear;

    // === P0-3 旗：测试与沙盒可关掉历史硬 trigger，避免污染"折子过期"等单测 ===
    public bool DisableHistoricalTriggers { get; set; } = false;

    public GameState()
    {
        // 大将军何进：外戚权臣，初始私蓄 1500。何进权势 80，好感 35。性格：平庸。Traits：[“拥兵自重”]
        RegisterNpc(new NpcState { 
            Id = "he_jin", Name = "何进", Title = "大将军", TitleTier = 4,
            Favorability = 35, Power = 80, Corruption = 45, StashedWealth = 1500, BirthYear = 135, BaseLongevity = 44,
            Traits = new List<string> { TraitNames.YongBingZiZhong, TraitNames.ShouXiaYouBing }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派",
            Martial = 40, Leadership = 35, Politics = 30, Charisma = 40, Ambition = 60,
            InitialLocation = "洛阳朝堂", EntryCondition = "开局", HistoricalRole = "外戚权臣，何皇后之兄，掌中央军权"
        });
        
        // 十常侍张让：历史极度贪婪，擅权夺利。初始私蓄 6000！张让权势 75，好感 65。Traits：[“贪得无厌”]
        RegisterNpc(new NpcState { 
            Id = "zhang_rang", Name = "张让", Title = "十常侍之首", TitleTier = 3,
            Favorability = 65, Power = 75, Corruption = 90, StashedWealth = 6000, BirthYear = 130, BaseLongevity = 60,
            Traits = new List<string> { TraitNames.TanDeWuYan, TraitNames.ChanMeiZhuanQuan }, Personality = "阴险", Style = TraitNames.ChanMeiZhuanQuan, Faction = "阉党派",
            Martial = 10, Leadership = 15, Politics = 55, Charisma = 60, Ambition = 85,
            InitialLocation = "洛阳宫中", EntryCondition = "开局", HistoricalRole = "十常侍核心，灵帝宠宦，内廷卖官与诏令枢纽"
        });
        
        // 典军校尉曹操：文武兼备，清流名臣。初始私蓄 50。曹操权势 15，好感 45。Traits：[“经天纬地”, “老谋深算”]
        RegisterNpc(new NpcState { 
            Id = "cao_cao", Name = "曹操", Title = "议郎/典军校尉", TitleTier = 1,
            Favorability = 45, Power = 15, Corruption = 5, StashedWealth = 50, BirthYear = 155, BaseLongevity = 65,
            Traits = new List<string> { TraitNames.JingTianWeiDi, TraitNames.LaoMouShenSuan }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派",
            Martial = 72, Leadership = 90, Politics = 85, Charisma = 80, Ambition = 75,
            InitialLocation = "洛阳朝堂", EntryCondition = "开局", HistoricalRole = "青年能臣，西园八校尉之一，未来乱世枭雄"
        });
        
        // 上军校尉蹇硕：灵帝心腹，西园统帅。初始私蓄 300。蹇硕权势 30，好感 80。Traits：[“孔武有力”]
        RegisterNpc(new NpcState { 
            Id = "jian_shuo", Name = "蹇硕", Title = "西园上军校尉", TitleTier = 2,
            Favorability = 80, Power = 30, Corruption = 25, StashedWealth = 300, BirthYear = 145, BaseLongevity = 50,
            Traits = new List<string> { TraitNames.KongWuYouLi }, Personality = "刚直", Style = "保皇尽忠", Faction = "西园亲军",
            Martial = 65, Leadership = 45, Politics = 20, Charisma = 30, Ambition = 40,
            InitialLocation = "洛阳西园", EntryCondition = "开局", HistoricalRole = "灵帝亲信宦官，西园军上军校尉"
        });

        // 史实关系网：静态关系边先入库，规则与 UI 按需读取；关系目标可指向冷备人物。
        SetNpcRelations(HistoricalNpcRelations.All);

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
                RegisterNpc(HistoricalNpcPresets.Clone(preset));
            }
        }

        // === 大汉十三州（完整 13 州郡开局）===
        foreach (var p in ProvinceCatalog.CreateInitialProvinces())
        {
            RegisterProvince(p);
        }

        // 预派太守（与 AssignGovernor 等价的字段直设，不走 -5 权势 / 不写编年史，让"开局即有太守"成为历史事实而非朝会决定）
        AssignInitialGovernor("jizhou", "qiao_xuan", supportBonus: 10);
        AssignInitialGovernor("yuzhou", "lu_zhi", supportBonus: 10);
        AssignInitialGovernor("bingzhou", "huangfu_song", supportBonus: 10);

        RefreshReignEra();
    }

    // 灵帝年号系统：光和(178 - 184年11月)，184年12月改元中平(184 - 189)。
    public void RefreshReignEra()
    {
        if (Year < 184 || (Year == 184 && Month < 12))
        {
            ReignTitle = "光和";
            ReignYear = Year - 177; // 178 = 光和元年
        }
        else
        {
            ReignTitle = "中平";
            ReignYear = Year - 183; // 184 = 中平元年
        }
    }

    private void AssignInitialGovernor(string provinceId, string npcId, int supportBonus = 0)
    {
        if (!Provinces.TryGetValue(provinceId, out var province)) return;
        if (!Npcs.TryGetValue(npcId, out var npc)) return;
        if (!npc.IsActive || npc.IsHostile) return;
        if (npc.GovernedProvinceId != null) return;
        if (province.GovernorId != null) return;
        province.GovernorId = npcId;
        npc.GovernedProvinceId = provinceId;
        if (supportBonus > 0)
        {
            province.AdjustLocalSupport(supportBonus);
        }
    }

    public void ApplyNumericalDelta(int imperialPowerDelta, int treasuryDelta, int healthDelta)
    {
        ImperialPower = Math.Clamp(ImperialPower + imperialPowerDelta, 0, 100);
        Treasury = Math.Clamp(Treasury + treasuryDelta, 0, 999999);
        Health = Math.Clamp(Health + healthDelta, 0, 100);
    }

    public void AddToChronicle(string text)
    {
        _chronicle.Add($"【{ReignTitle}{GetYearString(ReignYear)}】: {text}");
        if (_chronicle.Count > 100)
        {
            _chronicle.RemoveAt(0);
        }
    }

    private string GetYearString(int year)
    {
        if (year == 1) return "元年";
        return $"{year}年";
    }
}
