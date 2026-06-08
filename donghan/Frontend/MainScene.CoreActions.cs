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
        _npcScrollContainer?.Hide();
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

            // 生成转场故事文本
            if (_storyOutput != null)
            {
                if (location == "宣政殿")
                {
                    _storyOutput.Text = "【起驾 · 宣政殿】\n\n“起驾宣政殿——！”\n内监尖细的高唱声在深宫回荡。陛下登临天子龙辇，在满朝文武的拜跪高呼声中重返宝座。大汉帝国的齿轮，将再次随着陛下的御笔而转动。";
                }
                else if (location == "后宫")
                {
                    _storyOutput.Text = "【巡幸 · 温德殿】\n\n“天子起驾温德殿，闲人退避——！”\n车舆缓缓停在红墙绿瓦、花香袅袅的后宫。红幔轻摇，莺声燕语。陛下卸下了金銮殿上的重负，来到了属于帝王的绝对私密之所。";
                }
                else if (location == "西园")
                {
                    _storyOutput.Text = "【起驾 · 西园精舍】\n\n“起驾西园——！”\n陛下避开了何进等人的耳目，轻车简从，来到了陛下亲自督造的西园。这里有堆积如山的金银私库，有新募组建的精锐新军，是陛下摆脱掣肘、暗中夺回大权的铁血基地。";
                }
            }

            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 执行快捷场景动作
    private void DoQuickAction(string actionId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteQuickAction(actionId);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
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
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
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
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            if (_storyOutput != null)
            {
                _storyOutput.Text = $"【募兵未成】\n\n{ex.Message}";
            }
        }
    }

    private void ShowRecruitArmyDialog()
    {
        if (_gameState == null) return;

        const int maxArmySize = 12000;
        int capacity = Math.Max(0, maxArmySize - _gameState.WestGardenArmy.Size);
        int defaultTroops = Math.Min(2000, capacity);
        if (defaultTroops <= 0) defaultTroops = 1000;

        var panel = new Panel { CustomMinimumSize = new Vector2(520, 340) };
        panel.AnchorLeft = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -260;
        panel.OffsetTop = -170;
        panel.OffsetRight = 260;
        panel.OffsetBottom = 170;
        panel.MouseFilter = Control.MouseFilterEnum.Stop;
        panel.AddThemeStyleboxOverride("panel", CreateOpaquePanelStyle("RecruitArmyPopupPanel"));

        var vBox = new VBoxContainer();
        SetFullRect(vBox);
        vBox.OffsetLeft = 22;
        vBox.OffsetTop = 18;
        vBox.OffsetRight = -22;
        vBox.OffsetBottom = -18;
        vBox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(vBox);

        vBox.AddChild(new Label
        {
            Text = "西园募兵 · 补充新军",
            HorizontalAlignment = HorizontalAlignment.Center
        });

        var desc = new Label
        {
            Text = $"当前兵力：{_gameState.WestGardenArmy.Size}/{maxArmySize}\n每募 1000 人：国库 -300 万钱，天下民心 -1，士气 -1。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        vBox.AddChild(desc);

        var troopSpin = new SpinBox
        {
            MinValue = 1000,
            MaxValue = Math.Max(1000, capacity),
            Step = 1000,
            Value = defaultTroops,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        vBox.AddChild(troopSpin);

        var preview = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        vBox.AddChild(preview);

        void RefreshPreview(double value)
        {
            int troops = Math.Min((int)value, capacity);
            if (capacity <= 0)
            {
                preview.Text = "西园新军已满编，无需继续募兵。";
                troopSpin.Editable = false;
                return;
            }

            int batches = troops / 1000;
            preview.Text = $"预计征发：{troops} 人\n预计花费：{batches * 300} 万钱\n民心影响：-{batches}\n募兵后兵力：{_gameState.WestGardenArmy.Size + troops}/{maxArmySize}";
        }

        troopSpin.ValueChanged += RefreshPreview;
        RefreshPreview(troopSpin.Value);

        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 12);
        vBox.AddChild(row);

        var confirm = new Button
        {
            Text = "下诏募兵",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Disabled = capacity <= 0
        };
        var cancel = new Button
        {
            Text = "暂缓",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        row.AddChild(confirm);
        row.AddChild(cancel);

        confirm.Pressed += () =>
        {
            int troops = (int)troopSpin.Value;
            _windowManager.PopWindow();
            DoRecruitArmyAction(troops);
        };
        cancel.Pressed += _windowManager.PopWindow;

        panel.VisibilityChanged += () =>
        {
            if (!panel.Visible && IsInstanceValid(panel)) panel.QueueFree();
        };
        AddChild(panel);
        _windowManager.PushWindow(panel);
    }

    // 执行开仓赈灾动作
    private void DoDisasterReliefAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteDisasterReliefAction(amount, officerId);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
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
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Error in Turn Processing]: {ex.Message}");
            if (_storyOutput != null)
            {
                _storyOutput.Text += $"\n【内监急奏：圣旨解析失败，AI未响应或格式有误。】";
            }
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
}
