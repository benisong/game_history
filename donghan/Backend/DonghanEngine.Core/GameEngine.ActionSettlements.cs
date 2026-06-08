using System;

namespace DonghanEngine.Core;

public partial class GameEngine
{
    private enum DrillArmyOutcome
    {
        Underpaid,
        BarelyFunded,
        RewardedTrusted,
        RewardedDistrusted
    }

    private sealed record DrillArmySettlement(
        int PaidAmount,
        NpcState Officer,
        int SiphonedAmount,
        int ActualReceivedAmount,
        int BaseLoyaltyDelta,
        int FinalMoraleDelta,
        int FinalLoyaltyDelta,
        int ImperialPowerDelta,
        DrillArmyOutcome Outcome);

    private enum DisasterReliefOutcome
    {
        Starved,
        Relieved
    }

    private sealed record DisasterReliefSettlement(
        int ReliefAmount,
        NpcState Officer,
        int SiphonedAmount,
        int ActualReliefReceived,
        int FinalSupportDelta,
        int ImperialPowerDelta,
        DisasterReliefOutcome Outcome);

    private enum ConfiscationOutcome
    {
        CliqueBacklash,
        QuietSuccess
    }

    private sealed record ConfiscationSettlement(
        NpcState Framer,
        NpcState Target,
        int RawWealth,
        int FramerEmbezzled,
        int AmountToTreasury,
        int AmountToPrivate,
        int NetSupportDelta,
        int ImperialPowerDelta,
        int FinalPowerLoss,
        ConfiscationOutcome Outcome);

    private DrillArmySettlement CalculateDrillArmySettlement(int paidAmount, NpcState officer)
    {
        var army = _state.WestGardenArmy;

        double corruptionRate = (officer.Corruption / 100.0) * 0.50;
        int siphonedAmount = (int)(paidAmount * corruptionRate);
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > paidAmount) siphonedAmount = paidAmount;

        int actualReceivedAmount = paidAmount - siphonedAmount;
        double basePay = army.BasePayPerTurn;
        double premiumRatio = (actualReceivedAmount - basePay) / basePay;
        double avgState = (army.Morale + army.Loyalty) / 2.0;
        double efficiencyModifier = avgState < 60
            ? Math.Pow(avgState / 60.0, 2)
            : 1.0 + ((avgState - 60.0) / 80.0);

        int moraleDelta;
        int loyaltyDelta = 0;
        int imperialPowerDelta = 0;
        DrillArmyOutcome outcome;

        if (actualReceivedAmount < basePay)
        {
            moraleDelta = -25;
            loyaltyDelta = -15;
            imperialPowerDelta = -3;
            outcome = DrillArmyOutcome.Underpaid;
        }
        else if (premiumRatio < 0.2)
        {
            moraleDelta = (int)(3 * efficiencyModifier);
            outcome = DrillArmyOutcome.BarelyFunded;
        }
        else
        {
            moraleDelta = (int)(15 * premiumRatio * efficiencyModifier);
            loyaltyDelta = (int)(8 * premiumRatio * efficiencyModifier);
            if (avgState >= 60)
            {
                imperialPowerDelta = 2;
                outcome = DrillArmyOutcome.RewardedTrusted;
            }
            else
            {
                outcome = DrillArmyOutcome.RewardedDistrusted;
            }
        }

        double moraleMultiplier = NpcTraitEvaluator.GetDrillMoraleMultiplier(officer);
        int finalMoraleDelta = (int)(moraleDelta * moraleMultiplier);

        double loyaltyMultiplier = NpcTraitEvaluator.GetDrillLoyaltyMultiplier(officer);
        int finalLoyaltyDelta = (int)(loyaltyDelta * loyaltyMultiplier);

