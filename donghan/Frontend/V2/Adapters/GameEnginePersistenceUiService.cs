using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Persistence;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

/// <summary>
/// 纯 UI 适配层：将持久化存档、读档与自动存档包装给前端调用（单一职责）
/// </summary>
public sealed class GameEnginePersistenceUiService : IPersistenceUiService
{
    private readonly IGameEngine _engine;

    public GameEnginePersistenceUiService(IGameEngine engine)
    {
        _engine = engine;
    }

    public SaveOperationResult SaveGame(int slotIndex, string? customSaveName = null)
    {
        return _engine.SaveGame(slotIndex, customSaveName);
    }

    public SaveOperationResult LoadGame(int slotIndex)
    {
        return _engine.LoadGame(slotIndex);
    }

    public SaveOperationResult ExecuteAutoSave(string triggerReason = "每旬例行起居注")
    {
        return _engine.ExecuteAutoSave(triggerReason);
    }

    public IReadOnlyList<SaveSlotMetadata> ListSaveSlots()
    {
        return _engine.ListSaveSlots();
    }

    public bool DeleteSave(int slotIndex)
    {
        return _engine.DeleteSave(slotIndex);
    }

    public bool HasAutoSave()
    {
        return _engine.HasAutoSave();
    }
}
