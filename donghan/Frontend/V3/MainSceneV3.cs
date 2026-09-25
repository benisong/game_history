using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using DonghanEngine.Core.Economy;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V3;

public partial class MainSceneV3 : Control
{
    private V2Runtime _runtime = null!;
    private VBoxContainer _root = null!;
    private PanelContainer _topBar = null!;
    private Label _titleLabel = null!;
    private Label _statsLabel = null!;
    private HBoxContainer _mainSplit = null!;

    // 左侧：大汉十三州沉浸式地缘沙盘
    private VBoxContainer _leftSandTable = null!;
    private GridContainer _mapGrid = null!;
    private PanelContainer _provinceDetailPanel = null!;
    private Label _selectedProvinceTitle = null!;
    private Label _selectedProvinceDetail = null!;
    private HBoxContainer _provinceQuickActionWheel = null!;

    // 右侧：实体化御案工作台 (折匣 / 名册 / 军务 / 朝会)
    private VBoxContainer _rightDesk = null!;
    private TabContainer _deskTabs = null!;
    private VBoxContainer _affairsDeskPanel = null!;
    private VBoxContainer _edictDeskPanel = null!;
    private VBoxContainer _ministerCardPanel = null!;
    private VBoxContainer _westGardenPanel = null!;
    private VBoxContainer _chroniclePanel = null!;
    private VBoxContainer _saveLoadPanel = null!;

    // 选中的地块
    private string _currentSelectedProvinceId = "sili";

    public override void _Ready()
    {
        _runtime = V2RuntimeFactory.CreateDefault();
        BuildLayout();
        RefreshUi();
    }

    private void BuildLayout()
    {
        // 根全屏布局
        _root = new VBoxContainer();
        _root.SetAnchorsPreset(LayoutPreset.FullRect);
        _root.AddThemeConstantOverride("separation", 8);
        AddChild(_root);

        // 1. 顶部：天子威望天平与宏观国库状态栏
        BuildTopBar();

        // 2. 主分割区：左侧 55% 交互沙盘 + 右侧 45% 实体御案
        _mainSplit = new HBoxContainer();
        _mainSplit.SizeFlagsVertical = SizeFlags.ExpandFill;
        _mainSplit.AddThemeConstantOverride("separation", 12);
        _root.AddChild(_mainSplit);

        BuildLeftSandTable();
        BuildRightDesk();
    }

