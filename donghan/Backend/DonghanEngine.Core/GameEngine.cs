using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public partial class GameEngine
{
    private readonly GameState _state;
    private readonly IAIScheduler _scheduler;
    private readonly IEventOracle _oracle;
    private readonly IMinisterAgent _ministerAgent;
    private readonly INarrator _narrator;
    internal readonly Random _rng;

    public GameEngine(
        GameState state, 
        IAIScheduler scheduler, 
        IEventOracle oracle, 
        IMinisterAgent ministerAgent, 
        INarrator narrator,
        Random? rng = null)
    {
        _state = state;
        _scheduler = scheduler;
        _oracle = oracle;
        _ministerAgent = ministerAgent;
        _narrator = narrator;
        _rng = rng ?? new Random();
    }

    public GameState GetState() => _state;

    // P2-2 朝会主持人：玩家在朝会面板中可切换 4 名主要 NPC（曹操/何进/张让/蹇硕）为主持人
    // 影响：MockScheduler 会把 activeOfficerId 在 result 中的发言移到队首
    // 默认 "he_jin" 保持向后兼容
    public string ActiveOfficerId { get; set; } = "he_jin";

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

        var army = _state.WestGardenArmy;

        if (paidAmount > _state.PrivateTreasury)
        {
            army.Morale = Math.Clamp(army.Morale - 5, 0, 100);
            return new TurnResult { StoryText = BuildInsufficientDrillFundsStory() };
        }

        var settlement = CalculateDrillArmySettlement(paidAmount, officer);
        ApplyDrillArmySettlement(settlement);

        _state.AddToChronicle(BuildDrillArmyChronicle(settlement));
        return new TurnResult { StoryText = BuildDrillArmyStory(settlement) };
    }

    // 西园募兵补军
    public TurnResult ExecuteRaiseWestGardenTroopsAction(int troops)
    {
        const int maxWestGardenArmySize = 12000;
        const int troopsPerBatch = 1000;
        const int costPerBatch = 300;
        const int supportCostPerBatch = 1;

        if (_state.CurrentLocation != "西园")
            throw new InvalidOperationException("只能在西园校场募兵补军！");

        if (troops <= 0 || troops % troopsPerBatch != 0)
            throw new ArgumentException("募兵人数必须为 1000 的正整数倍！", nameof(troops));

        var army = _state.WestGardenArmy;
        int capacity = maxWestGardenArmySize - army.Size;
        if (capacity <= 0)
        {
            return new TurnResult { StoryText = BuildWestGardenArmyFullStory(maxWestGardenArmySize) };
        }

        int actualTroops = Math.Min(troops, capacity);
        int batches = actualTroops / troopsPerBatch;
        int cost = batches * costPerBatch;
        int supportDelta = -(batches * supportCostPerBatch);

        if (cost > _state.Treasury)
        {
            return new TurnResult { StoryText = BuildInsufficientRecruitFundsStory(actualTroops, cost) };
        }

        army.Size += actualTroops;
        army.Morale = Math.Clamp(army.Morale - batches, 0, 100);
        _state.Treasury -= cost;
        _state.PopularSupport = Math.Clamp(_state.PopularSupport + supportDelta, 0, 100);

        _state.AddToChronicle(BuildRaiseWestGardenTroopsChronicle(actualTroops, cost, supportDelta));
        return new TurnResult { StoryText = BuildRaiseWestGardenTroopsStory(actualTroops, cost, supportDelta, army.Size, maxWestGardenArmySize) };
    }

    // 宣政殿大朝赈灾
    public TurnResult ExecuteDisasterReliefAction(int reliefAmount, string officerId)
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("只能在宣政殿大朝会上商议开仓赈灾！");

        if (!_state.Npcs.TryGetValue(officerId, out var officer))
            throw new ArgumentException("朝堂无此大臣领旨！", nameof(officerId));

        if (reliefAmount > _state.Treasury)
        {
            _state.PopularSupport = Math.Clamp(_state.PopularSupport - 10, 0, 100);
            return new TurnResult { StoryText = BuildInsufficientReliefFundsStory() };
        }

        var settlement = CalculateDisasterReliefSettlement(reliefAmount, officer);
        ApplyDisasterReliefSettlement(settlement);

        _state.AddToChronicle(BuildDisasterReliefChronicle(settlement));
        return new TurnResult { StoryText = BuildDisasterReliefStory(settlement) };
    }

    // 🏛️ 历史向深层优化：借刀诬陷、官员漂没抄家款、国税拆分与民心提升系统
    public TurnResult ExecuteConfiscationAction(string targetMinisterId, string destination)
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("只有在宣政殿才能当朝宣布抄家圣旨！");

        if (!_state.Npcs.TryGetValue(targetMinisterId, out var target))
            throw new ArgumentException("朝堂上并无此大臣！", nameof(targetMinisterId));

        var framer = FindConfiscationFramer(targetMinisterId);
        if (framer == null)
        {
            _state.ImperialPower = Math.Clamp(_state.ImperialPower - 5, 0, 100);
            _state.AddToChronicle($"【御难】天子欲惩办【{target.Name}】，因朝堂上无近臣出列弹劾揭发，抄家流产。");
            return new TurnResult { StoryText = BuildFailedConfiscationStory(target) };
        }

        var settlement = CalculateConfiscationSettlement(framer, target);
        ApplyConfiscationSettlement(settlement);

        _state.AddToChronicle(BuildConfiscationChronicle(settlement));
        return new TurnResult { StoryText = BuildConfiscationStory(settlement) };
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
                    if (npc.Traits.Contains(TraitNames.ChanMeiZhuanQuan))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 15, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 5, 0, 100);
                        extraHealth += 5;
                    }
                    if (npc.Traits.Contains(TraitNames.HuiPaiMaPi))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 6, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 2, 0, 100);
                        extraHealth += 2;
                    }
                    if (npc.Traits.Contains(TraitNames.YiShuGaoMing))
                    {
                        extraHealth += 8;
                    }
                    if (npc.Traits.Contains(TraitNames.DongDianYiLi))
                    {
                        extraHealth += 3;
                    }
                    if (npc.Traits.Contains(TraitNames.XiHaoQingTan))
                    {
                        extraHealth += 2;
                        imperialPowerDelta -= 1; // 清谈误国，扣减 1 点皇权
                    }
                }
            }

            int cappedExtraHealth = Math.Min(extraHealth, 15);
            _state.Health = Math.Clamp(_state.Health + 10 + cappedExtraHealth, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + imperialPowerDelta, 0, 100);
            
            _state.AddToChronicle("【后宫】天子龙体困乏，宿于温德殿中调养休息。");
            result.StoryText = $"【后宫春深】\n\n红粉深处，金炉香暖。陛下于温德殿中卸下凡尘政务，临幸嫔妃，调养龙体，顿觉精神爽朗，疲意尽消。\n\n[color=green]● 皇帝健康：+{10 + cappedExtraHealth} (龙体充沛)[/color]\n[color=red]● 朝廷皇权：{(imperialPowerDelta != 0 ? imperialPowerDelta.ToString() : "无变动")}[/color]";
        }
        else
        {
            throw new InvalidOperationException("当前场景下，不可执行此动作！");
        }

        return result;
    }

    public TurnResult ResolveEdictAction(string edictId, int optionIndex)
    {
        var result = new TurnResult();
        var edict = _state.ActiveEdicts.Find(e => e.Id == edictId);
        if (edict == null) throw new ArgumentException("无此奏折！");
        if (optionIndex < 0 || optionIndex >= edict.Options.Count) throw new ArgumentException("无效的御批选项！");

        var option = edict.Options[optionIndex];

        // 基础数值结算
        _state.ApplyNumericalDelta(option.ImperialPowerDelta, option.TreasuryDelta, option.HealthDelta);
        _state.PrivateTreasury = Math.Clamp(_state.PrivateTreasury + option.PrivateTreasuryDelta, 0, 999999);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport + option.PopularSupportDelta, 0, 100);

        string promoBacklashText = "";
        string promotionRelationText = "";

        if (!string.IsNullOrEmpty(edict.TargetNpcId) && _state.Npcs.TryGetValue(edict.TargetNpcId, out var targetNpc))
        {
            var promotionEffects = option.GrantedTitleTierDelta > 0
                ? CalculatePromotionRelationEffects(targetNpc, option.GrantedTitleTierDelta)
                : Array.Empty<PromotionRelationEffect>();

            targetNpc.Power = Math.Clamp(targetNpc.Power + option.TargetNpcPowerDelta, 0, 100);
            targetNpc.Favorability = Math.Clamp(targetNpc.Favorability + option.TargetNpcFavorabilityDelta, 0, 100);

            // 处理跨级提拔反噬
            if (option.GrantedTitleTierDelta > 0)
            {
                targetNpc.TitleTier = Math.Clamp(targetNpc.TitleTier + option.GrantedTitleTierDelta, 0, 4);
                ApplyPromotionRelationEffects(promotionEffects);
                promotionRelationText = BuildPromotionRelationEffectText(promotionEffects);
                
                if (option.GrantedTitleTierDelta >= 2)
                {
                    int backlash = 5 * (option.GrantedTitleTierDelta - 1);
                    _state.ImperialPower = Math.Clamp(_state.ImperialPower - backlash, 0, 100);
                    promoBacklashText = $"\n[color=red]● 跨级拔擢反噬：朝野非议，皇权暴跌 -{backlash}点！[/color]";
                }
            }
        }

        _state.ActiveEdicts.Remove(edict);
        _state.AddToChronicle($"【御批】天子批阅《{edict.Title}》，决断：{option.Description}{(promotionRelationText.Length > 0 ? "，牵动朝中关系网" : "")}");
        
        result.StoryText = $"【政务决断】\n\n陛下朱批已下。\n{promoBacklashText}{promotionRelationText}";
        return result;
    }

    private sealed record PromotionRelationEffect(
        string NpcId,
        string NpcName,
        NpcRelationType Type,
        int Strength,
        int FavorabilityDelta,
        int PowerDelta,
        string Label);

    private IReadOnlyList<PromotionRelationEffect> CalculatePromotionRelationEffects(NpcState promoted, int titleTierDelta)
    {
        return _state.NpcRelations
            .Where(r => r.FromNpcId == promoted.Id || (r.IsMutual && r.ToNpcId == promoted.Id))
            .Select(r => BuildPromotionRelationEffect(r, promoted.Id, titleTierDelta))
            .Where(e => e != null)
            .Select(e => e!)
            .OrderBy(e => e.FavorabilityDelta)
            .ThenByDescending(e => e.Strength)
            .ToList();
    }

    private PromotionRelationEffect? BuildPromotionRelationEffect(NpcRelation relation, string promotedId, int titleTierDelta)
    {
        string affectedId = relation.FromNpcId == promotedId ? relation.ToNpcId : relation.FromNpcId;
        if (!_state.Npcs.TryGetValue(affectedId, out var affected) || !affected.IsActive || affected.IsHostile)
        {
            return null;
        }

        int scaled(int value) => (int)Math.Round(value * Math.Clamp(relation.Strength, 20, 100) / 100.0);
        int favorabilityDelta;
        int powerDelta;

        (favorabilityDelta, powerDelta) = relation.Type switch
        {
            NpcRelationType.Kinship => (scaled(5 + titleTierDelta), titleTierDelta >= 2 ? 1 : 0),
            NpcRelationType.FactionAlly => (scaled(4 + titleTierDelta), titleTierDelta >= 2 ? 1 : 0),
            NpcRelationType.Patronage => (scaled(3 + titleTierDelta), 0),
            NpcRelationType.TeacherStudent => (scaled(3 + titleTierDelta), 0),
            NpcRelationType.SwornBond => (scaled(5 + titleTierDelta), titleTierDelta >= 2 ? 1 : 0),
            NpcRelationType.Command => (scaled(4 + titleTierDelta), titleTierDelta >= 2 ? 1 : 0),
            NpcRelationType.RegionalTie => (scaled(2 + titleTierDelta), 0),
            NpcRelationType.Rivalry => (scaled(-(3 + titleTierDelta)), titleTierDelta >= 2 ? 1 : 0),
            NpcRelationType.Hostility => (scaled(-(4 + titleTierDelta)), titleTierDelta >= 2 ? 1 : 0),
            _ => (scaled(2 + titleTierDelta), 0)
        };

        return new PromotionRelationEffect(
            affected.Id,
            affected.Name,
            relation.Type,
            relation.Strength,
            favorabilityDelta,
            powerDelta,
            relation.Label);
    }

    private void ApplyPromotionRelationEffects(IReadOnlyList<PromotionRelationEffect> effects)
    {
        foreach (var effect in effects)
        {
            if (!_state.Npcs.TryGetValue(effect.NpcId, out var affected) || !affected.IsActive || affected.IsHostile)
            {
                continue;
            }

            affected.Favorability = Math.Clamp(affected.Favorability + effect.FavorabilityDelta, 0, 100);
            affected.Power = Math.Clamp(affected.Power + effect.PowerDelta, 0, 100);
        }
    }

    private static string BuildPromotionRelationEffectText(IReadOnlyList<PromotionRelationEffect> effects)
    {
        if (effects.Count == 0)
        {
            return "";
        }

        var lines = effects.Take(5).Select(e =>
        {
            string favorability = e.FavorabilityDelta == 0 ? "好感无变" : $"好感 {e.FavorabilityDelta:+#;-#;0}";
            string power = e.PowerDelta == 0 ? "" : $"，权势 {e.PowerDelta:+#;-#;0}";
            string color = e.FavorabilityDelta >= 0 ? "green" : "red";
            return $"[color={color}]● {e.Label}牵连：【{e.NpcName}】{favorability}{power}[/color]";
        });

        return "\n\n[color=yellow][b]【关系牵连】[/b][/color]\n" + string.Join("\n", lines);
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
                _state.ReignYear++;
            }
        }

        _state.AddToChronicle($"【时间更迭】大汉历纪：{_state.Year}年{_state.Month}月 {(_state.Xun == 1 ? "上旬" : _state.Xun == 2 ? "中旬" : "下旬")}。");

        // 异步后台演进官员想法与天灾日常
        await _scheduler.OrchestrateXunUpdateAsync(_state);

        // 奏折过期与流产判定
        var expiredEdicts = new List<ImperialEdict>();
        foreach (var edict in _state.ActiveEdicts)
        {
            edict.ExpiryXun--;
            if (edict.ExpiryXun <= 0)
            {
                expiredEdicts.Add(edict);
            }
        }

        foreach (var expired in expiredEdicts)
        {
            _state.ActiveEdicts.Remove(expired);
            // 流产惩罚：如果是急报，民心暴跌
            if (expired.Type == EdictType.UrgentCrisis)
            {
                _state.PopularSupport = Math.Clamp(_state.PopularSupport - 15, 0, 100);
                _state.AddToChronicle($"【国难】《{expired.Title}》留中不发，导致灾情/兵变恶化，民心大跌！");
            }
            else
            {
                _state.ImperialPower = Math.Clamp(_state.ImperialPower - 2, 0, 100);
                _state.AddToChronicle($"【怠政】《{expired.Title}》过期未批，朝堂议论天子怠政。");
            }
        }

        CheckRebellions();

        // P0-3 修复：黄巾起义历史硬 trigger。
        // 史实：184 年 3 月初 5 日，唐周告密，张角被迫提前起事，冀兖豫三州旬月之间皆应。
        // 游戏时间 184 年 4 月（开局当月），第 2 旬强制 3 郡起事，撤换其太守（桥玄/卢植），确保历史
        // 走向不会因"开局皇权 25 / 民心 28 / 冀州支持度 35"被反推取消。
        if (!_state.DisableHistoricalTriggers &&
            _state.Year == 184 && _state.Month == 4 && _state.Xun == 2)
        {
            TriggerHistoricalYellowTurban();
        }

        // P1-A2 修复：189 年何进之死 + 董卓进京
        if (!_state.DisableHistoricalTriggers)
        {
            if (_state.Year == 189 && _state.Month == 8 && _state.Xun == 3)
            {
                TriggerHeJinDeath();
            }
            if (_state.Year == 189 && _state.Month == 9 && _state.Xun == 1)
            {
                TriggerDongZhuoEntry();
            }
        }

        // P0-2 结局判定：每旬结算一次
        UpdateOutcome();

        // P3 事件叙事：每旬检查并追加叙事感文字
        CheckEventNarratives();

        // P1-A3：NPC 按 HistoricalDeathYear 自动下野（不杀"事件触发"型）
        CheckNpcHistoricalDeaths();
    }

    // P3 事件叙事：扫描注册表，时间匹配且未触发过的追加到 Chronicle
    // 与 trigger 硬逻辑（黄巾/何进之死/董卓进京）解耦，仅做"叙事感"补充
    // 不受 DisableHistoricalTriggers 约束：测试可单独关 trigger 硬逻辑但保留叙事层
    private void CheckEventNarratives()
    {
        if (_state.Outcome != GameOutcome.Playing) return;

        foreach (var ev in EventNarratives.FindTriggering(_state.Year, _state.Month, _state.Xun))
        {
            if (_triggeredEventNarratives.Contains(ev.Id)) continue;
            _state.AddToChronicle($"【{ev.Category}·{ev.Title}】{ev.Description}");
            _triggeredEventNarratives.Add(ev.Id);
        }
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
                Narrative = "\"圣上驾到！\" 黄门侍郎高呼，殿上铜磬齐鸣。殿前御史高唱\"肃静\"，静鞭三响，回音绕梁。满朝文武屏息整肃，面向御台朱漆龙椅深揖，静候陛下驾临御极。"
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
        // P2-2 修复：传 ActiveOfficerId（前端可切换），替代写死 "he_jin"
        var orchestratorResult = await _scheduler.OrchestrateGrandCourtAsync(playerInput, ActiveOfficerId, _state);
        
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


        return new TurnResult { StoryText = story, TriggeredEvent = triggeredEvent, Dialogues = dialogues };
    }

    // Defined in GameEngine.Rebellion.cs
    partial void CheckRebellions();

    // === P0-3 黄巾起义历史硬 trigger ===
    // 在 184/4/2 旬强制 冀/兖/豫 三郡同步起事，撤换其太守入京，触发 TriggerYellowTurban 流水线。
    // 守护：已反则跳过；太守离职时清理 GovernedProvinceId 防止孤立状态。
    private void TriggerHistoricalYellowTurban()
    {
        var targetProvinces = new[] { "jizhou", "yanzhou", "yuzhou" };
        foreach (var pid in targetProvinces)
        {
            if (!_state.Provinces.TryGetValue(pid, out var p) || p == null) continue;
            if (p.IsRebelling) continue;

            // 撤换太守：黄巾既起，太守实际被架空或战死，先下放回野
            if (!string.IsNullOrEmpty(p.GovernorId) &&
                _state.Npcs.TryGetValue(p.GovernorId, out var gov))
            {
                gov.GovernedProvinceId = null;
            }
            p.GovernorId = null;

            // 强制 trigger（不依赖 LocalSupport / ImperialPower / PopularSupport）
            TriggerYellowTurban(p);

            _state.AddToChronicle($"【黄巾起事】{p.Name}太平道蜂起响应，渠帅揭竿！州郡震动，太守败走。");
        }
    }

    // === P0-2 结局判定 ===
    // 优先级：崩殂 > 亡国 > 中兴 > 续命（每旬结算，已有结局不再覆盖）
    private void UpdateOutcome()
    {
        if (_state.Outcome != GameOutcome.Playing) return;

        if (_state.Health <= 0)
        {
            _state.Outcome = GameOutcome.Collapse;
            _state.AddToChronicle("【崩殂】龙驭宾天。灵帝驾崩，汉祚倾覆。");
            return;
        }

        if (_state.PopularSupport <= 5)
        {
            _state.Outcome = GameOutcome.Vanquished;
            _state.AddToChronicle("【亡国】黄巾入洛，烽烟遍地，汉鼎崩摧。");
            return;
        }

        int age = _state.GetEmperorAge();
        if (age >= 40)
        {
            int rebelCount = 0;
            foreach (var pv in _state.Provinces.Values)
                if (pv.IsRebelling) rebelCount++;
            if (_state.ImperialPower >= 60 && _state.PopularSupport >= 50 && rebelCount == 0)
            {
                _state.Outcome = GameOutcome.ZhongXing;
                _state.AddToChronicle($"【中兴】灵帝春秋 {_state.GetEmperorAge()} 而皇权复振、天下归心，可比光武再世！");
            }
            else
            {
                _state.Outcome = GameOutcome.XuMing;
                _state.AddToChronicle($"【续命】灵帝春秋 {_state.GetEmperorAge()}，汉祚得以延续。");
            }
        }
    }

    // P0-2：把结局翻译成人话字符串（前端/控制台渲染用）
    public string GetOutcomeMessage()
    {
        return _state.Outcome switch
        {
            GameOutcome.Playing    => "天运转，帝业待续。",
            GameOutcome.ZhongXing  => $"★★★ 中兴之治 ★★★\n灵帝在位 {_state.GetEmperorAge()} 岁，皇权复振、民心归一、海内无叛。\n史官当记：此君可比光武再世！",
            GameOutcome.XuMing     => $"☆ 续命成功 ☆\n灵帝在位 {_state.GetEmperorAge()} 岁，汉祚得以延续。\n虽有遗憾，到底改写了 189 年驾崩的宿命。",
            GameOutcome.Collapse   => $"✗ 崩殂 ✗\n灵帝龙体难支，魂归上苍。\n国无长君，汉祚倾覆。",
            GameOutcome.Vanquished => $"✗ 亡国 ✗\n黄巾入洛，烽烟遍地，汉鼎崩摧。\n天下离心，灵帝沦为阶下之囚。",
            _                      => ""
        };
    }

    // P1-A2 历史 trigger：189 年何进之死 / 董卓进京
    // 幂等标志：A2 trigger 各自只在首次进入目标旬时跑一次
    private bool _heJinDeathTriggered;
    private bool _dongZhuoEntryTriggered;

    // P3 事件叙事：防重复触发（每个 EventNarrative 只能触发一次）
    private readonly HashSet<string> _triggeredEventNarratives = new();

    // === P1-A3 NPC 寿终下野 ===
    // P0 警告：HistoricalDeathYear=史实倾向参考，不强制死亡。
    // 但 P1-A3 决定：到岁数且还活跃的 NPC 自动下野（IsActive=false + 释州郡），
    // 玩家可设法续命（服药延寿等系统若实现），否则按史实节奏谢幕。
    // 不杀"事件触发"型 NPC（董卓/吕布等，他们的退场由 trigger 决定）。
    private void CheckNpcHistoricalDeaths()
    {
        if (_state.DisableHistoricalTriggers) return;
        if (_state.Outcome != GameOutcome.Playing) return; // 结局已定，NPC 退场不写编年史

        foreach (var npc in _state.Npcs.Values)
        {
            if (!npc.IsActive) continue;
            if (!npc.HistoricalDeathYear.HasValue) continue;
            // 事件触发型 NPC 退场由 trigger 决定，不参与自动下野
            if (npc.EntryCondition == "事件触发") continue;

            if (_state.Year >= npc.HistoricalDeathYear.Value)
            {
                var provinceId = npc.GovernedProvinceId;
                npc.IsActive = false;
                npc.DeathReason = $"【寿终】据史实/传统说法，{npc.Name}寿终正寝于公元 {npc.HistoricalDeathYear.Value} 年。";
                npc.GovernedProvinceId = null;
                if (!string.IsNullOrEmpty(provinceId) && _state.Provinces.TryGetValue(provinceId, out var p))
                {
                    p.GovernorId = null;
                }
                _state.AddToChronicle($"【致仕/寿终】{npc.Name}据史实寿终，告退朝堂。");
            }
        }
    }

    // 189 年 8 月下旬：何进被十常侍矫诏伏诛于嘉德殿（189/8/25）
    // 史实：中平六年八月，何进谋诛十常侍，反被张让等矫诏杀害于嘉德殿前。
    private void TriggerHeJinDeath()
    {
        if (_heJinDeathTriggered) return;
        const string npcId = "he_jin";
        if (!_state.Npcs.TryGetValue(npcId, out var he)) return;
        if (!he.IsActive) { _heJinDeathTriggered = true; return; }

        he.IsActive = false;
        he.DeathReason = "【何进之死】中平六年八月，何进谋诛阉宦，反被张让等矫诏伏诛于嘉德殿前。";
        he.Power = 0;

        _state.ImperialPower = Math.Clamp(_state.ImperialPower - 8, 0, 100);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport - 3, 0, 100);

        if (_state.WestGardenArmy != null)
        {
            _state.WestGardenArmy.Morale = Math.Clamp(_state.WestGardenArmy.Morale - 15, 0, 100);
        }

        _state.AddToChronicle("【外戚崩殂】大将军何进被十常侍矫诏伏诛！外戚派群龙无首，朝局剧变。");
        _heJinDeathTriggered = true;
    }

    // 189 年 9 月上旬：董卓率西凉兵进京（189/9/1 ≈ 中平六年九月）
    // 史实：何进死后，京城大乱。袁绍等诛宦官。董卓闻讯，率三千西凉兵入京勤王，
    //       旋即废少帝立献帝，独揽大权。
    private void TriggerDongZhuoEntry()
    {
        if (_dongZhuoEntryTriggered) return;

        // 部署董卓入京（敌对派系）
        if (!_state.Npcs.ContainsKey("dong_zhuo"))
        {
            _scheduler?.NpcManager?.DeployNpcToCourt("dong_zhuo", _state);
        }
        if (!_state.Npcs.TryGetValue("dong_zhuo", out var dong))
        {
            // 部署失败（NPC manager 为 null 等）— 不标记完成，留给下次重试
            return;
        }

        dong.IsActive = true;
        dong.GovernedProvinceId = null;
        dong.Power = 95;       // 独揽朝政
        dong.Favorability = 30; // 视天子为傀儡
        dong.InitialLocation = "洛阳宫中";

        // 废立之事：皇权雪崩，西园军成空头
        _state.ImperialPower = Math.Clamp(_state.ImperialPower - 15, 0, 100);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport - 8, 0, 100);
        _state.Treasury = Math.Clamp(_state.Treasury - 2000, 0, int.MaxValue);

        if (_state.WestGardenArmy != null)
        {
            _state.WestGardenArmy.Morale = Math.Clamp(_state.WestGardenArmy.Morale - 25, 0, 100);
            _state.WestGardenArmy.Loyalty = Math.Clamp(_state.WestGardenArmy.Loyalty - 20, 0, 100);
        }

        _state.AddToChronicle("【董卓入京】凉州军阀董卓率三千西凉兵入京，旋即废少帝立献帝，独揽朝政！");
        _dongZhuoEntryTriggered = true;
    }
}
