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

namespace GeneSort.UI.ViewModels
{
    public partial class WorkspaceParamsVim : ObservableObject
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
        private string selectedRunsText = "No runs selected";

        [ObservableProperty]
        private ObservableCollection<string> reportKeys = new();

        private CancellationTokenSource? _runCancellationTokenSource;

        // Store the domain object for potential future operations
        public workspace? Workspace { get; private set; }

        public WorkspaceParamsVim()
        {
            // Initialize with default state
            CanRunSelected = false;
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
                var workspace = WorkspaceDto.fromWorkspaceDto(workspaceDto);
                Workspace = workspace; // Store for future use

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Extract workspace info from domain object
                    WorkspaceName = workspace.Name;
                    WorkspaceDescription = workspace.Description;
                    RootDirectory = workspace.RootDirectory;

                    // Load report keys
                    ReportKeys.Clear();
                    foreach (var reportKey in workspace.ReportKeys)
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

            // Subscribe to selection changes
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            dataGrid.SelectionMode = DataGridSelectionMode.Extended; // Allow multiple selections

            ParametersDataGrid = dataGrid;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var selectedCount = dataGrid.SelectedItems.Count;
                UpdateButtonStates(selectedCount);
            }
        }

        private void UpdateButtonStates(int selectedCount)
        {
            // Run button is enabled if we have selections OR if we're currently running (for cancellation)
            CanRunSelected = selectedCount > 0; //|| IsLoading;

            SelectedRunsText = selectedCount switch
            {
                0 => IsLoading ? "Running..." : "No runs selected",
                1 => IsLoading ? "Running 1 parameter set..." : "1 run selected",
                _ => IsLoading ? $"Running {selectedCount} parameter sets..." : $"{selectedCount} runs selected"
            };
        }

        [RelayCommand]
        private async Task RunSelected()
        {
            if (IsLoading) // If already running, cancel
            {
                _runCancellationTokenSource?.Cancel();
                return;
            }

            if (ParametersDataGrid == null || Workspace == null)
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

            // Create new cancellation token for this run
            _runCancellationTokenSource = new CancellationTokenSource();

            // Call your run method here
            await ExecuteRunParameters(selectedRunParameters, _runCancellationTokenSource.Token);
        }

        private async Task ExecuteRunParameters(List<runParameters> runParameters, CancellationToken cancellationToken)
        {
            try
            {
                IsLoading = true;

                // TODO: Implement your actual run logic here with cancellation support
                // For now, just simulate some work with cancellation
                for (int i = 0; i < runParameters.Count; i++)
                {
                    // Check for cancellation before processing each parameter set
                    cancellationToken.ThrowIfCancellationRequested();

                    // Simulate work (replace with actual run logic)
                    await Task.Delay(2000, cancellationToken);

                    System.Diagnostics.Debug.WriteLine($"Executed parameter set {i + 1} of {runParameters.Count}: {runParameters[i].toString()}");
                }

                // Example of what you might do:
                // foreach (var runParam in runParameters)
                // {
                //     cancellationToken.ThrowIfCancellationRequested();
                //     await YourRunEngine.ExecuteAsync(runParam, cancellationToken);
                // }

                System.Diagnostics.Debug.WriteLine($"Completed execution of {runParameters.Count} parameter sets");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Run execution was cancelled");
                // Don't set ErrorMessage for cancellation - it's expected
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error executing runs: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _runCancellationTokenSource?.Dispose();
                _runCancellationTokenSource = null;

                // Update button states with actual selection count
                if (ParametersDataGrid != null)
                {
                    var selectedCount = ParametersDataGrid.SelectedItems.Count;
                    UpdateButtonStates(selectedCount);
                }
            }
        }
        partial void OnIsLoadingChanged(bool value)
        {
            // Update button states when loading state changes
            if (ParametersDataGrid != null)
            {
                var selectedCount = ParametersDataGrid.SelectedItems.Count;
                UpdateButtonStates(selectedCount);
            }
        }









        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private bool _canCancel;

        private CancellationTokenSource _cts;

        [RelayCommand(CanExecute = nameof(CanRunOperation))]
        private async Task RunOperation()
        {
            IsRunning = true;
            CanCancel = true;
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(10000, _cts.Token); // Simulate long-running operation
            }
            catch (TaskCanceledException)
            {
                // Operation canceled
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
            return !IsRunning;
        }

        [RelayCommand(CanExecute = nameof(CanCancelOperation))]
        private void CancelOperation()
        {
            _cts?.Cancel();
        }

        private bool CanCancelOperation()
        {
            return IsRunning && CanCancel;
        }
    }
}