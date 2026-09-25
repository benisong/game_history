using System.IO;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Persistence;

namespace DonghanEngine.Tests;

public class SaveGamePersistenceTests
{
    private readonly string _tempSaveDir;

    public SaveGamePersistenceTests()
    {
        _tempSaveDir = Path.Combine(Path.GetTempPath(), "donghan_test_saves_" + System.Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public void Test_SaveAndLoadGame_PreservesFullState_AndMentalStateMetadata()
    {
        var saveService = new SaveGameService(_tempSaveDir);
        var state = new GameState
        {
            Year = 189,
            Month = 4,
            Xun = 2,
            ImperialPower = 75,
            PopularSupport = 68,
            Treasury = 12500,
            HiddenCurrentEnergy = 30, // 虚耗神伤
            HiddenYangVitality = 85,
            HiddenMaxEnergy = 95
        };

        // 1. 保存到手动槽位 1
        var saveResult = saveService.SaveGame(state, 1, "【手书】平定西园前夕");
        Assert.True(saveResult.Success);
        Assert.NotNull(saveResult.Metadata);
        Assert.Equal(1, saveResult.Metadata.SlotIndex);
        Assert.Equal(189, saveResult.Metadata.Year);
        Assert.Contains("虚耗神伤", saveResult.Metadata.MentalStateText);

        // 2. 从槽位 1 读档
        var loadResult = saveService.LoadGame(1);
        Assert.True(loadResult.Success);
        Assert.NotNull(loadResult.LoadedState);

        var loaded = loadResult.LoadedState;
        Assert.Equal(189, loaded.Year);
        Assert.Equal(4, loaded.Month);
        Assert.Equal(2, loaded.Xun);
        Assert.Equal(75, loaded.ImperialPower);
        Assert.Equal(68, loaded.PopularSupport);
        Assert.Equal(12500, loaded.Treasury);
        Assert.Equal(30, loaded.HiddenCurrentEnergy);
        Assert.Equal(85, loaded.HiddenYangVitality);
        Assert.Equal(95, loaded.HiddenMaxEnergy);

        // 清理测试目录
        if (Directory.Exists(_tempSaveDir))
        {
            Directory.Delete(_tempSaveDir, true);
        }
    }

    [Fact]
    public void Test_AutoSave_GeneratesSlotZero_AndSupportsFastListing()
    {
        var saveService = new SaveGameService(_tempSaveDir);
        var state = new GameState
        {
            Year = 184,
            Month = 10,
            Xun = 3,
            Treasury = 3000
        };

        // 自动存档
        var autoResult = saveService.ExecuteAutoSave(state, "击破张角战役前");
        Assert.True(autoResult.Success);
        Assert.True(saveService.HasAutoSave());

        // 检查槽位列表
        var allSlots = saveService.ListAllSlots();
        Assert.Equal(7, allSlots.Count); // 1 个自动存档 (Slot 0) + 6 个手动存档 (Slot 1~6)

        var autoSlot = allSlots[0];
        Assert.True(autoSlot.IsAutoSave);
        Assert.True(autoSlot.Exists);
        Assert.Equal(184, autoSlot.Year);
        Assert.Equal(10, autoSlot.Month);
        Assert.Equal(3, autoSlot.Xun);

        // 验证空手动槽位
        var manualSlot1 = allSlots[1];
        Assert.False(manualSlot1.Exists);

        // 清理测试目录
        if (Directory.Exists(_tempSaveDir))
        {
            Directory.Delete(_tempSaveDir, true);
        }
    }

    [Fact]
    public void Test_DeleteSave_RemovesFileGracefully()
    {
        var saveService = new SaveGameService(_tempSaveDir);
        var state = new GameState { Year = 185 };

        saveService.SaveGame(state, 2);
        Assert.True(saveService.LoadGame(2).Success);

        bool deleted = saveService.DeleteSave(2);
        Assert.True(deleted);

        var loadAfterDelete = saveService.LoadGame(2);
        Assert.False(loadAfterDelete.Success);

        // 清理测试目录
        if (Directory.Exists(_tempSaveDir))
        {
            Directory.Delete(_tempSaveDir, true);
        }
    }
}
