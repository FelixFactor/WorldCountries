namespace CountriesAPP.API_Models
{
    using SQLite;
    using SQLiteNetExtensions.Attributes;

    [Table("language")]
    public class Language
    {
        public string Iso639_1 { get; set; }
        [PrimaryKey]
        public string Iso639_2 { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        [ForeignKey(typeof(Country))]
        public string IdCountry { get; set; }
    }
}
