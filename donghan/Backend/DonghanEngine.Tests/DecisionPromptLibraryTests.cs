using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class DecisionPromptLibraryTests
{
    private readonly IDecisionPromptLibraryService _library = new DecisionPromptLibraryService();

    [Fact]
    public void Test_ClassifyNpcArchetype_PureStream_And_Eunuchs()
    {
        var scholar = new NpcState { Name = "卢植", Faction = FactionCatalog.PureStream };
        var eunuch = new NpcState { Name = "赵忠", Faction = FactionCatalog.EunuchFaction };
        var consort = new NpcState { Name = "何进", Faction = FactionCatalog.ImperialClan };
        var warlord = new NpcState { Name = "董卓", Faction = FactionCatalog.Warlord, Ambition = 80 };

        Assert.Equal(NpcArchetype.PureStreamScholar, _library.ClassifyNpcArchetype(scholar));
        Assert.Equal(NpcArchetype.CourtEunuch, _library.ClassifyNpcArchetype(eunuch));
        Assert.Equal(NpcArchetype.ImperialConsortLord, _library.ClassifyNpcArchetype(consort));
        Assert.Equal(NpcArchetype.AmbitiousWarlord, _library.ClassifyNpcArchetype(warlord));
    }

    [Fact]
    public void Test_MatchConsequences_ConfiscatePureStream_WarnsBacklash()
    {
        var scholar = new NpcState { Name = "蔡邕", Faction = FactionCatalog.PureStream };
        var prompt = _library.MatchConsequences(DecisionActionType.ConfiscateToTreasury, scholar);

        Assert.Contains("蔡邕", prompt.ActionTitle);
        Assert.Contains(prompt.ConsequenceBullets, b => b.Contains("清流反感") || b.Contains("清流名士"));
        Assert.Contains(prompt.ConsequenceBullets, b => b.Contains("国库"));
    }

    [Fact]
    public void Test_MatchConsequences_AppointGovernor_WarnsAmbitiousWarlord()
    {
        var warlord = new NpcState { Name = "袁绍", Ambition = 75 };
        var prompt = _library.MatchConsequences(DecisionActionType.AppointGovernor, warlord, targetLocation: "冀州");

        Assert.Contains("冀州", prompt.NarrativeDescription);
        Assert.Contains("袁绍", prompt.ActionTitle);
        Assert.Contains(prompt.ConsequenceBullets, b => b.Contains("割据自雄之风险"));
    }

    [Fact]
    public void Test_GameEngine_MatchDecisionPrompt_DelegatesCorrectly()
    {
        var state = new GameState();
        var scholar = new NpcState { Id = "lu_zhi", Name = "卢植", Faction = FactionCatalog.PureStream };
        state.RegisterNpc(scholar);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var prompt = engine.MatchDecisionPrompt(DecisionActionType.ConfiscateToTreasury, "lu_zhi");
        Assert.Contains("卢植", prompt.ActionTitle);
        Assert.Contains(prompt.ConsequenceBullets, b => b.Contains("清流反感") || b.Contains("清流名士"));
    }
}
