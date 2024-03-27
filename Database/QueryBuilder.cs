using GigaChad_Corp_Usermanager.GUI;
using GigaChad_Corp_Usermanager.MySQL;
using System.Diagnostics;

namespace GigaChad_Corp_Usermanager.Database {
    internal class QueryBuilder {

        private static readonly string[] tables = { "employee", "department", "project", "employee_project" };
        private static readonly string[] joins = {
            "INNER JOIN department ON employee.department_id=department.id",
            "",
            "",
            "INNER JOIN employee ON employee_project.emp_id=employee.id INNER JOIN project ON employee_project.project_id=project.id"
        };
        private static readonly PermissionsManager permissionManager = new();

        public static string BuildQuery(UserType userType, SearchMode searchMode, int tableIndex, string searchString) {
            Trace.WriteLine(userType);
            Trace.WriteLine(searchMode);
            Trace.WriteLine(tableIndex);
            Trace.WriteLine(searchString);
            string tableName = tables[tableIndex];
            string[] columns = permissionManager.GetViewableColumns(userType, tableName);
            string[] columnAliases = permissionManager.GetColumnDisplayNames(userType, tableName);
            string search = ApplySearchMode(SQLSanitizer.SanitizeInput(searchString), searchMode);
            string query = $"{GetColumnSelectString(columns, columnAliases)} FROM {tableName} {joins[tableIndex]} WHERE ";
            string[] ignoredColumns = { };
            for (int i = 0; i < columns.Length; i++) {
                if (ignoredColumns.Contains(columns[i])) continue;
                query += $"{columns[i]} LIKE '{search}'";
                if (i < columns.Length - 1) {
                    query += " OR ";
                }
            }
            return query;
        }

        private static string ApplySearchMode(string searchString, SearchMode searchMode) {
            return searchMode switch {
                SearchMode.ExactMatch => searchString,
                SearchMode.StartsWith => searchString + '%',
                SearchMode.Contains => '%' + searchString + '%',
                _ => searchString,
            };
        }

        private static string GetColumnSelectString(string[] columns, string[] aliases) {
            string columnSelection = "SELECT ";
            for (int i = 0; i < columns.Length; i++) {
                columnSelection += $"{columns[i]} AS {aliases[i]}";
                if (i < columns.Length - 1) columnSelection += ",";
                columnSelection += " ";
            }
            return columnSelection;
        }
    }
}
