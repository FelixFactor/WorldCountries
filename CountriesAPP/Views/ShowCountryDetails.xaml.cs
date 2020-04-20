namespace CountriesAPP.Views
{
    using System.Windows.Controls;
    using ViewModels;
    using API_Models;

    /// <summary>
    /// Interaction logic for ShowCountryDetails.xaml
    /// </summary>
    public partial class ShowCountryDetails : UserControl
    {
        private ShowCountryViewModel countryView;

        public ShowCountryDetails(Country country)
        {
            InitializeComponent();

            countryView = new ShowCountryViewModel(country);

            DataContext = countryView;
        }
    }
}
