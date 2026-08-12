using System;
using System.Threading.Tasks;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineTurnService : ITurnService
{
    private readonly GameEngine _engine;
    private readonly IGameStateReader _state;

    public GameEngineTurnService(GameEngine engine, IGameStateReader state)
    {
        _engine = engine;
        _state = state;
    }

    public async Task<TurnAdvanceResult> AdvanceXunAsync()
    {
        try
        {
            await _engine.NextXunAsync();
            return new TurnAdvanceResult(true, _state.GetSnapshot(), Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new TurnAdvanceResult(false, _state.GetSnapshot(), Array.Empty<string>(), ex.GetType().Name, ex.Message);
        }
    }

    public async Task<FastForwardResult> FastForwardAsync(FastForwardCommand command)
    {
        if (command.XunCount is < 1 or > 30)
        {
            return new FastForwardResult(false, command.XunCount, 0, _state.GetSnapshot(), Array.Empty<string>(), true, "旬数必须在 1 到 30 之间。", "invalid_xun_count");
        }

        int advanced = 0;
        try
        {
            for (; advanced < command.XunCount; advanced++)
            {
                await _engine.NextXunAsync();
                if (_engine.GetState().Outcome != GameOutcome.Playing)
                {
                    return new FastForwardResult(true, command.XunCount, advanced + 1, _state.GetSnapshot(), Array.Empty<string>(), true, "游戏结局已确定。");
                }
            }

            return new FastForwardResult(true, command.XunCount, advanced, _state.GetSnapshot(), Array.Empty<string>(), false);
        }
        catch (Exception ex)
        {
            return new FastForwardResult(false, command.XunCount, advanced, _state.GetSnapshot(), Array.Empty<string>(), true, ex.Message, ex.GetType().Name);
        }
    }
}
