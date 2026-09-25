using System.Collections.Generic;

namespace DonghanEngine.Core.Persistence;

/// <summary>
/// 纯领域持久化契约：存档、读档、自动存档与起居注管理（单一职责）
/// </summary>
public interface ISaveGameService
{
    /// <summary>
    /// 保存游戏到指定槽位（0 为自动存档 AutoSave，1~9 为手动存档）
    /// </summary>
    SaveOperationResult SaveGame(GameState state, int slotIndex, string? customSaveName = null);

    /// <summary>
    /// 从指定槽位读取完整游戏状态
    /// </summary>
    SaveOperationResult LoadGame(int slotIndex);

    /// <summary>
    /// 触发自动存档（通常在每旬时钟推进、重大历史分歧节点调用）
    /// </summary>
    SaveOperationResult ExecuteAutoSave(GameState state, string triggerReason = "每旬例行起居注");

    /// <summary>
    /// 获取所有支持槽位的摘要列表（包含自动存档 0 与手动存档 1~6）
    /// </summary>
    IReadOnlyList<SaveSlotMetadata> ListAllSlots();

    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    bool DeleteSave(int slotIndex);

    /// <summary>
    /// 检查是否存在有效的自动存档
    /// </summary>
    bool HasAutoSave();
}
