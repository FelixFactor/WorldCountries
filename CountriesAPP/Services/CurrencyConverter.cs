namespace CountriesAPP.Services
{
    using CountriesAPP.Models.API_Models;
    using System.Collections.Generic;

    public static class CurrencyConverter
    {
        public static List<Rate> Rates { get; set; }

        public static void AddEuro()
        {
            Rates.Add(new Rate { Code = "EUR", Name = "Euro", RateId = 148, TaxRate = 1 });
        }
    }
}
