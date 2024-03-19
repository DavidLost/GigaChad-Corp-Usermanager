using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows;
using System.Data;

namespace GigaChad_Corp_Usermanager {
    public partial class MainWindow : Window {

        private DBConnector dbConnector;
        private string databaseName = "gigachadcorp";
        private string[] tables = { "employee", "project", "department" };
        private Dictionary<string, List<string>> tablesColumns = [];

        public MainWindow() {
            InitializeComponent();
            dbConnector = new DBConnector("admin", "admin", databaseName);
            dbConnector.Open();
            Trace.WriteLine($"Tried to open connection. Success was: {dbConnector.IsConnectionAlive()}");
            GetTablesColumns();
            PrintTableColumns();
        }

        private void GetTableData(string query) {
            DataTable? dataTable = dbConnector.ExecuteTable(query);
            if (dataTable == null) {
                Trace.WriteLine("Got null, returning");
                return;
            }
            ResultDataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e) {
            var searchString = SearchBox.Text.Replace('*', '%');
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
        private void PrintTableColumns() {
            foreach (var table in tablesColumns) {
                Trace.WriteLine($"Table: {table.Key}");
                Trace.WriteLine("Columns:");
                foreach (var column in table.Value) {
                    Trace.WriteLine($"- {column}");
                }
                Trace.WriteLine(""); // Adds an empty line for better readability
            }
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