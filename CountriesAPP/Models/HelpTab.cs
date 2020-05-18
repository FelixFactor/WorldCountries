namespace CountriesAPP.Models
{
    using Views;
    using System.Windows.Controls;

    public class HelpTab : Tab
    {
        public HelpPage Window { get; set; }

        public HelpTab(string name, UserControl helptab)
        {
            TabName = name;
            Window = (HelpPage)helptab;
        }
    }
}
