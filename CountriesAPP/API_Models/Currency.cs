namespace CountriesAPP.API_Models
{
    using SQLite;
    using SQLiteNetExtensions.Attributes;

    [Table("currency")]
    public class Currency
    {
        [PrimaryKey]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        [ForeignKey(typeof(Country))]
        public string IdCountry { get; set; }
    }
}
