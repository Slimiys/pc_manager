using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Client.Avalonia.ViewModels;

namespace Client.Avalonia.Controls.Redmine;

/// <summary>
/// Панель вкладки Redmine.
/// </summary>
public partial class RedminePanel : UserControl
{
    /// <summary>
    /// Создаёт панель Redmine.
    /// </summary>
    public RedminePanel()
    {
        InitializeComponent();
    }

    private void OnIssuesLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.DataContext is not RedmineIssueRowViewModel row)
        {
            return;
        }

        var interaction = new IssueRowInteraction(e.Row, row);
        e.Row.Tag = interaction;
    }

    private void OnIssuesUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (e.Row.Tag is IssueRowInteraction interaction)
        {
            interaction.Dispose();
            e.Row.Tag = null;
        }
    }

    private void OnFilterFlyoutOpening(object? sender, EventArgs e)
    {
        if (DataContext is RedmineTabViewModel viewModel)
        {
            viewModel.RebuildFilterOptions();
        }
    }

    private void OnIssueFetchLimitLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RedmineTabViewModel viewModel)
        {
            viewModel.CommitIssueFetchLimit();
        }
    }

    /// <summary>
    /// Подсветка строки и сброс при наведении курсора.
    /// </summary>
    private sealed class IssueRowInteraction : IDisposable
    {
        private readonly DataGridRow _gridRow;
        private readonly RedmineIssueRowViewModel _row;
        private readonly PropertyChangedEventHandler _propertyChangedHandler;
        private readonly EventHandler<PointerEventArgs> _pointerEnteredHandler;

        /// <summary>
        /// Создаёт обработчики для строки таблицы задач.
        /// </summary>
        public IssueRowInteraction(DataGridRow gridRow, RedmineIssueRowViewModel row)
        {
            _gridRow = gridRow;
            _row = row;
            _propertyChangedHandler = OnRowPropertyChanged;
            _pointerEnteredHandler = OnPointerEntered;

            _row.PropertyChanged += _propertyChangedHandler;
            _gridRow.PointerEntered += _pointerEnteredHandler;
            UpdateHighlightClass();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _row.PropertyChanged -= _propertyChangedHandler;
            _gridRow.PointerEntered -= _pointerEnteredHandler;
        }

        private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RedmineIssueRowViewModel.IsNewHighlight))
            {
                UpdateHighlightClass();
            }
        }

        private void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            _row.AcknowledgeHighlight();
        }

        private void UpdateHighlightClass()
        {
            if (_row.IsNewHighlight)
            {
                if (!_gridRow.Classes.Contains("new-issue"))
                {
                    _gridRow.Classes.Add("new-issue");
                }
            }
            else
            {
                _gridRow.Classes.Remove("new-issue");
            }
        }
    }
}
