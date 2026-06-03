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
        state.ActiveEdicts.Add("【冀州急折】：冀州干旱，求赐赈米。");
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
        Assert.Contains("【冀州急折】：冀州干旱", state.ActiveEdicts[0]);
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
            BirthYear = 161, BaseLongevity = 62, Traits = new() { "经天纬地" },
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
        state.Npcs["cao_cao"].Favorability = 90;
        int initialPower = state.ImperialPower;
        engine.ExecuteConfiscationAction("zhang_rang", "国库");
        Assert.Equal(initialPower - 10, state.ImperialPower);

        // 5. 验证 Traits 累乘共存：刘备同时拥有 [经天纬地] 1.20x 与 [爱民如子] 1.15x
        // 累计系数：1.20 * 1.15 = 1.38x
        liuBei.Traits.Add("爱民如子");
        int secondarySupport = state.PopularSupport;
        // 再次赈灾 1000万，基础 supportDelta = 12 * (1000 / 1000) = 12点
        // 复合提振：12 * 1.38 = 16.56 -> 16 点提振
        engine.ExecuteDisasterReliefAction(1000, "liu_bei");
        Assert.Equal(secondarySupport + 16, state.PopularSupport);

        // 6. 验证调度师手动调用管理 NPC 的衰老机制（每年一月上旬触发）
        state.Month = 1;
        state.Xun = 1;
        state.Npcs["he_jin"].BirthYear = 100; // 84岁，超过期望寿命 65
        
        await manager.ProcessLifecycleStepAsync(state, false);
        // 刘备依然健康存活
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
        Assert.Contains("孔武有力", dongZhuo.Traits);
        Assert.Equal(100, dongZhuo.Health); // 初始健康的满额 100 状态
    }
}