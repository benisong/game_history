using System;
using System.Linq;

namespace DonghanEngine.Core;

public partial class GameEngine
{
    partial void CheckRebellions()
    {
        // TODO: Full implementation per design doc
        // This is a placeholder — rebellion system is being implemented iteratively
    }

    /// <summary>Assign an NPC as governor of a province.</summary>
    public TurnResult AssignGovernor(string provinceId, string npcId)
    {
        if (!_state.Provinces.TryGetValue(provinceId, out var province))
            throw new ArgumentException("无此郡县！", nameof(provinceId));
        if (!_state.Npcs.TryGetValue(npcId, out var npc))
            throw new ArgumentException("朝中无此大臣！", nameof(npcId));
        if (npc.GovernedProvinceId != null)
            throw new InvalidOperationException($"{npc.Name}已在{npc.GovernedProvinceId}任职！");

        // Unassign previous governor
        if (province.GovernorId != null && _state.Npcs.TryGetValue(province.GovernorId, out var oldGov))
            oldGov.GovernedProvinceId = null;

        province.GovernorId = npcId;
        npc.GovernedProvinceId = provinceId;
        npc.Power = Math.Clamp(npc.Power - 5, 0, 100);
        province.LocalSupport = Math.Clamp(province.LocalSupport + 10, 0, 100);

        _state.AddToChronicle($"【任命】天子任命【{npc.Name}】为{province.Name}地方官。");
        return new TurnResult
        {
            StoryText = $"【任命地方官】\n\n陛下朱批已下，任命【{npc.Name}】为{province.Name}地方官，即日赴任。\n\n[color=green]● {province.Name}民心：+10[/color]\n[color=yellow]● 【{npc.Name}】权势 -5（远离中央）[/color]"
        };
    }

    /// <summary>Remove a governor from their post, returning them to the capital.</summary>
    public TurnResult RecallGovernor(string provinceId)
    {
        if (!_state.Provinces.TryGetValue(provinceId, out var province))
            throw new ArgumentException("无此郡县！");
        if (province.GovernorId == null)
            throw new InvalidOperationException($"{province.Name}本无地方官！");

        if (_state.Npcs.TryGetValue(province.GovernorId, out var npc))
        {
            npc.GovernedProvinceId = null;
            npc.Power = Math.Clamp(npc.Power + 3, 0, 100); // Returning to capital
        }

        string oldName = _state.Npcs.TryGetValue(province.GovernorId!, out var g) ? g.Name : "?";
        province.GovernorId = null;

        _state.AddToChronicle($"【召还】天子召【{oldName}】回京，{province.Name}暂无主官。");
        return new TurnResult
        {
            StoryText = $"【召还地方官】\n\n陛下下旨召【{oldName}】回京述职，{province.Name}暂无地方官。\n\n[color=yellow]● 该郡无人治理，民心将加速下降[/color]"
        };
    }

    /// <summary>View all provinces and their status.</summary>
    public string GetProvinceReport()
    {
        var report = "═══ 大汉郡县 ═══\n\n";
        foreach (var (_, p) in _state.Provinces.OrderBy(p => p.Value.Distance))
        {
            string govName = p.GovernorId != null && _state.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无";
            string status = p.IsRebelling ? $"⚡【叛乱中】{p.RebelFaction}" : "○ 安定";
            report += $"  {p.Name}（距京{p.Distance}）| 民心:{p.LocalSupport} | 守军:{p.Garrison} | 地方官:{govName}\n";
            report += $"    状态：{status}\n";
        }
        return report;
    }
}
