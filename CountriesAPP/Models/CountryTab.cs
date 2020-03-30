namespace CountriesAPP.Models
{
    using System.Windows.Controls;
    using Views;

    public class CountryTab : Tab
    {
        public ShowCountryDetails Window { get; set; }

        public CountryTab(string tabName, UserControl newTab)
        {
            TabName = tabName;
            Window = (ShowCountryDetails)newTab;
        }
    }
}
