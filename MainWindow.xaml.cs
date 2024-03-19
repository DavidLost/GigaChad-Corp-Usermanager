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
        private readonly string[] tables = ["employee", "project", "department"];

        private readonly Dictionary<string, List<string>> tablesColumns = [];

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            PopulateSearchBoxItems();
            dbConnector = new DBConnector("admin", "admin", databaseName);
            dbConnector.Open();
            Trace.WriteLine($"Tried to open connection. Success was: {dbConnector.IsConnectionAlive()}");
            GetTablesColumns();
            // PrintTableColumns();
        }

        public static string GetEnumDescription(Enum value) {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0) {
                return attributes[0].Description;
            }
            else {
                return value.ToString();
            }   
        }

        private void PopulateSearchBoxItems() {
            ComboBox[] searchModeBoxes = [EmployeeSearchModeBox, DepartmentSearchModeBox, ProjectSearchModeBox];

            // Get the list of enum values along with their descriptions
            var values = Enum.GetValues(typeof(SearchMode))
                .Cast<Enum>()
                .Select(value => new {
                    Description = GetEnumDescription(value),
                    Value = value
                }).ToList();

            // Configure each ComboBox with the enum values and descriptions
            foreach (var searchModeBox in searchModeBoxes) {
                searchModeBox.ItemsSource = values; // Set the items source to the list of values
                searchModeBox.DisplayMemberPath = "Description"; // Display the Description property in the ComboBox
                searchModeBox.SelectedValuePath = "Value"; // Use the Value property as the actual value of the items
                searchModeBox.SelectedValue = SearchMode.ExactMatch; // Set the default selected value
            }
        }

        private void GetTableData(string query) {
            DataTable? dataTable = dbConnector.ExecuteTable(query);
            if (dataTable == null) {
                Trace.WriteLine("Got null, returning");
                return;
            }
            EmployeeResultDataGrid.ItemsSource = dataTable.DefaultView;
        }

        private DataTable? GetEmployeeData(string searchString) {
            SearchMode searchMode = (SearchMode)EmployeeSearchModeBox.SelectedValue;
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
            string query = "SELECT * FROM employee WHERE ";
            for (int i = 0; i < tablesColumns["employee"].Count; i++) {
                query += $"{tablesColumns["employee"][i]} LIKE '{searchString}'";
                if (i < tablesColumns["employee"].Count - 1) {
                    query += " OR ";
                }
            }
            //DataTable? dataTable = dbConnector.ExecuteTable(query);
            return null;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e) {
            TextBox searchBox = (TextBox)sender;
            string searchString = searchBox.Text.Replace("%", "");
            //switch (searchBox.Name) {
            //    case "EmployeeSearchBox": EmployeeResultDataGrid.ItemsSource = GetEmployeeData(searchString)!.DefaultView;
            //        break;
            //    case "DepartmentSearchBox": 
            //}
            Trace.WriteLine(EmployeeSearchModeBox.SelectedValue);
            Trace.WriteLine($"Event args: {e}");
            Trace.WriteLine($"Text changed: {searchBox.Name} - {searchBox.Text}");
            var searchString = EmployeeSearchBox.Text.Replace('*', '%');
            string query = "SELECT * FROM employee WHERE ";
            for (int i = 0; i < tablesColumns["employee"].Count; i++) {
                query += $"{tablesColumns["employee"][i]} LIKE '{searchString}'";
                if (i < tablesColumns["employee"].Count - 1) {
                    query += " OR ";
                }
            }
            Trace.WriteLine(query);
            GetTableData(query);
        }
        //private void PrintTableColumns() {
        //    foreach (var table in tablesColumns) {
        //        Trace.WriteLine($"Table: {table.Key}");
        //        Trace.WriteLine("Columns:");
        //        foreach (var column in table.Value) {
        //            Trace.WriteLine($"- {column}");
        //        }
        //        Trace.WriteLine(""); // Adds an empty line for better readability
        //    }
        //}
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