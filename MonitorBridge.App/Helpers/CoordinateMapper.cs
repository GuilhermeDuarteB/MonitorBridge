
using System.Windows;

using WpfPoint = System.Windows.Point;
using WpfRect = System.Windows.Rect;

namespace MonitorBridge.App.Helpers;

public static class CoordinateMapper
{
    public static WpfPoint MapToMonitor(
        WpfPoint pointer,
        WpfRect imageBounds,
        int monitorWidth,
        int monitorHeight)
    {
        if (imageBounds.Width <= 0 || imageBounds.Height <= 0)
            return new WpfPoint(0, 0);

        double relativeX =
            (pointer.X - imageBounds.X) / imageBounds.Width;

        double relativeY =
            (pointer.Y - imageBounds.Y) / imageBounds.Height;

        int x = (int)Math.Round(relativeX * (monitorWidth - 1));
        int y = (int)Math.Round(relativeY * (monitorHeight - 1));

        return new WpfPoint(
            Math.Clamp(x, 0, monitorWidth - 1),
            Math.Clamp(y, 0, monitorHeight - 1));
    }
}
