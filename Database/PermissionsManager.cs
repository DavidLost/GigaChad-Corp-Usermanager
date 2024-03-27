using GigaChad_Corp_Usermanager.GUI;

namespace GigaChad_Corp_Usermanager.Database {
    internal class PermissionsManager {

        private readonly Dictionary<UserType, List<TableColumnPermission>> userPermissions = new();

        public PermissionsManager() {
            RegisterUserPermissions();
        }

        private void RegisterUserPermissions() {
            userPermissions.Add(UserType.Employee, new List<TableColumnPermission> {
                new TableColumnPermission("employee",
                    new[] { "emp_num", "name", "forename", "mail", "department.name" },
                    new[] { "Personalnummer", "Name", "Vorname", "Mail", "Abteilung" },
                    new string[] {}),
                new TableColumnPermission("department",
                    new[] { "name", "room_num" },
                    new[] { "Abteilungsame", "Raumnummer" },
                    new string[] {}),
                new TableColumnPermission("project",
                    new[] { "name" },
                    new[] { "Projektname" },
                    new string[] {}),
                new TableColumnPermission("employee_project",
                    new[] { "employee.emp_num", "employee.name", "employee.forename", "project.name", "task" },
                    new[] { "Personalnummer", "'Mitarbeiter Name'", "'Mitarbeiter Vorname'", "Projektname", "Aufgabe" },
                    new string[] {})
            });
            userPermissions.Add(UserType.Manager, new List<TableColumnPermission> {
                new TableColumnPermission("employee",
                    new[] { "emp_num", "name", "forename", "mail", "department.name" },
                    new[] { "Personalnummer", "Name", "Vorname", "Mail", "Abteilung" },
                    new string[] {}),
                new TableColumnPermission("department",
                    new[] { "name", "room_num" },
                    new[] { "Abteilungsame", "Raumnummer" },
                    new string[] {}),
                new TableColumnPermission("project",
                    new[] { "name" },
                    new[] { "Projektname" },
                    new string[] {}),
                new TableColumnPermission("employee_project",
                    new[] { "employee.emp_num", "employee.name", "employee.forename", "project.name", "task" },
                    new[] { "Personalnummer", "'Mitarbeiter Name'", "'Mitarbeiter Vorname'", "Projektname", "Aufgabe" },
                    new string[] {})
            });
            userPermissions.Add(UserType.Admin, new List<TableColumnPermission> {
                new TableColumnPermission("employee",
                    new[] { "id", "emp_num", "name", "forename", "mail", "department.name" },
                    new[] { "ID", "Personalnummer", "Name", "Vorname", "Mail", "Abteilung" },
                    new string[] {}),
                new TableColumnPermission("department",
                    new[] { "id", "name", "room_num" },
                    new[] { "ID", "Abteilungsame", "Raumnummer" },
                    new string[] {}),
                new TableColumnPermission("project",
                    new[] { "id", "name" },
                    new[] { "ID", "Projektname" },
                    new string[] {}),
                new TableColumnPermission("employee_project",
                    new[] { "employee.emp_num", "employee.name", "employee.forename", "project.name", "task" },
                    new[] { "Personalnummer", "'Mitarbeiter Name'", "'Mitarbeiter Vorname'", "Projektname", "Aufgabe" },
                    new string[] {})
            });
        }

        private string[] GetColumns(UserType userType, string tableName, Func<TableColumnPermission, string[]> columnSelector) {
            if (userPermissions.TryGetValue(userType, out var permissions)) {
                var tablePermission = permissions.FirstOrDefault(tp => tp.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                if (tablePermission != null) return columnSelector(tablePermission);
            }
            return Array.Empty<string>();
        }

        public string[] GetViewableColumns(UserType userType, string tableName) {
            return GetColumns(userType, tableName, tp => tp.GetViewableColumns());
        }

        public string[] GetEditableColumns(UserType userType, string tableName) {
            return GetColumns(userType, tableName, tp => tp.GetEditableColumns());
        }

        public string[] GetColumnDisplayNames(UserType userType, string tableName) {
            return GetColumns(userType, tableName, tp => tp.GetDisplayedNames());
        }
    }
}
