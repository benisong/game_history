# 2026-06-06 《东汉末年汉灵帝》三项核心体验优化设计规范 (Opaque Modals, Opening Box & Intel Consolidation)

## 1. 设计目标

根据陛下的最高指示，解决游戏内多层叠加点击引起的“界面穿透操作逻辑混乱”问题，还原高保真的剧情沉浸体验。
本优化规范专注于：
1. **【A轨全屏古风宣召开局弹窗】**：盖住一切主界面，必须在玩家点击“临朝理政”后才淡化退场，绝不占用主故事区的首屏空间。
2. **【100%不透明防穿透模态弹窗系统】**：实装全屏暗底模态阻断器，确保在查看政务、舆图情报、大臣详情时，**底层的主界面及器物按钮处于完全不可点击的阻断态**。
3. **【情报舆图大一统，隐藏喧宾夺主的军势数据】**：将左侧常驻面板上的“天下民心”、“西园军势”等具体数字完全剔除，收纳统一塞进“漆木密札（情报/郡县舆图）”弹窗中，在看密报时才允许查看天下大事，保持左侧的主界面极其高冷和纯净。

---

## 2. 详细实装方案

### 2.1 全屏厚重古风开局宣召 (Opening Scroll Panel)
- **定义**：一个大小为 1280x720 的全屏不透明深色遮罩面板 `_openingOverlay`。
- **背景样式**：使用 `Panel`，采用 `StyleBoxFlat` 设置 100% 不透明的深古铜褐色底。
- **文案显示**：采用 `RichTextLabel` 进行自适应居中排版：
  ```
  【大汉光和七年 · 宣政殿】

  “苍天已死，黄天当立。岁在甲子，天下大吉！”
  外戚大将军何进坐镇京师，十常侍常侍张让把持禁中。
  社稷有累卵之危，百姓有倒悬之急。

  陛下，大汉的江山，您当如何执掌？
  ```
- **交互**：下方提供一个宽大的、高对比度金色字样按钮 `【临朝理政 / 君临天下】`。
  - 点击按钮后，播放淡出或直接 `QueueFree()`，显示其下方的繁华大汉朝堂主场景。

---

### 2.2 100%不透明防穿透模态弹窗 (Opaque Modal Blocker System)
在 `WindowManager.cs` 中新增一个全屏不透明遮罩管理逻辑，为每一个 Push 进来的弹窗动态挂载一个遮挡父级组件。

```csharp
public partial class WindowManager : Node
{
    private Stack<Control> _windowStack = new();
    private Stack<ColorRect> _blockerStack = new();

    public void PushWindow(Control window)
    {
        if (window == null) return;

        // 1. 创建全屏模态遮截器
        var blocker = new ColorRect();
        blocker.Name = $"{window.Name}_Blocker";
        blocker.Color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // 85% 暗化不透明，彻底盖住底层
        blocker.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        blocker.MouseFilter = Control.MouseFilterEnum.Stop; // 关键：彻底拦截所有鼠标事件

        // 2. 将遮截器插入到场景中
        window.GetParent().AddChild(blocker);
        window.GetParent().MoveChild(blocker, window.GetIndex()); // 保证遮截器正好位于弹窗下方，且处于主界面上方

        // 3. 修改弹窗本身的样式，使其使用不透明的背景 StyleBox
        if (window is Panel panel)
        {
            var opaqueStyle = new StyleBoxFlat();
            opaqueStyle.BgColor = new Color(0.12f, 0.12f, 0.12f, 1.0f); // 100% 不透明深灰色
            opaqueStyle.SetBorderWidthAll(2);
            opaqueStyle.BorderColor = new Color(0.84f, 0.67f, 0.12f, 1.0f); // 暗金框
            panel.AddThemeStyleboxOverride("panel", opaqueStyle);
        }

        _windowStack.Push(window);
        _blockerStack.Push(blocker);

        blocker.Show();
        window.Show();
    }

    public void PopWindow()
    {
        if (_windowStack.Count > 0)
        {
            var topWindow = _windowStack.Pop();
            topWindow.Hide();

            var topBlocker = _blockerStack.Pop();
            topBlocker.QueueFree(); // 销毁遮截器，释放底层点击
        }
    }
}
```

---

### 2.3 隐藏左侧军势并收纳进“情报”面板
1. **清理 `LeftPanel`**：
   - 彻底隐藏/删除 `LeftPanel/VBoxContainer/PopularSupportLabel` (天下民心)。
   - 彻底隐藏/删除 `LeftPanel/VBoxContainer/ArmyTitleLabel` (—— 西园军势 ——)。
   - 彻底隐藏/删除 `LeftPanel/VBoxContainer/ArmySizeLabel` (建制人数)。
   - 彻底隐藏/删除 `LeftPanel/VBoxContainer/ArmyMoraleLabel` (军心士气)。
   - 彻底隐藏/删除 `LeftPanel/VBoxContainer/ArmyLoyaltyLabel` (天子忠诚)。
   
2. **重构收纳进“漆木密札（情报弹窗）”**：
   - 当点击密札开启 `_intelPopup` 面板时，在情报面板的右侧上部或新增的顶部区域，增加专属大字号展示：
     ```
     【大汉全局态势】 天下民心: [PopularSupport]/100
     【西园精锐军势】 兵力: [Size] 人 | 士气: [Morale]/100 | 天子忠诚: [Loyalty]/100
     ```
   - 在密扎中展示这些最绝密的情报与兵戈之势，更显君临幕后的雄图之略。

---

## 3. 自我审查 (Self-Review)
- **有无占位符？** 无，所有节点操作与 WindowsManager 防穿透设计已精确写明。
- **是否完美解决了操作逻辑混乱？** 是。`MouseFilter = Stop` 结合 `ColorRect` 阻断，使得开启弹窗时，主屏幕输入区和御案按钮 100% 被冻结，无法透过弹窗被玩家意外点击！
