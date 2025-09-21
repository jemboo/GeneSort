using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class WorkspaceViewerViewModel : ObservableObject
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

        // Store the domain object for potential future operations
        public workspace? Workspace { get; private set; }

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
            ParametersDataGrid = dataGrid;
        }
    }
}