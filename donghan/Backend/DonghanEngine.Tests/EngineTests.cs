using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

// Mock 调度器实现
public class MockScheduler : IAIScheduler
{
    public INpcLifecycleManager NpcManager { get; } = new NpcLifecycleManager(new NpcRegistry());

    public bool ShouldAddEdicts { get; set; } = true;

    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult
        {
            PrimaryIntent = "POLITICS",
            NarrativeResponse = "天子震怒，群臣辩驳。"
        };

        result.Speeches.Add(new CourtSpeech
        {
            MinisterId = "zhang_rang",
            MinisterName = "张让",
            SpeechText = "大将军此举必是包藏祸心，臣万万不赞同！",
            Stance = "OPPOSE",
            ExpectedFavorabilityChange = -10,
            ExpectedPowerChange = 0
        });

        result.Speeches.Add(new CourtSpeech
        {
            MinisterId = "cao_cao",
            MinisterName = "曹操",
            SpeechText = "陛下，赈灾势在必行，但需提防贪官漂没！",
            Stance = "AGREED",
            ExpectedFavorabilityChange = 5,
            ExpectedPowerChange = 2
        });

        return Task.FromResult(result);
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        state.IntelReports.Add("【群臣密录】：大将军何进正暗中调兵，意图夺取洛阳西园防权。");
        if (ShouldAddEdicts)
        {
            state.ActiveEdicts.Add(new ImperialEdict {
                Title = "冀州急折", Type = EdictType.UrgentCrisis, NarrativeContent = "冀州干旱，求赐赈米。"
            });
        }
        return Task.CompletedTask;
    }
}

// Mock 事件Oracle实现
public class MockOracle : IEventOracle
{
    public Task<OracleEvent?> CheckRandomEventAsync(GameState state)
    {
        // 简单模拟如果有"天灾"关键词，就触发地震事件
        OracleEvent? evt = null;
        if (state.Chronicle.Count > 0 && state.Chronicle[state.Chronicle.Count - 1].Contains("天灾"))
        {
            evt = new OracleEvent
            {
                EventName = "洛阳地震",
                Description = "洛阳突发地震，百姓惊恐，朝廷需拨款赈灾。",
                ImperialPowerChange = -5,
                TreasuryChange = -100,
                HealthChange = 0
            };
        }
        return Task.FromResult(evt);
    }
}

// Mock 大臣AI实现
public class MockMinisterAgent : IMinisterAgent
{
    public Task<List<MinisterDialogue>> TalkToMinistersAsync(List<string> activeMinisters, string playerInput, GameState state)
    {
        var list = new List<MinisterDialogue>();
        foreach (var mId in activeMinisters)
        {
            if (mId == "he_jin")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "he_jin",
                    MinisterName = "何进",
                    DialogueText = "臣谢陛下隆恩！愿为陛下肝脑涂地！",
                    FavorabilityChange = 15,
                    PowerChange = 5
                });
            }
            else if (mId == "zhang_rang")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "zhang_rang",
                    MinisterName = "张让",
                    DialogueText = "陛下竟然如此待奴才，真是叫奴才寒心呐...",
                    FavorabilityChange = -20,
                    PowerChange = -2
                });
            }
        }
        return Task.FromResult(list);
    }
}

// Mock 叙事者实现
public class MockNarrator : INarrator
{
    public Task<string> RenderStoryAsync(string playerInput, OracleEvent? triggeredEvent, List<MinisterDialogue> ministerDialogues, GameState state)
    {
        string story = $"灵帝朱批：“{playerInput}”。\n";
        if (triggeredEvent != null)
        {
            story += $"天降异象：{triggeredEvent.EventName}。{triggeredEvent.Description}\n";
        }
        foreach (var dial in ministerDialogues)
        {
            story += $"{dial.MinisterName}进言称：\"{dial.DialogueText}\"\n";
        }
        story += $"目前皇权：{state.ImperialPower}，国库资金：{state.Treasury}万钱。";
        return Task.FromResult(story);
    }
}

