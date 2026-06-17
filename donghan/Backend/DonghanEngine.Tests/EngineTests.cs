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
        // 4. 钦差代表天子执行抄家特权，朝堂权势获得成长 +3；蹇硕与张让宫中军权相争，关系牵连再 +1
        Assert.Equal(30 + 4, state.Npcs["jian_shuo"].Power);
        // 5. 验证铲除贪官带来天下民心大振
        Assert.True(state.PopularSupport > initialSupport);

        Assert.Contains("蹇硕", result.StoryText);
    }

    [Fact]
    public void Test_Confiscation_ShouldTriggerRelationBacklash()
    {
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        state.Npcs["cao_cao"].Favorability = 90; // 确保有清廉近臣出列弹劾。
        int initialZhaoZhongFavor = state.Npcs["zhao_zhong"].Favorability;
        int initialZhaoZhongPower = state.Npcs["zhao_zhong"].Power;
        int initialHeJinFavor = state.Npcs["he_jin"].Favorability;
        int initialHeJinPower = state.Npcs["he_jin"].Power;
        int initialImperialPower = state.ImperialPower;

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        var preview = engine.PreviewConfiscationRelationBacklashes("zhang_rang");

        Assert.Contains(preview, r => r.NpcId == "zhao_zhong" && r.FavorabilityDelta < 0);
        Assert.Contains(preview, r => r.NpcId == "he_jin" && r.FavorabilityDelta > 0);

        var result = engine.ExecuteConfiscationAction("zhang_rang", "国库");

        Assert.True(state.Npcs["zhao_zhong"].Favorability < initialZhaoZhongFavor);
        Assert.True(state.Npcs["zhao_zhong"].Power > initialZhaoZhongPower);
        Assert.True(state.Npcs["he_jin"].Favorability > initialHeJinFavor);
        Assert.True(state.Npcs["he_jin"].Power > initialHeJinPower);
        Assert.True(state.ImperialPower < initialImperialPower - 15); // 原本张让强权反噬 -15，关系网会进一步加压。
        Assert.Contains("关系牵连", result.StoryText);
        Assert.Contains("赵忠", result.StoryText);
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
        // 关系网会额外加压：十常侍同党被牵连，曹操虽老谋深算，皇权仍多损 6 点。
        state.Npcs["liu_bei"].Favorability = 30; // 降至门槛以下，确保曹操当选
        state.Npcs["cao_cao"].Favorability = 90;
        int initialPower = state.ImperialPower;
        engine.ExecuteConfiscationAction("zhang_rang", "国库");
        Assert.Equal(initialPower - 16, state.ImperialPower);

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
        int initialQiaoXuanFavor = state.Npcs["qiao_xuan"].Favorability;
        int initialQiaoXuanPower = state.Npcs["qiao_xuan"].Power;

        var result = engine.ResolveEdictAction("merit_cao_cao", 1);

        // 断言：曹操成功连升 2 级，TitleTier 变为 1 + 2 = 3
        Assert.Equal(initialCaoCaoTier + 2, state.Npcs["cao_cao"].TitleTier);
        // 断言：因连跃 2 级，触发朝野反噬，皇权暴跌 5 * (2 - 1) = 5 点
        Assert.Equal(initialImperialPower - 5, state.ImperialPower);
        Assert.True(state.Npcs["qiao_xuan"].Favorability > initialQiaoXuanFavor);
        Assert.Equal(initialQiaoXuanPower, state.Npcs["qiao_xuan"].Power);
        Assert.Contains("跨级拔擢反噬", result.StoryText);
        Assert.Contains("关系牵连", result.StoryText);
        Assert.Contains("桥玄", result.StoryText);
        Assert.Empty(state.ActiveEdicts); // 批阅完后自动出栈移除
    }

    [Fact]
    public async Task Test_Edicts_ExpiryCrisis()
    {
        // Arrange
        var state = new GameState();
        // 关掉 P0-3 黄巾硬 trigger，避免 184/4/2 旬 -15 民心把"折子过期 -15"淹没
        state.DisableHistoricalTriggers = true;
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

    // === P0-3 黄巾起义历史硬 trigger ===

    [Fact]
    public async Task Test_P03_YellowTurban_HardTrigger_At184_4_2_FiresForThreeProvinces()
    {
        // Arrange: 默认开局就是 184/4/1，跑 1 旬进入 184/4/2 → 触发硬 trigger
        var state = new GameState();
        // 把 3 郡的 LocalSupport 都拉满，验证硬 trigger 仍然能无视 LocalSupport
        state.Provinces["jizhou"].LocalSupport = 100;
        state.Provinces["yanzhou"].LocalSupport = 100;
        state.Provinces["yuzhou"].LocalSupport = 100;

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert: 3 郡都已反，太守被撤
        Assert.Equal(184, state.Year);
        Assert.Equal(4, state.Month);
        Assert.Equal(2, state.Xun);
        Assert.True(state.Provinces["jizhou"].IsRebelling, "冀州必须被硬 trigger");
        Assert.True(state.Provinces["yanzhou"].IsRebelling, "兖州必须被硬 trigger");
        Assert.True(state.Provinces["yuzhou"].IsRebelling, "豫州必须被硬 trigger");
        Assert.Null(state.Provinces["jizhou"].GovernorId);  // 冀州太守桥玄必须被撤
        Assert.Null(state.Provinces["yuzhou"].GovernorId);   // 豫州太守卢植必须被撤
    }

    [Fact]
    public async Task Test_P03_YellowTurban_NoTriggerOutside184_4_2()
    {
        // Arrange: 把时间直接放到 184/4/3，跨过 184/4/2 之后
        var state = new GameState { Year = 184, Month = 4, Xun = 3 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act: 推到 184/5/1（Xun 1），硬 trigger 条件（184/4/2）不再满足
        await engine.NextXunAsync();

        // Assert: 此时冀州未反（虽然 3 旬过去但 CheckRebellions 因 LocalSupport=28 + 随机因子也不一定反，
        // 但至少 Historical trigger 没在 184/5/1 重放）
        // 我们不严格断言 IsRebelling=false（因为 CheckRebellions 可能已随机起事），
        // 只断言 Chroncile 里没有"黄巾起事"硬 trigger 文案
        bool hasHardTriggerLog = state.Chronicle.Exists(c => c.Contains("太平道蜂起响应"));
        Assert.False(hasHardTriggerLog, "硬 trigger 只在 184/4/2 触发，不应在 184/5/1 重放");
    }

    [Fact]
    public async Task Test_P03_YellowTurban_AlreadyRebelling_SkipsCleanly()
    {
        // Arrange: 把冀州预设为已反，验证硬 trigger 不会重复跑 / 不会崩
        var state = new GameState();
        state.Provinces["jizhou"].IsRebelling = true;
        state.Provinces["jizhou"].RebelFaction = "黄巾军";
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act & Assert: 不应抛
        await engine.NextXunAsync();
        Assert.True(state.Provinces["jizhou"].IsRebelling);
        // 兖/豫仍应被硬 trigger
        Assert.True(state.Provinces["yanzhou"].IsRebelling);
        Assert.True(state.Provinces["yuzhou"].IsRebelling);
    }

    // === P0-2 结局系统 ===

    [Fact]
    public void Test_P02_Outcome_DefaultIsPlaying()
    {
        var state = new GameState();
        Assert.Equal(GameOutcome.Playing, state.Outcome);
        Assert.Equal(28, state.GetEmperorAge()); // 184 - 156 = 28
    }

    [Fact]
    public async Task Test_P02_Outcome_Collapse_WhenHealthZero()
    {
        // Arrange: 把 Health 设到 0，跑一旬看 outcome
        var state = new GameState { Health = 1 };  // 1 → 期望 NextXunAsync 内部扣减或维持 → 0
        // 但 UpdateOutcome 优先级是 <= 0 ⇒ Collapse；为确定性，预先设 0
        state.Health = 0;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.Equal(GameOutcome.Collapse, state.Outcome);
        Assert.Contains("崩殂", engine.GetOutcomeMessage());
    }

    [Fact]
    public async Task Test_P02_Outcome_Vanquished_WhenSupportFive()
    {
        // Arrange
        var state = new GameState { PopularSupport = 5 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert: 崩殂优先级低于亡国吗？让我们看清楚 — 我设定 Health=35 默认，PopularSupport=5
        // 优先级：Health <= 0 > PopularSupport <= 5，所以应该 Vanquished
        Assert.Equal(GameOutcome.Vanquished, state.Outcome);
        Assert.Contains("亡国", engine.GetOutcomeMessage());
    }

    [Fact]
    public async Task Test_P02_Outcome_ZhongXing_WhenAllConditionsMet()
    {
        // Arrange: 直接跳到 灵帝 40 岁那年，皇权 60+ 民心 50+ 且无叛郡
        var state = new GameState
        {
            Year = 196,        // 196 - 156 = 40
            Month = 4,
            Xun = 1,
            ImperialPower = 70,
            PopularSupport = 60,
            Health = 80
        };
        // 确保无叛郡（默认就是空的，但显式断言）
        foreach (var p in state.Provinces.Values) p.IsRebelling = false;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.Equal(GameOutcome.ZhongXing, state.Outcome);
        Assert.Contains("中兴", engine.GetOutcomeMessage());
    }

    [Fact]
    public async Task Test_P02_Outcome_XuMing_WhenAgeMetButOtherConditionsFail()
    {
        // Arrange: 灵帝 40 岁，但皇权 < 60 → 续命
        var state = new GameState
        {
            Year = 196,        // 40 岁
            Month = 4,
            Xun = 1,
            ImperialPower = 30,   // < 60
            PopularSupport = 60,
            Health = 80
        };
        foreach (var p in state.Provinces.Values) p.IsRebelling = false;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.Equal(GameOutcome.XuMing, state.Outcome);
        Assert.Contains("续命", engine.GetOutcomeMessage());
    }

    [Fact]
    public async Task Test_P02_Outcome_StaysPlaying_WhenAgeBelow40()
    {
        // Arrange: 灵帝 30 岁，皇权拉满也无中兴
        var state = new GameState
        {
            Year = 186,        // 30 岁
            ImperialPower = 100,
            PopularSupport = 100,
            Health = 100
        };
        foreach (var p in state.Provinces.Values) p.IsRebelling = false;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.Equal(GameOutcome.Playing, state.Outcome);
    }

    [Fact]
    public async Task Test_P02_Outcome_OnceLocked_DoesNotRevert()
    {
        // Arrange: 先跑出 Collapse
        var state = new GameState { Health = 0 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act 1: 锁定为 Collapse
        await engine.NextXunAsync();
        Assert.Equal(GameOutcome.Collapse, state.Outcome);

        // 把 Health 拉回 100，再跑一旬 — 结局应保持 Collapse（不可逆）
        state.Health = 100;
        state.PopularSupport = 100;
        await engine.NextXunAsync();

        // Assert
        Assert.Equal(GameOutcome.Collapse, state.Outcome);
    }

    // === P1-A2 189 年历史 trigger ===

    [Fact]
    public async Task Test_A2_HeJinDeath_At189_8_3_FiresOnce()
    {
        // Arrange: 跳到 189/8/2，跑 1 旬进入 189/8/3 触发何进之死
        var state = new GameState { Year = 189, Month = 8, Xun = 2 };
        Assert.True(state.Npcs["he_jin"].IsActive, "何进开局应活跃");

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.False(state.Npcs["he_jin"].IsActive, "何进 189/8/3 必须 IsActive=false");
        Assert.Contains("何进", state.Npcs["he_jin"].DeathReason);
        Assert.Contains(state.Chronicle, e => e.Contains("外戚崩殂"));
    }

    [Fact]
    public async Task Test_A2_DongZhuoEntry_At189_9_1_DeploysNpc()
    {
        // Arrange: 玩家在 8/3，调用 NextXunAsync → 进入 9/1 触发董卓入京
        // （Trigger 在 Xun++ 之后检查，所以"189/9/1 触发"= 玩家从 8/3 推进到 9/1）
        var state = new GameState { Year = 189, Month = 8, Xun = 3 };
        Assert.False(state.Npcs.ContainsKey("dong_zhuo"), "董卓开局应不在朝堂");

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        await engine.NextXunAsync();

        // Assert
        Assert.True(state.Npcs["dong_zhuo"].IsActive, "189/9/1 董卓必须被部署");
        var dong = state.Npcs["dong_zhuo"];
        Assert.True(dong.IsActive, "董卓入京后必须 IsActive");
        Assert.Equal(95, dong.Power);
        Assert.Equal("洛阳宫中", dong.InitialLocation);
        Assert.Contains(state.Chronicle, e => e.Contains("董卓入京"));
    }

    [Fact]
    public async Task Test_A2_HeJinDeath_SecondCall_Idempotent()
    {
        // 何进已亡后，再跑 1 旬进 9/1（董卓进京），不应再触发何进之死
        var state = new GameState { Year = 189, Month = 8, Xun = 2 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        await engine.NextXunAsync(); // → 189/8/3 触发何进之死
        Assert.False(state.Npcs["he_jin"].IsActive);
        Assert.True(state.Chronicle.Any(e => e.Contains("外戚崩殂")));

        int heJinChronicleCount = state.Chronicle.Count(e => e.Contains("外戚崩殂"));

        // 继续推进 1 旬 → 9/1（触发董卓进京，但不应再触发何进）
        await engine.NextXunAsync();

        Assert.False(state.Npcs["he_jin"].IsActive, "何进保持已亡");
        Assert.Equal(heJinChronicleCount, state.Chronicle.Count(e => e.Contains("外戚崩殂")));
    }

    [Fact]
    public async Task Test_A2_DongZhuoEntry_SecondCall_Idempotent()
    {
        // 玩家从 8/3 推进到 9/1，触发董卓入京
        var state = new GameState { Year = 189, Month = 8, Xun = 3 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        await engine.NextXunAsync(); // → 9/1 触发董卓入京
        Assert.True(state.Npcs.ContainsKey("dong_zhuo"));
        int powerAfterFirstEntry = state.ImperialPower;
        var chronicleLenAfterFirst = state.Chronicle.Count;

        // 再跑 1 旬 → 9/2（不触发董卓进京幂等检查）
        await engine.NextXunAsync();

        Assert.Equal(powerAfterFirstEntry, state.ImperialPower);
        Assert.DoesNotContain(state.Chronicle.GetRange(chronicleLenAfterFirst, state.Chronicle.Count - chronicleLenAfterFirst),
            e => e.Contains("董卓入京"));
    }

    // === P1-A3 NPC 寿终下野 ===

    [Fact]
    public async Task Test_A3_NpcHistoricalDeath_Retire()
    {
        // Arrange: 把何进 HistoricalDeathYear 改到 184/3/2 之前，让他在 184/3/2 → 3 时被自动下野
        var state = new GameState { Year = 184, Month = 3, Xun = 2 };
        // 何进 HistoricalDeathYear 默认 184+59=243，远大于 184，所以不会触发。
        // 我们把一个测试 NPC 的 HistoricalDeathYear 改到 184 年。
        var testNpc = state.Npcs["he_jin"];
        testNpc.HistoricalDeathYear = 184; // 184 年内
        // 让他不归 he_jin 逻辑（A2 还没触发），保持 IsActive=true
        Assert.True(testNpc.IsActive);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        // 当前 184/3/2，下一旬 = 184/3/3，何进 HistoricalDeathYear=184 <= 184 → 触发下野
        await engine.NextXunAsync();

        Assert.False(testNpc.IsActive, "何进 HistoricalDeathYear=184 时，到 184 年应自动下野");
        Assert.Contains("寿终", testNpc.DeathReason);
    }

    [Fact]
    public async Task Test_A3_NpcHistoricalDeath_SkipsEventTriggeredNpcs()
    {
        // 董卓是"事件触发"型，HistoricalDeathYear 不应让他自动下野
        // 玩家先跑到 9/1 部署董卓，然后设 HistoricalDeathYear=189，
        // 再跑 1 旬（仍 189 年），董卓不应被自动下野（A2 trigger 之外的逻辑）
        var state = new GameState { Year = 189, Month = 8, Xun = 3 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        await engine.NextXunAsync(); // → 9/1 部署董卓
        Assert.True(state.Npcs["dong_zhuo"].IsActive);
        // 篡改董卓的死亡年为今年（A2 已 done），但因 EntryCondition=="事件触发" 应被跳过
        state.Npcs["dong_zhuo"].HistoricalDeathYear = 189;

        await engine.NextXunAsync(); // → 9/2，仍在 189 年

        Assert.True(state.Npcs["dong_zhuo"].IsActive, "事件触发型 NPC 不应被寿终逻辑自动下野");
    }

    [Fact]
    public async Task Test_A3_NpcHistoricalDeath_ReleasesGovernorProvince()
    {
        // 安排一个州郡的太守寿终，应自动释放 GovernorId
        var state = new GameState { Year = 184, Month = 3, Xun = 2 };
        var gov = state.Npcs["he_jin"]; // 大将军何进
        gov.HistoricalDeathYear = 184;
        // 给何进指派一个州郡
        var prov = state.Provinces["yuzhou"];
        prov.GovernorId = "he_jin";
        gov.GovernedProvinceId = "yuzhou";

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        await engine.NextXunAsync();

        Assert.False(gov.IsActive);
        Assert.True(gov.GovernedProvinceId == null, "太守寿终后应清空 GovernedProvinceId");
        Assert.True(prov.GovernorId == null, "州郡 GovernorId 应被释放");
    }

    // P2-5 守住 FactionStance 矩阵不漂移：现有 4 硬编码 NPC × 7 Intent 的 Stance 行为应严格保留
    [Fact]
    public void FactionStance_矩阵与现有硬编码一致()
    {
        // 赈灾：清流派(曹操)=AGREED, 阉党派(张让)=OPPOSE
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.PureStream, CourtIntent.Relief));
        Assert.Equal("OPPOSE", FactionStance.GetStance(FactionCatalog.EunuchFaction, CourtIntent.Relief));
        // 诛杀：清流派=AGREED
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.PureStream, CourtIntent.Execute));
        // 奖赏：清流派=AGREED, 外戚派(何进)=AGREED
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.PureStream, CourtIntent.Reward));
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.ImperialClan, CourtIntent.Reward));
        // 国帑：阉党派=AGREED, 外戚派=OPPOSE
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.EunuchFaction, CourtIntent.Treasury));
        Assert.Equal("OPPOSE", FactionStance.GetStance(FactionCatalog.ImperialClan, CourtIntent.Treasury));
        // 整军：外戚派=AGREED, 西园亲军(蹇硕)=AGREED
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.ImperialClan, CourtIntent.MilitaryBuild));
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.WesternGarden, CourtIntent.MilitaryBuild));
        // 整饬宦官：外戚派=AGREED, 阉党派=OPPOSE
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.ImperialClan, CourtIntent.EunuchReform));
        Assert.Equal("OPPOSE", FactionStance.GetStance(FactionCatalog.EunuchFaction, CourtIntent.EunuchReform));
        // 举荐：清流派=AGREED, 西园亲军=AGREED
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.PureStream, CourtIntent.Talent));
        Assert.Equal("AGREED", FactionStance.GetStance(FactionCatalog.WesternGarden, CourtIntent.Talent));

        // 矩阵未命中的派系×意图组合应返回 null（不主动表态）
        Assert.Null(FactionStance.GetStance(FactionCatalog.PureStream, CourtIntent.MilitaryBuild));
        Assert.Null(FactionStance.GetStance(FactionCatalog.WesternGarden, CourtIntent.Relief));
        Assert.Null(FactionStance.GetStance(FactionCatalog.Warlord, CourtIntent.Relief));
    }
}