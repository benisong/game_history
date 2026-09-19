using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Memorials;

/// <summary>
/// 纯领域天子诏令执行器：根据天子御批指令，反向作用于朝廷国库、天子皇权与地缘诸侯实体（单一职责）
/// </summary>
public sealed class ImperialEdictExecutor : IImperialEdictExecutor
{
    public EdictExecutionResult ExecuteEdict(
        GeopoliticalImperialEdict edict,
        GameState gameState,
        IReadOnlyDictionary<string, WarlordFaction> allFactions,
        FactionRelationGraph relations)
    {
        if (edict == null) throw new ArgumentNullException(nameof(edict));
        if (gameState == null) throw new ArgumentNullException(nameof(gameState));

        allFactions.TryGetValue(edict.TargetFactionId, out var targetFaction);
        allFactions.TryGetValue(edict.ThirdPartyFactionId, out var thirdPartyFaction);

        switch (edict.EdictType)
        {
            // 1. 官爵追认（承认事实、加封官位、收缴谢恩钱）
            case GeopoliticalEdictType.RatifyAndReward:
            {
                int gold = edict.GoldDeductionOrReward > 0 ? edict.GoldDeductionOrReward : 2000;
                gameState.Treasury = Math.Clamp(gameState.Treasury + gold, 0, 999999);
                gameState.ImperialPower = Math.Clamp(gameState.ImperialPower - 3, 0, 100); // 承认既成事实，微损中央威严

                targetFaction?.AdjustLoyalty(15);
                targetFaction?.AdjustAmbition(5);

                string chronicle = $"【官爵追认】天子准奏，加封{targetFaction?.FactionName ?? "诸侯"}为地方刺史，收纳谢恩贡金{gold}万钱。";
                gameState.AddToChronicle(chronicle);

                return new EdictExecutionResult(
                    Success: true,
                    NarrativeSummary: $"准奏追认，国库入账{gold}万，{targetFaction?.FactionName}忠诚度上升，暂时归附朝廷。",
                    ImperialPowerDelta: -3,
                    TreasuryDelta: gold,
                    PrivateTreasuryDelta: 0,
                    TargetLoyaltyDelta: 15,
                    TargetAmbitionDelta: 5,
                    ChronicleEntry: chronicle);
            }

            // 2. 明旨斥责并密诏背刺（驱虎吞狼）
            case GeopoliticalEdictType.DenounceAndProvoke:
            {
                gameState.ImperialPower = Math.Clamp(gameState.ImperialPower + 8, 0, 100); // 天子刚正不阿，皇权大振
                targetFaction?.AdjustLoyalty(-30);
                targetFaction?.AdjustAmbition(15);

                // 若有第三者强邻，大幅激化其与目标诸侯的敌对度
                if (thirdPartyFaction != null && targetFaction != null)
                {
                    relations?.AdjustHostility(thirdPartyFaction.FactionId, targetFaction.FactionId, 40);
                    thirdPartyFaction.AdjustLoyalty(10);
                }

                string chronicle = $"【天子明斥】天子颁严旨痛斥{targetFaction?.FactionName ?? "强藩"}擅动干戈，密诏天下忠义之士趁虚共讨！";
                gameState.AddToChronicle(chronicle);

                return new EdictExecutionResult(
                    Success: true,
                    NarrativeSummary: $"天子明斥逆行，皇权大振！密诏四方诸侯牵制其后方！",
                    ImperialPowerDelta: 8,
                    TreasuryDelta: 0,
                    PrivateTreasuryDelta: 0,
                    TargetLoyaltyDelta: -30,
                    TargetAmbitionDelta: 15,
                    ChronicleEntry: chronicle);
            }

            // 3. 遣使持节调停罢兵（居中收贡）
            case GeopoliticalEdictType.MediateTruce:
            {
                int goldPerParty = edict.GoldDeductionOrReward > 0 ? edict.GoldDeductionOrReward : 1000;
                gameState.Treasury = Math.Clamp(gameState.Treasury + goldPerParty * 2, 0, 999999);
                gameState.ImperialPower = Math.Clamp(gameState.ImperialPower + 5, 0, 100);

                if (targetFaction != null && thirdPartyFaction != null)
                {
                    relations?.EstablishTruce(targetFaction.FactionId, thirdPartyFaction.FactionId);
                }

                string chronicle = $"【朝廷持节】天子遣使持节亲临前线，勒令两家罢兵修好，各纳罢战贡金{goldPerParty}万钱。";
                gameState.AddToChronicle(chronicle);

                return new EdictExecutionResult(
                    Success: true,
                    NarrativeSummary: $"天子持节调停成功，两家罢战息民，共纳贡金{goldPerParty * 2}万钱！",
                    ImperialPowerDelta: 5,
                    TreasuryDelta: goldPerParty * 2,
                    PrivateTreasuryDelta: 0,
                    TargetLoyaltyDelta: 10,
                    TargetAmbitionDelta: -5,
                    ChronicleEntry: chronicle);
            }

            // 4. 欣然嘉纳忠臣岁贡
            case GeopoliticalEdictType.AcceptTribute:
            {
                int gold = edict.GoldDeductionOrReward;
                gameState.Treasury = Math.Clamp(gameState.Treasury + gold, 0, 999999);
                targetFaction?.AdjustLoyalty(20);

                string chronicle = $"【岁赋入关】天子嘉勉{targetFaction?.FactionName ?? "忠良"}，受纳岁贡{gold}万钱，赐锦袍御酒。";
                gameState.AddToChronicle(chronicle);

                return new EdictExecutionResult(
                    Success: true,
                    NarrativeSummary: $"受纳忠臣贡金{gold}万钱，国库大充，四海感戴天恩！",
                    ImperialPowerDelta: 3,
                    TreasuryDelta: gold,
                    PrivateTreasuryDelta: 0,
                    TargetLoyaltyDelta: 20,
                    TargetAmbitionDelta: 0,
                    ChronicleEntry: chronicle);
            }

            default:
                return new EdictExecutionResult(false, "未知诏令类型", 0, 0, 0, 0, 0, string.Empty);
        }
    }
}
