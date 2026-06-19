using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void UpdateUI()
    {
        if (_gameState == null) return;

        if (_reignLabel != null)
        {
            _reignLabel.Text = FormatTimeLabel();
        }
        if (_mainTimeLabel != null)
        {
            _mainTimeLabel.Text = FormatTimeLabel();
        }

        HideMainSceneStatusAndNpcChrome();
        SetAnnualMajorEventBanner();

        // 更新起居注/编年史
        if (_chronicleLog != null)
        {
            _chronicleLog.Clear();
            foreach (var record in _gameState.Chronicle)
            {
                _chronicleLog.AppendText(record + "\n");
            }
        }

        // 根据当前所在场景，动态控制右侧控制面板按钮的显示隐藏！
        UpdateSceneButtons();
    }

    private string FormatTimeLabel()
    {
        if (_gameState == null) return string.Empty;

        string xunName = _gameState.Xun switch
        {
            1 => "上旬",
            2 => "中旬",
            3 => "下旬",
            _ => $"第{_gameState.Xun}旬"
        };

        return $"{_gameState.ReignTitle}{_gameState.ReignYear}年 · {_gameState.Year}年{_gameState.Month}月{xunName}";
    }

    private void HideMainSceneStatusAndNpcChrome()
    {
        GetNodeOrNull<Panel>("LeftPanel")?.Hide();
        GetNodeOrNull<Panel>("RightPanel")?.Hide();
        GetNodeOrNull<Panel>("BottomPanel")?.Hide();

        _imperialPowerLabel?.Hide();
        _treasuryLabel?.Hide();
        _privateTreasuryLabel?.Hide();
        _healthLabel?.Hide();
        _chronicleLog?.Hide();

        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/TitleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ChronicleTitle")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PopularSupportLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyTitleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel")?.Hide();

        _sceneTitleLabel?.Hide();
        _interactiveLabel?.Hide();
        _heJinButton?.Hide();
        _zhangRangButton?.Hide();
        _caoCaoButton?.Hide();
        _jianShuoButton?.Hide();
        _travelButton?.Hide();
    }

    // 动态控制右侧控制面板
    private void UpdateSceneButtons()
    {
        if (_gameState == null) return;

        _sceneTitleLabel?.Hide();
        _interactiveLabel?.Hide();
        _heJinButton?.Hide();
        _zhangRangButton?.Hide();
        _caoCaoButton?.Hide();
        _jianShuoButton?.Hide();

        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        disasterBtn?.Hide();

        if (_actionLabel != null)
        {
            _actionLabel.Visible = false;
            _actionLabel.Text = "【西园军务】";
        }
        if (_sellOfficeButton != null) _sellOfficeButton.Visible = false;
        if (_drillArmyButton != null) _drillArmyButton.Visible = false;
        if (_recruitArmyButton != null) _recruitArmyButton.Visible = false;

        if (_haremActionLabel != null) _haremActionLabel.Visible = false;
        if (_haremRestButton != null) _haremRestButton.Visible = false;
    }

    // 处理起驾
    private void DoTravel(string location)
    {
        if (_gameEngine == null || _gameState == null) return;

        try
        {
            _gameEngine.TravelToLocation(location);
            
            // 关闭起驾弹窗
            _windowManager.PopWindow();

            // 生成转场奏报，不再污染主界面年度事件横条
            string travelStory = location switch
            {
                "宣政殿" => "【起驾 · 宣政殿】\n\n“起驾宣政殿——！”\n内监尖细的高唱声在深宫回荡。陛下登临天子龙辇，在满朝文武的拜跪高呼声中重返宝座。大汉帝国的齿轮，将再次随着陛下的御笔而转动。",
                "后宫" => "【巡幸 · 温德殿】\n\n“天子起驾温德殿，闲人退避——！”\n车舆缓缓停在红墙绿瓦、花香袅袅的后宫。红幔轻摇，莺声燕语。陛下卸下了金銮殿上的重负，来到了属于帝王的绝对私密之所。",
                "西园" => "【起驾 · 西园别苑】\n\n“起驾西园——！”\n陛下避开了何进等人的耳目，轻车简从，来到了陛下亲自督造的西园。这里有堆积如山的金银私库，有新募组建的精锐新军，是陛下摆脱掣肘、暗中夺回大权的铁血基地。",
                _ => $"【起驾】\n\n陛下移驾{location}。"
            };
            ShowTravelReportPopup("起驾奏报", travelStory, location);

            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private static PopupSkin GetTravelReportSkin(string location)
    {
        return location switch
        {
            "西园" => PopupSkin.WestGarden,
            "后宫" => PopupSkin.Warning,
            _ => PopupSkin.Court
        };
    }

    // 执行快捷场景动作
    private void DoQuickAction(string actionId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteQuickAction(actionId);
            ShowStoryReportPopup(actionId == "sell_office" ? "西园鬻官回奏" : "温德殿起居回奏", result.StoryText, actionId == "sell_office" ? PopupSkin.WestGarden : PopupSkin.Warning);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 执行发饷动作
    private void DoArmyDrillAction(int amount)
    {
        if (_gameEngine == null || _gameState == null) return;

        try
        {
            string officerId = "jian_shuo"; // 默认蹇硕（西园上军校尉）
            
            if (_gameState.Npcs.TryGetValue("cao_cao", out var cao) && cao.Favorability > 50)
            {
                officerId = "cao_cao";
            }
            else if (_gameState.Npcs.TryGetValue("zhang_rang", out var zhang) && zhang.Power > 75)
            {
                officerId = "zhang_rang";
            }

            var result = _gameEngine.ExecuteDrillArmyActionWithOfficer(amount, officerId);
            ShowWestGardenReportPopup("西园军报", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private void DoRecruitArmyAction(int troops)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteRaiseWestGardenTroopsAction(troops);
            ShowWestGardenReportPopup("西园军报", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowWarningReportPopup("募兵未成", $"【募兵未成】\n\n{ex.Message}");
        }
    }


    private void PushTemporaryPopup(Panel panel)
    {
        AddChild(panel);
        _windowManager.PushWindow(panel, freeOnClose: true);
    }

    // 执行开仓赈灾动作
    private void DoDisasterReliefAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteDisasterReliefAction(amount, officerId);
            ShowCourtReportPopup("宣政殿赈灾回奏", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 处理朱批下旨
    private async void OnSubmitButtonPressed()
    {
        if (_playerInputEdit == null || _gameEngine == null) return;
        string text = _playerInputEdit.Text;
        if (string.IsNullOrWhiteSpace(text)) return;

        _playerInputEdit.Text = "";
        _playerInputEdit.Editable = false;
        if (_submitButton != null) _submitButton.Disabled = true;

        try
        {
            var result = await _gameEngine.ProcessPlayerTurnAsync(text);
            ShowDocumentReportPopup("御案朱批回奏", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Error in Turn Processing]: {ex.Message}");
            ShowWarningReportPopup("内监急奏", "【内监急奏】\n\n圣旨解析失败，AI 未响应或格式有误。");
        }
        finally
        {
            _playerInputEdit.Editable = true;
            if (_submitButton != null) _submitButton.Disabled = false;
            _playerInputEdit.GrabFocus();
        }
    }

    private void OnPlayerInputSubmitted(string text)
    {
        OnSubmitButtonPressed();
    }

    // === A1 结局面板 ===
    // _Process 每帧调用：检查 GameState.Outcome 是否已非 Playing，是则弹"圣裁已定"奏报。
    // 中兴/续命走 Document 浅纸面（吉），崩殂/亡国走 Warning 急奏（凶）。
    // _outcomeHandled 防止重弹（_Process 每帧跑）。
    private void CheckAndShowOutcomeIfGameOver()
    {
        if (_outcomeHandled) return;
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome == GameOutcome.Playing) return;

        var outcome = _gameState.Outcome;
        string body = _gameEngine.GetOutcomeMessage();
        string summary = BuildOutcomeSummary();
        string fullText = body + "\n\n" + summary;

        if (outcome == GameOutcome.ZhongXing || outcome == GameOutcome.XuMing)
        {
            ShowDocumentReportPopup("圣裁已定", fullText);
        }
        else
        {
            ShowWarningReportPopup("圣裁已定", fullText);
        }

        _handledOutcome = outcome;
        _outcomeHandled = true;
    }

    private string BuildOutcomeSummary()
    {
        if (_gameState == null) return string.Empty;
        return $"— 史官终录 —\n" +
               $"年号：{_gameState.ReignTitle}{_gameState.ReignYear}年\n" +
               $"公元：{_gameState.Year}年{_gameState.Month}月{(_gameState.Xun == 1 ? "上" : _gameState.Xun == 2 ? "中" : "下")}旬\n" +
               $"灵帝春秋：{_gameState.GetEmperorAge()} 岁\n" +
               $"皇权终值：{_gameState.ImperialPower} / 100\n" +
               $"天下民心：{_gameState.PopularSupport} / 100\n" +
               $"皇帝健康：{_gameState.Health} / 100\n" +
               $"国帑结余：{_gameState.Treasury} 万钱\n" +
               $"西园私帑：{_gameState.PrivateTreasury} 万钱\n" +
               $"叛郡计数：{CountRebellingProvinces()} / {_gameState.Provinces.Count}";
    }

    private int CountRebellingProvinces()
    {
        if (_gameState == null) return 0;
        int n = 0;
        foreach (var p in _gameState.Provinces.Values)
            if (p.IsRebelling) n++;
        return n;
    }
}
