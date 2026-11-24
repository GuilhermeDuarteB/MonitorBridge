using MonitorBridge.App.Models;
using Forms = System.Windows.Forms;

namespace MonitorBridge.App.Services;

public class MonitorService
{
    public List<DisplayMonitor> GetMonitors()
    {
        return Forms.Screen.AllScreens.Select((screen, index) =>
        {
            var bounds = screen.Bounds;

            return new DisplayMonitor
            {
                DeviceName = screen.DeviceName,
                Name = screen.Primary ? $"Display {index + 1} (Primary)" : $"Display {index + 1}",

                Left = bounds.Left,
                Top = bounds.Top,
                Height = bounds.Height,
                Width = bounds.Width,
                IsPrimary = screen.Primary
            };
        })
        .ToList();
    }
}