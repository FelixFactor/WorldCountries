namespace CountriesAPP.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using CountriesAPP.Models.API_Models;
    using CountriesAPP.Services;
    
    public class ShowCountryViewModel
    {
        #region Attributes
        public ObservableCollection<Country> Countries { get; }
        public ObservableCollection<Rate> Rates { get; } = new ObservableCollection<Rate>();        
        private Country modCountry;
        #endregion

        public ShowCountryViewModel(Country country)
        {
            modCountry = country;
            CheckNulls();

            Countries = new ObservableCollection<Country>
            {
                modCountry
            };

            foreach (Rate rate in CurrencyConverter.Rates)
            {
                Rates.Add(rate);
            }
        }

        private void CheckNulls()
        {
            try
            {
                if (modCountry.Region == string.Empty)
                {
                    modCountry.Region = "None";
                }

                if (modCountry.Subregion == string.Empty)
                {
                    modCountry.Subregion = "None";
                }

                if (modCountry.Capital == string.Empty)
                {
                    modCountry.Capital = "None";
                }

                if (modCountry.AltSpellings.Count == 0)
                {
                    modCountry.AltSpellings.Add("None");
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
