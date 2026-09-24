using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineStateReader : IGameStateReader
{
    private readonly IGameStateProvider _stateProvider;

    public GameEngineStateReader(IGameStateProvider stateProvider) => _stateProvider = stateProvider;

    public GameStateSnapshot GetSnapshot() => Snapshot(_stateProvider.GetState());

    public ProvinceSnapshot? GetProvince(string provinceId)
    {
        var state = _stateProvider.GetState();
        return state.Provinces.TryGetValue(provinceId, out var province)
            ? Snapshot(state, province, _stateProvider)
            : null;
    }

    public IReadOnlyList<ProvinceSnapshot> GetAllProvinces()
    {
        var state = _stateProvider.GetState();
        return state.Provinces.Values.Select(p => Snapshot(state, p, _stateProvider)).ToList();
    }

    public IReadOnlyList<WarlordFactionSnapshot> GetAllFactions()
    {
        var state = _stateProvider.GetState();
        var list = new List<WarlordFactionSnapshot>();
        if (_stateProvider is GameEngine ge)
        {
            foreach (var f in ge.GeopoliticsEngine.Factions.Values)
            {
                string leaderName = state.Npcs.TryGetValue(f.LeaderNpcId, out var npc) ? npc.Name : f.LeaderNpcId;
                list.Add(new WarlordFactionSnapshot(
                    f.FactionId,
                    f.FactionName,
                    f.LeaderNpcId,
                    leaderName,
                    f.Posture.ToString(),
                    f.ControlledProvinces.ToList(),
                    f.TotalTroops,
                    f.Provisions,
                    f.ImperialLoyalty,
                    f.ExpansionDesire));
            }
        }
        return list;
    }

    public IReadOnlyList<MinisterSnapshot> GetMinisters() =>
        _stateProvider.GetState().Npcs.Values.Select(Snapshot).ToList();

    internal static GameStateSnapshot Snapshot(GameState state)
    {
        var prestigeEvaluator = new DonghanEngine.Core.Politics.ImperialPrestigeEvaluator();
        var prestigeResult = prestigeEvaluator.Evaluate(state.ImperialPower);

        return new GameStateSnapshot(
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
            state.Chronicle.ToList(),
            prestigeResult.State.ToString(),
            prestigeResult.StatusDescription);
    }

    private static MinisterSnapshot Snapshot(NpcState npc) => new(
        npc.Id, npc.Name, npc.Title, npc.Faction, npc.Favorability, npc.Power,
        npc.Corruption, npc.IsActive, npc.IsHostile,
        npc.Martial, npc.Leadership, npc.Politics, npc.Charisma, npc.Ambition,
        npc.Personality, npc.Style, npc.Traits.ToList());

    private static ProvinceSnapshot Snapshot(GameState state, Province province, IGameStateProvider stateProvider)
    {
        string governorName = province.GovernorId != null && state.Npcs.TryGetValue(province.GovernorId, out var governor)
            ? governor.Name
            : string.Empty;

        // 获取实际地缘割据势力归属
        string controllingFactionId = "court";
        string controllingFactionName = "朝廷直辖";

        if (stateProvider is GameEngine ge)
        {
            foreach (var f in ge.GeopoliticsEngine.Factions.Values)
            {
                if (f.ControlledProvinces.Contains(province.Id))
                {
                    controllingFactionId = f.FactionId;
                    controllingFactionName = f.FactionName;
                    break;
                }
            }
        }

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
            governorName,
            province.Population,
            province.LandCarryingCapacity,
            province.StateControlledLand,
            province.GentryControlledLand,
            province.ScorchedLand,
            province.ScorchedMonthsRemaining,
            controllingFactionId,
            controllingFactionName);
    }
}
