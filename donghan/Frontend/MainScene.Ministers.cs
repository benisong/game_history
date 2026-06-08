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

            if (_ministerTitleLabel != null) _ministerTitleLabel.Text = $"{minister.Name} ({minister.Title})";
            if (_ministerFavorabilityLabel != null) _ministerFavorabilityLabel.Text = $"好感度: {minister.Favorability} / 100";
            if (_ministerPowerLabel != null) _ministerPowerLabel.Text = $"朝堂权力: {minister.Power} / 100";

            // 显示贪腐度
            var corruptionLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption");
            if (corruptionLabel != null)
            {
                corruptionLabel.Text = $"官员贪腐度: {minister.Corruption} / 100";
            }

            // 显示贪腐存银
            var wealthLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth");
            if (wealthLabel != null)
            {
                wealthLabel.Text = $"私蓄赃款: {minister.StashedWealth} 万钱";
            }

            // 新增或更新五维属性标签
            var fiveAttributesLabel = _ministerPanel.GetNodeOrNull<Label>("VBox/FiveAttributes");
            if (fiveAttributesLabel == null)
            {
                fiveAttributesLabel = new Label();
                fiveAttributesLabel.Name = "FiveAttributes";
                ConfigureWrappingLabel(fiveAttributesLabel, HorizontalAlignment.Center);
                _ministerPanel.GetNode<VBoxContainer>("VBox").AddChild(fiveAttributesLabel);
                // 移动到 CloseButton 之前
                _ministerPanel.GetNode<VBoxContainer>("VBox").MoveChild(fiveAttributesLabel, 5);
            }
            else
            {
                ConfigureWrappingLabel(fiveAttributesLabel, HorizontalAlignment.Center);
            }
            string govText = minister.GovernedProvinceId != null ? $"【外任 {_gameState.Provinces[minister.GovernedProvinceId].Name} 太守】\n" : "【在京闲置】\n";
            fiveAttributesLabel.Text = govText +
                $"武力: {minister.Martial,-3} | 统帅: {minister.Leadership,-3} | 政治: {minister.Politics,-3}\n" +
                $"魅力: {minister.Charisma,-3} | 野心: {minister.Ambition,-3}";

            _windowManager.PushWindow(_ministerPanel);
        }
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
