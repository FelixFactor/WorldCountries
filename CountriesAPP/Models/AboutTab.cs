namespace CountriesAPP.Models
{
    using System.Windows.Controls;
    using CountriesAPP.Views;

    public class AboutTab : Tab
    {
        public About Window { get; set; }

        public AboutTab(string name, UserControl helptab)
        {
            TabName = name;
            Window = (About)helptab;
        }
    }
}
