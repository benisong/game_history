using System;
using System.Collections.Generic;
using System.Linq;

namespace DonghanEngine.Core;

public partial class GameEngine
{
    // ============================
    //  每旬叛乱检测
    // ============================
    partial void CheckRebellions()
    {
        var alerts = new List<string>();
        var newlyRebelling = new List<Province>();
        var resolved = new List<Province>();

        foreach (var (_, p) in _state.Provinces)
        {
            bool hasGov = p.GovernorId != null;

            // ── 1. 黄巾之乱触发 ──
            if (!p.IsRebelling && p.LocalSupport < 10)
            {
                p.LowSupportStreakMonths++;
                if (p.LowSupportStreakMonths >= 3)
                {
                    TriggerYellowTurban(p);
                    newlyRebelling.Add(p);
                    continue;
                }
            }
            else if (!p.IsRebelling && p.LocalSupport >= 10)
            {
                p.LowSupportStreakMonths = 0;
            }

            // ── 2. 野心叛乱 ──
            if (!p.IsRebelling && hasGov && p.LocalSupport is >= 10 and <= 30
                && _state.Npcs.TryGetValue(p.GovernorId!, out var gov)
                && gov.Ambition >= 60)
            {
                int rebelChance = (gov.Ambition - 50) * 2 + (30 - p.LocalSupport);
                if (_rng.Next(0, 100) < rebelChance)
                {
                    p.IsRebelling = true;
                    p.RebellionMonths = 0;
                    p.RebelFaction = $"【{gov.Name}】据{p.Name}自立！";
                    gov.Power = Math.Clamp(gov.Power + 15, 0, 100);
                    gov.Favorability = Math.Clamp(gov.Favorability - 50, 0, 100);
                    alerts.Add($"⚡⚡ 【野心叛乱】{gov.Name}在{p.Name}举兵自立，割据一方！");
                    _state.ImperialPower = Math.Clamp(_state.ImperialPower - 5, 0, 100);
                    _state.PopularSupport = Math.Clamp(_state.PopularSupport - 3, 0, 100);
                    newlyRebelling.Add(p);
                    continue;
                }
            }

            // ── 3. 无官民变 ──
            if (!p.IsRebelling && !hasGov && p.LocalSupport < 20 && _rng.Next(0, 100) < 25)
            {
                p.IsRebelling = true;
                p.RebellionMonths = 0;
                p.RebelFaction = $"{p.Name}民变";
                p.LocalSupport = Math.Clamp(p.LocalSupport - 5, 0, 100);
                alerts.Add($"⚡ 【民变】{p.Name}无人治理，饥民聚众为乱！");
                _state.PopularSupport = Math.Clamp(_state.PopularSupport - 2, 0, 100);
                newlyRebelling.Add(p);
                continue;
            }

            // ── 4. 已叛乱郡进度推进 ──
            if (p.IsRebelling)
            {
                p.RebellionMonths++;

                // 蔓延
                int spreadBase = p.RebellionMonths >= 12 ? 75 : p.RebellionMonths >= 6 ? 55 : 35;
                foreach (var neighborId in p.Neighbors)
                {
                    if (_state.Provinces.TryGetValue(neighborId, out var n) && !n.IsRebelling)
                    {
                        if (_rng.Next(0, 100) < spreadBase)
                        {
                            n.IsRebelling = true;
                            n.RebellionMonths = 0;
                            n.RebelFaction = $"{p.RebelFaction}余部";
                            alerts.Add($"⚡ 【蔓延】{p.Name}叛军流窜至{n.Name}！");
                            newlyRebelling.Add(n);
                        }
                    }
                }

                // 被动伤害
                _state.Treasury = Math.Clamp(_state.Treasury - p.RebellionMonths * 50, 0, int.MaxValue);
                p.LocalSupport = Math.Clamp(p.LocalSupport - 2, 0, 100);
                p.Wealth = Math.Clamp(p.Wealth - 100, 0, int.MaxValue);
            }

            // ── 5. 无官自然衰减 ──
            if (!p.IsRebelling && !hasGov)
            {
                p.LocalSupport = Math.Clamp(p.LocalSupport - _rng.Next(3, 6), 0, 100);
                p.DefenseLevel = Math.Clamp(p.DefenseLevel - 1, 0, 100);
            }

            // ── 6. 有官治理微回复 ──
            if (!p.IsRebelling && hasGov)
            {
                p.LocalSupport = Math.Clamp(p.LocalSupport + 1, 0, 100);
            }
        }

        // 批量写入编年史
        foreach (var alert in alerts)
            _state.AddToChronicle(alert);
    }

