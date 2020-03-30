
namespace CountriesAPP.API_Models
{
    using SQLite;
    using SQLiteNetExtensions.Attributes;

    [Table("regionalBloc")]
    public class Regionalbloc
    {
        [PrimaryKey]
        public string Acronym { get; set; }
        public string Name { get; set; }
        public string[] OtherAcronyms { get; set; }
        public string[] OtherNames { get; set; }
        [ForeignKey(typeof(Country))]
        public string IdCountry { get; set; }
    }
}
