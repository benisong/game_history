using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonghanEngine.Core;

// ===== Mock Implementations =====

class MockScheduler : IAIScheduler
{
    public INpcLifecycleManager NpcManager { get; } = new NpcLifecycleManager(new NpcRegistry());
    public bool ShouldAddEdicts { get; set; } = true;

    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult { PrimaryIntent = "POLITICS" };

        // 根据玩家输入生成简单的大臣反应
        if (playerInput.Contains("赈") || playerInput.Contains("灾"))
        {
            result.Speeches.Add(new CourtSpeech { MinisterId = "cao_cao", MinisterName = "曹操", SpeechText = "陛下圣明！赈灾乃安民之本，臣愿领旨督办！", Stance = "AGREED", ExpectedFavorabilityChange = 5, ExpectedPowerChange = 2 });
            result.Speeches.Add(new CourtSpeech { MinisterId = "zhang_rang", MinisterName = "张让", SpeechText = "陛下，国库空虚啊……不如由奴才来经办，定能省下不少银两。", Stance = "OPPOSE", ExpectedFavorabilityChange = -3, ExpectedPowerChange = 0 });
        }
        else if (playerInput.Contains("抄") || playerInput.Contains("诛"))
        {
            result.Speeches.Add(new CourtSpeech { MinisterId = "cao_cao", MinisterName = "曹操", SpeechText = "臣附议！乱臣贼子，人人得而诛之！", Stance = "AGREED", ExpectedFavorabilityChange = 10, ExpectedPowerChange = 5 });
        }
        else if (playerInput.Contains("赏") || playerInput.Contains("升"))
        {
            result.Speeches.Add(new CourtSpeech { MinisterId = "cao_cao", MinisterName = "曹操", SpeechText = "陛下隆恩浩荡！臣定当鞠躬尽瘁！", Stance = "AGREED", ExpectedFavorabilityChange = 15, ExpectedPowerChange = 3 });
        }

        return Task.FromResult(result);
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        state.IntelReports.Add("【暗探密报】大将军何进近日频繁调动北军五营，西园校尉蹇硕忧心忡忡。");

        if (ShouldAddEdicts)
        {
            // 动态生成奏折
            if (state.PopularSupport < 50 && !state.ActiveEdicts.Any(e => e.Title.Contains("赈灾")))
            {
                state.ActiveEdicts.Add(new ImperialEdict
                {
                    Title = "冀州旱灾急折",
                    Type = EdictType.UrgentCrisis,
                    SubmittingNpcId = "cao_cao",
                    NarrativeContent = "冀州大旱三月，赤地千里，流民数十万嗷嗷待哺。请陛下速发国库 2000 万钱赈济灾民！",
                    ExpiryXun = 3,
                    Options = new List<EdictOption>
                    {
                        new() { Description = "准奏！命曹操督办（廉洁可靠）", TreasuryDelta = -2000, PopularSupportDelta = 15, TargetNpcPowerDelta = 5, TargetNpcFavorabilityDelta = 10 },
                        new() { Description = "准奏…但命张让督办（中饱私囊风险极高）", TreasuryDelta = -2000, PopularSupportDelta = -5, TargetNpcPowerDelta = 5, TargetNpcFavorabilityDelta = 5 },
                        new() { Description = "留中不发（贪墨府库不予赈济）", HealthDelta = 0 }
                    }
                });
            }

            if (state.Npcs.ContainsKey("zhang_rang") && !state.ActiveEdicts.Any(e => e.SubmittingNpcId == "zhang_rang"))
            {
                state.ActiveEdicts.Add(new ImperialEdict
                {
                    Title = "十常侍邀功折",
                    Type = EdictType.Merit,
                    SubmittingNpcId = "zhang_rang",
                    TargetNpcId = "zhang_rang",
                    NarrativeContent = "奴才张让叩请陛下，念奴才多年忠心侍奉，赐奴才金帛之赏。",
                    ExpiryXun = 3,
                    Options = new List<EdictOption>
                    {
                        new() { Description = "赏千金（安抚阉党）", TreasuryDelta = -100, TargetNpcFavorabilityDelta = 10 },
                        new() { Description = "驳斥：贪得无厌！（削减其权势）", TargetNpcFavorabilityDelta = -20, TargetNpcPowerDelta = -5 }
                    }
                });
            }
        }

