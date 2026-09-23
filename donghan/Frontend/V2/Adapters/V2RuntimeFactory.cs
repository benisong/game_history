using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public static class V2RuntimeFactory
{
    public static V2Runtime CreateDefault() => CreateDefault(new GameState(), null);

    public static V2Runtime CreateDefault(GameState state) => CreateDefault(state, null);

    public static V2Runtime Create(GameEngine engine)
    {
        var stateReader = new GameEngineStateReader(engine);
        return new V2Runtime(
            stateReader,
            new GameEngineTravelService(engine, engine),
            new GameEngineWestGardenService(engine, engine, engine),
            new GameEngineIntelService(engine, stateReader),
            new GameEngineCourtService(engine),
            new GameEngineTurnService(engine, engine, stateReader),
            new GameEngineEdictService(engine, engine),
            new GameEngineSpecialActionService(engine, engine, engine, engine, engine),
            new GameEngineNominationUiService(engine),
            new GameEngineOfficialRankUiService(engine),
            new GameEngineAgriculturalPolicyUiService(engine, engine));
    }

    public static V2Runtime CreateDefault(GameState state, System.Random? rng)
    {
        var engine = new GameEngine(
            state,
            new MockScheduler(),
            new MockOracle(),
            new MockMinisterAgent(),
            new MockNarrator(),
            rng);

        return Create(engine);
    }
}
