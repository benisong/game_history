using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineStateReader : IGameStateReader
{
    private readonly GameEngine _engine;

    public GameEngineStateReader(GameEngine engine) => _engine = engine;

    public GameStateSnapshot GetSnapshot() => Snapshot(_engine.GetState());

    public ProvinceSnapshot? GetProvince(string provinceId)
    {
        var state = _engine.GetState();
        return state.Provinces.TryGetValue(provinceId, out var province)
            ? Snapshot(state, province)
            : null;
    }

    public IReadOnlyList<MinisterSnapshot> GetMinisters() =>
        _engine.GetState().Npcs.Values.Select(Snapshot).ToList();

    internal static GameStateSnapshot Snapshot(GameState state) => new(
        state.ReignTitle,
        state.ReignYear,
        state.Year,
        state.Month,
        state.Xun,
        state.CurrentLocation,
        state.ImperialPower,
        state.Treasury,
        state.PrivateTreasury,
        state.PopularSupport,
        state.Health,
        state.WestGardenArmy.Size,
        12000,
        state.WestGardenArmy.Morale,
        state.WestGardenArmy.Loyalty,
        state.Outcome.ToString(),
        state.Chronicle.ToList());

    private static MinisterSnapshot Snapshot(NpcState npc) => new(
        npc.Id, npc.Name, npc.Title, npc.Faction, npc.Favorability, npc.Power,
        npc.Corruption, npc.IsActive, npc.IsHostile);

    private static ProvinceSnapshot Snapshot(GameState state, Province province)
    {
        string governorName = province.GovernorId != null && state.Npcs.TryGetValue(province.GovernorId, out var governor)
            ? governor.Name
            : string.Empty;
        return new ProvinceSnapshot(
            province.Id,
            province.Name,
            province.IsRebelling,
            province.RebelFaction,
            province.RebellionMonths,
            province.LocalSupport,
            province.Garrison,
            province.Wealth,
            province.DefenseLevel,
            province.Distance,
            province.GovernorId,
            governorName);
    }
}
