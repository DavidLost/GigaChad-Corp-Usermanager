using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigaChad_Corp_Usermanager {
    public class TableColumnPermission {
        public string TableName { get; private set; }
        public List<string> ViewableColumns { get; set; } = new List<string>();
        public List<string> EditableColumns { get; set; } = new List<string>();

        public TableColumnPermission(string tableName) {
            TableName = tableName;
        }
    }

}