public class EngineTests
{
    [Fact]
    public async Task Test_RewardHeJin_ShouldIncreaseFavorabilityAndChangeTreasury()
    {
        // Arrange (准备)
        var state = new GameState();
        var scheduler = new MockScheduler();
        var oracle = new MockOracle();
        var ministerAgent = new MockMinisterAgent();
        var narrator = new MockNarrator();

        var engine = new GameEngine(state, scheduler, oracle, ministerAgent, narrator);

        int initialFavorability = state.Npcs["cao_cao"].Favorability;
        int initialPower = state.Npcs["cao_cao"].Power;

        // Act (执行)
        var result = await engine.ProcessPlayerTurnAsync("重赏大将军，赏赐何进锦缎百匹");

        // Assert (验证)
        Assert.NotNull(result);
        Assert.Contains("曹操", result.StoryText);
        Assert.Equal(initialFavorability + 5, state.Npcs["cao_cao"].Favorability);
        Assert.Equal(initialPower + 2, state.Npcs["cao_cao"].Power);
        Assert.True(state.Chronicle.Count > 0);
    }

    [Fact]
    public async Task Test_ColdZhangRang_ShouldDecreaseFavorability()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialFavor = state.Npcs["zhang_rang"].Favorability;

        // Act
        var result = await engine.ProcessPlayerTurnAsync("冷落张让，削其爪牙");

