namespace CountriesAPP.Services
{
    using API_Models;
    using MyToolkit.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;

    public class SQLService
    {
        private SQLiteCommand command;
        private SQLiteConnection sqlConnection;
        private List<string> SQLCmds = new List<string>();
        private Country query = new Country();
        
        /// <summary>
        /// constructor that generates the data directory,
        /// creates the tables to hold data from API service
        /// </summary>
        public SQLService()
        {
            AddCreateCommands();

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = @"Data\Countries.sqlite";

            try
            {
                sqlConnection = new SQLiteConnection("Data Source=" + path);
                sqlConnection.Open();

                using (command = new SQLiteCommand(sqlConnection))
                {
                    foreach (string cmd in SQLCmds)
                    {
                        command.CommandText = cmd;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Load data from SQL DB
        /// </summary>
        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            string selectCmd = "SELECT name, alpha3Code FROM country";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            SQLiteDataReader result = command.ExecuteReader();

            while (result.Read())
            {
                countries.Add(new Country
                {
                    Name = (string)result["name"],
                    Alpha3Code = (string)result["alpha3Code"]
                });
            }

            return countries;
        }

        /// <summary>
        /// Saves data to SQL DB
        /// </summary>
        public async void SaveData(List<Country> Countries)
        {
            await DownloadFlags(Countries);

            SeparateCurrencies(Countries);
            SeparateRegionalBloc(Countries);
            SeparateLanguage(Countries);
            SaveCountries(Countries);
        }

        /// <summary>
        /// Saves data to the country table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private void SaveCountries(List<Country> countries)
        {
            string latlng = string.Empty, borders = string.Empty, topLevelDomain = string.Empty, callingCodes = string.Empty, timezones = string.Empty, altSpellings = string.Empty;

            string insertCmd = "INSERT INTO country VALUES(@alpha3Code, @name, @alpha2Code, @capital, @region, @subRegion, @population, @demonym, @area, @gini, @nativeName, @numericCode, @flag, @cioc, @latlong, @borders, @topLevelDomain, @callingCodes, @timezones, @altSpellings)";

            try
            {
                foreach (Country country in countries)
                {
                    SaveTranslations(country.Translations, country.Alpha3Code);
                    SaveCountry_currency(country);
                    SaveCountry_regionalBloc(country);
                    SaveCountry_language(country);

                    command = new SQLiteCommand(insertCmd, sqlConnection);

                    #region Concatenate Strings
                    //list of borders
                    foreach (string border in country.Borders)
                    {
                        borders += $"{border},";
                    }
                    //list of latitude - longitude
                    foreach (double coord in country.Latlng)
                    {
                        latlng += $"{coord},";
                    }
                    //list of Internet Domain
                    foreach (string domain in country.TopLevelDomain)
                    {
                        topLevelDomain += $"{domain},";
                    }
                    //list of telephone international codes
                    foreach (string phone in country.CallingCodes)
                    {
                        callingCodes += $"{phone},";
                    }
                    //list of alternative country name spellings
                    foreach (string alt in country.AltSpellings)
                    {
                        altSpellings += $"{alt},";
                    }
                    //list of timezones
                    foreach (string time in country.Timezones)
                    {
                        timezones += $"{time},";
                    }
                    #endregion
                    #region Parameters and Values
                    command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                    command.Parameters.AddWithValue("@name", country.Name);
                    command.Parameters.AddWithValue("@alpha2Code", country.Alpha2Code);
                    command.Parameters.AddWithValue("@capital", country.Capital);
                    command.Parameters.AddWithValue("@region", country.Region);
                    command.Parameters.AddWithValue("@subRegion", country.Subregion);
                    command.Parameters.AddWithValue("@population", country.Population);
                    command.Parameters.AddWithValue("@demonym", country.Demonym);
                    command.Parameters.AddWithValue("@area", country.Area);
                    command.Parameters.AddWithValue("@gini", country.Gini);
                    command.Parameters.AddWithValue("@nativeName", country.NativeName);
                    command.Parameters.AddWithValue("@numericCode", country.NumericCode);
                    command.Parameters.AddWithValue("@flag", country.Flag);
                    command.Parameters.AddWithValue("@cioc", country.Cioc);
                    command.Parameters.AddWithValue("@latlong", latlng);
                    command.Parameters.AddWithValue("@borders", borders);
                    command.Parameters.AddWithValue("@topLevelDomain", topLevelDomain);
                    command.Parameters.AddWithValue("@callingCodes", callingCodes);
                    command.Parameters.AddWithValue("@timezones", timezones);
                    command.Parameters.AddWithValue("@altSpellings", altSpellings);
                    #endregion

                    command.ExecuteNonQuery();

                    #region Empty Strings
                    latlng = string.Empty;
                    borders = string.Empty;
                    topLevelDomain = string.Empty;
                    callingCodes = string.Empty;
                    timezones = string.Empty;
                    altSpellings = string.Empty;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Queries the DB for the selected country
        /// </summary>
        /// <param name="alpha3Code"></param>
        /// <returns></returns>
        public Country QueryCountry(string alpha3Code)
        {
            string selectCmd = "SELECT alpha3Code, name, alpha2Code, capital, region, subRegion, population, demonym, area, giniIndex, nativeName, numericCode, cioc, latlong, borders, topLevelDomain, callingCodes, timezones, altSpellings " +
                               "FROM country " +
                               "WHERE alpha3code like @alpha3Code";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            command.Parameters.AddWithValue("@alpha3Code", alpha3Code);

            SQLiteDataReader result = command.ExecuteReader();

            //Reader in action
            while (result.Read())
            {
                query.Alpha3Code = (string)result["alpha3Code"];
                query.Name = (string)result["name"];
                query.Alpha2Code = (string)result["alpha2Code"];
                query.Capital = (string)result["capital"];
                query.Region = (string)result["region"];
                query.Subregion = (string)result["subRegion"];
                query.Population = (int)result["population"];
                query.Demonym = (string)result["demonym"];
                query.Area = (double)result["area"];
                query.Gini = (double)result["giniIndex"];
                query.NativeName = (string)result["nativeName"];
                query.NumericCode = (string)result["numericCode"];
                query.Cioc = (string)result["cioc"];
                query.Latlng = DoubleConcatToList((string)result["latlong"]);
                query.Borders = StringConcatToList((string)result["borders"]);
                query.TopLevelDomain = StringConcatToList((string)result["topLevelDomain"]);
                query.CallingCodes = StringConcatToList((string)result["callingCodes"]);
                query.Timezones = StringConcatToList((string)result["timezones"]);
                query.AltSpellings = StringConcatToList((string)result["altSpellings"]);
            }
            result.Close();

            QueryCurrency();
            QueryLanguage();
            QueryTranslations();
            QueryRegionalBloc();

            return query;
        }

        /// <summary>
        /// Function to query DB
        /// </summary>
        private void QueryRegionalBloc()
        {
            string selectCmd = "SELECT acronym, name, otherAcronyms, otherNames FROM regionalBloc JOIN country_regionalBloc ON idBloc = acronym WHERE idCountry LIKE @alpha3Code";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            command.Parameters.AddWithValue("@alpha3Code", query.Alpha3Code);

            SQLiteDataReader result = command.ExecuteReader();

            query.RegionalBlocs = new List<Regionalbloc>();
            while (result.Read())
            {
                query.RegionalBlocs.Add(new Regionalbloc
                {
                    Acronym = (string)result["acronym"],
                    Name = (string)result["name"],
                    OtherAcronyms = StringConcatToArray((string)result["otherAcronyms"]),
                    OtherNames = StringConcatToArray((string)result["otherNames"])
                });
            }
        }

        /// <summary>
        /// Function to query DB
        /// </summary>
        private void QueryTranslations()
        {
            string selectCmd = "select de, es, fr, ja ,it, br, pt, nl, hr, fa FROM translations WHERE idCountry LIKE @alpha3Code";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            command.Parameters.AddWithValue("@alpha3Code", query.Alpha3Code);

            SQLiteDataReader result = command.ExecuteReader();

            query.Translations = new Translations();
            while (result.Read())
            {
                query.Translations.De = (string)result["de"];
                query.Translations.Es = (string)result["es"];
                query.Translations.Fr = (string)result["fr"];
                query.Translations.Ja = (string)result["ja"];
                query.Translations.It = (string)result["it"];
                query.Translations.Br = (string)result["br"];
                query.Translations.Pt = (string)result["pt"];
                query.Translations.Nl = (string)result["nl"];
                query.Translations.Hr = (string)result["hr"];
                query.Translations.Fa = (string)result["fa"];
            }
        }

        /// <summary>
        /// Function to query DB
        /// </summary>
        private void QueryLanguage()
        {
            string selectCmd = "SELECT iso639_2, iso639_1, name, nativeName FROM language JOIN country_language ON idLang = language.iso639_2 WHERE idCountry LIKE @alpha3Code";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            command.Parameters.AddWithValue("@alpha3Code", query.Alpha3Code);

            SQLiteDataReader result = command.ExecuteReader();

            query.Languages = new List<Language>();
            while (result.Read())
            {
                query.Languages.Add(new Language
                {
                    Iso639_1 = (string)result["iso639_1"],
                    Iso639_2 = (string)result["iso639_2"],
                    Name = (string)result["name"],
                    NativeName = (string)result["nativeName"],
                });
            }
        }

        /// <summary>
        /// Function to query DB
        /// </summary>
        private void QueryCurrency()
        {
            string selectCmd = "select code, name, symbol from currency join country_currency ON country_currency.idCurrency = currency.code where idCountry like @alpha3Code";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            command.Parameters.AddWithValue("@alpha3Code", query.Alpha3Code);

            SQLiteDataReader result = command.ExecuteReader();

            query.Currencies = new List<Currency>();
            while (result.Read())
            {
                query.Currencies.Add(new Currency
                {
                    Code = (string)result["code"],
                    Name = (string)result["name"],
                    Symbol = (string)result["symbol"]
                });
            }
        }

        /// <summary>
        /// Inserts values into mid table country_language
        /// </summary>
        /// <param name="country">alpha3Code, iso639_2</param>
        private void SaveCountry_language(Country country)
        {
            string insertCmd = "INSERT INTO country_language VALUES(@idCountry, @idLang)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Language lang in country.Languages)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idLang", lang.Iso639_2);
                #endregion

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserts values into mid table country_regionalBloc
        /// </summary>
        /// <param name="country">alpha3Code, acronym</param>
        private void SaveCountry_regionalBloc(Country country)
        {
            string insertCmd = "INSERT INTO country_regionalBloc VALUES(@idCountry, @idBloc)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Regionalbloc bloc in country.RegionalBlocs)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idBloc", bloc.Acronym);
                #endregion

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserts values in mid table country_currency
        /// </summary>
        /// <param name="country">alpha3Code, code</param>
        private void SaveCountry_currency(Country country)
        {
            string insertCmd = "INSERT INTO country_currency VALUES(@idCountry, @idCurrency)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Currency currency in country.Currencies)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idCurrency", currency.Code);
                #endregion

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Separates languages from countries list &
        /// Uses LINQ to distinct UNIQUE languages from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private void SeparateLanguage(List<Country> countries)
        {
            List<Language> Langs = new List<Language>();

            foreach (Country item in countries)
            {
                foreach (Language lang in item.Languages)
                {
                    Langs.Add(lang);
                }
            }

            var distinct = Langs.DistinctBy(b => b.Iso639_2).ToList();

            Langs = distinct;

            SaveLanguage(Langs);
        }

        /// <summary>
        /// Separates regionalBlocs from countries list &
        /// Uses LINQ to distinct UNIQUE regionalBlocs from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private void SeparateRegionalBloc(List<Country> countries)
        {
            List<Regionalbloc> Blocs = new List<Regionalbloc>();

            foreach (Country item in countries)
            {
                foreach (Regionalbloc bloc in item.RegionalBlocs)
                {
                    Blocs.Add(bloc);
                }
            }

            var distinct = Blocs.DistinctBy(b => b.Acronym).ToList();

            Blocs = distinct;

            SaveRegionalBloc(Blocs);
        }

        /// <summary>
        /// Separates the curency objects from the countries list &
        /// Uses LINQ to distinct UNIQUE currencies from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private void SeparateCurrencies(List<Country> countries)
        {
            List<Currency> Currencies = new List<Currency>();

            foreach (Country item in countries)
            {
                foreach (Currency currency in item.Currencies)
                {
                    Currencies.Add(currency);
                }
            }

            var distinc = Currencies.DistinctBy(c => c.Code).ToList();

            Currencies = distinc;

            SaveCurrency(Currencies);
        }

        /// <summary>
        /// Saves DISTINCT data from SeparateRegionalBloc function to the regionalBloc table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private void SaveRegionalBloc(List<Regionalbloc> regionalbloc)
        {
            string insertCmd = "INSERT INTO regionalBloc VALUES(@acronym, @name, @otherAcronym, @otherNames)";

            string otherAcronym = string.Empty, otherNames = string.Empty;

            //regional bloc
            foreach (Regionalbloc bloc in regionalbloc)
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                #region Concatenate Strings
                //list of otherAcronyms
                foreach (string other in bloc.OtherAcronyms)
                {
                    otherAcronym += $"{other},";
                }
                //List of otherNames
                foreach (string other in bloc.OtherNames)
                {
                    otherNames += $"{other},";
                }
                #endregion
                #region Parameters & Values
                command.Parameters.AddWithValue("@acronym", bloc.Acronym);
                command.Parameters.AddWithValue("@name", bloc.Name);
                command.Parameters.AddWithValue("@otherAcronym", otherAcronym);
                command.Parameters.AddWithValue("@otherNames", otherNames);
                #endregion

                command.ExecuteNonQuery();

                otherAcronym = string.Empty;
                otherNames = string.Empty;
            }
        }

        /// <summary>
        /// Saves data to the translations table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private void SaveTranslations(Translations language, string a3c)
        {
            string insertCmd = "INSERT INTO translations VALUES(@alpha3Code, @de, @es, @fr, @ja, @it, @br, @pt, @nl, @hr, @fa)";
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                #region Parameters & Values
                command.Parameters.AddWithValue("@de", language.De);
                command.Parameters.AddWithValue("@es", language.Es);
                command.Parameters.AddWithValue("@fr", language.Fr);
                command.Parameters.AddWithValue("@ja", language.Ja);
                command.Parameters.AddWithValue("@it", language.It);
                command.Parameters.AddWithValue("@br", language.Br);
                command.Parameters.AddWithValue("@pt", language.Pt);
                command.Parameters.AddWithValue("@nl", language.Nl);
                command.Parameters.AddWithValue("@hr", language.Hr);
                command.Parameters.AddWithValue("@fa", language.Fa);
                command.Parameters.AddWithValue("@alpha3Code", a3c);
                #endregion

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves DISTINCT data from SeparateLanguage function to the language table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private void SaveLanguage(List<Language> languages)
        {
            string insertCmd = "INSERT INTO language VALUES(@iso639_2, @iso639_1, @name, @nativeLanguageName)";
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);
                foreach (Language language in languages)
                {
                    #region Parameters & Values
                    command.Parameters.AddWithValue("@iso639_2", language.Iso639_2);
                    command.Parameters.AddWithValue("@iso639_1", language.Iso639_1);
                    command.Parameters.AddWithValue("@name", language.Name);
                    command.Parameters.AddWithValue("@nativeLanguageName", language.NativeName);
                    #endregion

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves DISTINCT data from SeparateCurrencies function to the currency table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private void SaveCurrency(List<Currency> currencies)
        {
            string insertCmd = "INSERT INTO currency VALUES(@code, @name, @symbol)";
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);
                foreach (Currency currency in currencies)
                {
                    #region Parameters & Values
                    command.Parameters.AddWithValue("@code", currency.Code);
                    command.Parameters.AddWithValue("@name", currency.Name);
                    command.Parameters.AddWithValue("@symbol", currency.Symbol);
                    #endregion

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// flags async download format SVG
        /// </summary>
        /// <param name="Countries"></param>
        /// <returns></returns>
        private async Task DownloadFlags(List<Country> Countries)
        {
            //TODO check if files exist to not download them all again
            WebClient client = new WebClient();

            if (!Directory.Exists("LocalFlags"))
            {
                Directory.CreateDirectory("LocalFlags");
            }
            
            foreach (Country country in Countries)
            {
                string path = $"LocalFlags/{country.Alpha3Code}.svg";

                await client.DownloadFileTaskAsync(country.Flag, path);
            }
        }

        /// <summary>
        /// deletes the data from SQLite DB
        /// </summary>
        public void DeleteData()
        {
            AddDeleteCommands();

            try
            {
                using (command = new SQLiteCommand(sqlConnection))
                {
                    foreach (string cmd in SQLCmds)
                    {
                        command.CommandText = cmd;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Creates the command list to create the SQL tables
        /// </summary>
        /// <returns>The list of SQL commands</returns>
        private void AddCreateCommands()
        {
            SQLCmds.Clear();

            SQLCmds.Add("CREATE TABLE IF NOT EXISTS country" +
                           "(alpha3Code CHAR(3) PRIMARY KEY,name VARCHAR NULL,alpha2Code CHAR(2) NULL,capital VARCHAR NULL," +
                           "region VARCHAR NULL,subregion VARCHAR NULL,population INT NULL,demonym VARCHAR NULL,area FLOAT NULL,giniIndex FLOAT NULL," +
                           "nativeName VARCHAR NULL,numericCode VARCHAR NULL,flag VARCHAR NULL,cioc CHAR(3) NULL,latlong VARCHAR NULL,borders VARCHAR NULL," +
                           "topLevelDomain VARCHAR NULL,callingCodes VARCHAR NULL,timezones VARCHAR NULL, altSpellings VARCHAR NULL)");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS currency(code CHAR(3) PRIMARY KEY,name VARCHAR NULL,symbol CHAR(5) NULL)");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS language(iso639_2 CHAR(3) PRIMARY KEY,iso639_1 CHAR(2) NULL,name VARCHAR NULL,nativeName VARCHAR NULL)");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS translations(idCountry CHAR(3) PRIMARY KEY REFERENCES country(alpha3Code),de VARCHAR(100),es VARCHAR(100),fr VARCHAR(100),ja VARCHAR(100),it VARCHAR(100),br VARCHAR(100),pt VARCHAR(100),nl VARCHAR(100),hr VARCHAR(100),fa VARCHAR(100))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS regionalBloc(acronym VARCHAR(10) PRIMARY KEY,name VARCHAR(100) NULL,otherAcronyms VARCHAR NULL,otherNames VARCHAR NULL)");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS country_currency(idCountry CHAR(3) REFERENCES country(alpha3Code), idCurrency CHAR(3) REFERENCES currency(code))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS country_regionalBloc(idCountry CHAR(3) REFERENCES country(alpha3Code), idBloc VARCHAR(10) REFERENCES regionalBloc(acronym))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS country_language(idCountry CHAR(3) REFERENCES country(alpha3Code), idLang CHAR(3) REFERENCES language(iso639_2))");
        }

        /// <summary>
        /// Creates the delete command string list
        /// </summary>
        /// <returns>The list of SQL commands</returns>
        private void AddDeleteCommands()
        {
            SQLCmds.Clear();

            SQLCmds.Add("DELETE FROM country");
            SQLCmds.Add("DELETE FROM currency");
            SQLCmds.Add("DELETE FROM language");
            SQLCmds.Add("DELETE FROM translations");
            SQLCmds.Add("DELETE FROM regionalBloc");
            SQLCmds.Add("DELETE FROM country_currency");
            SQLCmds.Add("DELETE FROM country_regionalBloc");
            SQLCmds.Add("DELETE FROM country_language");
        }

        /// <summary>
        /// Split concat string to the latlong list
        /// </summary>
        /// <param name="latlong"></param>
        private List<double> DoubleConcatToList(string latlong)
        {
            List<double> stuff = new List<double>();

            string[] splitted = latlong.Split(',');
            foreach (string item in splitted)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    stuff.Add(Convert.ToDouble(item));
                }
            }

            return stuff;
        }

        /// <summary>
        /// Split string concat to string list
        /// </summary>
        /// <param name="stuff"></param>
        /// <param name="items"></param>
        /// <returns>A list of strings</returns>
        private List<string> StringConcatToList(string items)
        {
            List<string> stuff = new List<string>();
            string[] splitted = items.Split(',');
            foreach (string item in splitted)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    stuff.Add(item);
                }
            }
            return stuff;
        }

        /// <summary>
        /// Split concat to array of strings
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns>An array of strings</returns>
        private string[] StringConcatToArray(string phrase)
        {
            string[] splitted = phrase.Trim().Split(',');

            var noEmpty = splitted.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            return noEmpty;
        }
    }
}