        return Task.CompletedTask;
    }
}

class MockOracle : IEventOracle
{
    public Task<OracleEvent?> CheckRandomEventAsync(GameState state)
    {
        // 5% 概率触发随机事件
        if (Random.Shared.Next(0, 100) < 5)
        {
            var events = new[]
            {
                new OracleEvent { EventName = "洛阳地震", Description = "洛阳突发地动，房屋倒塌百余间，百姓惊恐万分。", ImperialPowerChange = -3, TreasuryChange = -100, HealthChange = 0 },
                new OracleEvent { EventName = "边关告捷", Description = "凉州边境小胜，士气大振。", ImperialPowerChange = 2, TreasuryChange = 0, HealthChange = 0 },
                new OracleEvent { EventName = "天子偶感风寒", Description = "陛下近日龙体欠安，太医诊断需静养。", ImperialPowerChange = 0, TreasuryChange = 0, HealthChange = -5 },
            };
            return Task.FromResult<OracleEvent?>(events[Random.Shared.Next(events.Length)]);
        }
        return Task.FromResult<OracleEvent?>(null);
    }
}

class MockMinisterAgent : IMinisterAgent
{
    public Task<List<MinisterDialogue>> TalkToMinistersAsync(List<string> activeMinisters, string playerInput, GameState state)
    {
        var list = new List<MinisterDialogue>();
        foreach (var mId in activeMinisters)
        {
            if (state.Npcs.TryGetValue(mId, out var npc))
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = mId,
                    MinisterName = npc.Name,
                    DialogueText = npc.Favorability > 60 ? "陛下圣明！臣愿效犬马之劳！" : "陛下，此事还需从长计议……",
                    FavorabilityChange = npc.Favorability > 60 ? 5 : -2,
                    PowerChange = npc.Favorability > 60 ? 2 : 0
                });
            }
        }
        return Task.FromResult(list);
    }
}

class MockNarrator : INarrator
{
    public Task<string> RenderStoryAsync(string playerInput, OracleEvent? triggeredEvent, List<MinisterDialogue> ministerDialogues, GameState state)
    {
        var story = $"══════════════════════════════════\n";
        story += $"  圣旨朱批：「{playerInput}」\n";
        story += $"══════════════════════════════════\n\n";

        if (triggeredEvent != null)
        {
            story += $"⚡ 天降异象：{triggeredEvent.EventName}\n   {triggeredEvent.Description}\n\n";
        }

        foreach (var dial in ministerDialogues)
        {
            story += $"  【{dial.MinisterName}】{dial.DialogueText}\n";
        }

        story += $"\n皇帝缓缓靠在龙椅上。朝堂波诡云谲，陛下今日的朱批将悄然重构这危如累卵的天下……";
        return Task.FromResult(story);
    }
}

// ===== Console Game =====

class DonghanConsole
{
    static GameEngine? _engine;
    static GameState? _state;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "东汉末年汉灵帝 · 光和七年";

        // Init
        _state = new GameState();
        var scheduler = new MockScheduler();
        var oracle = new MockOracle();
        var ministerAgent = new MockMinisterAgent();
        var narrator = new MockNarrator();
        _engine = new GameEngine(_state, scheduler, oracle, ministerAgent, narrator);

        // Opening scroll
        PrintBanner();
        Console.WriteLine("光和七年（中平元年），四月上旬。");
        Console.WriteLine("外戚何进专权，宦官张让秉政，天下百姓疾苦。");
        Console.WriteLine("大汉江山风雨飘摇，陛下，您将如何执掌朝政？\n");
        Console.WriteLine("按任意键开始……");
        Console.ReadKey(true);

