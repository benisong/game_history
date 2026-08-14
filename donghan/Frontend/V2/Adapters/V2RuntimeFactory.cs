using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public static class V2RuntimeFactory
{
    public static V2Runtime CreateDefault()
    {
        var state = new GameState();
        var engine = new GameEngine(
            state,
            new MockScheduler(),
            new MockOracle(),
            new MockMinisterAgent(),
            new MockNarrator());

        var stateReader = new GameEngineStateReader(engine);
        return new V2Runtime(
            stateReader,
            new GameEngineTravelService(engine, stateReader),
            new GameEngineWestGardenService(engine),
            new GameEngineIntelService(engine, stateReader),
            new GameEngineCourtService(engine),
            new GameEngineTurnService(engine, stateReader),
            new GameEngineEdictService(engine));
    }
}
