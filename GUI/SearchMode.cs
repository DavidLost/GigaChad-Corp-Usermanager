using System.ComponentModel;

namespace GigaChad_Corp_Usermanager.GUI
{
    public enum SearchMode
    {
        [Description("Ist gleich")]
        ExactMatch,

        [Description("Beginnt mit")]
        StartsWith,

        [Description("Enthält")]
        Contains
    }
}