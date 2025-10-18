using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GeneSort.Project;
using GeneSort.Runs.Params;
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
    public partial class ProjectParamsVm : ObservableObject, IDisposable
    {
        private bool _disposed;
        [ObservableProperty]
        private string projectName = string.Empty;

        [ObservableProperty]
        private string projectDescription = string.Empty;

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

        [ObservableProperty]
        private int progressValue;

        [ObservableProperty]
        private int progressMaximum;

        [ObservableProperty]
        private string progressMessage = string.Empty;

        private CancellationTokenSource? _cts;

        public project? Workspace { get; private set; }

        public ProjectParamsVm()
        {
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
                var projectDto = MessagePackSerializer.Deserialize<projectDto>(fileBytes, Models.MessagePackConfig.Options);

                await LoadProjectDto(projectDto);
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
                ErrorMessage = null;

                IProgress<(int current, int total, string message)> progress = new Progress<(int current, int total, string message)>(report =>
                {
                    ProgressValue = report.current;
                    ProgressMaximum = report.total;
                    ProgressMessage = report.message;
                });

                // TODO: Implement your actual report generation logic here
                await GenerateReportWithProgress(reportKey, runParameters, progress);

                System.Diagnostics.Debug.WriteLine($"Generated report '{reportKey}' for {runParameters.Count} parameter sets");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating report '{reportKey}': {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                ProgressValue = 0;
                ProgressMaximum = 0;
                ProgressMessage = string.Empty;
            }
        }

        private async Task GenerateReportWithProgress(string reportKey, List<runParameters> runParameters, IProgress<(int current, int total, string message)> progress)
        {
            var total = runParameters.Count;
            for (int i = 0; i < total; i++)
            {
                var runParam = runParameters[i];
                progress.Report((i + 1, total, $"Processing parameter set {i + 1} of {total}"));

                // Simulate work
                await Task.Delay(1000);

                System.Diagnostics.Debug.WriteLine($"  - {runParam.toString()}");
            }
        }

        private async Task LoadProjectDto(projectDto workspaceDto)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Validate input
                    if (workspaceDto == null)
                    {
                        throw new ArgumentNullException(nameof(workspaceDto), "Project data cannot be null");
                    }

                    // Convert DTO to workspace domain object
                    var project = ProjectDto.toDomain(workspaceDto);

                    if (project == null)
                    {
                        throw new InvalidOperationException("Failed to convert project DTO to domain object");
                    }

                    // Validate workspace data
                    if (string.IsNullOrWhiteSpace(project.Name))
                    {
                        throw new InvalidDataException("Workspace name is required");
                    }

                    if (project.RunParametersArray == null || project.RunParametersArray.Length == 0)
                    {
                        throw new InvalidDataException("Workspace must contain at least one run parameter set");
                    }

                    Workspace = project; // Store for future use

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            // Extract workspace info from domain object
                            ProjectName = project.Name ?? string.Empty;
                            ProjectDescription = project.Description ?? string.Empty;
                            RootDirectory = project.RootDirectory ?? string.Empty;

                            // Load report keys
                            ReportKeys.Clear();
                            if (project.ReportNames != null)
                            {
                                foreach (var reportKey in project.ReportNames)
                                {
                                    if (!string.IsNullOrWhiteSpace(reportKey))
                                    {
                                        ReportKeys.Add(reportKey);
                                    }
                                }
                            }

                            // Create the DataGrid with all columns and data
                            CreateParametersDataGrid(project);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Error updating UI with workspace data: {ex.Message}", ex);
                        }
                    });
                });
            }
            catch (ArgumentNullException ex)
            {
                ErrorMessage = $"Invalid project data: {ex.Message}";
                throw;
            }
            catch (InvalidDataException ex)
            {
                ErrorMessage = $"Project validation failed: {ex.Message}";
                throw;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                throw;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected error loading project data: {ex.Message}";
                throw;
            }
        }

        private void CreateParametersDataGrid(project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

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

            //// Add Index Column
            //var indexColumn = new DataGridTextColumn
            //{
            //    Header = "Run #",
            //    Width = new DataGridLength(60, DataGridLengthUnitType.Pixel),
            //    HeaderStyle = headerStyle,
            //    Binding = new Binding("[Index]")
            //};

            //var indexElementStyle = new Style(typeof(TextBlock));
            //indexElementStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            //indexElementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            //indexColumn.ElementStyle = indexElementStyle;

            //dataGrid.Columns.Add(indexColumn);

            // Add columns for each parameter key
            var parameterKeys = project.ParameterKeys;
            if (parameterKeys != null)
            {
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
            }

            // Create data rows
            var dataRows = new ObservableCollection<Dictionary<string, object>>();
            var runParametersArray = project.RunParametersArray;

            for (int i = 0; i < runParametersArray.Length; i++)
            {
                var runParam = runParametersArray[i];
                var row = new Dictionary<string, object>
                {
                    ["Index"] = i + 1,
                    ["RunParams"] = runParam
                };

                // Add parameter values for each key
                if (parameterKeys != null)
                {
                    foreach (var key in parameterKeys)
                    {
                        if (runParam.ParamMap != null && runParam.ParamMap.TryGetValue(key, out var value))
                        {
                            row[key] = value ?? string.Empty;
                        }
                        else
                        {
                            row[key] = string.Empty;
                        }
                    }
                }

                dataRows.Add(row);
            }

            dataGrid.ItemsSource = dataRows;

            // Make selected rows visible even when unfocused
            dataGrid.Resources[SystemColors.InactiveSelectionHighlightBrushKey] = SystemColors.HighlightBrush;
            dataGrid.Resources[SystemColors.InactiveSelectionHighlightTextBrushKey] = SystemColors.HighlightTextBrush;

            // Subscribe to selection changes using weak events
            WeakEventManager<DataGrid, SelectionChangedEventArgs>.AddHandler(
                dataGrid,
                nameof(DataGrid.SelectionChanged),
                DataGrid_SelectionChanged);

            WeakEventManager<DataGrid, MouseButtonEventArgs>.AddHandler(
                dataGrid,
                nameof(DataGrid.PreviewMouseDown),
                DataGrid_PreviewMouseDown);

            dataGrid.SelectionMode = DataGridSelectionMode.Extended;

            ParametersDataGrid = dataGrid;
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsRunning)
            {
                e.Handled = true;
                return;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var selectedCount = dataGrid.SelectedItems.Count;
                SelectedCount = selectedCount;
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

        #region Run Command

        [RelayCommand(CanExecute = nameof(CanRunOperation))]
        private async Task RunOperation()
        {
            IsRunning = true;
            CanCancel = true;
            _cts = new CancellationTokenSource();
            ErrorMessage = null;

            try
            {
                if (ParametersDataGrid?.SelectedItems == null)
                {
                    throw new InvalidOperationException("No items selected for processing");
                }

                var selectedItems = ParametersDataGrid.SelectedItems.Cast<Dictionary<string, object>>().ToList();
                var total = selectedItems.Count;

                IProgress<(int current, int total, string message)> progress = new Progress<(int current, int total, string message)>(report =>
                {
                    ProgressValue = report.current;
                    ProgressMaximum = report.total;
                    ProgressMessage = report.message;
                });

                ProgressMaximum = total;

                for (int i = 0; i < selectedItems.Count; i++)
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;

                    var selectedRow = selectedItems[i];
                    var runIndex = selectedRow.ContainsKey("Index") ? selectedRow["Index"] : "???";
                    progress.Report((i + 1, total, $"Executing Run {runIndex}"));
                    RunMessage = $"Executing Run {runIndex} ({i + 1} of {total})";
                    await Task.Delay(1000, _cts.Token);
                }

                RunMessage = _cts.Token.IsCancellationRequested ? "Operation canceled" : "Finished";
            }
            catch (TaskCanceledException)
            {
                RunMessage = "Operation canceled";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error during operation: {ex.Message}";
                RunMessage = "Operation failed";
            }
            finally
            {
                IsRunning = false;
                CanCancel = false;
                ProgressValue = 0;
                ProgressMaximum = 0;
                ProgressMessage = string.Empty;
                _cts?.Dispose();
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

        #region IDisposable Pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources
                _cts?.Cancel();
                _cts?.Dispose();

                if (ParametersDataGrid != null)
                {
                    // Remove weak event handlers
                    WeakEventManager<DataGrid, SelectionChangedEventArgs>.RemoveHandler(
                        ParametersDataGrid,
                        nameof(DataGrid.SelectionChanged),
                        DataGrid_SelectionChanged);

                    WeakEventManager<DataGrid, MouseButtonEventArgs>.RemoveHandler(
                        ParametersDataGrid,
                        nameof(DataGrid.PreviewMouseDown),
                        DataGrid_PreviewMouseDown);
                }
            }

            _disposed = true;
        }

        #endregion
    }
}