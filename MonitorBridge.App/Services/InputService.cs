
using System.Runtime.InteropServices;

namespace MonitorBridge.App.Services;

public class InputService
{
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mouse;

        [FieldOffset(0)]
        public KEYBDINPUT keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint numberOfInputs,
        INPUT[] inputs,
        int size);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    public void MoveMouse(int x, int y)
    {
        SetCursorPos(x, y);
    }

    public void LeftClick()
    {
        SendMouseButton(MOUSEEVENTF_LEFTDOWN);
        SendMouseButton(MOUSEEVENTF_LEFTUP);
    }

    public void RightClick()
    {
        SendMouseButton(MOUSEEVENTF_RIGHTDOWN);
        SendMouseButton(MOUSEEVENTF_RIGHTUP);
    }

    public void Scroll(int delta)
    {
        SendMouse(MOUSEEVENTF_WHEEL, unchecked((uint)delta));
    }

    private void SendMouseButton(uint flags)
    {
        SendMouse(flags);
    }

    private void SendMouse(uint flags, uint mouseData = 0)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,
            data = new InputUnion
            {
                mouse = new MOUSEINPUT
                {
                    dwFlags = flags,
                    mouseData = mouseData
                }
            }
        };

        SendInput(
            1,
            new[] { input },
            Marshal.SizeOf<INPUT>());
    }

    public void MoveMouseAbsolute(int x, int y)
    {
        int virtualLeft = GetSystemMetrics(76);
        int virtualTop = GetSystemMetrics(77);
        int virtualWidth = GetSystemMetrics(78);
        int virtualHeight = GetSystemMetrics(79);

        if (virtualWidth <= 1 || virtualHeight <= 1)
            return;

        int normalizedX = (int)Math.Round(
            (x - virtualLeft) * 65535.0 / (virtualWidth - 1));

        int normalizedY = (int)Math.Round(
            (y - virtualTop) * 65535.0 / (virtualHeight - 1));

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            data = new InputUnion
            {
                mouse = new MOUSEINPUT
                {
                    dx = Math.Clamp(normalizedX, 0, 65535),
                    dy = Math.Clamp(normalizedY, 0, 65535),
                    dwFlags = MOUSEEVENTF_MOVE |
                              MOUSEEVENTF_ABSOLUTE |
                              MOUSEEVENTF_VIRTUALDESK
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }
}
