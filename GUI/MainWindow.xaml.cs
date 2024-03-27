using System.Diagnostics;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;
using GigaChad_Corp_Usermanager.MySQL;
using GigaChad_Corp_Usermanager.Database;

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

        private void PopulateUserSelectBoxItems() {
            PopulateComboBoxItems(new ComboBox[] { UserSelectBox }, typeof(UserType), UserType.Employee);
        }

        private void PopulateSearchBoxItems() {
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

        private DataTable? GetDataTable(int selectedMenuIndex) {
            if (selectedMenuIndex < 0 || selectedMenuIndex >= resultGrids.Length) {  return null; }
            UserType userType = (UserType)UserSelectBox.SelectedValue;
            SearchMode searchMode = (SearchMode)searchModeBoxes[selectedMenuIndex].SelectedValue;
            string query = QueryBuilder.BuildQuery(userType, searchMode, selectedMenuIndex, searchTextBoxes[selectedMenuIndex].Text);
            Trace.WriteLine($"Executing query: {query}");
            return dbConnector.ExecuteTable(query);
        }

        private DataTable? GetEmployeeData() {
            return null;
        }

        private void OnSearchParametersChanged(object sender, EventArgs e) {
            int index = MenuTabPanel.SelectedIndex;
            DataTable? dataTable = GetDataTable(index);
            if (dataTable == null) return;
            resultGrids[index].ItemsSource = dataTable.DefaultView;
        }

    }
}