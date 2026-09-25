using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DonghanEngine.Core.Health;

namespace DonghanEngine.Core.Persistence;

/// <summary>
/// 纯领域持久化引擎：安全写入（原子写入/防断电损坏）、校验与多槽位起居注管理（单一职责）
/// </summary>
public sealed class SaveGameService : ISaveGameService
{
    private readonly string _saveDirectory;
    private readonly IImperialHealthService _healthService;
    public const int AutoSaveSlotIndex = 0;
    public const int MaxManualSlots = 6;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SaveGameService(
        string? customSaveDirectory = null,
        IImperialHealthService? healthService = null)
    {
        _saveDirectory = customSaveDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".donghan_saves");

        _healthService = healthService ?? new ImperialHealthService();

        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }

    private string GetSaveFilePath(int slotIndex) =>
        Path.Combine(_saveDirectory, slotIndex == AutoSaveSlotIndex ? "autosave.json" : $"save_slot_{slotIndex}.json");

    private string GetTempSaveFilePath(int slotIndex) =>
        Path.Combine(_saveDirectory, slotIndex == AutoSaveSlotIndex ? "autosave.json.tmp" : $"save_slot_{slotIndex}.json.tmp");

    public SaveOperationResult SaveGame(GameState state, int slotIndex, string? customSaveName = null)
    {
        if (state == null)
        {
            return new SaveOperationResult(false, slotIndex, "无法保存空状态！", ErrorCode: "NullState");
        }

        try
        {
            var diag = _healthService.GetPhysicianDiagnosis(state);
            bool isAuto = slotIndex == AutoSaveSlotIndex;
            string dateText = $"{state.Year}年{state.Month}月{(state.Xun == 1 ? "上旬" : state.Xun == 2 ? "中旬" : "下旬")}";
            string name = customSaveName ?? (isAuto ? $"【自动起居注】{dateText}" : $"【手动起居注】{dateText}");

            var metadata = new SaveSlotMetadata(
                SlotIndex: slotIndex,
                SaveName: name,
                IsAutoSave: isAuto,
                Exists: true,
                Year: state.Year,
                Month: state.Month,
                Xun: state.Xun,
                CurrentDateText: dateText,
                ImperialPower: state.ImperialPower,
                PopularSupport: state.PopularSupport,
                Treasury: state.Treasury,
                MentalStateText: diag.MentalStateDescription,
                Timestamp: DateTime.Now,
                SummaryPreview: $"皇权:{state.ImperialPower} 民心:{state.PopularSupport} 国库:{state.Treasury}万贯 精神:{diag.MentalStateDescription}");

            var package = new SaveGamePackage(metadata, state);
            string json = JsonSerializer.Serialize(package, JsonOptions);

            string targetPath = GetSaveFilePath(slotIndex);
            string tempPath = GetTempSaveFilePath(slotIndex);

            // 原子写入机制：先写入临时文件，刷盘完毕后再覆盖原文件，防止断电导致文件损坏半截
            File.WriteAllText(tempPath, json);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(tempPath, targetPath);

            return new SaveOperationResult(true, slotIndex, $"起居注已成功封存入槽位【{(isAuto ? "自动存档" : $"槽位{slotIndex}")}】！", metadata);
        }
        catch (Exception ex)
        {
            return new SaveOperationResult(false, slotIndex, $"封存起居注异常: {ex.Message}", ErrorCode: "SaveException");
        }
    }

    public SaveOperationResult LoadGame(int slotIndex)
    {
        string path = GetSaveFilePath(slotIndex);
        if (!File.Exists(path))
        {
            return new SaveOperationResult(false, slotIndex, $"起居注槽位【{slotIndex}】尚无记录！", ErrorCode: "FileNotFound");
        }

        try
        {
            string json = File.ReadAllText(path);
            var package = JsonSerializer.Deserialize<SaveGamePackage>(json, JsonOptions);

            if (package == null || package.State == null)
            {
                return new SaveOperationResult(false, slotIndex, "起居注记录损坏，未能成功恢复！", ErrorCode: "CorruptedSave");
            }

            return new SaveOperationResult(
                Success: true,
                SlotIndex: slotIndex,
                Message: $"已成功展开【{package.Metadata.SaveName}】历史卷轴！",
                Metadata: package.Metadata,
                LoadedState: package.State);
        }
        catch (Exception ex)
        {
            return new SaveOperationResult(false, slotIndex, $"展开起居注失败: {ex.Message}", ErrorCode: "LoadException");
        }
    }

    public SaveOperationResult ExecuteAutoSave(GameState state, string triggerReason = "每旬例行起居注")
    {
        string dateText = $"{state.Year}年{state.Month}月{(state.Xun == 1 ? "上旬" : state.Xun == 2 ? "中旬" : "下旬")}";
        string autoName = $"【自动起居注】{dateText} ({triggerReason})";
        return SaveGame(state, AutoSaveSlotIndex, autoName);
    }

    public IReadOnlyList<SaveSlotMetadata> ListAllSlots()
    {
        var list = new List<SaveSlotMetadata>();

        // 1. 自动存档槽位 (Slot 0)
        list.Add(ReadMetadataOnly(AutoSaveSlotIndex, true));

        // 2. 手动存档槽位 (Slot 1 ~ 6)
        for (int i = 1; i <= MaxManualSlots; i++)
        {
            list.Add(ReadMetadataOnly(i, false));
        }

        return list.AsReadOnly();
    }

    private SaveSlotMetadata ReadMetadataOnly(int slotIndex, bool isAuto)
    {
        string path = GetSaveFilePath(slotIndex);
        if (!File.Exists(path))
        {
            return new SaveSlotMetadata(
                SlotIndex: slotIndex,
                SaveName: isAuto ? "【自动起居注】（空）" : $"【起居注 槽位 {slotIndex}】（虚位以待）",
                IsAutoSave: isAuto,
                Exists: false,
                Year: 184,
                Month: 1,
                Xun: 1,
                CurrentDateText: "无记录",
                ImperialPower: 0,
                PopularSupport: 0,
                Treasury: 0,
                MentalStateText: "无",
                Timestamp: DateTime.MinValue,
                SummaryPreview: "暂无封存历史卷轴");
        }

        try
        {
            string json = File.ReadAllText(path);
            var package = JsonSerializer.Deserialize<SaveGamePackage>(json, JsonOptions);
            if (package?.Metadata != null)
            {
                return package.Metadata;
            }
        }
        catch
        {
            // 容错降级
        }

        return new SaveSlotMetadata(
            SlotIndex: slotIndex,
            SaveName: isAuto ? "【自动起居注】（记录损坏）" : $"【槽位 {slotIndex}】（记录损坏）",
            IsAutoSave: isAuto,
            Exists: true,
            Year: 184,
            Month: 1,
            Xun: 1,
            CurrentDateText: "损坏",
            ImperialPower: 0,
            PopularSupport: 0,
            Treasury: 0,
            MentalStateText: "未知",
            Timestamp: File.GetLastWriteTime(path),
            SummaryPreview: "文件损坏，无法读取");
    }

    public bool DeleteSave(int slotIndex)
    {
        try
        {
            string path = GetSaveFilePath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
    }

    public bool HasAutoSave()
    {
        string path = GetSaveFilePath(AutoSaveSlotIndex);
        return File.Exists(path);
    }
}