    private void TriggerYellowTurban(Province p)
    {
        p.IsRebelling = true;
        p.RebellionMonths = 0;
        p.RebelFaction = "黄巾军";
        p.LocalSupport = 5;
        p.Garrison = Math.Clamp(p.Garrison * 2, 0, 20000);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport - 5, 0, 100);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower - 3, 0, 100);
        _state.Treasury = Math.Clamp(_state.Treasury - 500, 0, int.MaxValue);
    }

    // ============================
    //  军事平叛
    // ============================
    public TurnResult SuppressRebellion(string provinceId, string generalId)
    {
        return SuppressRebellion(provinceId, generalId, 3000);
    }

    public TurnResult SuppressRebellion(string provinceId, string generalId, int troops)
    {
        if (!_state.Provinces.TryGetValue(provinceId, out var province))
            throw new ArgumentException("无此郡县！", nameof(provinceId));
        if (!province.IsRebelling)
            throw new InvalidOperationException($"{province.Name}并未叛乱！");
        if (!_state.Npcs.TryGetValue(generalId, out var general))
            throw new ArgumentException("朝中无此将领！", nameof(generalId));
        if (general.GovernedProvinceId != null)
            throw new InvalidOperationException($"{general.Name}已在外任职！");
        if (troops < 1000)
            throw new ArgumentException("平叛至少需发兵 1000 人！", nameof(troops));
        if (troops > _state.WestGardenArmy.Size)
            throw new InvalidOperationException($"西园军仅有 {_state.WestGardenArmy.Size} 人，无法发兵 {troops} 人！");

        int campaignCost = Math.Max(100, troops / 10); // 万钱：千人百钱，八千人八百钱
        if (_state.Treasury < campaignCost)
            throw new InvalidOperationException($"国库仅余 {_state.Treasury} 万钱，不足以支付 {campaignCost} 万平叛军费！");

        double combatPower = NpcTraitEvaluator.GetCombatPower(general);
        double distancePenalty = province.Distance * 5;
        double troopRatio = troops / (double)Math.Max(province.Garrison, 1);
        double troopBonus = Math.Clamp((troopRatio - 1.0) * 20, -20, 25);
        double successRate = Math.Clamp(combatPower - distancePenalty + troopBonus, 5, 95);
        int rebelGarrisonBeforeBattle = province.Garrison;
        string battleReview = BuildSuppressBattleReview(combatPower, distancePenalty, troopBonus, successRate, troops, rebelGarrisonBeforeBattle, campaignCost);

        _state.Treasury = Math.Clamp(_state.Treasury - campaignCost, 0, int.MaxValue);
        bool success = _rng.Next(0, 100) < successRate;

        if (success)
        {
            int casualty = Math.Max(200, troops / 8);
            province.IsRebelling = false;
            province.RebellionMonths = 0;
            province.RebelFaction = "";
            province.LocalSupport = Math.Clamp(province.LocalSupport + 15, 0, 100);
            province.Garrison = Math.Clamp(province.Garrison / 2, 500, 20000);
            _state.WestGardenArmy.Size = Math.Clamp(_state.WestGardenArmy.Size - casualty, 0, int.MaxValue);
            general.Power = Math.Clamp(general.Power + 10, 0, 100);
            general.Favorability = Math.Clamp(general.Favorability + 5, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + 5, 0, 100);
            _state.PopularSupport = Math.Clamp(_state.PopularSupport + 3, 0, 100);

            _state.AddToChronicle($"【平叛】{general.Name}率{troops}西园军平定{province.Name}叛乱，大获全胜！");
            return new TurnResult
            {
                StoryText = $"【军事平叛 — 成功】\n\n陛下命【{general.Name}】率西园精锐 {troops} 人出征{province.Name}，距京{province.Distance}千里。\n\n" +
                            $"战力评估：{combatPower:F1} | 兵力修正：{troopBonus:+0;-0;0}% | 成功率：{successRate:F0}%\n" +
                            $"军费支出：{campaignCost} 万 | 战损：{casualty} 人\n\n" +
                            battleReview + "\n\n" +
                            $"旌旗蔽日，铁骑隆隆。{general.Name}不负圣恩，一举荡平叛军！\n\n" +
                            $"[color=green]● {province.Name}叛乱平定[/color]\n" +
                            $"[color=green]● {province.Name}民心 +15[/color]\n" +
                            $"[color=green]● 皇权 +5[/color]\n" +
                            $"[color=green]● {general.Name}权势 +10，忠诚 +5[/color]\n" +
                            $"[color=yellow]● 国库 -{campaignCost} 万，西园军 -{casualty} 人[/color]"
            };
        }
        else
        {
            int lostTroops = Math.Max(400, troops / 4);
            int rebelGain = Math.Max(300, troops / 10);
            general.Power = Math.Clamp(general.Power - 8, 0, 100);
            general.Favorability = Math.Clamp(general.Favorability - 10, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - 3, 0, 100);
            province.Garrison = Math.Clamp(province.Garrison + rebelGain, 0, 20000); // 叛军缴获兵械后壮大
            _state.WestGardenArmy.Size = Math.Clamp(_state.WestGardenArmy.Size - lostTroops, 0, int.MaxValue);

            _state.AddToChronicle($"【平叛失败】{general.Name}率{troops}征讨{province.Name}失利，损兵折将！");
            return new TurnResult
            {
                StoryText = $"【军事平叛 — 失败】\n\n陛下命【{general.Name}】率军 {troops} 人出征{province.Name}……\n\n" +
                            $"战力评估：{combatPower:F1} | 兵力修正：{troopBonus:+0;-0;0}% | 成功率：{successRate:F0}%\n" +
                            $"军费支出：{campaignCost} 万 | 折损兵马：{lostTroops} 人\n\n" +
                            battleReview + "\n\n" +
                            $"奈何叛军势大，{general.Name}久攻不下，粮草不济，只得暂且收兵。\n\n" +
                            $"[color=red]● 平叛失败！{province.Name}叛军 +{rebelGain}[/color]\n" +
                            $"[color=red]● {general.Name}权势 -8，忠诚 -10[/color]\n" +
                            $"[color=red]● 皇权 -3[/color]\n" +
                            $"[color=red]● 国库 -{campaignCost} 万，西园军 -{lostTroops} 人[/color]"
            };
        }
    }

    private static string BuildSuppressBattleReview(double combatPower, double distancePenalty, double troopBonus, double successRate, int troops, int rebelGarrison, int campaignCost)
    {
        var causes = new List<string>();

        if (combatPower >= 80)
            causes.Add("主将统御严整，临阵调度有方");
        else if (combatPower < 45)
            causes.Add("主将武略不足，难以独当方面");

        if (distancePenalty >= 20)
            causes.Add("道路迢递，转运不继，军心渐疲");
        else if (distancePenalty <= 5)
            causes.Add("近畿转运便利，诏令与粮草可迅速抵达");

        if (troopBonus >= 15)
            causes.Add("王师兵力压境，贼众望旗而怯");
        else if (troopBonus <= -10)
            causes.Add("兵少而贼众，虽将帅奋战亦难破围");

        if (campaignCost >= 600)
            causes.Add("此役调度浩大，国帑压力显著");

        if (causes.Count == 0)
            causes.Add(successRate >= 60 ? "诸项条件平稳，胜机主要来自稳健调度" : "诸项条件并无明显优势，胜负多系临阵发挥");

        return "【战局复盘】\n" +
               $"● 主将战力：{combatPower:F1}\n" +
               $"● 距京惩罚：-{distancePenalty:F0}%\n" +
               $"● 出兵/叛军：{troops}/{rebelGarrison}\n" +
               $"● 兵力修正：{troopBonus:+0;-0;0}%\n" +
               $"● 最终胜率：{successRate:F0}%\n" +
               $"● 主要因素：{string.Join("；", causes)}";
    }

    // ============================
    //  安抚策略选项
    // ============================
    [Flags]
    public enum PacifyStrategy
    {
        None     = 0,
        SowDiscord    = 1,  // 离间
        Persuade      = 2,  // 说服
        DisasterRelief = 4,  // 赈灾
        Punish        = 8   // 惩治
    }

    // ============================
    //  安抚平叛
    // ============================
    public TurnResult PacifyRebellion(string provinceId, string envoyId, PacifyStrategy strategies, int reliefGold = 0)
    {
        if (!_state.Provinces.TryGetValue(provinceId, out var province))
            throw new ArgumentException("无此郡县！", nameof(provinceId));
        if (!province.IsRebelling)
            throw new InvalidOperationException($"{province.Name}并未叛乱！");
        if (!_state.Npcs.TryGetValue(envoyId, out var envoy))
            throw new ArgumentException("朝中无此大臣！", nameof(envoyId));
        if (envoy.GovernedProvinceId != null)
            throw new InvalidOperationException($"{envoy.Name}已在外任职！");
        if (strategies == PacifyStrategy.None)
            throw new ArgumentException("至少选择一种安抚策略！");

        double politicalSkill = NpcTraitEvaluator.GetPoliticalSkill(envoy);
        double baseRate = Math.Clamp(politicalSkill / 2, 15, 60);
        var strategyDetails = new List<string>();
        bool envoyKilled = false;

        // ── 离间 ──
        bool usedSowDiscord = strategies.HasFlag(PacifyStrategy.SowDiscord);
        if (usedSowDiscord)
        {
            if (envoy.Politics < 50)
                throw new InvalidOperationException($"{envoy.Name}政治能力不足（需≥50，当前{envoy.Politics}），无法行离间之计！");
            baseRate += 15;
            strategyDetails.Add("┣ 【离间】遣密使潜入叛军，散布内部分裂谣言（+15%）");
        }

        // ── 说服 ──
        if (strategies.HasFlag(PacifyStrategy.Persuade))
        {
            if (envoy.Charisma < 45)
                throw new InvalidOperationException($"{envoy.Name}魅力不足（需≥45，当前{envoy.Charisma}），无法说服叛军！");
            baseRate += 20;
            strategyDetails.Add("┣ 【说服】特使亲赴叛军大营，晓以利害（+20%）");
        }

        // ── 赈灾 ──
        if (strategies.HasFlag(PacifyStrategy.DisasterRelief))
        {
            if (reliefGold < 500)
                throw new ArgumentException($"赈灾至少需要 500 万钱（当前{reliefGold}万）！");
            if (_state.Treasury < reliefGold)
                throw new InvalidOperationException($"国库仅余 {_state.Treasury} 万钱，不足 {reliefGold} 万！");
            double reliefBonus = Math.Min(reliefGold / 500.0 * 8, 24); // 每500万+8%，上限1500万
            baseRate += reliefBonus;
            _state.Treasury -= reliefGold;
            strategyDetails.Add($"┣ 【赈灾】拨付 {reliefGold} 万钱赈济灾民（+{reliefBonus:F0}%）");
        }

        // ── 惩治 ──
        if (strategies.HasFlag(PacifyStrategy.Punish))
        {
            if (_state.ImperialPower >= 35)
            {
                baseRate += 15;
                strategyDetails.Add("┣ 【惩治】以天子名义下诏严惩叛乱首领，先声夺人（+15%）");
            }
            else if (_state.ImperialPower < 20)
            {
                baseRate -= 10;
                strategyDetails.Add("┣ 【惩治】皇权微弱，下诏惩治适得其反（-10%）");
            }
            else
            {
                strategyDetails.Add("┣ 【惩治】皇权不足（需≥35），惩治无效");
            }
        }

        double finalRate = Math.Clamp(baseRate, 5, 95);
        bool success = _rng.Next(0, 100) < finalRate;

        string detailText = string.Join("\n", strategyDetails);

        if (success)
        {
            province.IsRebelling = false;
            province.RebellionMonths = 0;
            province.RebelFaction = "";
            province.LocalSupport = Math.Clamp(province.LocalSupport + 20, 0, 100);
            envoy.Power = Math.Clamp(envoy.Power + 5, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + 3, 0, 100);
            _state.PopularSupport = Math.Clamp(_state.PopularSupport + 5, 0, 100);

            _state.AddToChronicle($"【安抚】{envoy.Name}智抚{province.Name}，叛乱平息。");
            return new TurnResult
            {
                StoryText = $"【安抚平叛 — 成功】\n\n陛下遣【{envoy.Name}】为安抚特使，赴{province.Name}平乱。\n\n" +
                            $"外交力：{politicalSkill:F1} | 成功率：{finalRate:F0}%\n\n" +
                            $"策略：\n{detailText}\n\n" +
                            $"{envoy.Name}不辱使命！{province.Name}叛军放下武器，重归朝廷。\n\n" +
                            $"[color=green]● {province.Name}叛乱平息[/color]\n" +
                            $"[color=green]● {province.Name}民心 +20[/color]\n" +
                            $"[color=green]● 皇权 +3，天下民心 +5[/color]\n" +
                            $"[color=green]● {envoy.Name}权势 +5[/color]"
            };
        }
        else
        {
            // 特使阵亡判定
            int deathRisk = NpcTraitEvaluator.GetEnvoyDeathRisk(envoy, province.RebellionMonths, province.Distance, usedSowDiscord, _state.ImperialPower);
            int roll = _rng.Next(0, 100);
            envoyKilled = roll < deathRisk;

            envoy.Power = Math.Clamp(envoy.Power - 5, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - 5, 0, 100);
            _state.PopularSupport = Math.Clamp(_state.PopularSupport - 3, 0, 100);

            string deathText = "";
            if (envoyKilled)
            {
                envoy.IsActive = false;
                envoy.DeathReason = $"安抚{province.Name}叛乱失败，被叛军所杀";
                deathText = $"\n[color=red]● 特使【{envoy.Name}】不幸阵亡！{envoy.Name}死于叛军刀下，朝野震动。[/color]";
                if (envoy.Power >= 60)
                {
                    _state.ImperialPower = Math.Clamp(_state.ImperialPower - 8, 0, 100);
                    deathText += $"\n[color=red]● {envoy.Name}乃朝廷重臣（权势≥60），其阵亡引发朝堂动荡，皇权额外 -8！[/color]";
                }
            }

            _state.AddToChronicle($"【安抚失败】{envoy.Name}安抚{province.Name}失败{(envoyKilled ? "，不幸殉国" : "")}。");
            return new TurnResult
            {
                StoryText = $"【安抚平叛 — 失败】\n\n陛下遣【{envoy.Name}】为安抚特使，赴{province.Name}平乱。\n\n" +
                            $"外交力：{politicalSkill:F1} | 成功率：{finalRate:F0}%\n\n" +
                            $"策略：\n{detailText}\n\n" +
                            $"奈何叛军冥顽不灵，{envoy.Name}的安抚之策未能奏效。\n\n" +
                            $"[color=red]● 安抚失败[/color]\n" +
                            $"[color=red]● {envoy.Name}权势 -5[/color]\n" +
                            $"[color=red]● 皇权 -5，天下民心 -3[/color]" +
                            deathText
            };
        }
    }

    // ============================
    //  任命地方官
    // ============================
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
            StoryText = $"【任命地方官】\n\n陛下朱批已下，任命【{npc.Name}】为{province.Name}地方官，即日赴任。\n\n" +
                        $"[color=green]● {province.Name}民心：+10[/color]\n" +
                        $"[color=yellow]● 【{npc.Name}】权势 -5（远离中央）[/color]"
        };
    }

    // ============================
    //  召回地方官
    // ============================
    public TurnResult RecallGovernor(string provinceId)
    {
        if (!_state.Provinces.TryGetValue(provinceId, out var province))
            throw new ArgumentException("无此郡县！");
        if (province.GovernorId == null)
            throw new InvalidOperationException($"{province.Name}本无地方官！");

        if (_state.Npcs.TryGetValue(province.GovernorId, out var npc))
        {
            npc.GovernedProvinceId = null;
            npc.Power = Math.Clamp(npc.Power + 3, 0, 100);
        }

        string oldName = _state.Npcs.TryGetValue(province.GovernorId!, out var g) ? g.Name : "?";
        province.GovernorId = null;

        _state.AddToChronicle($"【召还】天子召【{oldName}】回京，{province.Name}暂无主官。");
        return new TurnResult
        {
            StoryText = $"【召还地方官】\n\n陛下下旨召【{oldName}】回京述职，{province.Name}暂无地方官。\n\n" +
                        $"[color=yellow]● 该郡无人治理，民心将加速下降[/color]"
        };
    }

    // ============================
    //  郡县报告
    // ============================
    public string GetProvinceReport()
    {
        var report = "═══ 大汉郡县 ═══\n\n";
        foreach (var (_, p) in _state.Provinces.OrderBy(p => p.Value.Distance))
        {
            string govName = p.GovernorId != null && _state.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无";
            string status = p.IsRebelling ? $"⚡【叛乱中】{p.RebelFaction}" : "○ 安定";
            report += $"  {p.Name}（距京{p.Distance}）| 民心:{p.LocalSupport} | 守军:{p.Garrison} | 地方官:{govName}\n";
            report += $"    状态：{status}\n";
            if (p.IsRebelling) report += $"    叛乱持续：{p.RebellionMonths} 个月\n";
        }
        return report;
    }
}
