using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DPrintSpoolerManager;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<PrintJob> _jobs = new();
    private readonly ObservableCollection<string> _logEntries = new();
    private LogWindow? _logWindow;
    private readonly DispatcherTimer _refreshTimer;
    private bool _busy;
    private bool _refreshing;
    private bool _manualRefreshPending;
    private bool _autoRefreshEnabled = true;

    public MainWindow()
    {
        InitializeComponent();

        QueueGrid.ItemsSource = _jobs;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += async (_, _) =>
        {
            if (_autoRefreshEnabled)
                await RefreshDashboard();
        };

        Log("Application started.");
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshDashboard();
        UpdateAutoRefreshButton();
        _refreshTimer.Start();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _refreshTimer.Stop();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e) =>
        await RunAction("Starting Print Spooler...", SpoolerService.Start);

    private async void StopButton_Click(object sender, RoutedEventArgs e) =>
        await RunAction("Stopping Print Spooler...", SpoolerService.Stop);

    private async void RestartButton_Click(object sender, RoutedEventArgs e) =>
        await RunAction("Restarting Print Spooler...", SpoolerService.Restart);

    private async void ClearSelectedButton_Click(object sender, RoutedEventArgs e) =>
        await ClearSelectedJob();

    private async void ClearAllButton_Click(object sender, RoutedEventArgs e) =>
        await ClearAllQueues();

    private void QueueGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateClearButtons();

    private void AutoRefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _autoRefreshEnabled = !_autoRefreshEnabled;

        if (_autoRefreshEnabled)
            _refreshTimer.Start();
        else
            _refreshTimer.Stop();

        UpdateAutoRefreshButton();
        Log(_autoRefreshEnabled ? "Automatic refresh enabled." : "Automatic refresh disabled.");
    }

    private async void RefreshNowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy)
        {
            Log("Manual refresh requested, but an operation is still running.");
            return;
        }

        if (_refreshing)
        {
            _manualRefreshPending = true;
            Log("Manual refresh queued.");
            return;
        }

        Log("Manual refresh started.");
        await RefreshDashboard();
    }

    private void UpdateAutoRefreshButton()
    {
        AutoRefreshButton.Content = _autoRefreshEnabled
            ? "AUTO: ON"
            : "AUTO: OFF";

        AutoRefreshButton.Style = _autoRefreshEnabled
            ? (Style)FindResource("PrimaryButtonStyle")
            : (Style)FindResource("ActionButtonStyle");
    }

    private async Task ClearSelectedJob()
    {
        if (QueueGrid.SelectedItem is not PrintJob selectedJob)
        {
            MessageBox.Show("Select a print job first.", "Clear Selected Job",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var answer = MessageBox.Show(
            $"Delete this print job?\n\nDocument: {selectedJob.Document}\nPrinter: {selectedJob.Printer}",
            "Clear Selected Job",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (answer != MessageBoxResult.Yes)
            return;

        string document = selectedJob.Document;
        await RunAction(
            $"Deleting selected job: {document}...",
            () =>
            {
                bool removed = PrintQueueService.ClearJob(selectedJob.WmiName);
                return removed
                    ? $"Deleted selected job: {document}."
                    : $"The selected job was no longer present: {document}.";
            });
    }

    private async Task ClearAllQueues()
    {
        if (_jobs.Count == 0)
        {
            MessageBox.Show("There are no pending print jobs.", "Clear All Queues",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var answer = MessageBox.Show(
            "Delete ALL pending print jobs from ALL printers?\n\nThis action cannot be undone.",
            "Clear All Queues",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (answer != MessageBoxResult.Yes)
            return;

        await RunAction(
            "Clearing all printer queues...",
            () =>
            {
                int count = PrintQueueService.ClearJobs();
                return $"Removed {count} print job(s) from all printers.";
            });
    }

    private async Task RunAction(string message, Func<string> operation)
    {
        if (_busy)
            return;

        _busy = true;
        SetActionButtons(false);

        OperationText.Text = "Operation: " + message;
        OperationText.Foreground = (Brush)FindResource("StatusWorking");
        OperationProgress.Visibility = Visibility.Visible;
        Log(message);

        try
        {
            string result = await Task.Run(operation);
            Log(result);
            OperationText.Text = "Operation: Completed";
            OperationText.Foreground = (Brush)FindResource("StatusSuccess");
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            OperationText.Text = "Operation: Failed";
            OperationText.Foreground = (Brush)FindResource("StatusError");

            MessageBox.Show(ex.Message, "Operation Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _busy = false;
            OperationProgress.Visibility = Visibility.Collapsed;
            SetActionButtons(true);
            await RefreshDashboard();
        }
    }

    private void SetActionButtons(bool enabled)
    {
        StartButton.IsEnabled = enabled;
        StopButton.IsEnabled = enabled;
        RestartButton.IsEnabled = enabled;
        UpdateClearButtons();
    }

    private void UpdateClearButtons()
    {
        if (_busy)
        {
            ClearSelectedButton.IsEnabled = false;
            ClearAllButton.IsEnabled = false;
            return;
        }

        ClearSelectedButton.IsEnabled = QueueGrid.SelectedItem is PrintJob;
        ClearAllButton.IsEnabled = _jobs.Count > 0;
    }

    private async Task RefreshDashboard()
    {
        if (_refreshing || _busy || !IsLoaded)
            return;

        _refreshing = true;

        try
        {
            var jobs = await Task.Run(PrintQueueService.ReadPrintJobs);
            string serviceStatus = await Task.Run(SpoolerService.GetStatus);

            if (!IsLoaded)
                return;

            ServiceStatusText.Text = serviceStatus;
            ServiceStatusText.Foreground = serviceStatus switch
            {
                "Running" => (Brush)FindResource("StatusSuccess"),
                "Stopped" => (Brush)FindResource("StatusError"),
                _ => (Brush)FindResource("StatusWorking")
            };

            TotalJobsText.Text = jobs.Count.ToString();

            _jobs.Clear();
            foreach (var job in jobs)
                _jobs.Add(job);

            UpdateClearButtons();
        }
        catch (Exception ex)
        {
            ServiceStatusText.Text = "Unavailable";
            ServiceStatusText.Foreground = (Brush)FindResource("StatusError");
            Log("Refresh error: " + ex.Message);
        }
        finally
        {
            _refreshing = false;

            // If the user clicked REFRESH NOW while another refresh was
            // running, perform one additional refresh after the current one.
            if (_manualRefreshPending && !_busy && IsLoaded)
            {
                _manualRefreshPending = false;
                Log("Running queued manual refresh.");
                await RefreshDashboard();
            }
        }
    }

    private bool _isDarkTheme = true;

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow
        {
            Owner = this
        };
        aboutWindow.ShowDialog();
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isDarkTheme = !_isDarkTheme;
        ApplyTheme();
        Log(_isDarkTheme ? "Dark theme enabled." : "Light theme enabled.");
    }

    private void ApplyTheme()
    {
        var resources = Application.Current.Resources;

        if (_isDarkTheme)
        {
            SetBrush(resources, "PageBackground", "#111827");
            SetBrush(resources, "CardBackground", "#1F2937");
            SetBrush(resources, "PrimaryText", "#F3F4F6");
            SetBrush(resources, "SecondaryText", "#9CA3AF");
            SetBrush(resources, "Accent", "#60A5FA");
            SetBrush(resources, "SurfaceAlt", "#273449");
            SetBrush(resources, "BorderBrush", "#374151");
            SetBrush(resources, "GridLine", "#374151");
            SetBrush(resources, "HeaderBackground", "#273449");
            SetBrush(resources, "ButtonBackground", "#374151");
            SetBrush(resources, "ButtonForeground", "#F3F4F6");
            SetBrush(resources, "SelectedRowBackground", "#1D4ED8");
            SetBrush(resources, "SelectedRowForeground", "#FFFFFF");
            SetBrush(resources, "DangerBackground", "#4B2528");
            SetBrush(resources, "DangerForeground", "#FCA5A5");
            SetBrush(resources, "StatusIdle", "#D1D5DB");
            SetBrush(resources, "StatusWorking", "#FBBF24");
            SetBrush(resources, "StatusSuccess", "#4ADE80");
            SetBrush(resources, "StatusError", "#F87171");
            ThemeToggleButton.Content = "LIGHT MODE";
        }
        else
        {
            SetBrush(resources, "PageBackground", "#F3F6FA");
            SetBrush(resources, "CardBackground", "#FFFFFF");
            SetBrush(resources, "PrimaryText", "#17243A");
            SetBrush(resources, "SecondaryText", "#64748B");
            SetBrush(resources, "Accent", "#2563EB");
            SetBrush(resources, "SurfaceAlt", "#EAF0F8");
            SetBrush(resources, "BorderBrush", "#E2E8F0");
            SetBrush(resources, "GridLine", "#E8EDF4");
            SetBrush(resources, "HeaderBackground", "#F7F9FC");
            SetBrush(resources, "ButtonBackground", "#E8EEF7");
            SetBrush(resources, "ButtonForeground", "#24344D");
            SetBrush(resources, "SelectedRowBackground", "#DBEAFE");
            SetBrush(resources, "SelectedRowForeground", "#17243A");
            SetBrush(resources, "DangerBackground", "#FEECEC");
            SetBrush(resources, "DangerForeground", "#B42318");
            SetBrush(resources, "StatusIdle", "#475569");
            SetBrush(resources, "StatusWorking", "#B45309");
            SetBrush(resources, "StatusSuccess", "#15803D");
            SetBrush(resources, "StatusError", "#B91C1C");
            ThemeToggleButton.Content = "DARK MODE";
        }
    }

    private static void SetBrush(
        ResourceDictionary resources,
        string key,
        string color)
    {
        resources[key] = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(color));
    }

    private void ShowLogButton_Click(object sender, RoutedEventArgs e)
    {
        if (_logWindow is null)
        {
            _logWindow = new LogWindow(_logEntries)
            {
                Owner = this
            };

            _logWindow.Closed += (_, _) => _logWindow = null;
            _logWindow.Show();
            return;
        }

        if (_logWindow.WindowState == WindowState.Minimized)
            _logWindow.WindowState = WindowState.Normal;

        _logWindow.Activate();
    }

    private void Log(string message)
    {
        if (!IsInitialized)
            return;

        _logEntries.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
