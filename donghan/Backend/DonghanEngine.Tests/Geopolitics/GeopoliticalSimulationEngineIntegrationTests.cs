using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.MathEngine;
using DonghanEngine.Core.Geopolitics.Models;
using Xunit;

namespace DonghanEngine.Tests.Geopolitics;

public class GeopoliticalSimulationEngineIntegrationTests
{
    [Fact]
    public void Test_SimulationEngine_InitializesWithSevenHistoricalFactions()
    {
        var engine = new GeopoliticalSimulationEngine();

        Assert.Equal(7, engine.Factions.Count);
        Assert.True(engine.Factions.ContainsKey("cao_cao"));
        Assert.True(engine.Factions.ContainsKey("yuan_shao"));
        Assert.True(engine.Factions.ContainsKey("sun_jian"));
        Assert.True(engine.Factions.ContainsKey("huangfu_song"));
        Assert.True(engine.Factions.ContainsKey("liu_yu"));
        Assert.True(engine.Factions.ContainsKey("liu_biao"));
        Assert.True(engine.Factions.ContainsKey("liu_yan"));
    }

    [Fact]
    public void Test_SimulationEngine_TickTurn_ProcessesLoyalTributeAndGeneratesMemorials()
    {
        var seededRand = new SeededRandomProvider();
        // 设置随机数为 99（确保枭雄不发起随机进攻，仅测试忠臣纳贡）
        for (int i = 0; i < 20; i++) seededRand.EnqueuePercentage(99);

        var engine = new GeopoliticalSimulationEngine(randomProvider: seededRand);
        var state = new GameState { Treasury = 5000 };

        var result = engine.TickTurn(state);

        // 忠臣皇甫嵩、刘虞及自守宗室应纳贡入库
        Assert.True(result.TotalTributeGoldCollected > 0);
        Assert.True(state.Treasury > 5000);
        Assert.Contains(result.GeneratedMemorials, m => m.MemorialType == GeopoliticalMemorialType.TributeOffered);
    }

    [Fact]
    public void Test_SimulationEngine_TickTurn_LaunchesCampaignAndGeneratesPetitionOnVictory()
    {
        var seededRand = new SeededRandomProvider();
        // 设置随机数为 0（枭雄必命中出兵概率）
        for (int i = 0; i < 20; i++) seededRand.EnqueuePercentage(0);

        var engine = new GeopoliticalSimulationEngine(randomProvider: seededRand);
        var state = new GameState();

        var result = engine.TickTurn(state);

        // 验证有枭雄发动战役决策
        Assert.Contains(result.DecisionsMade, d => d.ActionType == WarlordActionType.LaunchCampaign);
        // 验证战役产生战报或讨封奏折
        Assert.True(result.CampaignResults.Count > 0 || result.GeneratedMemorials.Count > 0);
    }
}
