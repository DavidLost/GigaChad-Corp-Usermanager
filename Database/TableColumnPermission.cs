namespace GigaChad_Corp_Usermanager.Database
{
    public class TableColumnPermission {
        public string TableName { get; private set; }
        private readonly string[] viewableColumns;
        private readonly string[] editableColumns;
        private readonly string[] displayedNames;

        public TableColumnPermission(string tableName, string[] viewableColumns, string[] displayedNames, string[] editableColumns) {
            TableName = tableName;
            this.viewableColumns = viewableColumns;
            this.displayedNames = displayedNames;
            this.editableColumns = editableColumns;
        }
        public string[] GetViewableColumns() {
            return viewableColumns.Select(column => !column.Contains('.') ? $"{TableName}.{column}" : column).ToArray();
        }
        public string[] GetEditableColumns() {
            return editableColumns;
        }

        public string[] GetDisplayedNames() {
            return displayedNames;
        }
    }

}