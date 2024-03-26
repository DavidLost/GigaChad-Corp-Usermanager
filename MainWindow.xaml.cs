using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;

namespace GigaChad_Corp_Usermanager {
    public partial class MainWindow : Window {

        private DBConnector dbConnector;
        private readonly string databaseName = "gigachadcorp";
        private readonly string[] tables = { "employee", "department", "project" };
        private 
        private readonly Dictionary<string, List<string>> tablesColumns = new();
        private readonly Dictionary<UserType, List<TableColumnPermission>> userPermissions = new();

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            PopulateUserSelectBoxItems();
            PopulateSearchBoxItems();
            dbConnector = new DBConnector("admin", "admin", databaseName);
            Trace.WriteLine($"Tried to open connection. Success was: {dbConnector.IsConnectionAlive()}");
            GetTablesColumns();
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

        private void CreateUserPermissions() {
            userPermissions.Add(UserType.Employee, new List<TableColumnPermission> {
                new TableColumnPermission("employee") {
                    ViewableColumns = new List<string> { "emp_num", "name", "forename", "mail", "department_name" },
                    EditableColumns = new List<string> { }
                },
                new TableColumnPermission("department") {
                    ViewableColumns = new List<string> { "name", "room_num" },
                    EditableColumns = new List<string> { }
                },
                new TableColumnPermission("project") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                },
                new TableColumnPermission("employee_project") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                }
            });
            userPermissions.Add(UserType.Manager, new List<TableColumnPermission> {
                new TableColumnPermission("employee") {
                    ViewableColumns = new List<string> { "emp_num", "name", "forename", "mail", "department_name" },
                    EditableColumns = new List<string> { }
                },
                new TableColumnPermission("department") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                },
                new TableColumnPermission("project") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                }
            });
            userPermissions.Add(UserType.Admin, new List<TableColumnPermission> {
                new TableColumnPermission("employee") {
                    ViewableColumns = new List<string> { "emp_num", "name", "forename", "mail", "department_name" },
                    EditableColumns = new List<string> { }
                },
                new TableColumnPermission("department") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                },
                new TableColumnPermission("project") {
                    ViewableColumns = new List<string> { "Column1", "Column2" },
                    EditableColumns = new List<string> { "Column1" }
                }
            });
        }

        private void PopulateUserSelectBoxItems() {
            ComboBox[] userSelectBoxes = { UserSelectBox };
            PopulateComboBoxItems(userSelectBoxes, typeof(UserType), UserType.Employee);
        }

        private void PopulateSearchBoxItems() {
            ComboBox[] searchModeBoxes = { EmployeeSearchModeBox, DepartmentSearchModeBox, ProjectSearchModeBox };
            PopulateComboBoxItems(searchModeBoxes, typeof(SearchMode), SearchMode.ExactMatch);
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

        private DataTable? GetDataTable(string searchString, ComboBox searchModeBox) {
            SearchMode searchMode = (SearchMode)searchModeBox.SelectedValue;
            switch (searchMode) {
                case SearchMode.ExactMatch:
                    break;
                case SearchMode.StartsWith:
                    searchString += '%';
                    break;
                case SearchMode.Contains:
                    searchString = '%' + searchString + '%';
                    break;
            }
            int selectedMenuIndex = MenuTabPanel.SelectedIndex;
            string tableName = tables[selectedMenuIndex];
            string query = $"SELECT * FROM {tableName} WHERE ";
            string[] ignoredColumns = { }; //"id" Add columns that should be excluded from the search
            for (int i = 0; i < tablesColumns[tableName].Count; i++) {
                string column = tablesColumns[tableName][i];
                if (ignoredColumns.Contains(column)) continue;
                query += $"{column} LIKE '{searchString}'";
                if (i < tablesColumns[tableName].Count - 1) {
                    query += " OR ";
                }
            }
            Trace.WriteLine($"Executing query: {query}");
            return dbConnector.ExecuteTable(query);
        }

        private DataTable? GetEmployeeData() {
            
            return null;
        }

        private void OnSearchParametersChanged(object sender, EventArgs e) {
            if (sender is TextBox searchTextBox) {

            }
            else if (sender is ComboBox searchModeBox) {

            }
            
        }

        private void SearchBox_TextChanged(object sender, EventArgs e) {
            TextBox searchBox = (TextBox)sender;
            string searchString = searchBox.Text.Replace("%", "");
            switch (searchBox.Name) {
                case "EmployeeSearchBox":
                    EmployeeResultDataGrid.ItemsSource = GetDataTable(searchString, EmployeeSearchModeBox)!.DefaultView;
                    break;
                case "DepartmentSearchBox":
                    DepartmentResultDataGrid.ItemsSource = GetDataTable(searchString, DepartmentSearchModeBox)!.DefaultView;
                    break;
                case "ProjectSearchBox":
                    ProjectResultDataGrid.ItemsSource = GetDataTable(searchString, ProjectSearchModeBox)!.DefaultView;
                    break;
            }
            Trace.WriteLine(EmployeeSearchModeBox.SelectedValue);
            Trace.WriteLine($"Event args: {e}");
            Trace.WriteLine($"Text changed: {searchBox.Name} - {searchBox.Text}");
        }

        private void GetTablesColumns() {
            foreach (string table in tables) {
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}';";
                DataTable? dataTable = dbConnector.ExecuteTable(query); // Assuming dbConnector is already initialized
                if (dataTable == null) return;
                List<string> columns = new List<string>();
                foreach (DataRow row in dataTable.Rows) {
                    columns.Add(row["COLUMN_NAME"].ToString());
                }
                tablesColumns.Add(table, columns);
            }
        }
    }
}