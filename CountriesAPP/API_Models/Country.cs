namespace CountriesAPP.API_Models
{
    using SQLite;
    using SQLiteNetExtensions.Attributes;
    using System.Collections.Generic;

    [Table("country")]
    public class Country
    {
        public string Name { get; set; }
        public List<string> TopLevelDomain { get; set; }
        public string Alpha2Code { get; set; }
        [PrimaryKey]
        public string Alpha3Code { get; set; }
        public List<string> CallingCodes { get; set; }
        public string Capital { get; set; }
        public List<string> AltSpellings { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public int Population { get; set; }
        public List<double> Latlng { get; set; }
        public string Demonym { get; set; }
        public double Area { get; set; }
        public double Gini { get; set; }
        [OneToMany]
        public List<string> Timezones { get; set; }
        public List<string> Borders { get; set; }
        public string NativeName { get; set; }
        public string NumericCode { get; set; }
        [OneToMany]
        public List<Currency> Currencies { get; set; }
        [OneToMany]
        public List<Language> Languages { get; set; }
        [OneToOne]
        public Translations Translations { get; set; }
        public string Flag { get; set; }
        [OneToMany]
        public List<Regionalbloc> RegionalBlocs { get; set; }
        public string Cioc { get; set; }
    }
}