    private void BuildTopBar()
    {
        _topBar = new PanelContainer();
        _topBar.CustomMinimumSize = new Vector2(0, 56);
        _root.AddChild(_topBar);

        var barHBox = new HBoxContainer();
        barHBox.AddThemeConstantOverride("separation", 16);
        _topBar.AddChild(barHBox);

        _titleLabel = new Label
        {
            Text = "🏛️ 大汉宣政殿 · V3 沉浸式天子御案",
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 16);
        barHBox.AddChild(_titleLabel);

        _statsLabel = new Label
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        barHBox.AddChild(_statsLabel);

        // 一键切换界面/主题按钮
        var switchBtn = new Button
        {
            Text = "🔄 切换至 V2 典雅列表界面",
            CustomMinimumSize = new Vector2(170, 36)
        };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(switchBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
        switchBtn.Pressed += SwitchToV2Theme;
        barHBox.AddChild(switchBtn);
    }

    private void SwitchToV2Theme()
    {
        GetTree().ChangeSceneToFile("res://V2/MainSceneV2.tscn");
    }

    private void BuildLeftSandTable()
    {
        _leftSandTable = new VBoxContainer();
        _leftSandTable.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _leftSandTable.SizeFlagsStretchRatio = 1.15f;
        _leftSandTable.AddThemeConstantOverride("separation", 8);
        _mainSplit.AddChild(_leftSandTable);

        var header = new HBoxContainer();
        var mapTitle = new Label
        {
            Text = "🗺️ 【天下大势 · 十三州交互沙盘】（点击地块唤起决策轮盘）",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        mapTitle.AddThemeFontSizeOverride("font_size", 15);
        header.AddChild(mapTitle);
        _leftSandTable.AddChild(header);

        // 13 州地块网格布局
        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 360)
        };
        _leftSandTable.AddChild(scroll);

        _mapGrid = new GridContainer
        {
            Columns = 3,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _mapGrid.AddThemeConstantOverride("h_separation", 10);
        _mapGrid.AddThemeConstantOverride("v_separation", 10);
        scroll.AddChild(_mapGrid);

        // 底部：选中古州郡实时情报与触控轮盘面板
        _provinceDetailPanel = new PanelContainer();
        _provinceDetailPanel.CustomMinimumSize = new Vector2(0, 160);
        _leftSandTable.AddChild(_provinceDetailPanel);

        var detailBox = new VBoxContainer();
        detailBox.AddThemeConstantOverride("separation", 6);
        _provinceDetailPanel.AddChild(detailBox);

        _selectedProvinceTitle = new Label();
        _selectedProvinceTitle.AddThemeFontSizeOverride("font_size", 15);
        detailBox.AddChild(_selectedProvinceTitle);

        _selectedProvinceDetail = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        detailBox.AddChild(_selectedProvinceDetail);

        _provinceQuickActionWheel = new HBoxContainer();
        _provinceQuickActionWheel.AddThemeConstantOverride("separation", 10);
        detailBox.AddChild(_provinceQuickActionWheel);
    }

    private void BuildRightDesk()
    {
        _rightDesk = new VBoxContainer();
        _rightDesk.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _rightDesk.SizeFlagsStretchRatio = 0.95f;
        _rightDesk.AddThemeConstantOverride("separation", 8);
        _mainSplit.AddChild(_rightDesk);

        var deskHeader = new HBoxContainer();
        var deskTitle = new Label
        {
            Text = "📜 【天子御案 · 政务与行止】",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        deskTitle.AddThemeFontSizeOverride("font_size", 15);
        deskHeader.AddChild(deskTitle);

        // 推进一旬按钮
        var nextTurnBtn = new Button
        {
            Text = "⏳ 运筹帷幄 · 推进一旬",
            CustomMinimumSize = new Vector2(150, 34)
        };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(nextTurnBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
        nextTurnBtn.Pressed += OnNextTurnPressed;
        deskHeader.AddChild(nextTurnBtn);

        // 温德殿静养按钮 (恢复当前精力40%)
        var restBtn = new Button
        {
            Text = "🍵 温德殿静养",
            CustomMinimumSize = new Vector2(120, 34)
        };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(restBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
        restBtn.Pressed += () =>
        {
            var res = _runtime.Health.RestAtWendePalace();
            ShowActionResult(res);
            RefreshUi();
        };
        deskHeader.AddChild(restBtn);

        // 临幸后宫按钮 (以阳气兑换精力，1点阳气换3-5点精力)
        var haremBtn = new Button
        {
            Text = "🌸 临幸后宫",
            CustomMinimumSize = new Vector2(110, 34)
        };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(haremBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
        haremBtn.Pressed += () =>
        {
            var res = _runtime.Health.IndulgeInHarem();
            ShowActionResult(res);
            RefreshUi();
        };
        deskHeader.AddChild(haremBtn);

        _rightDesk.AddChild(deskHeader);

        // 实体御案多功能 Tab 分页
        _deskTabs = new TabContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _rightDesk.AddChild(_deskTabs);

        // Tab 1: 廷议待办与分权交办 (新增：亲裁 vs 一键分发群僚)
        _affairsDeskPanel = new VBoxContainer { Name = "朝堂廷议待办" };
        _affairsDeskPanel.AddThemeConstantOverride("separation", 10);
        _deskTabs.AddChild(_affairsDeskPanel);

        // Tab 2: 奏折折匣 (朱批与御案)
        _edictDeskPanel = new VBoxContainer { Name = "御案折匣" };
        _edictDeskPanel.AddThemeConstantOverride("separation", 10);
        _deskTabs.AddChild(_edictDeskPanel);

        // Tab 3: 朝臣名册与考课
        _ministerCardPanel = new VBoxContainer { Name = "尚书台群僚" };
        _ministerCardPanel.AddThemeConstantOverride("separation", 10);
        _deskTabs.AddChild(_ministerCardPanel);

        // Tab 4: 西园秘营与万金堂
        _westGardenPanel = new VBoxContainer { Name = "西园军务与鬻官" };
        _westGardenPanel.AddThemeConstantOverride("separation", 10);
        _deskTabs.AddChild(_westGardenPanel);

        // Tab 5: 起居注编年史
        _chroniclePanel = new VBoxContainer { Name = "起居注" };
        _chroniclePanel.AddThemeConstantOverride("separation", 8);
        _deskTabs.AddChild(_chroniclePanel);

        // Tab 6: 存档与读档管理 (起居注封存与披阅)
        _saveLoadPanel = new VBoxContainer { Name = "💾 存档与读档" };
        _saveLoadPanel.AddThemeConstantOverride("separation", 8);
        _deskTabs.AddChild(_saveLoadPanel);
    }

    private void RefreshUi()
    {
        var snap = _runtime.State.GetSnapshot();
        var diagnosis = _runtime.Health.GetPhysicianDiagnosis();

        string xunText = snap.Xun switch { 1 => "上旬", 2 => "中旬", _ => "下旬" };

        string mentalStatusText = diagnosis.MentalState switch
        {
            DonghanEngine.Core.Health.ImperialMentalState.Radiant => "🔴 精神状态：龙精虎猛",
            DonghanEngine.Core.Health.ImperialMentalState.ClearAndCalm => "🟢 精神状态：神闲气定",
            DonghanEngine.Core.Health.ImperialMentalState.SlightlyFatigued => "🟡 精神状态：神思稍倦",
            DonghanEngine.Core.Health.ImperialMentalState.YangDeficient => "🟣 精神状态：虚阳浮越(畏寒阳亏)",
            DonghanEngine.Core.Health.ImperialMentalState.DeeplyExhausted => "🟠 精神状态：虚耗神伤",
            _ => "💀 精神状态：气若游丝(病笃)"
        };

        // 1. 刷新顶部威望与财政状态（精神状态暗线暗示精力与阳气，保留医理诊断）
        _statsLabel.Text = $"【{snap.ReignTitle} {snap.ReignYear}年】 {snap.Year}年{snap.Month}月 {xunText} ｜ 驻跸: {snap.CurrentLocation} ｜ {mentalStatusText} ｜ " +
                           $"威望: {snap.ImperialPower} ({snap.PrestigeDescription}) ｜ 太仓: {snap.Treasury:N0}万 ｜ 内帑: {snap.PrivateTreasury:N0}万 ｜ 禁军: {snap.WestGardenArmySize:N0}人";

        // 2. 渲染天下十三州地块卡片
        RenderSandTableProvinces();

        // 3. 渲染选中州郡详情与决策轮盘
        RenderSelectedProvinceDetail();

        // 4. 渲染右侧 Tab 各面板
        RenderAffairsDesk();
        RenderEdictDesk();
        RenderMinisterCards();
        RenderWestGardenDesk();
        RenderChronicle();
        RenderSaveLoadDesk();
    }

    private void RenderSandTableProvinces()
    {
        foreach (var child in _mapGrid.GetChildren())
        {
            child.QueueFree();
        }

        var provinces = _runtime.State.GetAllProvinces();
        foreach (var p in provinces)
        {
            var card = new Button
            {
                CustomMinimumSize = new Vector2(160, 110),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                Alignment = HorizontalAlignment.Left
            };

            bool isSelected = p.Id == _currentSelectedProvinceId;
            string selMark = isSelected ? "👉 " : "";

            int loadPct = p.LandCarryingCapacity > 0 ? (p.Population * 100) / p.LandCarryingCapacity : 100;
            string loadIcon = loadPct > 120 ? "🔴" : (loadPct > 100 ? "🟡" : "🟢");
            string scorchTag = p.ScorchedMonthsRemaining > 0 ? "🔥" : "";

            int stateRatio = (p.StateControlledLand * 100) / Math.Max(1, p.StateControlledLand + p.GentryControlledLand);

            card.Text = $"{selMark}【{p.Name}】 {scorchTag}\n" +
                        $"• 割据: {p.ControllingFactionName}\n" +
                        $"• 承载: {loadIcon}{loadPct}% ｜ 治安: {p.LocalSupport}\n" +
                        $"• 官田占比: {stateRatio}% (官{p.StateControlledLand / 1000}k/世{p.GentryControlledLand / 1000}k)";

            DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(
                card, 
                DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.SandTableLand, 
                isSelected: isSelected);

            string targetId = p.Id;
            card.Pressed += () =>
            {
                _currentSelectedProvinceId = targetId;
                RefreshUi();
            };

            _mapGrid.AddChild(card);
        }
    }

    private void RenderSelectedProvinceDetail()
    {
        foreach (var btn in _provinceQuickActionWheel.GetChildren())
        {
            btn.QueueFree();
        }

        var p = _runtime.State.GetProvince(_currentSelectedProvinceId);
        if (p == null) return;

        int loadPct = p.LandCarryingCapacity > 0 ? (p.Population * 100) / p.LandCarryingCapacity : 100;
        string loadDesc = loadPct > 120 ? "【重度超载·流民四起】" : (loadPct > 100 ? "【轻度承载吃紧】" : "【沃野充盈宜居】");
        string scorchDesc = p.ScorchedMonthsRemaining > 0 ? $" ｜ 战乱焦土休耕中(余{p.ScorchedMonthsRemaining}月)" : "";

        _selectedProvinceTitle.Text = $"📍 焦点州郡：【{p.Name}】 ｜ 实际控制势力：{p.ControllingFactionName} ｜ 长吏：{(string.IsNullOrEmpty(p.GovernorName) ? "暂缺" : p.GovernorName)}{scorchDesc}";
        _selectedProvinceDetail.Text = $"• 编户人口：{p.Population:N0} / 承载上限：{p.LandCarryingCapacity:N0} ({loadDesc}) ｜ 治安民心：{p.LocalSupport} ｜ 守备兵力：{p.Garrison}\n" +
                                       $"• 土地产权：国家官田 {p.StateControlledLand:N0} 顷（全额纳赋） ｜ 世家私田 {p.GentryControlledLand:N0} 顷（瞒报少收四成） ｜ 焦土荒田：{p.ScorchedLand:N0} 顷";

        // 快捷决策轮盘按钮
        var surveyBtn = new Button { Text = "🌾 下诏度田清查" };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(surveyBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.TealPolicy);
        surveyBtn.Pressed += () =>
        {
            var res = _runtime.Agriculture.SurveyLand(p.Id, CadastralSurveyIntensity.Standard);
            ShowActionResult(res);
            RefreshUi();
        };
        _provinceQuickActionWheel.AddChild(surveyBtn);

        var irrBtn = new Button { Text = "💧 官修水利扩容" };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(irrBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.TealPolicy);
        irrBtn.Pressed += () =>
        {
            var res = _runtime.Agriculture.BuildIrrigation(p.Id);
            ShowActionResult(res);
            RefreshUi();
        };
        _provinceQuickActionWheel.AddChild(irrBtn);

        if (p.StateControlledLand >= 1000)
        {
            var buyBtn = new Button { Text = "🪙 准世家赎田入库" };
            DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(buyBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.ActionWheel);
            buyBtn.Pressed += () =>
            {
                var res = _runtime.Agriculture.RepurchaseGentryLand(p.Id, 5000);
                ShowActionResult(res);
                RefreshUi();
            };
            _provinceQuickActionWheel.AddChild(buyBtn);
        }

        if (p.IsRebelling)
        {
            var suppressBtn = new Button { Text = "⚔️ 遣将发兵平叛" };
            DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(suppressBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.CrimsonWarning);
            suppressBtn.Pressed += () =>
            {
                var res = _runtime.Intel.ExecuteProvinceAction(new ProvinceActionCommand(p.Id, ProvinceActionKind.SuppressRebellion, "cao_cao", 3000));
                ShowActionResult(res);
                RefreshUi();
            };
            _provinceQuickActionWheel.AddChild(suppressBtn);
        }
    }

    private void RenderAffairsDesk()
    {
        foreach (var c in _affairsDeskPanel.GetChildren()) c.QueueFree();

        var affairs = _runtime.Delegation.GetPendingAffairs();
        if (affairs.Count == 0)
        {
            _affairsDeskPanel.AddChild(new Label
            {
                Text = "🏛️ 朝堂政简刑清：暂无待办廷议政务。",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
            return;
        }

        var headerBox = new HBoxContainer();
        headerBox.AddThemeConstantOverride("separation", 10);
        _affairsDeskPanel.AddChild(headerBox);

        var tip = new Label
        {
            Text = "【廷议分权】陛下可亲自朱批决断（耗费全额精力），亦可大笔一挥将剩余政务一揽子分派司徒/大将军/中常侍代办（仅耗2点精力）！",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        headerBox.AddChild(tip);

        var batchBtn = new Button
        {
            Text = "📜 一键分交群僚代办 (仅耗2精力)",
            CustomMinimumSize = new Vector2(210, 36)
        };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(batchBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
        batchBtn.Pressed += () =>
        {
            var res = _runtime.Delegation.ExecuteBatchDelegation();
            ShowActionResult(res);
            RefreshUi();
        };
        headerBox.AddChild(batchBtn);

        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var box = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(box);
        _affairsDeskPanel.AddChild(scroll);

        foreach (var affair in affairs)
        {
            var card = new PanelContainer();
            var cardBox = new VBoxContainer();
            cardBox.AddThemeConstantOverride("separation", 6);
            card.AddChild(cardBox);

            string hubBadge = affair.PreferredHub switch
            {
                DonghanEngine.Core.Politics.CourtDelegationHubKind.ThreeExcellencies => "🏛️【司徒府·清流民政】",
                DonghanEngine.Core.Politics.CourtDelegationHubKind.GrandGeneral => "⚔️【大将军府·外戚军务】",
                DonghanEngine.Core.Politics.CourtDelegationHubKind.PalaceAttendants => "💰【内侍省·十常侍理财】",
                _ => "📜【尚书台·台阁考课】"
            };

            var title = new Label
            {
                Text = $"{hubBadge} 【{affair.Title}】 ｜ 支用公帑：{affair.BaseTreasuryCost}万钱 ｜ 亲裁精力：{affair.BaseDirectEnergyCost}点"
            };
            title.AddThemeFontSizeOverride("font_size", 14);
            cardBox.AddChild(title);

            cardBox.AddChild(new Label
            {
                Text = $"• 事务奏请：{affair.Description}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });

            var btnRow = new HBoxContainer();
            btnRow.AddThemeConstantOverride("separation", 8);
            cardBox.AddChild(btnRow);

            var directBtn = new Button { Text = "🔴 天子亲裁决断" };
            DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(directBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
            string targetAffairId = affair.AffairId;
            directBtn.Pressed += () =>
            {
                var res = _runtime.Delegation.ExecuteDirectAffair(targetAffairId);
                ShowActionResult(res);
                RefreshUi();
            };
            btnRow.AddChild(directBtn);

            box.AddChild(card);
        }
    }

    private void RenderEdictDesk()
    {
        foreach (var c in _edictDeskPanel.GetChildren()) c.QueueFree();

        var edicts = _runtime.Edicts.GetPendingEdicts();
        if (edicts.Count == 0)
        {
            _edictDeskPanel.AddChild(new Label
            {
                Text = "📮 御案清朗：暂无待批奏折。陛下可起驾巡幸、视察西园或推进一旬。",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
            return;
        }

        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var box = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 12);
        scroll.AddChild(box);
        _edictDeskPanel.AddChild(scroll);

        foreach (var edict in edicts)
        {
            var card = new PanelContainer();
            var cardBox = new VBoxContainer();
            cardBox.AddThemeConstantOverride("separation", 6);
            card.AddChild(cardBox);

            var title = new Label
            {
                Text = $"📄 【{edict.Title}】 类别：{edict.Type}"
            };
            title.AddThemeFontSizeOverride("font_size", 14);
            cardBox.AddChild(title);

            cardBox.AddChild(new Label
            {
                Text = edict.NarrativeContent,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });

            var btnRow = new FlowContainer();
            btnRow.AddThemeConstantOverride("separation", 8);
            cardBox.AddChild(btnRow);

            for (int i = 0; i < edict.Options.Count; i++)
            {
                int optIdx = i;
                var opt = edict.Options[i];
                var optBtn = new Button { Text = $"🔴 朱批：{opt.Description}" };
                DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(optBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
                optBtn.Pressed += () =>
                {
                    var res = _runtime.Edicts.Resolve(new ResolveEdictCommand(edict.Id, optIdx));
                    ShowActionResult(res);
                    RefreshUi();
                };
                btnRow.AddChild(optBtn);
            }

            box.AddChild(card);
        }
    }

    private void RenderMinisterCards()
    {
        foreach (var c in _ministerCardPanel.GetChildren()) c.QueueFree();

        var ministers = _runtime.State.GetMinisters().Where(m => m.IsActive).ToList();
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var box = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(box);
        _ministerCardPanel.AddChild(scroll);

        foreach (var m in ministers)
        {
            var card = new PanelContainer();
            var cardBox = new VBoxContainer();
            cardBox.AddThemeConstantOverride("separation", 4);
            card.AddChild(cardBox);

            cardBox.AddChild(new Label
            {
                Text = $"👤 【{m.Name}】 官职：{m.Title} ｜ 派系：{m.Faction} ｜ 忠诚：{m.Favorability} ｜ 权势：{m.Power} ｜ 野心：{m.Ambition}"
            });

            var btnRow = new HBoxContainer();
            btnRow.AddThemeConstantOverride("separation", 8);
            cardBox.AddChild(btnRow);

            var confiscateBtn = new Button { Text = "⚖️ 查办抄家" };
            DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(confiscateBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.CrimsonWarning);
            confiscateBtn.Pressed += () =>
            {
                var res = _runtime.SpecialActions.Execute(new SpecialActionCommand("confiscate_direct", TargetNpcId: m.Id));
                ShowActionResult(res);
                RefreshUi();
            };
            btnRow.AddChild(confiscateBtn);

            box.AddChild(card);
        }
    }

    private void RenderWestGardenDesk()
    {
        foreach (var c in _westGardenPanel.GetChildren()) c.QueueFree();

        _westGardenPanel.AddChild(new Label
        {
            Text = "🏛️ 【西园万金堂 · 鬻官私库与禁军校场】\n" +
                   "• 西园私库卖官明码标价：可直售【太尉 / 司徒 / 司空】一品三公官职入内帑（民心受损）。\n" +
                   "• 禁军校场支持阅兵赏赐与大扩军。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var btnRow = new FlowContainer();
        btnRow.AddThemeConstantOverride("separation", 8);
        _westGardenPanel.AddChild(btnRow);

        var buyTaiweiBtn = new Button { Text = "💰 卖【太尉】官职（售金一亿入内帑）" };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(buyTaiweiBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
        buyTaiweiBtn.Pressed += () =>
        {
            var res = _runtime.Ranks.SellOffice("cao_cao", "太尉");
            ShowActionResult(res);
            RefreshUi();
        };
        btnRow.AddChild(buyTaiweiBtn);

        var payArmyBtn = new Button { Text = "🎖️ 内帑犒赏西园三军（提振士气与皇权）" };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(payArmyBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
        payArmyBtn.Pressed += () =>
        {
            var res = _runtime.WestGarden.PayArmy(new ArmyPayCommand(2000, "huangfu_song"));
            ShowActionResult(res);
            RefreshUi();
        };
        btnRow.AddChild(payArmyBtn);

        var recruitBtn = new Button { Text = "🚩 募兵两千入西园" };
        DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(recruitBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
        recruitBtn.Pressed += () =>
        {
            var res = _runtime.WestGarden.DrillArmy(new ArmyDrillCommand(2000, "huangfu_song"));
            ShowActionResult(res);
            RefreshUi();
        };
        btnRow.AddChild(recruitBtn);
    }

    private void RenderChronicle()
    {
        foreach (var c in _chroniclePanel.GetChildren()) c.QueueFree();

        var snap = _runtime.State.GetSnapshot();
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var box = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 6);
        scroll.AddChild(box);
        _chroniclePanel.AddChild(scroll);

        var entries = snap.Chronicle.Reverse().Take(20);
        foreach (var entry in entries)
        {
            box.AddChild(new Label
            {
                Text = entry,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
        }
    }

    private void RenderSaveLoadDesk()
    {
        foreach (var c in _saveLoadPanel.GetChildren()) c.QueueFree();

        var header = new Label
        {
            Text = "📜 【太史令起居注 · 历史卷轴封存与披阅】",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _saveLoadPanel.AddChild(header);

        var slots = _runtime.Persistence.ListSaveSlots();
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        var listContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        listContainer.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(listContainer);
        _saveLoadPanel.AddChild(scroll);

        foreach (var slot in slots)
        {
            var slotBox = new PanelContainer();
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);
            slotBox.AddChild(row);

            var infoBox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            string titleText = slot.IsAutoSave
                ? (slot.Exists ? $"⚡ 【自动起居注】 {slot.CurrentDateText}" : "⚡ 【自动起居注】 (虚位以待)")
                : (slot.Exists ? $"💾 【槽位 {slot.SlotIndex}】 {slot.SaveName}" : $"💾 【槽位 {slot.SlotIndex}】 (虚位以待)");

            var titleLabel = new Label
            {
                Text = titleText,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            var detailLabel = new Label
            {
                Text = slot.Exists ? $"   • {slot.SummaryPreview} ｜ 时间: {slot.Timestamp:yyyy-MM-dd HH:mm}" : "   • 点击右侧按钮封存当前起居注",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            infoBox.AddChild(titleLabel);
            infoBox.AddChild(detailLabel);
            row.AddChild(infoBox);

            // 操作按钮
            if (slot.Exists)
            {
                var loadBtn = new Button { Text = "📖 披阅(读取)" };
                DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(loadBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.PrimaryGold);
                int sIndex = slot.SlotIndex;
                loadBtn.Pressed += () =>
                {
                    var res = _runtime.Persistence.LoadGame(sIndex);
                    ShowActionResult(new ActionResult(res.Success, res.Success ? "【披阅前事】" : "【读档受阻】", res.Message, res.Success ? ReportKind.Information : ReportKind.Warning, Array.Empty<StateChange>()));
                    RefreshUi();
                };
                row.AddChild(loadBtn);
            }

            if (!slot.IsAutoSave)
            {
                var saveBtn = new Button { Text = slot.Exists ? "✍️ 覆写(保存)" : "✍️ 封存(保存)" };
                DonghanFrontend.Common.ImperialUiThemeHelper.ApplyInteractiveFeedback(saveBtn, DonghanFrontend.Common.ImperialUiThemeHelper.ButtonSkin.DarkWood);
                int sIndex = slot.SlotIndex;
                saveBtn.Pressed += () =>
                {
                    var res = _runtime.Persistence.SaveGame(sIndex);
                    ShowActionResult(new ActionResult(res.Success, res.Success ? "【封存起居注】" : "【存档受阻】", res.Message, res.Success ? ReportKind.Information : ReportKind.Warning, Array.Empty<StateChange>()));
                    RefreshUi();
                };
                row.AddChild(saveBtn);
            }

            listContainer.AddChild(slotBox);
        }
    }

    private async void OnNextTurnPressed()
    {
        var res = await _runtime.Turns.AdvanceXunAsync();
        RefreshUi();
    }

    private void ShowActionResult(ActionResult result)
    {
        var dialog = new AcceptDialog
        {
            Title = result.Title,
            DialogText = result.StoryText
        };
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(480, 240));
        dialog.Confirmed += () => dialog.QueueFree();
    }
}
