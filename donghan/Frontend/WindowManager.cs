using Godot;
using System.Collections.Generic;

namespace DonghanFrontend;

public partial class WindowManager : Node
{
    private Stack<Control> _windowStack = new();

    // 打开一个多层浮窗（如大臣资料、后宫列表）
    public void PushWindow(Control window)
    {
        if (window == null) return;

        // 设置全屏遮罩/Stop过滤，阻止鼠标事件穿透到下层UI
        window.MouseFilter = Control.MouseFilterEnum.Stop;
        
        _windowStack.Push(window);
        window.Show();
        
        GD.Print($"[WindowManager]: 已开启新窗口 {window.Name}，当前层级深度: {_windowStack.Count}");
    }

    // 关闭最上层的浮窗
    public void PopWindow()
    {
        if (_windowStack.Count > 0)
        {
            var topWindow = _windowStack.Pop();
            topWindow.Hide();
            GD.Print($"[WindowManager]: 已关闭窗口 {topWindow.Name}，剩余窗口数量: {_windowStack.Count}");
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
                // 标记输入已被消耗，不再向下传递
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