        // Assert
        Assert.Equal(initialFavor - 10, state.Npcs["zhang_rang"].Favorability);
    }

    [Fact]
    public void Test_ArmyDrill_WithCorruptOfficer_ShouldSiphonFunds()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 验证高贪腐张让经办 (Corruption = 90%，漂没比例 = 45%)
        int initialPrivateTreasury = state.PrivateTreasury;
        int initialZhangRangPower = state.Npcs["zhang_rang"].Power;

        // Act
        var result = engine.ExecuteDrillArmyActionWithOfficer(1000, "zhang_rang");

        // Assert
        // 私库支出 1000
        Assert.Equal(initialPrivateTreasury - 1000, state.PrivateTreasury);
        // 代天子犒军，权势稳定成长 +2 点（不再受赃款金额影响暴涨）
        Assert.Equal(initialZhangRangPower + 2, state.Npcs["zhang_rang"].Power);
        Assert.Contains("张让", result.StoryText);
        Assert.Contains("漂没", result.StoryText);
        Assert.Contains("675 万钱", result.StoryText);
    }

    [Fact]
    public void Test_ArmyDrill_WithCleanOfficer_ShouldNotSiphonFunds()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 验证廉洁曹操经办
        int initialPrivateTreasury = state.PrivateTreasury;
        int initialCaoCaoPower = state.Npcs["cao_cao"].Power;

        // Act
        var result = engine.ExecuteDrillArmyActionWithOfficer(1000, "cao_cao");

        // Assert
        Assert.Equal(initialPrivateTreasury - 1000, state.PrivateTreasury);
        // 代天子犒军，权势稳定成长 +2 点
        Assert.Equal(initialCaoCaoPower + 2, state.Npcs["cao_cao"].Power);
        Assert.Contains("曹操", result.StoryText);
        Assert.Contains("漂没 25 万钱", result.StoryText);
    }

    [Fact]
    public void Test_RaiseWestGardenTroops_ShouldCostTreasuryAndRecoverArmySize()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        state.WestGardenArmy.Size = 7000;
        int initialTreasury = state.Treasury;
        int initialSupport = state.PopularSupport;
        int initialMorale = state.WestGardenArmy.Morale;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        var result = engine.ExecuteRaiseWestGardenTroopsAction(2000);

        // Assert
        Assert.Equal(9000, state.WestGardenArmy.Size);
        Assert.Equal(initialTreasury - 600, state.Treasury);
        Assert.Equal(initialSupport - 2, state.PopularSupport);
        Assert.Equal(initialMorale - 2, state.WestGardenArmy.Morale);
        Assert.Contains("西园募兵", result.StoryText);
        Assert.Contains("+2000", result.StoryText);
        Assert.Contains("-600 万钱", result.StoryText);
    }

    [Fact]
    public void Test_RaiseWestGardenTroops_ShouldClampToMaxArmySize()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        state.WestGardenArmy.Size = 11000;
        int initialTreasury = state.Treasury;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        var result = engine.ExecuteRaiseWestGardenTroopsAction(3000);

        // Assert
        Assert.Equal(12000, state.WestGardenArmy.Size);
        Assert.Equal(initialTreasury - 300, state.Treasury);
        Assert.Contains("当前 12000/12000", result.StoryText);
    }

    [Fact]
    public void Test_RaiseWestGardenTroops_InsufficientTreasury_ShouldNotChangeArmySize()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        state.Treasury = 200;
        int initialArmySize = state.WestGardenArmy.Size;
        int initialSupport = state.PopularSupport;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        var result = engine.ExecuteRaiseWestGardenTroopsAction(1000);

        // Assert
        Assert.Equal(initialArmySize, state.WestGardenArmy.Size);
        Assert.Equal(200, state.Treasury);
        Assert.Equal(initialSupport, state.PopularSupport);
        Assert.Contains("募兵失败", result.StoryText);
    }

    [Fact]
    public void Test_RaiseWestGardenTroops_InvalidAmount_ShouldThrow()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => engine.ExecuteRaiseWestGardenTroopsAction(1500));
    }

    [Fact]
    public void Test_DisasterRelief_WithCorruptOfficer_ShouldDeclineMoraleAndEnrichOfficer()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialTreasury = state.Treasury;
        int initialZhangRangPower = state.Npcs["zhang_rang"].Power;
        int initialSupport = state.PopularSupport;

        // Act
        // 拨发国库 2000万 赈灾，指派极其腐败的张让去办
        var result = engine.ExecuteDisasterReliefAction(2000, "zhang_rang");

        // Assert
        Assert.Equal(initialTreasury - 2000, state.Treasury);
        // 指派钦差开仓赈灾，执掌朝廷大宗财税，该钦差大臣朝堂权势稳步成长 +5 点（不受贪污账款暴涨）
        Assert.Equal(initialZhangRangPower + 5, state.Npcs["zhang_rang"].Power);
        // 实际得粮不足活命线，民心暴跌
        Assert.True(state.PopularSupport < initialSupport);
        Assert.Contains("灾民食之腹胀而死", result.StoryText);
        Assert.Contains("中饱私囊：+2000 万钱", result.StoryText);
    }

    [Fact]
    public void Test_DisasterRelief_WithCleanOfficer_ShouldIncreaseMoraleAndKeepSupport()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialTreasury = state.Treasury;
        int initialCaoCaoPower = state.Npcs["cao_cao"].Power;
        int initialSupport = state.PopularSupport;

        // Act
        // 拨发国库 2000万 赈灾，指派廉洁曹操去办
        var result = engine.ExecuteDisasterReliefAction(2000, "cao_cao");

        // Assert
        Assert.Equal(initialTreasury - 2000, state.Treasury);
        // 指派钦差开仓赈灾，执掌朝廷大宗财税，该钦差大臣朝堂权势稳步成长 +5 点
        Assert.Equal(initialCaoCaoPower + 5, state.Npcs["cao_cao"].Power);
        // 民心显著上升
        Assert.True(state.PopularSupport > initialSupport);
        Assert.Contains("数万嗷嗷待哺的灾民得保一命", result.StoryText);
        Assert.Contains("中饱私囊：+75 万钱", result.StoryText);
    }

    [Fact]
    public void Test_Confiscation_WithCliqueBacklash_LootSplittingAndEmbezzlement()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 验证查抄张让 (赃款 6000 万钱，由钦差蹇硕经办，其贪腐度为 25%)
        // 蹇硕侵吞比例 = 25% * 40% = 10%。他会私吞 600 万钱！
        // 剩余可分赃款 = 5400 万钱。
        // 按历史财税分割：70% 进国库 (3780 万钱)，30% 进天子私库 (1620 万钱)。
        int initialTreasury = state.Treasury;
        int initialPrivateTreasury = state.PrivateTreasury;
        int initialSupport = state.PopularSupport;
        int initialJianShuoWealth = state.Npcs["jian_shuo"].StashedWealth;

        // Act
        var result = engine.ExecuteConfiscationAction("zhang_rang", "国库");

        // 重新计算与 Engine 内部一致的数学分红
        int rawWealth = 6000;
        int framerEmbezzled = (int)(6000 * (25 / 100.0) * 0.40); // 600
        int wealthLeft = rawWealth - framerEmbezzled; // 5400
        int amountToTreasury = (int)(wealthLeft * 0.70); // 3780
        int amountToPrivate = wealthLeft - amountToTreasury; // 1620

        // Assert
        // 1. 验证钦差蹇硕中饱私囊了 10% 即 600 万钱
        Assert.Equal(initialJianShuoWealth + framerEmbezzled, state.Npcs["jian_shuo"].StashedWealth);
        // 2. 验证朝廷国库获得 70% 扣除后的赃款：15000 + 3780 = 18780 万钱
        Assert.Equal(initialTreasury + amountToTreasury, state.Treasury);
        // 3. 验证天子私库获得 30% 扣除后的赃款：2000 + 1620 = 3620 万钱
        Assert.Equal(initialPrivateTreasury + amountToPrivate, state.PrivateTreasury);
        // 4. 钦差代表天子执行抄家特权，朝堂权势获得成长 +3
        Assert.Equal(30 + 3, state.Npcs["jian_shuo"].Power);
        // 5. 验证铲除贪官带来天下民心大振
        Assert.True(state.PopularSupport > initialSupport);

        Assert.Contains("蹇硕", result.StoryText);
    }

    [Fact]
    public void Test_Confiscation_WithNoLoyalFramer_ShouldFail()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        foreach (var m in state.Npcs.Values)
        {
            m.Favorability = 30;
        }

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        int initialPower = state.ImperialPower;

        // Act
        var result = engine.ExecuteConfiscationAction("zhang_rang", "国库");

        // Assert
        Assert.Equal(initialPower - 5, state.ImperialPower);
        Assert.Contains("无一人出列附和弹劾", result.StoryText);
    }

    [Fact]
    public async Task Test_Xun_TimeSystem_Update()
    {
        // Arrange
        var state = new GameState { Year = 184, Month = 12, Xun = 3 }; // 下旬
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act & Assert
        // 跨越旬
        await engine.NextXunAsync();

        // 此时应该是：185年 1月 上旬 (Xun = 1)
        Assert.Equal(185, state.Year);
        Assert.Equal(1, state.Month);
        Assert.Equal(1, state.Xun);
        // 验证 AI 调度员在旬更时，是否成功派发了日常情报与政务
        Assert.Contains("【群臣密录】：大将军何进正暗中调兵", state.IntelReports[0]);
        Assert.Contains("冀州急折", state.ActiveEdicts[0].Title);
    }

    [Fact]
    public async Task Test_GrandCourt_AsyncDebatePipeline()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        state.PopularSupport = 30; // 民心低，应触发何进赈灾首发上奏
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        // 1. 同步首发奏折上堂
        string firstIssue = engine.StartGrandCourtSync();

        // Assert 第一通道
        Assert.Contains("何进", firstIssue);
        Assert.Contains("赈济灾民", firstIssue);
        Assert.Empty(state.CourtDebateQueue); // 队列目前应该为空

        // Act 第二通道
        // 2. 玩家在阅读或操作间隙，后台启动异步群辩编排
        await engine.TriggerCourtDebateAsync("准奏！命曹操督办。", "he_jin");

        // Assert 第二通道
        Assert.Equal(2, state.CourtDebateQueue.Count); // 缓冲队列中应成功进栈 2 名大臣发言

        // 3. 模拟前端顺次出栈弹出对话
        var speech1 = state.CourtDebateQueue.Dequeue();
        Assert.Equal("zhang_rang", speech1.MinisterId);
        Assert.Equal("OPPOSE", speech1.Stance);

        var speech2 = state.CourtDebateQueue.Dequeue();
        Assert.Equal("cao_cao", speech2.MinisterId);
        Assert.Equal("AGREED", speech2.Stance);

        Assert.Empty(state.CourtDebateQueue);
    }

    [Fact]
    public void Test_GrandCourt_RitualStages_Retrieval()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        var stages = engine.GetGrandCourtRitualStages();

        // Assert
        Assert.Equal(3, stages.Count);
        Assert.Equal(1, stages[0].StageIndex);
        Assert.Contains("起驾换装", stages[0].Title);
        Assert.Contains("百官趋步", stages[1].Title);
        Assert.Contains("静鞭鸣磬", stages[2].Title);
    }

    [Fact]
    public async Task Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits()
    {
        // Arrange
        var state = new GameState();
        var registry = new NpcRegistry();
        var manager = new NpcLifecycleManager(registry);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 1. 验证 A轨流失，读取 B轨 内置静态硬编码冷备返回
        var presetNpcs = manager.GetPresetNpcsFallback();
        Assert.Contains(presetNpcs, n => n.Name == "董卓");

        // 2. 验证统一接口登录刘备
        var liuBei = new NpcState
        {
            Id = "liu_bei", Name = "刘备", Title = "平原相",
            BirthYear = 161, BaseLongevity = 62, Traits = new() { TraitNames.JingTianWeiDi },
            Corruption = 0, Power = 10, Favorability = 90
        };
        registry.RegisterNpc(liuBei, state);
        Assert.True(state.Npcs.ContainsKey("liu_bei"));

        // 3. 验证单 Traits [经天纬地] 的 1.20x 开仓赈灾民心修正
        state.CurrentLocation = "宣政殿";
        int initialSupport = state.PopularSupport;
        // 拨发 2000万 赈灾，钦差为刘备（贪腐 0%，漂没 0%，到手 2000 > 1000 溢出线）
        // 基础 supportDelta = 12 * (2000 / 1000) = 24 点
        // 经天纬地额外 1.2x：24 * 1.20 = 28 点民心提振
        engine.ExecuteDisasterReliefAction(2000, "liu_bei");
        Assert.Equal(initialSupport + 28, state.PopularSupport);

        // 4. 验证文学词汇 [老谋深算] 降低抄家反噬
        // 曹操 (Traits 包含老谋深算) 诬陷抄家张让（张让 Power 75 触发党羽反噬）
        // 反噬降低：曹操办案，皇权仅降 10 点 (原 base 15 * 0.7 = 10点)
        // 排除刘备干扰（其 Corruption=0 < 曹操的5，新钦差选择规则会优先选最低贪腐度）
        state.Npcs["liu_bei"].Favorability = 30; // 降至门槛以下，确保曹操当选
        state.Npcs["cao_cao"].Favorability = 90;
        int initialPower = state.ImperialPower;
        engine.ExecuteConfiscationAction("zhang_rang", "国库");
        Assert.Equal(initialPower - 10, state.ImperialPower);

        // 5. 验证 Traits 累乘共存：刘备同时拥有 [经天纬地] 1.20x 与 [爱民如子] 1.15x
        // 累计系数：1.20 * 1.15 = 1.38x
        liuBei.Traits.Add(TraitNames.AiMinRuZi);
        int secondarySupport = state.PopularSupport;
        // 再次赈灾 1000万，基础 supportDelta = 12 * (1000 / 1000) = 12点
        // 复合提振：12 * 1.38 = 16.56 -> 16 点提振
        engine.ExecuteDisasterReliefAction(1000, "liu_bei");
        Assert.Equal(secondarySupport + 16, state.PopularSupport);

        // 6. 验证衰老机制 + 确定性随机（seeded Random）
        state.Month = 1;
        state.Xun = 1;
        state.Npcs["he_jin"].BirthYear = 100; // 84岁，超过期望寿命 44

        // seed=14: Next(0,100) returns 4 (< 15 → death)，后续所有 Next(0,1000) ≥ 3 无人染病
        var seededRng = new Random(14);
        await manager.ProcessLifecycleStepAsync(state, false, seededRng);

        // 何进 100% 确定死亡
        Assert.False(state.Npcs.ContainsKey("he_jin"),
            "何进 84岁超过期望寿命，应触发寿终判定死亡");
        // 刘备依然存活（未超 BaseLongevity）
        Assert.True(state.Npcs.ContainsKey("liu_bei"));

        // 7. 验证按需惰性登场部署（DeployNpcToCourt）
        // 游戏初始状态下，Npcs 字典中不包含尚未上场的董卓 (dong_zhuo)
        Assert.False(state.Npcs.ContainsKey("dong_zhuo"));

        // 模拟 AI 调度师主动发送指令部署董卓上场
        manager.DeployNpcToCourt("dong_zhuo", state);

        // 董卓此时应成功从 A/B 轨冷备库中脱水实例化上台
        Assert.True(state.Npcs.ContainsKey("dong_zhuo"));
        var dongZhuo = state.Npcs["dong_zhuo"];
        Assert.Equal("董卓", dongZhuo.Name);
        Assert.Equal("割据军阀", dongZhuo.Faction);
        Assert.Contains(TraitNames.KongWuYouLi, dongZhuo.Traits);
        Assert.Equal(100, dongZhuo.Health); // 初始健康的满额 100 状态
    }

    [Fact]
    public void Test_Edicts_ResolutionAndPromoBacklash()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 1. 创建一封邀功赏赐折，赏赐曹操 (初始 TitleTier = 1: 议郎)
        var edict = new ImperialEdict
        {
            Id = "merit_cao_cao",
            Title = "平叛邀功折",
            Type = EdictType.Merit,
            TargetNpcId = "cao_cao",
            NarrativeContent = "议郎曹操大破乱军，前来讨赏。"
        };

        // 选项 A：赏千金（无晋升）
        edict.Options.Add(new EdictOption
        {
            Description = "赏千金",
            TreasuryDelta = -100,
            TargetNpcFavorabilityDelta = 10
        });

        // 选项 B：跨级超升（跨 2 级，直接封为九卿 3 级）
        edict.Options.Add(new EdictOption
        {
            Description = "拜为九卿（九卿为3级，曹操初始为1级，跃升2级）",
            GrantedTitleTierDelta = 2,
            TargetNpcPowerDelta = 30,
            TargetNpcFavorabilityDelta = 30
        });

        state.ActiveEdicts.Add(edict);

        // Act & Assert 1: 选择 B，触发跨级提拔皇权反噬
        int initialImperialPower = state.ImperialPower;
        int initialCaoCaoTier = state.Npcs["cao_cao"].TitleTier; // 1

        var result = engine.ResolveEdictAction("merit_cao_cao", 1);

        // 断言：曹操成功连升 2 级，TitleTier 变为 1 + 2 = 3
        Assert.Equal(initialCaoCaoTier + 2, state.Npcs["cao_cao"].TitleTier);
        // 断言：因连跃 2 级，触发朝野反噬，皇权暴跌 5 * (2 - 1) = 5 点
        Assert.Equal(initialImperialPower - 5, state.ImperialPower);
        Assert.Contains("跨级拔擢反噬", result.StoryText);
        Assert.Empty(state.ActiveEdicts); // 批阅完后自动出栈移除
    }

    [Fact]
    public async Task Test_Edicts_ExpiryCrisis()
    {
        // Arrange
        var state = new GameState();
        var scheduler = new MockScheduler { ShouldAddEdicts = false };
        var engine = new GameEngine(state, scheduler, new MockOracle(), new MockMinisterAgent(), new MockNarrator(), new Random(42));

        // 提升全郡民心，防止 CheckRebellions 产生非确定性副作用
        foreach (var prov in state.Provinces.Values)
            prov.LocalSupport = 100;

        // 创建一封 3 旬寿命的 UrgentCrisis 折
        var edict = new ImperialEdict
        {
            Id = "crisis_hb",
            Title = "并州兵变急折",
            Type = EdictType.UrgentCrisis,
            NarrativeContent = "并州胡兵叛乱，十万火急！",
            ExpiryXun = 3
        };
        state.ActiveEdicts.Add(edict);

        int initialSupport = state.PopularSupport;

        // Act：流逝 3 旬 (1 个月)
        await engine.NextXunAsync(); // Xun 2
        await engine.NextXunAsync(); // Xun 3
        await engine.NextXunAsync(); // Xun 1 (跨月，此时第 3 次流逝，保质期归 0 触发流产惩罚)

        // Assert：折子应该因留中不发而被彻底剔除，且急报流产，民心大悲暴跌 -15
        Assert.DoesNotContain(state.ActiveEdicts, e => e.Id == "crisis_hb");
        Assert.Equal(initialSupport - 15, state.PopularSupport);
    }

    [Fact]
    public void Test_SellOffice_ShouldIncreasePrivateTreasuryAndReduceImperialPower()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "西园";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialPrivateTreasury = state.PrivateTreasury;
        int initialImperialPower = state.ImperialPower;

        // Act
        var result = engine.ExecuteQuickAction("sell_office");

        // Assert
        Assert.Equal(initialPrivateTreasury + 1000, state.PrivateTreasury);
        Assert.Equal(initialImperialPower - 3, state.ImperialPower);
        Assert.Contains("西园鬻官", result.StoryText);
        Assert.Contains("1000 万钱", result.StoryText);
    }

    [Fact]
    public void Test_HaremRest_ShouldRestoreHealth()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "后宫";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialHealth = state.Health; // 35

        // Act
        var result = engine.ExecuteQuickAction("harem_rest");

        // Assert
        Assert.True(state.Health > initialHealth); // Health should increase by at least 10
        Assert.Contains("后宫春深", result.StoryText);
    }

    [Fact]
    public void Test_HaremRest_WithFlatteringOfficer_ShouldGetExtraHealthAndOfficerPowerBoost()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "后宫";
        // 给张让加上谄媚专权 trait
        state.Npcs["zhang_rang"].Traits.Add(TraitNames.ChanMeiZhuanQuan);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialHealth = state.Health; // 35
        int initialZhangRangPower = state.Npcs["zhang_rang"].Power;
        int initialZhangRangFavorability = state.Npcs["zhang_rang"].Favorability;

        // Act
        engine.ExecuteQuickAction("harem_rest");

        // Assert
        // 基础 +10，群臣随驾额外恢复封顶 +15 = +25（开局扩充后有多名中官参与）
        Assert.Equal(initialHealth + 25, state.Health);
        // 谄媚专权随驾：好感+15，权势+5
        Assert.Equal(initialZhangRangFavorability + 15, state.Npcs["zhang_rang"].Favorability);
        Assert.Equal(initialZhangRangPower + 5, state.Npcs["zhang_rang"].Power);
    }

    [Fact]
    public void Test_HaremRest_UsesTraitConstantsForAllCompanionEffects()
    {
        var state = new GameState();
        state.CurrentLocation = "后宫";
        foreach (var npc in state.Npcs.Values)
        {
            npc.IsActive = false;
        }

        state.Npcs["flatterer"] = new NpcState
        {
            Id = "flatterer", Name = "佞臣", Traits = new() { TraitNames.ChanMeiZhuanQuan }, Favorability = 40, Power = 20
        };
        state.Npcs["bootlicker"] = new NpcState
        {
            Id = "bootlicker", Name = "近侍", Traits = new() { TraitNames.HuiPaiMaPi }, Favorability = 40, Power = 20
        };
        state.Npcs["physician"] = new NpcState
        {
            Id = "physician", Name = "医官", Traits = new() { TraitNames.YiShuGaoMing }
        };
        state.Npcs["assistant_physician"] = new NpcState
        {
            Id = "assistant_physician", Name = "医佐", Traits = new() { TraitNames.DongDianYiLi }
        };
        state.Npcs["talker"] = new NpcState
        {
            Id = "talker", Name = "清谈客", Traits = new() { TraitNames.XiHaoQingTan }
        };

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        int initialHealth = state.Health;
        int initialPower = state.ImperialPower;

        engine.ExecuteQuickAction("harem_rest");

        // 五类后宫随驾特质合计额外恢复 20，但规则应封顶为 +15；基础恢复 +10。
        Assert.Equal(initialHealth + 25, state.Health);
        Assert.Equal(initialPower - 1, state.ImperialPower);
        Assert.Equal(55, state.Npcs["flatterer"].Favorability);
        Assert.Equal(25, state.Npcs["flatterer"].Power);
        Assert.Equal(46, state.Npcs["bootlicker"].Favorability);
        Assert.Equal(22, state.Npcs["bootlicker"].Power);
    }

    [Fact]
    public void Test_SceneTravel_ShouldChangeLocation()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        Assert.Equal("宣政殿", state.CurrentLocation);

        // Act
        engine.TravelToLocation("西园");

        // Assert
        Assert.Equal("西园", state.CurrentLocation);

        // Act
        engine.TravelToLocation("后宫");

        // Assert
        Assert.Equal("后宫", state.CurrentLocation);
    }

    [Fact]
    public void Test_SceneTravel_InvalidLocation_ShouldThrow()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        Assert.Throws<ArgumentException>(() => engine.TravelToLocation("洛阳城"));
    }

    [Fact]
    public void Test_TraitEvaluator_ConflictingTraits_CompoundingMultiplier()
    {
        // 验证正负 traits 共存时的累乘行为
        var officer = new NpcState
        {
            Id = "test", Name = "测试官",
            Traits = new() { TraitNames.JingTianWeiDi, TraitNames.HaoSheWuDu } // 1.20 * 0.75 = 0.90
        };

        double multiplier = NpcTraitEvaluator.GetDisasterReliefSupportMultiplier(officer);
        Assert.Equal(0.90, multiplier, 4);
    }

    [Fact]
    public void Test_TraitEvaluator_Embezzlement_CleanVsCorrupt()
    {
        var cleanOfficer = new NpcState { Id = "c", Name = "清官", Traits = new() { TraitNames.QingZhengLianJie } };
        var greedyOfficer = new NpcState { Id = "g", Name = "贪官", Traits = new() { TraitNames.TanDeWuYan } };

        int siphon = 100;

        Assert.Equal(0, NpcTraitEvaluator.ApplyEmbezzlementSiphon(cleanOfficer, siphon));
        Assert.Equal(150, NpcTraitEvaluator.ApplyEmbezzlementSiphon(greedyOfficer, siphon));
    }

    [Fact]
    public void Test_QuickAction_InvalidLocation_ShouldThrow()
    {
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 在宣政殿不能卖官（只能在西园）
        Assert.Throws<InvalidOperationException>(() => engine.ExecuteQuickAction("sell_office"));
        // 在宣政殿不能后宫休息
        Assert.Throws<InvalidOperationException>(() => engine.ExecuteQuickAction("harem_rest"));
    }
}