namespace CountriesAPP.Views
{
    using API_Models;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ShowCountryDetails.xaml
    /// </summary>
    public partial class ShowCountryDetails : UserControl
    {
        public ShowCountryDetails()
        {
            InitializeComponent();
        }

        public void CreateControl(Country country)
        {
            LblCapital.Text = country.Capital;
            LblRegion.Text = country.Region;
            LblSubRegion.Text = country.Subregion;
            LblPop.Text = country.Population.ToString();
            LblGini.Text = country.Gini.ToString();
        }
    }
}
