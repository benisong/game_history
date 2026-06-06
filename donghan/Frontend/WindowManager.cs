using Godot;
using System.Collections.Generic;

namespace DonghanFrontend;

public partial class WindowManager : Node
{
    private Stack<Control> _windowStack = new();
    private Stack<ColorRect> _blockerStack = new();

    // 打开一个多层全不透明模态防穿透浮窗
    public void PushWindow(Control window)
    {
        if (window == null) return;

        // 1. 创建全屏模态防穿透点击拦截器
        var blocker = new ColorRect();
        blocker.Name = $"{window.Name}_Blocker";
        blocker.Color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // 85% 暗化不透明，盖死底层
        blocker.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        blocker.MouseFilter = Control.MouseFilterEnum.Stop; // 拦截所有事件，彻底不漏点

        // 2. 将遮蔽阻断器动态加入场景，正好垫在弹窗下方
        window.GetParent().AddChild(blocker);
        window.GetParent().MoveChild(blocker, window.GetIndex());

        // 3. 将弹窗自身 Style 重写为 100% 物理不透明，以防半透明漏影
        if (window is Panel panel)
        {
            var opaqueStyle = new StyleBoxFlat();
            opaqueStyle.BgColor = new Color(0.12f, 0.12f, 0.12f, 1.0f); // 100% 绝对不透明深黑灰
            opaqueStyle.SetBorderWidthAll(2);
            opaqueStyle.BorderColor = new Color(0.84f, 0.67f, 0.12f, 1.0f); // 暗金框
            panel.AddThemeStyleboxOverride("panel", opaqueStyle);
        }

        _windowStack.Push(window);
        _blockerStack.Push(blocker);

        blocker.Show();
        window.Show();
        
        GD.Print($"[WindowManager]: 已呼出防穿透弹窗 {window.Name}，阻断器已生效！");
    }

    // 关闭最上层的浮窗，释放其下的阻断器
    public void PopWindow()
    {
        if (_windowStack.Count > 0)
        {
            var topWindow = _windowStack.Pop();
            topWindow.Hide();

            if (_blockerStack.Count > 0)
            {
                var topBlocker = _blockerStack.Pop();
                topBlocker.QueueFree(); // 销毁该层遮挡，底层重新可点
            }
            GD.Print($"[WindowManager]: 已关闭弹窗 {topWindow.Name} 并解冻一层遮挡。");
        }
    }

    // 监听全局输入 ESC 键，逐层退栈关闭窗口
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) // 默认对应 ESC
        {
            if (_windowStack.Count > 0)
            {
                PopWindow();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
