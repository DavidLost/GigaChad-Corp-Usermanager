using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GigaChad_Corp_Usermanager {
    
    enum UserType {
        [Description("Mitarbeiter")]
        Employee = 1,

        [Description("Manager")]
        Manager = 2,

        [Description("Administrator")]
        Admin = 3
    }
}
