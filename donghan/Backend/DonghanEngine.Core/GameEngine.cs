using System;
using System.Collections.Generic;
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

        if (!string.IsNullOrEmpty(edict.TargetNpcId) && _state.Npcs.TryGetValue(edict.TargetNpcId, out var targetNpc))
        {
            targetNpc.Power = Math.Clamp(targetNpc.Power + option.TargetNpcPowerDelta, 0, 100);
            targetNpc.Favorability = Math.Clamp(targetNpc.Favorability + option.TargetNpcFavorabilityDelta, 0, 100);

            // 处理跨级提拔反噬
            if (option.GrantedTitleTierDelta > 0)
            {
                targetNpc.TitleTier = Math.Clamp(targetNpc.TitleTier + option.GrantedTitleTierDelta, 0, 4);
                
                if (option.GrantedTitleTierDelta >= 2)
                {
                    int backlash = 5 * (option.GrantedTitleTierDelta - 1);
                    _state.ImperialPower = Math.Clamp(_state.ImperialPower - backlash, 0, 100);
                    promoBacklashText = $"\n[color=red]● 跨级拔擢反噬：朝野非议，皇权暴跌 -{backlash}点！[/color]";
                }
            }
        }

        _state.ActiveEdicts.Remove(edict);
        _state.AddToChronicle($"【御批】天子批阅《{edict.Title}》，决断：{option.Description}");
        
        result.StoryText = $"【政务决断】\n\n陛下朱批已下。\n{promoBacklashText}";
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


        return new TurnResult { StoryText = story, TriggeredEvent = triggeredEvent, Dialogues = dialogues };
    }

    // Defined in GameEngine.Rebellion.cs
    partial void CheckRebellions();
}
