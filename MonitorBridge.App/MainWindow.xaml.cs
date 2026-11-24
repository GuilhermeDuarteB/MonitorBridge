
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MonitorBridge.App.Models;
using MonitorBridge.App.Services;

namespace MonitorBridge.App;

public partial class MainWindow : Window
{
    private readonly MonitorService _monitorService = new();
    private readonly ScreenCaptureService _captureService = new();

    private readonly DispatcherTimer _captureTimer;

    private bool _isPreviewRunning;
    private bool _isCapturing;

    public MainWindow()
    {
        InitializeComponent();

        _captureTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        _captureTimer.Tick += CaptureTimer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshMonitors();
    }

    private void RefreshMonitors()
    {
        var monitors = _monitorService.GetMonitors();

        MonitorComboBox.ItemsSource = monitors;

        var secondaryMonitor = monitors.FirstOrDefault(
            monitor => !monitor.IsPrimary);

        MonitorComboBox.SelectedItem =
            secondaryMonitor ?? monitors.FirstOrDefault();

        StatusText.Text = $"{monitors.Count} monitor(s) detected.";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        StopPreview();
        RefreshMonitors();
    }

    private async void StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_isPreviewRunning)
        {
            StopPreview();
            return;
        }

        if (MonitorComboBox.SelectedItem is not DisplayMonitor)
        {
            StatusText.Text = "Please select a monitor.";
            return;
        }

        _isPreviewRunning = true;

        StartStopButton.Content = "Stop Preview";
        EmptyState.Visibility = Visibility.Collapsed;

        _captureTimer.Start();

        await CaptureFrameAsync();
    }

    private void StopPreview()
    {
        _captureTimer.Stop();

        _isPreviewRunning = false;

        StartStopButton.Content = "Start Preview";
        PreviewImage.Source = null;

        EmptyState.Visibility = Visibility.Visible;

        StatusText.Text = "Preview stopped.";
    }

private async void CaptureTimer_Tick(
    object? sender,
    EventArgs e)
{
    await CaptureFrameAsync();
}

private async Task CaptureFrameAsync()
{
    if (_isCapturing || !_isPreviewRunning)
        return;

    if (MonitorComboBox.SelectedItem is not DisplayMonitor monitor)
        return;

    _isCapturing = true;

    try
    {
        var frame = await _captureService.CaptureAsync(monitor);

        if (_isPreviewRunning)
        {
            PreviewImage.Source = frame;

            StatusText.Text =
                $"Live preview | {monitor.Name} | {monitor.Resolution}";
        }
    }
    catch (Exception ex)
    {
        StopPreview();

        StatusText.Text = $"Capture error: {ex.Message}";
    }
    finally
    {
        _isCapturing = false;
    }
}

    private async void MonitorComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isPreviewRunning)
            await CaptureFrameAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        _captureTimer.Stop();
        base.OnClosed(e);
    }
}
