using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Persistence;

/// <summary>
/// 存档槽位元数据摘要（用于 UI 列表快速展示，无需加载完整 GameState）
/// </summary>
public sealed record SaveSlotMetadata(
    int SlotIndex,
    string SaveName,
    bool IsAutoSave,
    bool Exists,
    int Year,
    int Month,
    int Xun,
    string CurrentDateText,
    int ImperialPower,
    int PopularSupport,
    int Treasury,
    string MentalStateText,
    DateTime Timestamp,
    string SummaryPreview);

/// <summary>
/// 完整存档封包（包含元数据头与完整 GameState 状态图）
/// </summary>
public sealed record SaveGamePackage(
    SaveSlotMetadata Metadata,
    GameState State,
    string EngineVersion = "3.0.0");

/// <summary>
/// 存档/读档操作结果响应
/// </summary>
public sealed record SaveOperationResult(
    bool Success,
    int SlotIndex,
    string Message,
    SaveSlotMetadata? Metadata = null,
    GameState? LoadedState = null,
    string? ErrorCode = null);
