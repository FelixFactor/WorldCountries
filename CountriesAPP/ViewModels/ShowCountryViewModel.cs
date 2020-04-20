namespace CountriesAPP.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using API_Models;

    public class ShowCountryViewModel
    {
        public ShowCountryViewModel(Country country)
        {
            //CheckNulls(country);

            Countries = new ObservableCollection<Country>
            {
                country
            };
        }

        private void CheckNulls(Country country)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<Country> Countries { get; }
    }
}
