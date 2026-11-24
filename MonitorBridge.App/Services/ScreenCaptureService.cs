
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MonitorBridge.App.Models;

namespace MonitorBridge.App.Services;

public class ScreenCaptureService
{
    private const uint SRCCOPY = 0x00CC0020;
    private const uint CAPTUREBLT = 0x40000000;

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(
        IntPtr hWnd,
        IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(
        IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(
        IntPtr hDC,
        int width,
        int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(
        IntPtr hDC,
        IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(
        IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(
        IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest,
        int xDest,
        int yDest,
        int width,
        int height,
        IntPtr hdcSource,
        int xSource,
        int ySource,
        uint rasterOperation);

    public BitmapSource Capture(DisplayMonitor monitor)
    {
        IntPtr screenDC = IntPtr.Zero;
        IntPtr memoryDC = IntPtr.Zero;
        IntPtr bitmap = IntPtr.Zero;
        IntPtr previousObject = IntPtr.Zero;

        try
        {
            screenDC = GetDC(IntPtr.Zero);

            if (screenDC == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Unable to access desktop.");

            memoryDC = CreateCompatibleDC(screenDC);

            if (memoryDC == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Unable to create memory DC.");

            bitmap = CreateCompatibleBitmap(
                screenDC,
                monitor.Width,
                monitor.Height);

            if (bitmap == IntPtr.Zero)
                throw new InvalidOperationException(
                    "Unable to create bitmap.");

            previousObject = SelectObject(memoryDC, bitmap);

            bool success = BitBlt(
                memoryDC,
                0,
                0,
                monitor.Width,
                monitor.Height,
                screenDC,
                monitor.Left,
                monitor.Top,
                SRCCOPY | CAPTUREBLT);

            if (!success)
                throw new InvalidOperationException(
                    "Screen capture failed.");

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            source.Freeze();

            return source;
        }
        finally
        {
            if (previousObject != IntPtr.Zero &&
                memoryDC != IntPtr.Zero)
            {
                SelectObject(memoryDC, previousObject);
            }

            if (bitmap != IntPtr.Zero)
                DeleteObject(bitmap);

            if (memoryDC != IntPtr.Zero)
                DeleteDC(memoryDC);

            if (screenDC != IntPtr.Zero)
                ReleaseDC(IntPtr.Zero, screenDC);
        }
    }
    public Task<BitmapSource> CaptureAsync(DisplayMonitor monitor)
{
    return Task.Run(() => Capture(monitor));
}
}