        // Game loop
        bool running = true;
        while (running)
        {
            if (_state.Health <= 0)
            {
                Console.Clear();
                Console.WriteLine("\n\n    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("       汉灵帝刘宏，崩于光和七年。");
                Console.WriteLine("       享年……天命已尽。");
                Console.WriteLine("    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                running = false;
                break;
            }

            if (_state.PopularSupport <= 5)
            {
                Console.Clear();
                Console.WriteLine("\n    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("      天下民心尽失！黄巾军攻入洛阳！");
                Console.WriteLine("      大厦倾覆，汉室……亡了。");
                Console.WriteLine("    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                running = false;
                break;
            }

            ShowMainMenu();
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.D1: DoDisasterRelief(); break;
                case ConsoleKey.D2: await DoGrandCourt(); break;
                case ConsoleKey.D3: DoConfiscate(); break;
                case ConsoleKey.D4: DoReviewEdicts(); break;
                case ConsoleKey.D5: ShowMinisters(); break;
                case ConsoleKey.D6: DoArmyDrill(); break;
                case ConsoleKey.D7: DoSellOffice(); break;
                case ConsoleKey.D8: DoHaremRest(); break;
                case ConsoleKey.D9: DoShowIntel(); break;
                case ConsoleKey.P: ShowProvinces(); break;
                case ConsoleKey.A: DoAssignGovernor(); break;
                case ConsoleKey.U: DoSuppressRebellion(); break;
                case ConsoleKey.I: DoPacifyRebellion(); break;
                case ConsoleKey.N: await DoNextXun(); break;
                case ConsoleKey.T: DoTravel(); break;
                case ConsoleKey.S: ShowState(); break;
                case ConsoleKey.Q: running = false; break;
            }
        }

        Console.WriteLine("\n退出《东汉末年汉灵帝》。江山永在，只是换了人间。\n");
    }

    static void PrintBanner()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
    ╔══════════════════════════════════════╗
    ║       東 漢 末 年 · 漢 靈 帝        ║
    ║         光 和 七 年 春              ║
    ║    ── 大朝会、西园与多智能体 ──       ║
    ╚══════════════════════════════════════╝
");
        Console.ResetColor();
    }

    static void PrintDivider() => Console.WriteLine(new string('─', 50));

    static void ShowState()
    {
        Console.Clear();
        PrintBanner();
        Console.WriteLine($"  年号：{_state!.ReignTitle}{_state.ReignYear}年  |  {_state.Year}年{_state.Month}月{(_state.Xun == 1 ? "上" : _state.Xun == 2 ? "中" : "下")}旬");
        Console.WriteLine($"  当前：{_state.CurrentLocation}");
        PrintDivider();

        Console.ForegroundColor = _state.ImperialPower < 30 ? ConsoleColor.Red : ConsoleColor.White;
        Console.WriteLine($"  皇    权：{_state.ImperialPower,3} / 100");
        Console.ForegroundColor = _state.Health < 40 ? ConsoleColor.Red : ConsoleColor.White;
        Console.WriteLine($"  皇帝健康：{_state.Health,3} / 100");
        Console.ForegroundColor = _state.PopularSupport < 30 ? ConsoleColor.Red : ConsoleColor.White;
        Console.WriteLine($"  天下民心：{_state.PopularSupport,3} / 100");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  朝廷国库：{_state.Treasury,5} 万钱");
        Console.WriteLine($"  西园私库：{_state.PrivateTreasury,5} 万钱");
        Console.WriteLine($"  西园新军：{_state.WestGardenArmy.Size} 人 | 士气 {_state.WestGardenArmy.Morale} | 忠诚 {_state.WestGardenArmy.Loyalty}");
        PrintDivider();

        // Active edicts
        if (_state.ActiveEdicts.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ 待批奏折：{_state.ActiveEdicts.Count} 封");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var e in _state.ActiveEdicts)
            {
                string typeTag = e.Type switch
                {
                    EdictType.UrgentCrisis => "[急报]",
                    EdictType.Impeachment => "[弹劾]",
                    EdictType.Merit => "[邀功]",
                    EdictType.Remonstrance => "[劝诫]",
                    _ => "[奏折]"
                };
                Console.WriteLine($"    {typeTag} {e.Title} (剩余{e.ExpiryXun}旬)");
            }
            PrintDivider();
        }

        // Recent chronicle
        var recent = _state.Chronicle.TakeLast(5).ToList();
        if (recent.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  近期起居注：");
            foreach (var c in recent)
                Console.WriteLine($"    {c}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("\n按任意键返回……");
        Console.ReadKey(true);
    }

    static void ShowMainMenu()
    {
        Console.Clear();
        ShowStateQuick();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ═══ 宣政殿 ═══");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  1. 开仓赈灾        2. 召集大朝会");
        Console.WriteLine("  3. 抄家籍没        4. 批阅奏折");
        Console.WriteLine("  5. 查看大臣");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ═══  西园  ═══");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  6. 阅兵发饷        7. 西园卖官");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ═══  后宫  ═══");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  8. 临幸调养");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  ═══  廷议  ═══");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  9. 密札情报        P. 郡县治理");
        Console.WriteLine("  A. 任命地方官      U. 军事平叛");
        Console.WriteLine("  I. 安抚平叛        N. 推进一旬");
        Console.WriteLine("  T. 起驾换场景      S. 国势总览      Q. 退位");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\n  陛下请吩咐 > ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void ShowStateQuick()
    {
        PrintBanner();
        Console.WriteLine($"  {_state!.ReignTitle}{_state.ReignYear}年{_state.Month}月{(_state.Xun == 1 ? "上" : _state.Xun == 2 ? "中" : "下")}旬  |  {_state.CurrentLocation}");
        Console.Write($"  皇权:{_state.ImperialPower,2}  ");
        Console.ForegroundColor = _state.Health < 35 ? ConsoleColor.Red : ConsoleColor.Gray;
        Console.Write($"健康:{_state.Health,2}  ");
        Console.ForegroundColor = _state.PopularSupport < 30 ? ConsoleColor.Red : ConsoleColor.Gray;
        Console.Write($"民心:{_state.PopularSupport,2}  ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"国库:{_state.Treasury}万  私库:{_state.PrivateTreasury}万");
        if (_state.ActiveEdicts.Count > 0) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"  ⚠{_state.ActiveEdicts.Count}折"); }
        var rebelling = _state.Provinces.Values.Where(p => p.IsRebelling).ToList();
        if (rebelling.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            var names = string.Join("、", rebelling.Select(p => p.Name));
            Console.Write($"  ⚡{names}叛乱");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    // === Actions ===

    static void DoDisasterRelief()
    {
        Console.Clear();
        Console.WriteLine("═══ 宣政殿 · 大朝赈灾 ═══\n");
        Console.Write("拨发银两（万钱）：");
        if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0) { Console.WriteLine("\n数额不合法。"); Console.ReadKey(true); return; }

        Console.WriteLine("\n指派钦差：");
        var candidates = _state!.Npcs.Values.Where(n => n.IsActive && n.Faction != "割据军阀").ToList();
        for (int i = 0; i < candidates.Count; i++)
            Console.WriteLine($"  [{i + 1}] {candidates[i].Name} ({candidates[i].Title}) 贪腐:{candidates[i].Corruption}");
        Console.Write("选择 [1-{0}]：", candidates.Count);
        if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 1 || sel > candidates.Count) { Console.WriteLine("\n无效选择。"); Console.ReadKey(true); return; }

