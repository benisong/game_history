using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineEdictService : IEdictService
{
    private readonly GameEngine _engine;

    public GameEngineEdictService(GameEngine engine) => _engine = engine;

    public IReadOnlyList<EdictSnapshot> GetPendingEdicts()
    {
        return _engine.GetState().ActiveEdicts.Select(edict =>
            new EdictSnapshot(
                edict.Id,
                edict.Title,
                edict.Type.ToString(),
                edict.NarrativeContent,
                edict.ExpiryXun,
                edict.Options.Select(option => new EdictOptionSnapshot(option.Description, option.ConsequencePreview)).ToList()))
            .ToList();
    }

    public ActionResult Resolve(ResolveEdictCommand command)
    {
        try
        {
            var result = _engine.ResolveEdictAction(command.EdictId, command.OptionIndex);
            return new ActionResult(true, "尚书台朱批回奏", result.StoryText, ReportKind.Information, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("朱批未成", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
