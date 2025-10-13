using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.Project;
using MessagePack;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace GeneSort.UI.ViewModels
{
    public partial class WorkspaceParamsVm : ObservableObject
    {
        [ObservableProperty]
        private string workspaceName = string.Empty;

        [ObservableProperty]
        private string workspaceDescription = string.Empty;

        [ObservableProperty]
        private string rootDirectory = string.Empty;

        [ObservableProperty]
        private string filePath = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private DataGrid? parametersDataGrid;

        [ObservableProperty]
        private bool canRunSelected;

        [ObservableProperty]
        private string runMessage = "No runs selected";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunOperationCommand))]
        private int _selectedCount;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunOperationCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelOperationCommand))]
        private bool _isRunning;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CancelOperationCommand))]
        private bool _canCancel;

        [ObservableProperty]
        private ObservableCollection<string> reportKeys = new();


        public workspace? Workspace { get; private set; }

        public WorkspaceParamsVm()
        {
            _cts = new CancellationTokenSource();
            CanRunSelected = false;
            IsRunning = false;
        }

        public async Task LoadWorkspaceAsync(string filePath)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                FilePath = filePath;

                // Read the serialized WorkspaceDto
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var workspaceDto = MessagePackSerializer.Deserialize<workspaceDto>(fileBytes, Models.MessagePackConfig.Options);

                await LoadWorkspaceData(workspaceDto);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading workspace: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GenerateReport(string reportKey)
        {
            if (ParametersDataGrid == null || Workspace == null || string.IsNullOrEmpty(reportKey))
                return;

            var selectedItems = ParametersDataGrid.SelectedItems.Cast<Dictionary<string, object>>().ToList();
            if (!selectedItems.Any())
                return;

            // Extract the runParameters for the selected rows
            var selectedRunParameters = new List<runParameters>();

            foreach (var selectedItem in selectedItems)
            {
                // Get the index (1-based in UI, 0-based in array)
                if (selectedItem.TryGetValue("Index", out var indexObj) && indexObj is int index)
                {
                    var arrayIndex = index - 1; // Convert to 0-based
                    if (arrayIndex >= 0 && arrayIndex < Workspace.RunParametersArray.Length)
                    {
                        selectedRunParameters.Add(Workspace.RunParametersArray[arrayIndex]);
                    }
                }
            }

            // Call your report generation method here
            await ExecuteReportGeneration(reportKey, selectedRunParameters);
        }

        private async Task ExecuteReportGeneration(string reportKey, List<runParameters> runParameters)
        {
            try
            {
                IsLoading = true;

                // TODO: Implement your actual report generation logic here
                // For now, just simulate some work
                await Task.Delay(1000);

                // Example of what you might do:
                // await YourReportEngine.GenerateReportAsync(reportKey, runParameters);

                System.Diagnostics.Debug.WriteLine($"Would generate report '{reportKey}' for {runParameters.Count} parameter sets:");
                foreach (var runParam in runParameters)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {runParam.toString()}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating report '{reportKey}': {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWorkspaceData(workspaceDto workspaceDto)
        {
            await Task.Run(() =>
            {
                // Convert DTO to workspace domain object
                var workspace = WorkspaceDto.toDomain(workspaceDto);
                Workspace = workspace; // Store for future use

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Extract workspace info from domain object
                    WorkspaceName = workspace.Name;
                    WorkspaceDescription = workspace.Description;
                    RootDirectory = workspace.RootDirectory;

                    // Load report keys
                    ReportKeys.Clear();
                    foreach (var reportKey in workspace.ReportNames)
                    {
                        ReportKeys.Add(reportKey);
                    }

                    // Create the DataGrid with all columns and data
                    CreateParametersDataGrid(workspace);
                });
            });
        }

        private void CreateParametersDataGrid(workspace workspace)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                HeadersVisibility = DataGridHeadersVisibility.All,
                AlternationCount = 2
            };

            // Add styling
            var rowStyle = new Style(typeof(DataGridRow));
            var alternationTrigger = new Trigger { Property = ItemsControl.AlternationIndexProperty, Value = 1 };
            alternationTrigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, System.Windows.Media.Brushes.LightGray));
            rowStyle.Triggers.Add(alternationTrigger);
            dataGrid.RowStyle = rowStyle;

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(5)));
            cellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0, 0, 1, 0)));
            cellStyle.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, System.Windows.Media.Brushes.LightGray));
            dataGrid.CellStyle = cellStyle;

            // Create header style
            var headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, System.Windows.Media.Brushes.DarkBlue));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.ForegroundProperty, System.Windows.Media.Brushes.White));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.FontWeightProperty, FontWeights.Bold));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.PaddingProperty, new Thickness(5)));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));

            // Add Index Column
            var indexColumn = new DataGridTextColumn
            {
                Header = "Run #",
                Width = new DataGridLength(60, DataGridLengthUnitType.Pixel),
                HeaderStyle = headerStyle,
                Binding = new Binding("[Index]")  // Use dictionary-style binding
            };

            var indexElementStyle = new Style(typeof(TextBlock));
            indexElementStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            indexElementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            indexColumn.ElementStyle = indexElementStyle;

            dataGrid.Columns.Add(indexColumn);

            // Add columns for each parameter key
            var parameterKeys = workspace.ParameterKeys;
            foreach (var key in parameterKeys.OrderBy(k => k))
            {
                var column = new DataGridTextColumn
                {
                    Header = key,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                    HeaderStyle = headerStyle,
                    Binding = new Binding($"[{key}]")
                };

                dataGrid.Columns.Add(column);
            }

            // Create data rows
            var dataRows = new ObservableCollection<Dictionary<string, object>>();
            var runParametersArray = workspace.RunParametersArray;

            for (int i = 0; i < runParametersArray.Length; i++)
            {
                var runParam = runParametersArray[i];
                var row = new Dictionary<string, object>
                {
                    ["Index"] = i + 1
                };

                // Add parameter values for each key
                foreach (var key in parameterKeys)
                {
                    if (runParam.ParamMap.TryGetValue(key, out var value))
                    {
                        row[key] = value;
                    }
                    else
                    {
                        row[key] = "";
                    }
                }

                dataRows.Add(row);
            }

            dataGrid.ItemsSource = dataRows;

            // Make selected rows visible even when unfocused
            dataGrid.Resources[SystemColors.InactiveSelectionHighlightBrushKey] = SystemColors.HighlightBrush;
            dataGrid.Resources[SystemColors.InactiveSelectionHighlightTextBrushKey] = SystemColors.HighlightTextBrush;

            // Subscribe to selection changes
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            dataGrid.PreviewMouseDown += DataGrid_PreviewMouseDown;
            dataGrid.SelectionMode = DataGridSelectionMode.Extended; // Allow multiple selections

            ParametersDataGrid = dataGrid;
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsRunning)
            {
                e.Handled = true; // Prevent selection changes while running
                return;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var selectedCount = dataGrid.SelectedItems.Count;
                SelectedCount = selectedCount; // Update the observable property (this will trigger command requery)
                UpdateSelectedRuns(selectedCount);
            }
        }

        private void UpdateSelectedRuns(int selectedCount)
        {
            RunMessage = selectedCount switch
            {
                0 => IsLoading ? "Running..." : "No runs selected",
                1 => IsLoading ? "Running 1 parameter set..." : "1 run selected",
                _ => IsLoading ? $"Running {selectedCount} parameter sets..." : $"{selectedCount} runs selected"
            };
        }


        private CancellationTokenSource _cts;


        #region Run Command

        [RelayCommand(CanExecute = nameof(CanRunOperation))]
        private async Task RunOperation()
        {
            IsRunning = true;
            CanCancel = true;
            _cts = new CancellationTokenSource();

            try
            {
                foreach (var selectedRow in ParametersDataGrid!.SelectedItems)
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;
                    // Simulate processing each selected item
                    var qua = selectedRow as Dictionary<string, object>;
                    var yab = (int)qua["Index"];
                    RunMessage = $"Executing Run {yab}";
                    await Task.Delay(1000, _cts.Token); // Simulate work per item
                }
                RunMessage = $"Finished";
            }
            catch (TaskCanceledException)
            {
                RunMessage = "Operation canceled";
            }
            finally
            {
                IsRunning = false;
                CanCancel = false;
                _cts = null;
            }
        }

        private bool CanRunOperation()
        {
            return !IsRunning && SelectedCount > 0;
        }

        #endregion



        #region Cancel

        [RelayCommand(CanExecute = nameof(CanCancelOperation))]
        private void CancelOperation()
        {
            _cts?.Cancel();
        }

        private bool CanCancelOperation()
        {
            return IsRunning && CanCancel;
        }

        #endregion
    }
}