using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public class GameEngine
{
    private readonly GameState _state;
    private readonly IAIScheduler _scheduler;
    private readonly IEventOracle _oracle;
    private readonly IMinisterAgent _ministerAgent;
    private readonly INarrator _narrator;

    public GameEngine(
        GameState state, 
        IAIScheduler scheduler, 
        IEventOracle oracle, 
        IMinisterAgent ministerAgent, 
        INarrator narrator)
    {
        _state = state;
        _scheduler = scheduler;
        _oracle = oracle;
        _ministerAgent = ministerAgent;
        _narrator = narrator;
    }

    public GameState GetState() => _state;

    public void TravelToLocation(string newLocation)
    {
        if (newLocation != "宣政殿" && newLocation != "后宫" && newLocation != "西园")
            throw new ArgumentException("禁宫之中，无此去处！", nameof(newLocation));

        if (_state.CurrentLocation == newLocation) return;
        string oldLocation = _state.CurrentLocation;
        _state.CurrentLocation = newLocation;
        _state.AddToChronicle($"帝驾巡幸：龙辇起驾，由【{oldLocation}】移驾至【{newLocation}】。");
    }

    // 阅兵发饷
    public TurnResult ExecuteDrillArmyActionWithOfficer(int paidAmount, string officerId)
    {
        if (_state.CurrentLocation != "西园")
            throw new InvalidOperationException("只能在西园校场阅兵发饷！");

        if (!_state.Npcs.TryGetValue(officerId, out var officer))
            throw new ArgumentException("西园校场无此官员！", nameof(officerId));

        var result = new TurnResult();
        var army = _state.WestGardenArmy;

        if (paidAmount > _state.PrivateTreasury)
        {
            result.StoryText = "【发饷失败】\n\n天子私库空虚，不足以支撑此次开支！群情哗然！\n\n[color=red]● 士气: -5[/color]";
            army.Morale = Math.Clamp(army.Morale - 5, 0, 100);
            return result;
        }

        _state.PrivateTreasury -= paidAmount;

        double corruptionRate = (officer.Corruption / 100.0) * 0.50; 
        int siphonedAmount = (int)(paidAmount * corruptionRate);

        // 统一对接评估器：清正/不拿钱/贪得无厌/脏手
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > paidAmount) siphonedAmount = paidAmount;

        int actualReceivedAmount = paidAmount - siphonedAmount;
        officer.StashedWealth += siphonedAmount;

        double basePay = army.BasePayPerTurn;
        double premiumRatio = (actualReceivedAmount - basePay) / basePay;
        
        double avgState = (army.Morale + army.Loyalty) / 2.0;
        double efficiencyModifier = 1.0;

        if (avgState < 60)
        {
            efficiencyModifier = Math.Pow(avgState / 60.0, 2);
        }
        else
        {
            efficiencyModifier = 1.0 + ((avgState - 60.0) / 80.0);
        }

        string narrativeFeedback = "";
        int moraleDelta = 0;
        int loyaltyDelta = 0;
        int imperialPowerDelta = 0;

        string corruptionNarrative = "";
        if (siphonedAmount > 0)
        {
            corruptionNarrative = $"[color=red]经办官【{officer.Name}】从中层层中饱私囊，天子虽拨下 {paidAmount} 万钱，但将士最终仅分得 {actualReceivedAmount} 万钱（其中被贪污漂没 {siphonedAmount} 万钱，滚入其私蓄中）！[/color]\n\n";
        }

        if (actualReceivedAmount < basePay)
        {
            moraleDelta = -25;
            loyaltyDelta = -15;
            imperialPowerDelta = -3;
            narrativeFeedback = $"将士们看到手里的那几枚铜钱，个个咬牙切齿。西园新军愤怒地瞪着在台上督办的【{officer.Name}】，低沉的骂声在军营中蔓延：\u201c昏君奸臣！拨了这么多钱，到我们手里就剩这点！\u201d";
        }
        else if (premiumRatio < 0.2)
        {
            moraleDelta = (int)(3 * efficiencyModifier);
            narrativeFeedback = $"士兵们领到本月军饷，虽足额，但对官吏层层抽水的贪婪行径极度不满。军营中弥漫着压抑的冷漠，将士们领赏后只是木然行礼。";
        }
        else
        {
            moraleDelta = (int)(15 * premiumRatio * efficiencyModifier);
            loyaltyDelta = (int)(8 * premiumRatio * efficiencyModifier);
            
            if (avgState >= 60)
            {
                imperialPowerDelta = 2;
                narrativeFeedback = $"陛下朱批重赏！即使【{officer.Name}】漂没了部分赏钱，但剩下的钱依然让士卒们喜笑颜开。校场上战鼓齐鸣，三军山呼万岁，忠于天子的军心大振！";
            }
            else
            {
                narrativeFeedback = $"虽然天子重赏，但将士们对官吏吃拿卡要的作风深恶痛绝。将士们领过钱财，脸上并无感激，纷纷在私下讥笑朝廷的无能与贪墨。";
            }
        }

        // 对接评估器：孔武有力/有些力气/治军严整/懂兵法/不学无术
        double moraleMultiplier = NpcTraitEvaluator.GetDrillMoraleMultiplier(officer);
        int finalMoraleDelta = (int)(moraleDelta * moraleMultiplier);

        // 对接评估器：爱兵如子/体恤士卒
        double loyaltyMultiplier = NpcTraitEvaluator.GetDrillLoyaltyMultiplier(officer);
        int finalLoyaltyDelta = (int)(loyaltyDelta * loyaltyMultiplier);

        army.Morale = Math.Clamp(army.Morale + finalMoraleDelta, 0, 100);
        army.Loyalty = Math.Clamp(army.Loyalty + finalLoyaltyDelta, 0, 100);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower + imperialPowerDelta, 0, 100);

        // 经办西园犒军阅兵，代天子掌军事务，该官员权势稳健成长 +2（不因贪污资金暴涨）
        officer.Power = Math.Clamp(officer.Power + 2, 0, 100);

        _state.AddToChronicle($"【西园】天子命【{officer.Name}】犒赏禁军，拨发 {paidAmount} 万钱。被侵吞漂没 {siphonedAmount} 万钱！实际到手 {actualReceivedAmount} 万钱。军心士气变化: {finalMoraleDelta}。");

        result.StoryText = $"【西园大阅 · 发放犒赏】\n\n天子拨出：{paidAmount} 万钱\n{corruptionNarrative}{narrativeFeedback}\n\n[color=yellow]● 士气变化：{(finalMoraleDelta >= 0 ? "+" : "")}{finalMoraleDelta}[/color]\n[color=yellow]● 忠诚变化：{(loyaltyDelta >= 0 ? "+" : "")}{loyaltyDelta}[/color]\n[color=green]● 皇权变化：{(imperialPowerDelta >= 0 ? "+" : "")}{imperialPowerDelta}[/color]\n[color=red]● 【{officer.Name}】搜刮中饱：+{siphonedAmount} 万钱 (滚入赃款私蓄，朝堂权势 +2)[/color]";
        
        return result;
    }

    // 宣政殿大朝赈灾
    public TurnResult ExecuteDisasterReliefAction(int reliefAmount, string officerId)
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("只能在宣政殿大朝会上商议开仓赈灾！");

        if (!_state.Npcs.TryGetValue(officerId, out var officer))
            throw new ArgumentException("朝堂无此大臣领旨！", nameof(officerId));

        var result = new TurnResult();

        if (reliefAmount > _state.Treasury)
        {
            result.StoryText = "【赈灾中止】\n\n大司农跪奏：国库空空如也，连一万钱也拿不出来！赈灾下旨流产，天下大哗，灾民流离失所！\n\n[color=red]● 天下民心: -10[/color]";
            _state.PopularSupport = Math.Clamp(_state.PopularSupport - 10, 0, 100);
            return result;
        }

        _state.Treasury -= reliefAmount;

        double corruptionRate = (officer.Corruption / 100.0) * 0.75;
        int siphonedAmount = (int)(reliefAmount * corruptionRate);

        // 对接评估器
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > reliefAmount) siphonedAmount = reliefAmount;

        int actualReliefReceived = reliefAmount - siphonedAmount;
        officer.StashedWealth += siphonedAmount;

        double reliefNeed = 1000.0;
        double performanceRatio = actualReliefReceived / reliefNeed;

        string narrativeFeedback = "";
        int supportDelta = 0;
        int imperialPowerDelta = 0;

        string corruptionNarrative = "";
        if (siphonedAmount > 0)
        {
            corruptionNarrative = $"[color=red]钦差大臣【{officer.Name}】领御旨赈灾，从中雁过拔毛，侵吞扣下了 {siphonedAmount} 万钱并存入个人赃款私蓄！实际运抵灾区的粮草仅折合 {actualReliefReceived} 万钱。[/color]\n\n";
        }

        if (actualReliefReceived < reliefNeed)
        {
            supportDelta = (int)(-15 * (1.0 - performanceRatio));
            imperialPowerDelta = -3;
            narrativeFeedback = $"【{officer.Name}】贪婪成性，层层盘剥，运往灾区的米粮多是掺了沙子的陈米米糠。灾民食之腹胀而死，白骨露于野，千里无鸡鸣。天下百姓指天痛骂昏君奸臣。";
        }
        else
        {
            supportDelta = (int)(12 * performanceRatio);
            imperialPowerDelta = 3;
            narrativeFeedback = $"【{officer.Name}】总算将一车车饱满的谷物运抵灾区，设粥厂、施医药。数万嗷嗷待哺的灾民得保一命。百姓无不面向京洛叩首高呼万岁，盛赞当今天子圣明。";
        }

        // 对接评估器：经天纬地/擅长民政/爱民如子/亲民温和/豪奢/铺张
        double supportMultiplier = NpcTraitEvaluator.GetDisasterReliefSupportMultiplier(officer);
        int finalSupportDelta = (int)(supportDelta * supportMultiplier);

        _state.PopularSupport = Math.Clamp(_state.PopularSupport + finalSupportDelta, 0, 100);
        _state.ImperialPower = Math.Clamp(_state.ImperialPower + imperialPowerDelta, 0, 100);

        // 指派钦差开仓赈灾，执掌朝廷大宗财税，该钦差大臣朝堂权势 +5（不因贪污资金暴涨）
        officer.Power = Math.Clamp(officer.Power + 5, 0, 100);

        _state.AddToChronicle($"【朝会】天子命【{officer.Name}】开仓赈灾，拨发国库 {reliefAmount} 万钱。被侵吞扣下 {siphonedAmount} 万钱！灾民实际得 {actualReliefReceived} 万钱。天下民心变化: {finalSupportDelta}。");

        result.StoryText = $"【大朝会 · 开仓赈灾】\n\n朝廷拨出：{reliefAmount} 万钱\n{corruptionNarrative}{narrativeFeedback}\n\n[color=yellow]● 民心指数：{(finalSupportDelta >= 0 ? "+" : "")}{finalSupportDelta}[/color]\n[color=green]● 朝廷皇权：{(imperialPowerDelta >= 0 ? "+" : "")}{imperialPowerDelta}[/color]\n[color=red]● 【{officer.Name}】中饱私囊：+{siphonedAmount} 万钱 (领旨钦差，朝堂权势 +5)[/color]";

        return result;
    }

    // 🏛️ 历史向深层优化：借刀诬陷、官员漂没抄家款、国税拆分与民心提升系统
    public TurnResult ExecuteConfiscationAction(string targetMinisterId, string destination)
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("只有在宣政殿才能当朝宣布抄家圣旨！");

        if (!_state.Npcs.TryGetValue(targetMinisterId, out var target))
            throw new ArgumentException("朝堂上并无此大臣！", nameof(targetMinisterId));

        var result = new TurnResult();

        // 1. 天子假心诬陷。需要指派一位近臣（忠诚度>=55，廉洁度较高）作为钦差经办抄家
        NpcState? framer = null;
        foreach (var pair in _state.Npcs)
        {
            if (pair.Key != targetMinisterId && pair.Value.Favorability >= 55 && pair.Value.Corruption < 40)
            {
                framer = pair.Value;
                break;
            }
        }

        if (framer == null)
        {
            result.StoryText = $"【抄家受阻】\n\n天子正欲罗织罪名籍没[b]【{target.Name}】[/b]，然而环顾宣政殿，台下群臣面面相觑，竟无一人出列附和弹劾！\n\n[color=red]大将军和十常侍党羽冷笑连连，朝堂空气冰冷。天子深感孤立无援，御口一塞，只得作罢。[/color]\n\n[color=red]● 朝廷皇权：-5 (威信受损，政令不出宫门)[/color]";
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - 5, 0, 100);
            _state.AddToChronicle($"【御难】天子欲惩办【{target.Name}】，因朝堂上无近臣出列弹劾揭发，抄家流产。");
            return result;
        }

        // 2. 官员抄家款的层层克扣漂没（钦差大臣也会在抄家大宗钱财中捞一把！）
        // 没收的总资产
        int rawWealth = target.StashedWealth;
        // 经办钦差大臣的贪腐度会直接侵吞抄家款！侵吞比例：钦差贪腐系数 * 40%
        double framerSiphonRate = (framer.Corruption / 100.0) * 0.40;
        int framerEmbezzled = (int)(rawWealth * framerSiphonRate);
        int wealthLeftAfterFramer = rawWealth - framerEmbezzled; // 扣除经办官侵吞后真正运回朝廷的资产

        // 钦差发一笔横财，私蓄增长
        framer.StashedWealth += framerEmbezzled;
        // 钦差代表天子执行抄家（彰显雷霆皇权特权），权势稳健上升 +3（不因分赃漂没资金暴涨）
        framer.Power = Math.Clamp(framer.Power + 3, 0, 100);

        // 3. 抄家资金国家财政拆分（天子不能全拿，大部分充归国家财政）
        // 无论玩家选择"国库"还是"西园私库"，真正解押归库的资产在国库与内库之间按历史律法进行硬分配：
        // 70% 必须充入朝廷国库 (Treasury) 以供国家运转与大政，天子内库只能暗中中饱 30% 到西园私库 (PrivateTreasury)！
        int amountToTreasury = (int)(wealthLeftAfterFramer * 0.70);
        int amountToPrivate = wealthLeftAfterFramer - amountToTreasury;

        _state.Treasury = Math.Clamp(_state.Treasury + amountToTreasury, 0, 999999);
        _state.PrivateTreasury = Math.Clamp(_state.PrivateTreasury + amountToPrivate, 0, 999999);

        // 4. 没收大贪官浮财，铲除国贼，极大地鼓舞天下民心！
        // 民心提升度：直接取决于没收的真实赃款总额。每约 333 万钱，民心跃升 +1 (最高提升 15 点)！
        int supportDelta = Math.Clamp(rawWealth / 333, 1, 15);

        // 5. 判定党羽抗议弹劾政治反噬
        bool cliqueBacklash = (target.Power >= 60);

        string corruptionLossText = "";
        if (framerEmbezzled > 0)
        {
            corruptionLossText = $"[color=red]● 经办贪腐：抄家钦差【{framer.Name}】上下抽水，中饱私囊扣下了 {framerEmbezzled} 万钱！[/color]\n";
        }

        string splitLootText = $"[color=green]● 籍没分配：朝廷国库进账 +{amountToTreasury} 万钱 (占70%)，天子西园私库进账 +{amountToPrivate} 万钱 (占30%)[/color]";

        // 对接评估器
        int finalPowerLoss = NpcTraitEvaluator.GetConfiscationImperialPowerLoss(framer, target);

        if (cliqueBacklash)
        {
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - finalPowerLoss, 0, 100);

            // 双方党羽反噬下：无论是否清官，铲除巨蠹本身提振民心，但党羽肆虐导致净效果扣减 8 点
            int netSupportDelta;
            if (target.Traits.Contains("清正廉洁"))
            {
                netSupportDelta = -20; // 构陷清官，引爆天下清议，民心暴跌
            }
            else
            {
                netSupportDelta = supportDelta - 8; // 惩治贪官得民心，但党羽抵制抵消部分
            }
            _state.PopularSupport = Math.Clamp(_state.PopularSupport + netSupportDelta, 0, 100);

            target.StashedWealth = 0;
            target.Power = Math.Clamp(target.Power - 30, 0, 100);
            target.Favorability = Math.Clamp(target.Favorability - 40, 0, 100);

            _state.AddToChronicle($"【政潮】近臣【{framer.Name}】诬陷弹劾【{target.Name}】。虽遭其党羽抵制，最终成功抄其家。民心变动: {netSupportDelta}。");

            result.StoryText = $"【🚨 朝堂风暴 · 强行抄家】\n\n宣政殿上，近臣[b]【{framer.Name}】[/b]突然出列弹劾[b]【{target.Name}】[/b]暗通外藩、意图谋反。\n\n由于其权势滔天，十常侍和大将军党羽人人自危，在宣政殿上发起疯狂弹劾抵制：\"陛下诬陷开国元勋，老臣不服！\"天子强令御林军缉拿，查封府邸。\n\n{corruptionLossText}{splitLootText}\n\n[color=red]● 朝堂反噬：政令招致官吏阶层大恐慌，朝廷皇权暴跌 -{finalPowerLoss}[/color]\n[color=yellow]● 百姓拍手：铲除朝廷巨蠹，天下民心净变动 {(netSupportDelta >= 0 ? "+" : "")}{netSupportDelta}[/color]\n[color=green]● 【{framer.Name}】执掌抄家：中饱漂没 +{framerEmbezzled} 万钱，其朝堂权势稳步上升 +3[/color]";
        }
        else
        {
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + 8, 0, 100);

            // 无党羽反噬：直接应用民心变动
            int netSupportDelta = target.Traits.Contains("清正廉洁") ? -20 : supportDelta;
            _state.PopularSupport = Math.Clamp(_state.PopularSupport + netSupportDelta, 0, 100);

            target.StashedWealth = 0;
            target.Power = Math.Clamp(target.Power - 40, 0, 100);
            target.Favorability = Math.Clamp(target.Favorability - 30, 0, 100);

            _state.AddToChronicle($"【抄家】近臣【{framer.Name}】罗织罪名诬陷【{target.Name}】并籍没家产。天下民心大振 +{netSupportDelta}。");

            result.StoryText = $"【🏛️ 帝王心术 · 诬陷籍没】\n\n大朝会上，近臣[b]【{framer.Name}】[/b]心领神会，高声揭发[b]【{target.Name}】[/b]结党营私、收受贿赂。陛下朱批定罪，御林军重甲封锁其府邸查抄！\n\n由于对方毫无党羽，满朝群臣噤若寒蝉。查抄所得大快人心！\n\n{corruptionLossText}{splitLootText}\n\n[color=green]● 皇权震慑：巧妙施展手腕清除异己，皇权提升 +8[/color]\n[color=green]● 惩奸除恶：惩治巨贪，天下百姓无不额手称庆，天下民心大幅攀升 +{netSupportDelta}[/color]\n[color=green]● 钦差办案：经办官【{framer.Name}】中饱漂没 +{framerEmbezzled} 万钱，朝堂权势稳步上升 +3[/color]";
        }

        return result;
    }

    public TurnResult ExecuteQuickAction(string actionId)
    {
        var result = new TurnResult();

        if (actionId == "sell_office" && _state.CurrentLocation == "西园")
        {
            _state.PrivateTreasury = Math.Clamp(_state.PrivateTreasury + 1000, 0, 999999);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - 3, 0, 100); 
            _state.AddToChronicle("【西园】皇帝下旨拍卖东郡太守一职，得钱一千万钱，悉数运入西园天子私库。");
            
            result.StoryText = "【西园鬻官】\n\n陛下端坐在西园精舍中，亲自朱笔御批，将并州刺史、东郡太守等要职明码标价，引得四方豪商、世家庶子趋之若鹜。抬着真金白银的箱子在西园外排成长龙。\n\n[color=green]● 天子西园私库：+1000 万钱 (得钱一千万)[/color]\n[color=red]● 朝廷皇权声望：-3 (买官鬻爵，纲纪败坏，民心不稳)[/color]";
        }
        else if (actionId == "harem_rest" && _state.CurrentLocation == "后宫")
        {
            int extraHealth = 0;
            int imperialPowerDelta = 0;

            foreach (var npc in _state.Npcs.Values)
            {
                if (npc.IsActive)
                {
                    if (npc.Traits.Contains("谄媚专权"))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 15, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 5, 0, 100);
                        extraHealth += 5;
                    }
                    if (npc.Traits.Contains("会拍马屁"))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 6, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 2, 0, 100);
                        extraHealth += 2;
                    }
                    if (npc.Traits.Contains("医术高明"))
                    {
                        extraHealth += 8;
                    }
                    if (npc.Traits.Contains("懂点医理"))
                    {
                        extraHealth += 3;
                    }
                    if (npc.Traits.Contains("喜好清谈"))
                    {
                        extraHealth += 2;
                        imperialPowerDelta -= 1; // 清谈误国，扣减 1 点皇权
                    }
                }
            }

            _state.Health = Math.Clamp(_state.Health + 10 + extraHealth, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + imperialPowerDelta, 0, 100);
            
            _state.AddToChronicle("【后宫】天子龙体困乏，宿于温德殿中调养休息。");
            result.StoryText = $"【后宫春深】\n\n红粉深处，金炉香暖。陛下于温德殿中卸下凡尘政务，临幸嫔妃，调养龙体，顿觉精神爽朗，疲意尽消。\n\n[color=green]● 皇帝健康：+{10 + extraHealth} (龙体充沛)[/color]\n[color=red]● 朝廷皇权：{(imperialPowerDelta != 0 ? imperialPowerDelta.ToString() : "无变动")}[/color]";
        }
        else
        {
            throw new InvalidOperationException("当前场景下，不可执行此动作！");
        }

        return result;
    }

    public async Task NextXunAsync()
    {
        _state.Xun++;
        if (_state.Xun > 3)
        {
            _state.Xun = 1;
            _state.Month++;
            if (_state.Month > 12)
            {
                _state.Month = 1;
                _state.Year++;
            }
        }

        _state.AddToChronicle($"【时间更迭】大汉历纪：{_state.Year}年{_state.Month}月 {(_state.Xun == 1 ? "上旬" : _state.Xun == 2 ? "中旬" : "下旬")}。");

        // 异步后台演进官员想法与天灾日常
        await _scheduler.OrchestrateXunUpdateAsync(_state);
    }

    public string StartGrandCourtSync()
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("未起驾宣政殿，不可开启大朝会！");

        _state.CourtDebateQueue.Clear();

        string primaryIssueText;
        if (_state.PopularSupport < 50)
        {
            primaryIssueText = "【大将军何进上奏】：陛下！今大汉十三州民心凋敝、旱灾肆虐，流民嗷嗷待哺。臣请天子速发国库 3000 万钱赈济灾民，以防黄巾贼党作乱！";
        }
        else
        {
            primaryIssueText = "【常侍张让谄言】：陛下，西园新军扩建在即，内库空虚。奴才建言，可效仿桓帝旧制，在西园公开悬牌卖官，以充实陛下私库，岂不美哉？";
        }

        return primaryIssueText;
    }

    // 同步获取大朝会开幕前三阶段大朝仪情境文案，用作转场遮罩展示
    public List<RitualStageInfo> GetGrandCourtRitualStages()
    {
        return new List<RitualStageInfo>
        {
            new RitualStageInfo {
                StageIndex = 1,
                Title = "【第一仪：起驾换装】",
                Narrative = "陛下在温德殿后暖阁换装。尚衣监、尚冠局太监躬身呈上玄衣纁裳，佩玉大带，头戴天子十二旒冕冠，环佩锵鸣。龙舆启行，天子仪仗往宣政殿进发……"
            },
            new RitualStageInfo {
                StageIndex = 2,
                Title = "【第二仪：百官趋步】",
                Narrative = "宣政殿外朱漆重门訇然大开，晨光破晓，洒满京洛。殿前黄门侍郎扯开嗓子长啼，大将军、十常侍、朝中百官执笏板，按官阶品秩低头趋步入殿，两列金甲羽林肃立，庄严肃穆。"
            },
            new RitualStageInfo {
                StageIndex = 3,
                Title = "【第三仪：静鞭鸣磬】",
                Narrative = "\u201c圣上驾到！\u201d 黄门侍郎高呼，殿上铜磬齐鸣。殿前御史高唱\u201c肃静\u201d，静鞭三响，回音绕梁。满朝文武屏息整肃，面向御台朱漆龙椅深揖，静候陛下驾临御极。"
            }
        };
    }

    public async Task TriggerCourtDebateAsync(string playerInput, string activeOfficerId)
    {
        // 1. 发起 AI 后台异步调度
        var orchestratorResult = await _scheduler.OrchestrateGrandCourtAsync(playerInput, activeOfficerId, _state);

        // 2. 将 AI 智能体群辩结果塞进缓冲区队列
        foreach (var speech in orchestratorResult.Speeches)
        {
            _state.CourtDebateQueue.Enqueue(speech);
        }
    }

    public async Task<TurnResult> ProcessPlayerTurnAsync(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput)) throw new ArgumentException("玩家指令不能为空。", nameof(playerInput));

        // 调用升级后的 OrchestrateGrandCourtAsync 以支持分析意图与多智能体朝辩
        var orchestratorResult = await _scheduler.OrchestrateGrandCourtAsync(playerInput, "he_jin", _state);
        
        var dialogues = new List<MinisterDialogue>();
        foreach (var speech in orchestratorResult.Speeches)
        {
            dialogues.Add(new MinisterDialogue
            {
                MinisterId = speech.MinisterId,
                MinisterName = speech.MinisterName,
                DialogueText = speech.SpeechText,
                FavorabilityChange = speech.ExpectedFavorabilityChange,
                PowerChange = speech.ExpectedPowerChange
            });
        }

        var eventTask = _oracle.CheckRandomEventAsync(_state);
        await eventTask;
        var triggeredEvent = eventTask.Result;

        if (triggeredEvent != null)
        {
            _state.ApplyNumericalDelta(triggeredEvent.ImperialPowerChange, triggeredEvent.TreasuryChange, triggeredEvent.HealthChange);
            _state.AddToChronicle($"【天灾内廷】{triggeredEvent.EventName} - {triggeredEvent.Description}");
        }

        foreach (var dialogue in dialogues)
        {
            if (_state.Npcs.TryGetValue(dialogue.MinisterId, out var minister))
            {
                minister.Favorability = Math.Clamp(minister.Favorability + dialogue.FavorabilityChange, 0, 100);
                minister.Power = Math.Clamp(minister.Power + dialogue.PowerChange, 0, 100);
            }
            _state.AddToChronicle($"【朝堂】{dialogue.MinisterName} 进言: \"{dialogue.DialogueText}\"");
        }

        string story = await _narrator.RenderStoryAsync(playerInput, triggeredEvent, dialogues, _state);

        if (_state.Chronicle.Count % 5 == 0) _state.ReignYear++;

        return new TurnResult { StoryText = story, TriggeredEvent = triggeredEvent, Dialogues = dialogues };
    }
}