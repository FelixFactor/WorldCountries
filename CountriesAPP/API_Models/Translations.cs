namespace CountriesAPP.API_Models
{
    using SQLite;
    using SQLiteNetExtensions.Attributes;

    [Table("translations")]
    public class Translations
    {
        [PrimaryKey, ForeignKey(typeof(Country))]
        public string IdCountry { get; set; }
        public string De { get; set; }
        public string Es { get; set; }
        public string Fr { get; set; }
        public string Ja { get; set; }
        public string It { get; set; }
        public string Br { get; set; }
        public string Pt { get; set; }
        public string Nl { get; set; }
        public string Hr { get; set; }
        public string Fa { get; set; }
    }
}
