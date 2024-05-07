using System.Diagnostics;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;
using GigaChad_Corp_Usermanager.MySQL;
using GigaChad_Corp_Usermanager.Database;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;

namespace GigaChad_Corp_Usermanager.GUI
{
    public partial class MainWindow : Window {

        private readonly string databaseName = "gigachadcorp";
        private DBConnector dbConnector;
        private readonly TextBox[] searchTextBoxes;
        private readonly ComboBox[] searchModeBoxes;
        private readonly DataGrid[] resultGrids;

        public MainWindow() {
            InitializeComponent();
            searchTextBoxes = new TextBox[] { EmployeeSearchBox, DepartmentSearchBox, ProjectSearchBox, ProjectAssignmentSearchBox };
            searchModeBoxes = new ComboBox[] { EmployeeSearchModeBox, DepartmentSearchModeBox, ProjectSearchModeBox, ProjectAssignmentSearchModeBox };
            resultGrids = new DataGrid[] { EmployeeResultDataGrid, DepartmentResultDataGrid, ProjectResultDataGrid, ProjectAssignmentResultDataGrid };
            DataContext = this;
            PopulateUserSelectBoxItems();
            PopulateSearchBoxItems();
            dbConnector = new DBConnector("admin", "admin", databaseName);
            Trace.WriteLine($"Tried to open connection. Success was: {dbConnector.IsConnectionAlive()}");
            for (int i = 0; i < resultGrids.Length; i++) {
                UpdateDataGrid(i);
            }
        }

        public static string GetEnumDescription(Enum value) {
            FieldInfo fi = value.GetType().GetField(value.ToString())!;
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0) {
                return attributes[0].Description;
            }
            else {
                return value.ToString();
            }   
        }

        private void PopulateComboBoxItems(ComboBox[] boxes, Type enumClass, object defaultValue) {
            var values = Enum.GetValues(enumClass)
                .Cast<Enum>()
                .Select(value => new {
                    Description = GetEnumDescription(value),
                    Value = value
                }).ToList();

            foreach (var box in boxes) {
                box.ItemsSource = values;
                box.DisplayMemberPath = "Description";
                box.SelectedValuePath = "Value";
                box.SelectedValue = defaultValue;
            }
        }

        private void PopulateUserSelectBoxItems() {
            PopulateComboBoxItems(new ComboBox[] { UserSelectBox }, typeof(UserType), UserType.Employee);
        }

        private void PopulateSearchBoxItems() {
            PopulateComboBoxItems(searchModeBoxes, typeof(SearchMode), SearchMode.Contains);
        }

        private DataTable? GetDataTable(int selectedMenuIndex) {
            if (selectedMenuIndex < 0 || selectedMenuIndex >= resultGrids.Length) {  return null; }
            UserType userType = (UserType)UserSelectBox.SelectedValue;
            SearchMode searchMode = (SearchMode)searchModeBoxes[selectedMenuIndex].SelectedValue;
            string query = QueryBuilder.BuildQuery(userType, searchMode, selectedMenuIndex, searchTextBoxes[selectedMenuIndex].Text);
            Trace.WriteLine($"Executing query: {query}");
            return dbConnector.ExecuteTable(query);
        }

        private DataTable? GetDataTableFromDataGrid(DataGrid dataGrid) {
            dataGrid.CommitEdit();
            if (dataGrid.ItemsSource is DataView dataView) {
                return dataView.Table;
            }
            else if (dataGrid.ItemsSource is DataTable dataTable) {
                return dataTable;
            }
            return null;
        }

        private string ToFormattedString(DataTable dataTable) {
            int columnPadding = 2;
            // Since .PadRight() is used, it is important to use a monospaced font for the formatting to work right!
            int[] maxLengths = new int[dataTable.Columns.Count];
            for (int i = 0; i < dataTable.Columns.Count; i++) {
                maxLengths[i] = dataTable.Columns[i].ColumnName.Length;
                foreach (DataRow row in dataTable.Rows) {
                    if (row[i] == null) continue;
                    maxLengths[i] = Math.Max(maxLengths[i], row[i].ToString()!.Length);
                }
            }
            string header = "";
            for (int i = 0; i < dataTable.Columns.Count; i++) {
                header += dataTable.Columns[i].ColumnName.PadRight(maxLengths[i] + columnPadding);
            }
            string divider = new('-', maxLengths.Sum() + (maxLengths.Length - 1) * columnPadding);
            var rows = new List<string>();
            foreach (DataRow row in dataTable.Rows) {
                string currentRow = "";
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    if (row[i] == null) continue;
                    currentRow += row[i].ToString()!.PadRight(maxLengths[i] + columnPadding);
                }
                rows.Add(currentRow);
            }
            return header + "\n" + divider + "\n" + string.Join("\n", rows);
        }

        public void OnExportButtonClick(object sender, EventArgs e) {
            DataTable? dataTable = GetDataTableFromDataGrid(resultGrids[MenuTabPanel.SelectedIndex]);
            if (dataTable == null) return;
            SaveDataTableToJsonFile(dataTable);
        }

        public void OnSaveButtonClick(object sender, EventArgs e) {
            DataTable? dataTable = GetDataTableFromDataGrid(resultGrids[MenuTabPanel.SelectedIndex]);
            if (dataTable == null) return;
            Trace.WriteLine(ToFormattedString(dataTable));
        }

        public void SaveDataTableToJsonFile(DataTable dataTable) {
            string jsonString = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            SaveFileDialog saveFileDialog = new() {
                Filter = "JSON Datei (*.json)|*.json|Alle Dateien (*.*)|*.*",
                Title = "Als Json speichern",
                FileName = $"db_export_{DateTime.Now.ToString("yyyy-MM-dd")}.json"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result == null) return;
            if ((bool)result) {
                if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName)) {
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                }
            }
        }

        private void UpdateDataGrid(int index) {
            DataTable? dataTable = GetDataTable(index);
            if (dataTable == null) return;
            resultGrids[index].ItemsSource = dataTable.DefaultView;
        }

        private void ChangeDataEditability(bool allowed) {
            SaveChangesButton.IsEnabled = allowed;
            foreach (var grid in resultGrids) {
                grid.IsReadOnly = !allowed;
            }
        }

        private void OnUserTypeChanged(object sender, EventArgs e) {
            ChangeDataEditability((UserType)UserSelectBox.SelectedValue != UserType.Employee);
            OnSearchParametersChanged(sender, e);
        }

        private void OnSearchParametersChanged(object sender, EventArgs e) {
            UpdateDataGrid(MenuTabPanel.SelectedIndex);
        }

    }
}