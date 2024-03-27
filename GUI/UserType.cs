using System.ComponentModel;

namespace GigaChad_Corp_Usermanager.GUI
{

    enum UserType
    {
        [Description("Mitarbeiter")]
        Employee = 1,

        [Description("Manager")]
        Manager = 2,

        [Description("Administrator")]
        Admin = 3
    }
}