        return new DrillArmySettlement(
            paidAmount,
            officer,
            siphonedAmount,
            actualReceivedAmount,
            loyaltyDelta,
            finalMoraleDelta,
            finalLoyaltyDelta,
            imperialPowerDelta,
            outcome);
    }

    private void ApplyDrillArmySettlement(DrillArmySettlement settlement)
    {
        _state.PrivateTreasury -= settlement.PaidAmount;
        settlement.Officer.StashedWealth += settlement.SiphonedAmount;

        var army = _state.WestGardenArmy;
        army.Morale = Math.Clamp(army.Morale + settlement.FinalMoraleDelta, 0, 100);
        army.Loyalty = Math.Clamp(army.Loyalty + settlement.FinalLoyaltyDelta, 0, 100);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower + settlement.ImperialPowerDelta, 0, 100);

        settlement.Officer.Power = Math.Clamp(settlement.Officer.Power + 2, 0, 100);
    }

    private DisasterReliefSettlement CalculateDisasterReliefSettlement(int reliefAmount, NpcState officer)
    {
        double corruptionRate = (officer.Corruption / 100.0) * 0.75;
        int siphonedAmount = (int)(reliefAmount * corruptionRate);
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > reliefAmount) siphonedAmount = reliefAmount;

        int actualReliefReceived = reliefAmount - siphonedAmount;
        double reliefNeed = 1000.0;
        double performanceRatio = actualReliefReceived / reliefNeed;

        int supportDelta;
        int imperialPowerDelta;
        DisasterReliefOutcome outcome;

        if (actualReliefReceived < reliefNeed)
        {
            supportDelta = (int)(-15 * (1.0 - performanceRatio));
            imperialPowerDelta = -3;
            outcome = DisasterReliefOutcome.Starved;
        }
        else
        {
            supportDelta = (int)(12 * performanceRatio);
            imperialPowerDelta = 3;
            outcome = DisasterReliefOutcome.Relieved;
        }

        double supportMultiplier = NpcTraitEvaluator.GetDisasterReliefSupportMultiplier(officer);
        int finalSupportDelta = (int)(supportDelta * supportMultiplier);

        return new DisasterReliefSettlement(
            reliefAmount,
            officer,
            siphonedAmount,
            actualReliefReceived,
            finalSupportDelta,
            imperialPowerDelta,
            outcome);
    }

    private void ApplyDisasterReliefSettlement(DisasterReliefSettlement settlement)
    {
        _state.Treasury -= settlement.ReliefAmount;
        settlement.Officer.StashedWealth += settlement.SiphonedAmount;

        _state.PopularSupport = Math.Clamp(_state.PopularSupport + settlement.FinalSupportDelta, 0, 100);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower + settlement.ImperialPowerDelta, 0, 100);
        settlement.Officer.Power = Math.Clamp(settlement.Officer.Power + 5, 0, 100);
    }

    private NpcState? FindConfiscationFramer(string targetMinisterId)
    {
        foreach (var pair in _state.Npcs)
        {
            if (pair.Key != targetMinisterId && pair.Value.Favorability >= 55 && pair.Value.Corruption < 40)
            {
                return pair.Value;
            }
        }

        return null;
    }

    private ConfiscationSettlement CalculateConfiscationSettlement(NpcState framer, NpcState target)
    {
        int rawWealth = target.StashedWealth;
        double framerSiphonRate = (framer.Corruption / 100.0) * 0.40;
        int framerEmbezzled = (int)(rawWealth * framerSiphonRate);
        int wealthLeftAfterFramer = rawWealth - framerEmbezzled;

        int amountToTreasury = (int)(wealthLeftAfterFramer * 0.70);
        int amountToPrivate = wealthLeftAfterFramer - amountToTreasury;

        int supportDelta = (int)Math.Round(15.0 * (1.0 - Math.Exp(-rawWealth / 3000.0)));
        supportDelta = Math.Clamp(supportDelta, 1, 15);

        bool cliqueBacklash = target.Power >= 60;
        int finalPowerLoss = NpcTraitEvaluator.GetConfiscationImperialPowerLoss(framer, target);

        int netSupportDelta;
        int imperialPowerDelta;
        ConfiscationOutcome outcome;

        if (cliqueBacklash)
        {
            netSupportDelta = target.Traits.Contains(TraitNames.QingZhengLianJie) ? -20 : supportDelta - 8;
            imperialPowerDelta = -finalPowerLoss;
            outcome = ConfiscationOutcome.CliqueBacklash;
        }
        else
        {
            netSupportDelta = target.Traits.Contains(TraitNames.QingZhengLianJie) ? -20 : supportDelta;
            imperialPowerDelta = 8;
            outcome = ConfiscationOutcome.QuietSuccess;
        }

        return new ConfiscationSettlement(
            framer,
            target,
            rawWealth,
            framerEmbezzled,
            amountToTreasury,
            amountToPrivate,
            netSupportDelta,
            imperialPowerDelta,
            finalPowerLoss,
            outcome);
    }

    private void ApplyConfiscationSettlement(ConfiscationSettlement settlement)
    {
        settlement.Framer.StashedWealth += settlement.FramerEmbezzled;
        settlement.Framer.Power = Math.Clamp(settlement.Framer.Power + 3, 0, 100);

        _state.Treasury = Math.Clamp(_state.Treasury + settlement.AmountToTreasury, 0, 999999);
        _state.PrivateTreasury = Math.Clamp(_state.PrivateTreasury + settlement.AmountToPrivate, 0, 999999);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower + settlement.ImperialPowerDelta, 0, 100);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport + settlement.NetSupportDelta, 0, 100);

        settlement.Target.StashedWealth = 0;
        if (settlement.Outcome == ConfiscationOutcome.CliqueBacklash)
        {
            settlement.Target.Power = Math.Clamp(settlement.Target.Power - 30, 0, 100);
            settlement.Target.Favorability = Math.Clamp(settlement.Target.Favorability - 40, 0, 100);
        }
        else
        {
            settlement.Target.Power = Math.Clamp(settlement.Target.Power - 40, 0, 100);
            settlement.Target.Favorability = Math.Clamp(settlement.Target.Favorability - 30, 0, 100);
        }
    }
}
