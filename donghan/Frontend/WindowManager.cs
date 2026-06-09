using Godot;
using System.Collections.Generic;

namespace DonghanFrontend;

public partial class WindowManager : Node
{
    private const int ModalBaseZIndex = 1_000;

    private Stack<Control> _windowStack = new();
    private Stack<ColorRect> _blockerStack = new();

    // 打开一个多层全不透明模态防穿透浮窗
    public void PushWindow(Control window)
    {
        if (window == null) return;
        if (window.GetParent() == null) return;
        if (_windowStack.Count > 0 && _windowStack.Peek() == window) return;

        GetViewport().GuiReleaseFocus();

        // 1. 创建全屏模态防穿透点击拦截器
        var blocker = new ColorRect();
        blocker.Name = $"{window.Name}_Blocker";
        blocker.Color = new Color(0.04f, 0.035f, 0.03f, 1.0f); // 100% 不透明，底层完全不可见不可点
        blocker.MouseFilter = Control.MouseFilterEnum.Stop; // 拦截所有事件，彻底不漏点
        blocker.ZIndex = ModalBaseZIndex + _windowStack.Count * 2;
        SetFullRect(blocker);

        // 2. 将遮蔽阻断器动态加入场景，正好垫在弹窗下方
        window.GetParent().AddChild(blocker);
        window.GetParent().MoveChild(blocker, window.GetIndex());

        // 3. 弹窗自身只接管输入与层级；视觉皮肤由各弹窗的 PopupSkin 保留
        window.MouseFilter = Control.MouseFilterEnum.Stop;
        window.FocusMode = Control.FocusModeEnum.All;
        window.ZIndex = blocker.ZIndex + 1;

        _windowStack.Push(window);
        _blockerStack.Push(blocker);

        blocker.Show();
        window.Show();
        window.GrabFocus();
        
        GD.Print($"[WindowManager]: 已呼出防穿透弹窗 {window.Name}，阻断器已生效！");
    }

    // 关闭最上层的浮窗，释放其下的阻断器
    public void PopWindow()
    {
        if (_windowStack.Count > 0)
        {
            var topWindow = _windowStack.Pop();
            topWindow.Hide();
            topWindow.ReleaseFocus();

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

    private static void SetFullRect(Control control)
    {
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        control.OffsetLeft = 0;
        control.OffsetTop = 0;
        control.OffsetRight = 0;
        control.OffsetBottom = 0;
    }

}
