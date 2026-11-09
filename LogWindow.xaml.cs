using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace DPrintSpoolerManager;

public partial class LogWindow : Window
{
    private readonly ObservableCollection<string> _logEntries;

    public LogWindow(ObservableCollection<string> logEntries)
    {
        InitializeComponent();

        _logEntries = logEntries;
        LogListBox.ItemsSource = _logEntries;

        _logEntries.CollectionChanged += LogEntries_CollectionChanged;

        Loaded += (_, _) => ScrollToLatest();
        Closed += (_, _) => _logEntries.CollectionChanged -= LogEntries_CollectionChanged;
    }

    private void LogEntries_CollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        ScrollToLatest();
    }

    private void ScrollToLatest()
    {
        if (_logEntries.Count == 0)
            return;

        LogListBox.ScrollIntoView(_logEntries[^1]);
    }
}