        try
        {
            var result = _engine!.ExecuteDisasterReliefAction(amount, candidates[sel - 1].Id);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static async Task DoGrandCourt()
    {
        Console.Clear();
        try
        {
            string issue = _engine!.StartGrandCourtSync();
            Console.WriteLine("═══ 宣政殿 · 大朝会 ═══\n");
            Console.WriteLine("【大朝仪三段式转场】");
            foreach (var stage in _engine.GetGrandCourtRitualStages())
            {
                Console.WriteLine($"\n  {stage.Title}");
                Console.WriteLine($"  {stage.Narrative}");
                await Task.Delay(800);
            }
            Console.WriteLine($"\n══════════════════════════════════");
            Console.WriteLine(issue);
            Console.WriteLine("══════════════════════════════════\n");
            Console.Write("陛下御批 > ");
            string input = Console.ReadLine() ?? "";

            await _engine.TriggerCourtDebateAsync(input, "he_jin");
            Console.WriteLine("\n【群臣辩论】");
            while (_state!.CourtDebateQueue.Count > 0)
            {
                var speech = _state.CourtDebateQueue.Dequeue();
                Console.WriteLine($"  [{speech.Stance}] {speech.MinisterName}：{speech.SpeechText}");
            }
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoConfiscate()
    {
        Console.Clear();
        Console.WriteLine("═══ 宣政殿 · 抄家籍没 ═══\n");

        var targets = _state!.Npcs.Values.Where(n => n.IsActive && n.StashedWealth > 0).ToList();
        if (targets.Count == 0) { Console.WriteLine("朝中暂无有产之臣可抄。"); Console.ReadKey(true); return; }

        for (int i = 0; i < targets.Count; i++)
            Console.WriteLine($"  [{i + 1}] {targets[i].Name} ({targets[i].Title}) 贪腐:{targets[i].Corruption} 私蓄:{targets[i].StashedWealth}万");

        Console.Write($"\n选择目标 [1-{targets.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 1 || sel > targets.Count) { Console.WriteLine("\n无效选择。"); Console.ReadKey(true); return; }

        try
        {
            var result = _engine!.ExecuteConfiscationAction(targets[sel - 1].Id, "国库");
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoReviewEdicts()
    {
        Console.Clear();
        Console.WriteLine("═══ 批阅奏折 ═══\n");

        if (_state!.ActiveEdicts.Count == 0)
        {
            Console.WriteLine("案头干净，暂无待批奏折。推进一旬（N）后可能会有新折。");
            Console.ReadKey(true); return;
        }

        for (int i = 0; i < _state.ActiveEdicts.Count; i++)
        {
            var e = _state.ActiveEdicts[i];
            string typeTag = e.Type switch
            {
                EdictType.UrgentCrisis => "[急报]",
                EdictType.Impeachment => "[弹劾]",
                EdictType.Merit => "[邀功]",
                _ => "[奏折]"
            };
            Console.WriteLine($"  [{i + 1}] {typeTag} {e.Title} (剩余{e.ExpiryXun}旬)");
            Console.WriteLine($"      {e.NarrativeContent}");
            Console.WriteLine();
            for (int j = 0; j < e.Options.Count; j++)
                Console.WriteLine($"      [{j + 1}] {e.Options[j].Description}");
            Console.WriteLine();
        }

        Console.Write("选择奏折编号 (或 0 返回)：");
        if (!int.TryParse(Console.ReadLine(), out int eSel) || eSel == 0) return;
        if (eSel < 1 || eSel > _state.ActiveEdicts.Count) { Console.WriteLine("无效。"); Console.ReadKey(true); return; }

        var edict = _state.ActiveEdicts[eSel - 1];
        Console.Write("选择方案：");
        if (!int.TryParse(Console.ReadLine(), out int oSel) || oSel < 1 || oSel > edict.Options.Count) { Console.WriteLine("无效。"); Console.ReadKey(true); return; }

        try
        {
            var result = _engine!.ResolveEdictAction(edict.Id, oSel - 1);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void ShowMinisters()
    {
        Console.Clear();
        Console.WriteLine("═══ 朝堂百官 ═══\n");

        foreach (var npc in _state!.Npcs.Values.Where(n => n.IsActive))
        {
            Console.WriteLine($"  【{npc.Name}】{npc.Title} (Tier {npc.TitleTier})");
            Console.WriteLine($"    派系：{npc.Faction}  |  好感：{npc.Favorability}  |  权势：{npc.Power}");
            Console.WriteLine($"    贪腐：{npc.Corruption}  |  私蓄：{npc.StashedWealth} 万钱");
            Console.WriteLine($"    性格：{npc.Personality}  |  风格：{npc.Style}");
            if (npc.Traits.Count > 0)
                Console.WriteLine($"    特质：{string.Join("、", npc.Traits)}");
            Console.WriteLine();
        }

        Console.WriteLine("按任意键返回……"); Console.ReadKey(true);
    }

    static void DoArmyDrill()
    {
        Console.Clear();
        Console.WriteLine("═══ 西园 · 阅兵发饷 ═══\n");

        Console.Write("犒赏金额（万钱）：");
        if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0) { Console.WriteLine("\n数额不合法。"); Console.ReadKey(true); return; }

        var candidates = _state!.Npcs.Values.Where(n => n.IsActive).ToList();
        Console.WriteLine("\n指派将领：");
        for (int i = 0; i < candidates.Count; i++)
            Console.WriteLine($"  [{i + 1}] {candidates[i].Name} ({candidates[i].Title}) 贪腐:{candidates[i].Corruption}");
        Console.Write($"选择 [1-{candidates.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 1 || sel > candidates.Count) { Console.WriteLine("\n无效。"); Console.ReadKey(true); return; }

        try
        {
            var result = _engine!.ExecuteDrillArmyActionWithOfficer(amount, candidates[sel - 1].Id);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoSellOffice()
    {
        Console.Clear();
        try
        {
            var result = _engine!.ExecuteQuickAction("sell_office");
            Console.WriteLine(result.StoryText);
        }
        catch (Exception ex) { Console.WriteLine($"【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoHaremRest()
    {
        Console.Clear();
        try
        {
            var result = _engine!.ExecuteQuickAction("harem_rest");
            Console.WriteLine(result.StoryText);
        }
        catch (Exception ex) { Console.WriteLine($"【失败】{ex.Message}"); }
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoShowIntel()
    {
        Console.Clear();
        Console.WriteLine("═══ 黄门暗探 · 密札情报 ═══\n");

        if (_state!.IntelReports.Count == 0)
        {
            Console.WriteLine("暂无密报。推进一旬（N）后暗探自会呈上情报。");
        }
        else
        {
            foreach (var report in _state.IntelReports.TakeLast(10))
                Console.WriteLine($"  ◆ {report}");
        }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoTravel()
    {
        Console.Clear();
        Console.WriteLine("═══ 起驾 ═══\n");
        Console.WriteLine("  [1] 宣政殿 — 百官大朝");
        Console.WriteLine("  [2] 西  园 — 私库与禁军");
        Console.WriteLine("  [3] 后  宫 — 温德殿调养");
        Console.WriteLine("  [0] 取消");

        var key = Console.ReadKey(true);
        string target = key.Key switch
        {
            ConsoleKey.D1 or ConsoleKey.NumPad1 => "宣政殿",
            ConsoleKey.D2 or ConsoleKey.NumPad2 => "西园",
            ConsoleKey.D3 or ConsoleKey.NumPad3 => "后宫",
            _ => ""
        };

        if (string.IsNullOrEmpty(target)) return;

        try
        {
            _engine!.TravelToLocation(target);
            string msg = target switch
            {
                "宣政殿" => "\"起驾宣政殿——！\" 内监尖细的高唱声在深宫回荡。陛下登临龙辇，重返宝座。",
                "西园" => "\"起驾西园——！\" 陛下轻车简从，来到了堆满金银私库与新募精锐新军的铁血基地。",
                "后宫" => "\"天子起驾温德殿——！\" 红幔轻摇，莺声燕语。陛下卸下金銮重负，来到帝王私密之所。",
                _ => ""
            };
            Console.WriteLine($"\n{msg}");
        }
        catch (Exception ex) { Console.WriteLine($"\n{ex.Message}"); }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static async Task DoNextXun()
    {
        Console.Clear();
        Console.WriteLine("═══ 时光流转 ═══\n");

        await _engine!.NextXunAsync();

        int totalEdicts = _state!.ActiveEdicts.Count;
        int crisisEdicts = _state.ActiveEdicts.Count(e => e.Type == EdictType.UrgentCrisis);

        Console.WriteLine($"时间推进至 {_state.ReignTitle}{_state.ReignYear}年 {_state.Year}年{_state.Month}月{(_state.Xun == 1 ? "上" : _state.Xun == 2 ? "中" : "下")}旬");
        Console.WriteLine($"\n暗探呈上 {_state.IntelReports.Count} 条密报。");
        Console.WriteLine($"新到奏折 {totalEdicts} 封" + (crisisEdicts > 0 ? $"（其中急报 {crisisEdicts} 封！）" : ""));

        var expired = _state.ActiveEdicts.Where(e => e.ExpiryXun <= 1).ToList();
        if (expired.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"⚠ {expired.Count} 封奏折即将过期！请速批阅！");
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void ShowProvinces()
    {
        Console.Clear();
        Console.WriteLine(_engine!.GetProvinceReport());
        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoAssignGovernor()
    {
        Console.Clear();
        Console.WriteLine("═══ 任命地方官 ═══\n");

        // Show provinces needing governors
        var ungoverned = _state!.Provinces.Values.Where(p => p.GovernorId == null).ToList();
        if (ungoverned.Count == 0)
        {
            Console.WriteLine("各郡均有主官，暂无空缺。");
            Console.ReadKey(true); return;
        }

        Console.WriteLine("无主郡县：");
        for (int i = 0; i < ungoverned.Count; i++)
            Console.WriteLine($"  [{i + 1}] {ungoverned[i].Name}（距京{ungoverned[i].Distance}，民心{ungoverned[i].LocalSupport}）");

        Console.Write($"\n选择郡县 [1-{ungoverned.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int ps) || ps < 1 || ps > ungoverned.Count) return;

        var province = ungoverned[ps - 1];

        // Show available NPCs
        var available = _state.Npcs.Values.Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null).ToList();
        if (available.Count == 0)
        {
            Console.WriteLine("\n朝中无闲散大臣可派！"); Console.ReadKey(true); return;
        }

        Console.WriteLine("\n可选大臣：");
        for (int i = 0; i < available.Count; i++)
        {
            var n = available[i];
            Console.WriteLine($"  [{i + 1}] {n.Name} ({n.Title}) 武力:{n.Martial} 统帅:{n.Leadership} 政治:{n.Politics} 野心:{n.Ambition}");
        }

        Console.Write($"\n选择 [1-{available.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int ns) || ns < 1 || ns > available.Count) return;

        try
        {
            var result = _engine!.AssignGovernor(province.Id, available[ns - 1].Id);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoSuppressRebellion()
    {
        Console.Clear();
        Console.WriteLine("═══ 军事平叛 ═══\n");

        var rebelling = _state!.Provinces.Values.Where(p => p.IsRebelling).ToList();
        if (rebelling.Count == 0)
        {
            Console.WriteLine("天下太平，暂无叛乱。");
            Console.ReadKey(true); return;
        }

        Console.WriteLine("叛乱郡县：");
        for (int i = 0; i < rebelling.Count; i++)
            Console.WriteLine($"  [{i + 1}] {rebelling[i].Name} — {rebelling[i].RebelFaction}（持续{rebelling[i].RebellionMonths}月，距京{rebelling[i].Distance}）");

        Console.Write($"\n选择平叛目标 [1-{rebelling.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int ps) || ps < 1 || ps > rebelling.Count) return;

        var province = rebelling[ps - 1];

        var generals = _state.Npcs.Values.Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null && n.Martial >= 20).ToList();
        if (generals.Count == 0)
        {
            Console.WriteLine("\n朝中无可用将领（需武力≥20且未外派）！");
            Console.ReadKey(true); return;
        }

        Console.WriteLine("\n可选将领：");
        for (int i = 0; i < generals.Count; i++)
        {
            var g = generals[i];
            double combat = NpcTraitEvaluator.GetCombatPower(g);
            Console.WriteLine($"  [{i + 1}] {g.Name} ({g.Title}) 战力:{combat:F0} 武力:{g.Martial} 统帅:{g.Leadership}");
        }

        Console.Write($"\n选择将领 [1-{generals.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int gs) || gs < 1 || gs > generals.Count) return;

        try
        {
            var result = _engine!.SuppressRebellion(province.Id, generals[gs - 1].Id);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }

    static void DoPacifyRebellion()
    {
        Console.Clear();
        Console.WriteLine("═══ 安抚平叛 ═══\n");

        var rebelling = _state!.Provinces.Values.Where(p => p.IsRebelling).ToList();
        if (rebelling.Count == 0)
        {
            Console.WriteLine("天下太平，暂无叛乱。");
            Console.ReadKey(true); return;
        }

        Console.WriteLine("叛乱郡县：");
        for (int i = 0; i < rebelling.Count; i++)
            Console.WriteLine($"  [{i + 1}] {rebelling[i].Name} — {rebelling[i].RebelFaction}（持续{rebelling[i].RebellionMonths}月）");

        Console.Write($"\n选择安抚目标 [1-{rebelling.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int ps) || ps < 1 || ps > rebelling.Count) return;

        var province = rebelling[ps - 1];

        var envoys = _state.Npcs.Values.Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null).ToList();
        if (envoys.Count == 0)
        {
            Console.WriteLine("\n朝中无闲散大臣可派！");
            Console.ReadKey(true); return;
        }

        Console.WriteLine("\n可选特使：");
        for (int i = 0; i < envoys.Count; i++)
        {
            var e = envoys[i];
            double pol = NpcTraitEvaluator.GetPoliticalSkill(e);
            Console.WriteLine($"  [{i + 1}] {e.Name} ({e.Title}) 外交力:{pol:F0} 政治:{e.Politics} 魅力:{e.Charisma} 武力:{e.Martial}");
        }

        Console.Write($"\n选择特使 [1-{envoys.Count}]：");
        if (!int.TryParse(Console.ReadLine(), out int es) || es < 1 || es > envoys.Count) return;

        var envoy = envoys[es - 1];

        // Strategy selection
        Console.WriteLine("\n═══ 选择安抚策略（可多选，空格分隔）═══");
        Console.WriteLine("  [1] 离间（需政治≥50，+15%成功率）");
        Console.WriteLine("  [2] 说服（需魅力≥45，+20%成功率）");
        Console.WriteLine("  [3] 赈灾（拨付钱粮，每500万+8%，上限1500万）");
        Console.WriteLine("  [4] 惩治（需皇权≥35，+15%；皇权<20反而-10%）");
        Console.Write("\n输入选择（如 \"1 3 4\"）：");
        var stratInput = Console.ReadLine()?.Trim() ?? "";

        GameEngine.PacifyStrategy strategies = GameEngine.PacifyStrategy.None;
        int reliefGold = 0;
        var parts = stratInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            switch (part)
            {
                case "1": strategies |= GameEngine.PacifyStrategy.SowDiscord; break;
                case "2": strategies |= GameEngine.PacifyStrategy.Persuade; break;
                case "3":
                    Console.Write("赈灾金额（万钱）：");
                    if (int.TryParse(Console.ReadLine(), out int gold) && gold >= 500)
                    {
                        strategies |= GameEngine.PacifyStrategy.DisasterRelief;
                        reliefGold = gold;
                    }
                    else { Console.WriteLine("金额不足500万，跳过赈灾。"); }
                    break;
                case "4": strategies |= GameEngine.PacifyStrategy.Punish; break;
            }
        }

        if (strategies == GameEngine.PacifyStrategy.None)
        {
            Console.WriteLine("\n未选择任何有效策略。");
            Console.ReadKey(true); return;
        }

        try
        {
            var result = _engine!.PacifyRebellion(province.Id, envoy.Id, strategies, reliefGold);
            Console.WriteLine($"\n{result.StoryText}");
        }
        catch (Exception ex) { Console.WriteLine($"\n【失败】{ex.Message}"); }

        Console.WriteLine("\n按任意键返回……"); Console.ReadKey(true);
    }
}
