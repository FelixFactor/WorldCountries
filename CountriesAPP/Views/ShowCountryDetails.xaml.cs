namespace CountriesAPP.Views
{
    using System.Windows.Controls;
    using System.Windows;
    using ViewModels;
    using API_Models;
    using CountriesAPP.Services;
    using CountriesAPP.Models;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using CountriesAPP.Models.API_Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Interaction logic for ShowCountryDetails.xaml
    /// </summary>
    public partial class ShowCountryDetails : UserControl
    {
        public ShowCountryDetails(Country country)
        {
            InitializeComponent();

            DataContext = new ShowCountryViewModel(country);

            CbRates.ItemsSource = CurrencyConverter.Rates;
            CbRates.DisplayMemberPath = "Name";
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            SQLService fast = new SQLService();
            var search = fast.QueryCountry(e.Source.ToString()); ;

            ShowCountryDetails border = new ShowCountryDetails(search);

            CountryTab newTab = new CountryTab(search.Name, border);

            CountryMainViewModel quick = new CountryMainViewModel();
            quick.AddTab(newTab);
        }

        /// <summary>
        /// Validates the input for the Converter textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// cambio converter that updates on text input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TbMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(2000);

            Rate countryRate = new Rate();
            Currency currency = (Currency)CbRateCountry.SelectedItem;

            try
            {
                var result = CurrencyConverter.Rates.Where(x => x.Code == currency.Code);

                foreach (Rate item in result)
                {
                    countryRate = item;
                }

                var convertRate = (Rate)CbRates.SelectedItem;

                var converted = Convert.ToInt32(TbMoney.Text) / (decimal)countryRate.TaxRate * (decimal)convertRate.TaxRate;

                LblConversion.Text = string.Format("{0} {1:C2} = {2} {3:C2}",
                    countryRate.Code, TbMoney.Text, convertRate.Code, converted);
            }
            catch
            {
                LblConversion.Text = "The origin rate doesn't exist on our database.";
            }
        }
    }
}
