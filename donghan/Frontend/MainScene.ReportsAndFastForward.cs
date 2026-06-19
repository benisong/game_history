using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void ShowCourtReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Court);
    }

    private void ShowIntelReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Intel);
    }

    private void ShowWestGardenReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.WestGarden);
    }

    private void ShowDocumentReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Document);
    }

    private void ShowWarningReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Warning);
    }

    private void ShowTravelReportPopup(string fallbackTitle, string storyText, string location)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, GetTravelReportSkin(location));
    }

    private void ShowFastForwardDialog()
    {
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome != GameOutcome.Playing) return;

        var panel = new Panel();
        ConfigureCenteredPopupPanel(panel, PopupSkin.Court, new Vector2(580, 420));

        var vBox = CreateActionPopupRoot(panel, 22, 18);

        var title = new Label { Text = "御 批 · 快 进 N 旬" };
        StylePopupTitle(title, PopupSkin.Court);
        vBox.AddChild(title);

        var desc = new Label
        {
            Text = $"将连续推进 N 旬（1-30）。\n" +
                   $"遇以下情形会立即暂停弹奏报：\n" +
                   $"  · 新叛变暴起\n" +
                   $"  · 龙体欠安（健康 ≤ 30）\n" +
                   $"  · 国帑枯竭（国库 ≤ 1000 万钱）\n" +
                   $"  · 触发重大历史事件\n" +
                   $"  · 灵帝崩殂 / 亡国 / 中兴 / 续命\n\n" +
                   $"当前：{_gameState.ReignTitle}{_gameState.ReignYear}年 {_gameState.Year}年{_gameState.Month}月"
        };
        desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        StylePopupBodyText(desc, PopupSkin.Court);
        vBox.AddChild(desc);

        var stepSpin = new SpinBox
        {
            MinValue = 1,
            MaxValue = 30,
            Step = 1,
            Value = 3,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(stepSpin, PopupSkin.Court);
        vBox.AddChild(stepSpin);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Court));
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.Court);
        previewMargin.AddChild(preview);
        vBox.AddChild(previewFrame);

        void RefreshPreview(double value)
        {
            int n = Math.Clamp((int)value, 1, 30);
            preview.Text = $"快进 {n} 旬 ≈ {(n + 2) / 3} 个月。\n" +
                          $"期间可能触发黄巾、何进之死、董卓入京等历史 trigger。";
        }
        stepSpin.ValueChanged += RefreshPreview;
        RefreshPreview(stepSpin.Value);

        var row = CreateActionPopupButtonRow();
        var confirm = new Button { Text = "驾临快进", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var cancel = new Button { Text = "暂缓", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(confirm, ActionButtonSkin.Court);
        StyleSceneActionButton(cancel, ActionButtonSkin.Court);
        row.AddChild(confirm);
        row.AddChild(cancel);
        vBox.AddChild(row);

        confirm.Pressed += () =>
        {
            int n = Math.Clamp((int)stepSpin.Value, 1, 30);
            _windowManager.PopWindow();
            _ = DoFastForwardAsync(n);
        };
        cancel.Pressed += _windowManager.PopWindow;

        PushTemporaryPopup(panel);
    }

    private async Task DoFastForwardAsync(int n)
    {
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome != GameOutcome.Playing) return;

        if (_fastForwardButton != null) _fastForwardButton.Disabled = true;

        try
        {
            int ran = 0;
            int newRebellions = 0;
            int pacifiedRebellions = 0;
            var newRebellionNames = new List<string>();
            var pacifiedNames = new List<string>();
            var historicalEvents = new List<string>();
            bool aborted = false;
            string abortReason = "";
            string criticalTitle = "";

            for (int i = 0; i < n; i++)
            {
                if (_gameState.Outcome != GameOutcome.Playing)
                {
                    aborted = true;
                    abortReason = $"灵帝 {_gameState.GetEmperorAge()} 岁：{_gameEngine.GetOutcomeMessage()}";
                    break;
                }

                var wasRebelling = _gameState.Provinces.Values.Where(p => p.IsRebelling).Select(p => p.Id).ToHashSet();
                int chronicleBefore = _gameState.Chronicle.Count;

                await _gameEngine.NextXunAsync();
                ran++;

                var nowRebelling = _gameState.Provinces.Values.Where(p => p.IsRebelling).Select(p => p.Id).ToHashSet();
                var newlyRebelling = nowRebelling.Except(wasRebelling).ToList();
                var pacified = wasRebelling.Except(nowRebelling).ToList();
                newRebellions += newlyRebelling.Count;
                pacifiedRebellions += pacified.Count;
                newRebellionNames.AddRange(newlyRebelling.Select(id => _gameState.Provinces[id].Name));
                pacifiedNames.AddRange(pacified.Select(id => _gameState.Provinces[id].Name));

                var newChronicle = _gameState.Chronicle.Skip(chronicleBefore).ToList();
                foreach (var entry in newChronicle)
                {
                    if (entry.Contains("黄巾") || entry.Contains("何进") || entry.Contains("董卓"))
                    {
                        historicalEvents.Add(entry);
                    }
                }

                UpdateUI();
                SetAnnualMajorEventBanner();

                if (_gameState.Outcome != GameOutcome.Playing)
                {
                    aborted = true;
                    abortReason = $"灵帝 {_gameState.GetEmperorAge()} 岁：{_gameEngine.GetOutcomeMessage()}";
                    break;
                }
                if (newlyRebelling.Count > 0)
                {
                    aborted = true;
                    abortReason = string.Join("、", newRebellionNames.Distinct()) + " 起兵叛乱！请速平叛。";
                    break;
                }
                if (_gameState.Health <= 30)
                {
                    aborted = true;
                    abortReason = $"龙体欠安（健康 {_gameState.Health}）。请移驾后宫调养。";
                    break;
                }
                if (_gameState.Treasury <= 1000)
                {
                    aborted = true;
                    abortReason = $"国帑枯竭（国库 {_gameState.Treasury} 万钱）。请赈灾/抄家/卖官补充。";
                    break;
                }

                await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【快进 {ran} 旬完毕】");
            sb.AppendLine($"当前：{_gameState.ReignTitle}{_gameState.ReignYear}年 {_gameState.Year}年{_gameState.Month}月{(_gameState.Xun == 1 ? "上" : _gameState.Xun == 2 ? "中" : "下")}旬");
            sb.AppendLine($"皇权：{_gameState.ImperialPower}  健康：{_gameState.Health}  民心：{_gameState.PopularSupport}  国库：{_gameState.Treasury} 万钱");
            if (newRebellions > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚡ 新叛乱：+{newRebellions} 郡（{string.Join("、", newRebellionNames.Distinct())}）");
            }
            if (pacifiedRebellions > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"✓ 平息：-{pacifiedRebellions} 郡（{string.Join("、", pacifiedNames.Distinct())}）");
            }
            if (historicalEvents.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("★ 历史事件：");
                foreach (var ev in historicalEvents)
                    sb.AppendLine($"  · {ev}");
            }

            if (aborted)
            {
                criticalTitle = "快进中止";
                sb.AppendLine();
                sb.AppendLine($"⚠ {abortReason}");
            }
            else if (ran < n)
            {
                criticalTitle = "快进提前结束";
                sb.AppendLine();
                sb.AppendLine($"已推进 {ran}/{n} 旬。");
            }
            else
            {
                criticalTitle = "快进完成";
                sb.AppendLine();
                sb.AppendLine("  一切平稳，未触发临界事件。");
            }

            if (aborted || historicalEvents.Count > 0 || newRebellions > 0)
            {
                ShowWarningReportPopup(criticalTitle, sb.ToString());
            }
            else
            {
                ShowDocumentReportPopup(criticalTitle, sb.ToString());
            }
        }
        catch (Exception ex)
        {
            ShowWarningReportPopup("快进出错", $"【快进出错】\n\n{ex.Message}");
        }
        finally
        {
            if (_fastForwardButton != null) _fastForwardButton.Disabled = false;
        }
    }

    private void ShowStoryReportPopup(string fallbackTitle, string storyText, PopupSkin skin)
    {
        if (string.IsNullOrWhiteSpace(storyText)) return;

        var panel = new Panel();
        panel.Name = "StoryReportPopup";
        ConfigureCenteredPopupPanel(panel, skin, GetReportPopupSize(skin));

        var root = CreateActionPopupRoot(panel, 24, 20);
        root.AddThemeConstantOverride("separation", 12);

        var seal = new Label { Text = GetReportSealText(skin, fallbackTitle) };
        StyleReportSealLabel(seal, skin);
        root.AddChild(seal);

        var title = new Label { Text = ExtractReportTitle(fallbackTitle, storyText) };
        StylePopupTitle(title, skin);
        root.AddChild(title);

        var meta = new Label { Text = BuildReportMetaLine(skin) };
        StyleReportMetaLabel(meta, skin);
        root.AddChild(meta);

        var reportFrame = new Panel
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        reportFrame.AddThemeStyleboxOverride("panel", CreateReportBodyFrameStyle(skin));
        root.AddChild(reportFrame);

        var report = new RichTextLabel
        {
            BbcodeEnabled = true,
            Text = StripLeadingReportTitle(storyText),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            ScrollActive = true
        };
        report.AddThemeFontSizeOverride("normal_font_size", 16);
        report.AddThemeColorOverride("default_color", GetPopupBodyColor(skin));
        SetFullRect(report);
        report.OffsetLeft = 16;
        report.OffsetTop = 14;
        report.OffsetRight = -16;
        report.OffsetBottom = -14;
        reportFrame.AddChild(report);

        var footer = new Label { Text = GetReportFooterText(skin) };
        StyleReportMetaLabel(footer, skin);
        root.AddChild(footer);

        var close = new Button
        {
            Text = GetReportCloseText(skin),
            CustomMinimumSize = new Vector2(0, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StyleSceneActionButton(close, GetActionButtonSkinForPopup(skin));
        close.Pressed += _windowManager.PopWindow;
        root.AddChild(close);

        PushTemporaryPopup(panel);
    }

    private static Vector2 GetReportPopupSize(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => new Vector2(760, 500),
            PopupSkin.Intel => new Vector2(780, 510),
            PopupSkin.WestGarden => new Vector2(760, 500),
            PopupSkin.Document => new Vector2(740, 500),
            PopupSkin.Travel => new Vector2(720, 470),
            PopupSkin.Warning => new Vector2(700, 450),
            _ => new Vector2(720, 460)
        };
    }

    private static string GetReportSealText(PopupSkin skin, string fallbackTitle)
    {
        return skin switch
        {
            PopupSkin.Court => "尚书台 · 百官回奏",
            PopupSkin.Intel => fallbackTitle.Contains("军情") ? "黄门密札 · 军情战报" : "黄门密札 · 州郡回传",
            PopupSkin.WestGarden => "西园密署 · 军簿回报",
            PopupSkin.Document => "御案折匣 · 朱批回奏",
            PopupSkin.Travel => "黄门导驾 · 龙辇奏报",
            PopupSkin.Warning => fallbackTitle.Contains("御史") ? "御史台 · 风闻弹奏" : "黄门短札 · 急奏",
            _ => "内廷奏报"
        };
    }

    private static string BuildReportMetaLine(PopupSkin skin)
    {
        string source = skin switch
        {
            PopupSkin.Court => "来源：宣政殿 / 尚书台",
            PopupSkin.Intel => "来源：黄门密札 / 州郡舆图",
            PopupSkin.WestGarden => "来源：西园别苑 / 天子亲军",
            PopupSkin.Document => "来源：御案折匣 / 奏章朱批",
            PopupSkin.Travel => "来源：龙辇仪仗 / 宫门黄门",
            PopupSkin.Warning => "来源：御史台 / 黄门急奏",
            _ => "来源：内廷"
        };
        return $"{source} ｜ {DateTime.Now:HH:mm:ss}";
    }

    private static string GetReportFooterText(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => "钤印：圣裁已入起居注，百官反应将在后续旬日发酵。",
            PopupSkin.Intel => "钤印：密札已封存，地方风险请继续于黄门密札复核。",
            PopupSkin.WestGarden => "钤印：军簿已登记，兵额、军费与士气变化即时生效。",
            PopupSkin.Document => "钤印：朱批已下，尚书台据此流转卷宗。",
            PopupSkin.Travel => "钤印：导驾已毕，当前驻跸之所已经更新。",
            PopupSkin.Warning => "钤印：此为前置警示，未必消耗本旬行动。",
            _ => "钤印：奏报已收。"
        };
    }

    private static string GetReportCloseText(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => "御览毕 · 收起回奏",
            PopupSkin.Intel => "封存密札",
            PopupSkin.WestGarden => "归档军簿",
            PopupSkin.Document => "合上折匣",
            PopupSkin.Travel => "收起导驾奏报",
            PopupSkin.Warning => "朕已知晓",
            _ => "收起奏报"
        };
    }

    private static void StyleReportSealLabel(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", GetReportAccentColor(skin));
    }

    private static void StyleReportMetaLabel(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.AddThemeFontSizeOverride("font_size", 13);
        label.AddThemeColorOverride("font_color", GetPopupBodyColor(skin).Darkened(skin == PopupSkin.Intel || skin == PopupSkin.Document ? 0.16f : 0.10f));
    }

    private static Color GetReportAccentColor(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Intel => new Color(0.98f, 0.82f, 0.52f, 1.0f),
            PopupSkin.Document => new Color(0.28f, 0.13f, 0.04f, 1.0f),
            _ => GetPopupTitleColor(skin).Lightened(0.08f)
        };
    }

    private static StyleBoxFlat CreateReportBodyFrameStyle(PopupSkin skin)
    {
        var style = skin == PopupSkin.Document || skin == PopupSkin.Intel
            ? CreatePopupParchmentStyle()
            : CreatePopupInnerPanelStyle(skin);
        if (skin == PopupSkin.Intel)
        {
            style.BgColor = new Color(0.760f, 0.650f, 0.430f, 1.0f);
            style.BorderColor = new Color(0.42f, 0.24f, 0.10f, 1.0f);
        }
        if (skin == PopupSkin.Warning)
        {
            style.BorderColor = new Color(0.90f, 0.22f, 0.12f, 1.0f);
            style.SetBorderWidthAll(2);
        }
        return style;
    }

    private static string ExtractReportTitle(string fallbackTitle, string storyText)
    {
        if (!string.IsNullOrWhiteSpace(storyText) && storyText.StartsWith("【"))
        {
            int end = storyText.IndexOf('】');
            if (end > 1) return storyText.Substring(1, end - 1);
        }
        return fallbackTitle;
    }

    private static string StripLeadingReportTitle(string storyText)
    {
        if (!string.IsNullOrWhiteSpace(storyText) && storyText.StartsWith("【"))
        {
            int end = storyText.IndexOf('】');
            if (end >= 0)
            {
                string body = storyText.Substring(end + 1).TrimStart();
                return string.IsNullOrWhiteSpace(body) ? storyText : body;
            }
        }
        return storyText;
    }
}
