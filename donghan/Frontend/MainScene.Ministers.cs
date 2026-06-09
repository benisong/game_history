using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void ShowMinisterDetails(string ministerId)
    {
        if (_gameState == null || _ministerPanel == null) return;

        if (_gameState.Npcs.TryGetValue(ministerId, out var minister))
        {
            _currentDetailsMinisterId = ministerId; // 记录当前正在查看的大臣ID，用于抄家指令
            ApplyMinisterDocumentSkin();

            if (_ministerTitleLabel != null)
            {
                _ministerTitleLabel.Text = $"{minister.Name} · {minister.Title}";
                _ministerTitleLabel.AddThemeColorOverride("font_color", GetPopupTitleColor(PopupSkin.Document));
            }
            if (_ministerFavorabilityLabel != null)
            {
                _ministerFavorabilityLabel.Text = $"君臣情分：{DescribeAttitudeLevel(minister.Favorability)}（{minister.Favorability}/100）";
            }
            if (_ministerPowerLabel != null)
            {
                _ministerPowerLabel.Text = $"朝局分量：{DescribePowerLevel(minister.Power)}（{minister.Power}/100）";
            }

            var corruptionLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption");
            if (corruptionLabel != null)
            {
                corruptionLabel.Text = $"廉污风评：{DescribeCorruptionLevel(minister.Corruption)}（{minister.Corruption}/100）";
            }

            var wealthLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth");
            if (wealthLabel != null)
            {
                wealthLabel.Text = $"可籍赃银：{minister.StashedWealth} 万钱";
            }

            var fiveAttributesLabel = _ministerPanel.GetNodeOrNull<Label>("VBox/FiveAttributes");
            if (fiveAttributesLabel == null)
            {
                fiveAttributesLabel = new Label();
                fiveAttributesLabel.Name = "FiveAttributes";
                _ministerPanel.GetNode<VBoxContainer>("VBox").AddChild(fiveAttributesLabel);
                _ministerPanel.GetNode<VBoxContainer>("VBox").MoveChild(fiveAttributesLabel, 5);
            }
            ConfigureWrappingLabel(fiveAttributesLabel, HorizontalAlignment.Left);
            fiveAttributesLabel.AddThemeColorOverride("font_color", new Color(0.20f, 0.12f, 0.06f, 1.0f));

            string govText = minister.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(minister.GovernedProvinceId, out var province)
                ? $"外任记录：{province.Name} 太守"
                : "任所记录：在京候旨";
            fiveAttributesLabel.Text =
                $"【奏牍摘录】\n{govText}\n" +
                $"武略 {minister.Martial}｜统御 {minister.Leadership}｜政术 {minister.Politics}\n" +
                $"声望 {minister.Charisma}｜野心 {minister.Ambition}\n" +
                $"派系：{minister.Faction}";

            _windowManager.PushWindow(_ministerPanel);
        }
    }

    private void ApplyMinisterDocumentSkin()
    {
        if (_ministerPanel == null) return;
        _ministerPanel.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.Document));

        var vBox = _ministerPanel.GetNodeOrNull<VBoxContainer>("VBox");
        if (vBox != null) vBox.AddThemeConstantOverride("separation", 10);

        foreach (var label in new[]
        {
            _ministerFavorabilityLabel,
            _ministerPowerLabel,
            GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption"),
            GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth")
        })
        {
            if (label == null) continue;
            ConfigureWrappingLabel(label);
            label.AddThemeColorOverride("font_color", new Color(0.20f, 0.12f, 0.06f, 1.0f));
        }

        var actionRow = GetNodeOrNull<HBoxContainer>("MinisterOverlayPanel/VBox/HBox");
        if (actionRow != null)
        {
            foreach (var child in actionRow.GetChildren())
            {
                if (child is Button button)
                {
                    button.Text = button.Name.ToString() switch
                    {
                        "ConfiscateTreasuryBtn" => "籍没入国库",
                        "ConfiscatePrivateBtn" => "籍没入西园私库",
                        _ => button.Text
                    };
                }
            }
        }
    }

    private static string DescribeAttitudeLevel(int value)
    {
        if (value >= 75) return "亲近";
        if (value >= 55) return "可用";
        if (value >= 35) return "观望";
        return "疏离";
    }

    private static string DescribePowerLevel(int value)
    {
        if (value >= 75) return "炽盛";
        if (value >= 55) return "有势";
        if (value >= 35) return "中平";
        return "微弱";
    }

    private static string DescribeCorruptionLevel(int value)
    {
        if (value >= 75) return "巨蠹";
        if (value >= 55) return "可疑";
        if (value >= 35) return "有瑕";
        return "尚廉";
    }

    // 执行抄家动作
    private void DoConfiscateAction(string destination)
    {
        if (_gameEngine == null || string.IsNullOrEmpty(_currentDetailsMinisterId)) return;

        try
        {
            // 关闭详情面板
            _windowManager.PopWindow();

            // 如果不在宣政殿，发出警告
            if (_gameState?.CurrentLocation != "宣政殿")
            {
                if (_storyOutput != null)
                {
                    _storyOutput.Text = "【御史弹劾】\n\n“陛下，抄没朝臣家产兹事体大，必须在宣政殿百官大朝会上宣旨籍没，方可调动京师御林军，否则名不正言不顺！”";
                }
                return;
            }

            var result = _gameEngine.ExecuteConfiscationAction(_currentDetailsMinisterId, destination);
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

    private ScrollContainer? _npcScrollContainer;
    private VBoxContainer? _npcListVBox;

    private void InitializeDynamicNpcList()
    {
        var ministersVBox = GetNodeOrNull<VBoxContainer>("RightPanel/Ministers");
        if (ministersVBox == null) return;

        // 隐藏旧的硬编码按钮，防止并立冲突
        if (_heJinButton != null) _heJinButton.Hide();
        if (_zhangRangButton != null) _zhangRangButton.Hide();
        if (_caoCaoButton != null) _caoCaoButton.Hide();
        if (_jianShuoButton != null) _jianShuoButton.Hide();

        _npcScrollContainer = new ScrollContainer();
        _npcScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _npcScrollContainer.CustomMinimumSize = new Vector2(0, 200);

        _npcListVBox = new VBoxContainer();
        _npcListVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _npcScrollContainer.AddChild(_npcListVBox);

        // 将动态滚动列表插入到 ministersVBox 中，位于 SceneTitle 和 InteractiveLabel 之后
        ministersVBox.AddChild(_npcScrollContainer);
    }

    private void UpdateNpcList()
    {
        if (_npcListVBox == null || _gameState == null) return;

        // 清理旧节点
        foreach (Node child in _npcListVBox.GetChildren())
        {
            child.QueueFree();
        }

        // 动态载入当前在朝的所有活跃大臣
        foreach (var npc in _gameState.Npcs.Values)
        {
            if (!npc.IsActive) continue;

            string locationTag = npc.GovernedProvinceId != null ? $"【任{_gameState.Provinces[npc.GovernedProvinceId].Name}】" : "【在京】";
            var btn = new Button();
            btn.Text = $"[{npc.Faction}] {npc.Name} {locationTag}";
            btn.Alignment = HorizontalAlignment.Left;
            btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            
            string npcId = npc.Id;
            btn.Pressed += () => ShowMinisterDetails(npcId);
            _npcListVBox.AddChild(btn);
        }
    }
}
