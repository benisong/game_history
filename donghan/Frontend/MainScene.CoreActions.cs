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

        // 更新左侧数值
        if (_reignLabel != null) _reignLabel.Text = $"年号: {_gameState.ReignTitle} {_gameState.ReignYear} 年";
        if (_imperialPowerLabel != null) _imperialPowerLabel.Text = $"皇权值: {_gameState.ImperialPower} / 100";
        if (_treasuryLabel != null) _treasuryLabel.Text = $"国库资金: {_gameState.Treasury} 万钱";
        if (_privateTreasuryLabel != null) _privateTreasuryLabel.Text = $"西园私库: {_gameState.PrivateTreasury} 万钱";
        
        // 更新天下民心
        var supportLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PopularSupportLabel");
        if (supportLabel != null) supportLabel.Hide();

        if (_healthLabel != null) _healthLabel.Text = $"皇帝健康: {_gameState.Health} / 100";

        // 更新左侧西园军势
        var armyTitleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyTitleLabel");
        if (armyTitleLabel != null) armyTitleLabel.Hide();

        var armySizeLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel");
        if (armySizeLabel != null) armySizeLabel.Hide();

        var armyMoraleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel");
        if (armyMoraleLabel != null) armyMoraleLabel.Hide();

        var armyLoyaltyLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel");
        if (armyLoyaltyLabel != null) armyLoyaltyLabel.Hide();

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
        UpdateNpcList();
    }

    // 动态控制右侧控制面板
    private void UpdateSceneButtons()
    {
        if (_gameState == null) return;

        string loc = _gameState.CurrentLocation;

        if (_sceneTitleLabel != null) _sceneTitleLabel.Text = $"当前：{loc}";

        // 1. 宣政殿显示：何进、张让，及赈灾快捷按钮
        bool isCourt = loc == "宣政殿";
        if (_heJinButton != null) _heJinButton.Visible = isCourt;
        if (_zhangRangButton != null) _zhangRangButton.Visible = isCourt;
        
        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        if (disasterBtn != null) disasterBtn.Visible = isCourt;

        // 2. 西园显示：曹操、蹇硕，以及西园专属操作按钮
        bool isGarden = loc == "西园";
        if (_caoCaoButton != null) _caoCaoButton.Visible = isGarden;
        if (_jianShuoButton != null) _jianShuoButton.Visible = isGarden;
        
        if (_actionLabel != null) _actionLabel.Visible = isGarden;
        if (_sellOfficeButton != null) _sellOfficeButton.Visible = isGarden;
        if (_drillArmyButton != null) _drillArmyButton.Visible = isGarden;
        if (_recruitArmyButton != null) _recruitArmyButton.Visible = isGarden;

        // 3. 后宫显示：后宫专属按钮（隐藏所有大臣，后宫不准外臣涉足）
        bool isHarem = loc == "后宫";
        if (_haremActionLabel != null) _haremActionLabel.Visible = isHarem;
        if (_haremRestButton != null) _haremRestButton.Visible = isHarem;

        // 如果是后宫或西园，隐藏通用“召见群臣”文字标签
        if (_interactiveLabel != null)
        {
            _interactiveLabel.Visible = isCourt || isGarden;
            _interactiveLabel.Text = isCourt ? "【召见朝臣】" : "【召见将领】";
        }
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
