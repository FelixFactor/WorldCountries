namespace CountriesAPP.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using CountriesAPP.Models.API_Models;
    using Models;
    using MyToolkit.Utilities;
    
    public class SQLService
    {
        #region Attributes
        private SQLiteCommand command;
        private SQLiteConnection sqlConnection;
        private List<string> SQLCmds;
        private Country query;
        private readonly ProgressReportModel Report;
        private IProgress<ProgressReportModel> ReportProgress;
        private int saved;
        #endregion

        /// <summary>
        /// generates the data directory,
        /// creates the tables to hold data from API service
        /// </summary>
        public SQLService()
        {
            #region Init Attributes
            SQLCmds = new List<string>();
            Report = new ProgressReportModel();
            query = new Country();
            #endregion

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

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Load data from country table in SQL DB
        /// </summary>
        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            string selectCmd = "SELECT name, alpha3Code, region, subregion FROM country";

            command = new SQLiteCommand(selectCmd, sqlConnection);

            sqlConnection.Open();

            SQLiteDataReader result = command.ExecuteReader();
            
            while (result.Read())
            {
                countries.Add(new Country
                {
                    Name = (string)result["name"],
                    Alpha3Code = (string)result["alpha3Code"],
                    Region = (string)result["region"],
                    Subregion = (string)result["subregion"]
                });
            }
            result.Close();

            sqlConnection.Close();

            GetRates();

            return countries;
        }

        /// <summary>
        /// Load data from rate table in SQL DB
        /// </summary>
        private void GetRates()
        {
            try
            {
                string sql = "select RateId, Code, TaxRate, Name from rate";
                
                command = new SQLiteCommand(sql, sqlConnection);

                sqlConnection.Open();

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    CurrencyConverter.Rates.Add(new Rate
                    {
                        RateId = (int)reader["RateID"],
                        Code = (string)reader["Code"],
                        Name = (string)reader["Name"],
                        TaxRate = (double)reader["TaxRate"]
                    });
                }
                reader.Close();

                sqlConnection.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
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
            try
            {
                sqlConnection.Open();

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
                    query.Demonym = result["demonym"].ToString();
                    query.Area = (double)result["area"];
                    query.Gini = (double)result["giniIndex"];
                    query.NativeName = (string)result["nativeName"];
                    query.NumericCode = result["numericCode"].ToString();
                    query.Cioc = result["cioc"].ToString();
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

                sqlConnection.Close();

                return query;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);

                sqlConnection.Close();

                return null;
            }
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

            result.Close();
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
                query.Translations.De = (string)result["de"].ToString();
                query.Translations.Es = (string)result["es"].ToString();
                query.Translations.Fr = (string)result["fr"].ToString();
                query.Translations.Ja = (string)result["ja"].ToString();
                query.Translations.It = (string)result["it"].ToString();
                query.Translations.Br = (string)result["br"].ToString();
                query.Translations.Pt = (string)result["pt"].ToString();
                query.Translations.Nl = (string)result["nl"].ToString();
                query.Translations.Hr = (string)result["hr"].ToString();
                query.Translations.Fa = (string)result["fa"].ToString();
            }

            result.Close();
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

            result.Close();
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
                    Code = (string)result["code"].ToString(),
                    Name = (string)result["name"].ToString(),
                    Symbol = (string)result["symbol"].ToString()
                });
            }

            result.Close();
        }


        /// <summary>
        /// Saves data to SQL DB
        /// </summary>
        public async Task SaveData(List<Country> Countries, IProgress<ProgressReportModel> progress, bool firstRun)
        {
            ReportProgress = progress;

            await DownloadFlags(Countries, firstRun);

            await SeparateCurrencies(Countries);

            await SeparateRegionalBloc(Countries);

            await SeparateLanguage(Countries);

            await SaveCountries(Countries);

            await SaveRates();
        }

        /// <summary>
        /// Saves data to the rate table in SQL
        /// </summary>
        /// <param name="rates"></param>
        /// <returns></returns>
        private async Task SaveRates()
        {
            string insertCmd = "INSERT INTO rate VALUES(@rateId, @code, @taxRate, @name)";

            saved = 0;
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                sqlConnection.Open();

                await Task.Run(() =>
                {
                    foreach (var rate in CurrencyConverter.Rates)
                    {
                        

                        #region Parameters and Values
                        command.Parameters.AddWithValue("@rateId", rate.RateId);
                        command.Parameters.AddWithValue("@code", rate.Code);
                        command.Parameters.AddWithValue("@taxRate", rate.TaxRate);
                        command.Parameters.AddWithValue("@name", rate.Name);
                        #endregion

                        command.ExecuteNonQuery();

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / CurrencyConverter.Rates.Count;
                        Report.ItemName = $"Updating Rate: {rate.Name}";

                        ReportProgress.Report(Report);
                        #endregion

                    }
                });
                
                sqlConnection.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Saves data to the country table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private async Task SaveCountries(List<Country> countries)
        {
            string latlng = string.Empty, borders = string.Empty, topLevelDomain = string.Empty, callingCodes = string.Empty, timezones = string.Empty, altSpellings = string.Empty;

            string insertCmd = "INSERT INTO country VALUES(@alpha3Code, @name, @alpha2Code, @capital, @region, @subRegion, @population, @demonym, @area, @gini, @nativeName, @numericCode, @flag, @cioc, @latlong, @borders, @topLevelDomain, @callingCodes, @timezones, @altSpellings)";

            saved = 0;
            try
            {
                sqlConnection.Open();

                await Task.Run(() =>
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

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / countries.Count;
                        Report.ItemName = $"Updating Country: {country.Name}";

                        ReportProgress.Report(Report);
                        #endregion

                        #region Empty Strings
                        latlng = string.Empty;
                        borders = string.Empty;
                        topLevelDomain = string.Empty;
                        callingCodes = string.Empty;
                        timezones = string.Empty;
                        altSpellings = string.Empty;
                        #endregion
                    }
                });

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);                
            }
        }

        /// <summary>
        /// Inserts values into mid table country_language
        /// </summary>
        /// <param name="country">alpha3Code, iso639_2</param>
        private async Task SaveCountry_language(Country country)
        {
            string insertCmd = "INSERT INTO country_language VALUES(@idCountry, @idLang)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Language lang in country.Languages)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idLang", lang.Iso639_2);
                #endregion

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Inserts values into mid table country_regionalBloc
        /// </summary>
        /// <param name="country">alpha3Code, acronym</param>
        private async Task SaveCountry_regionalBloc(Country country)
        {
            string insertCmd = "INSERT INTO country_regionalBloc VALUES(@idCountry, @idBloc)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Regionalbloc bloc in country.RegionalBlocs)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idBloc", bloc.Acronym);
                #endregion

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Inserts values in mid table country_currency
        /// </summary>
        /// <param name="country">alpha3Code, code</param>
        private async Task SaveCountry_currency(Country country)
        {
            string insertCmd = "INSERT INTO country_currency VALUES(@idCountry, @idCurrency)";

            command = new SQLiteCommand(insertCmd, sqlConnection);

            foreach (Currency currency in country.Currencies)
            {
                #region Parameters & Values
                command.Parameters.AddWithValue("@idCountry", country.Alpha3Code);
                command.Parameters.AddWithValue("@idCurrency", currency.Code);
                #endregion

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Separates languages from countries list &
        /// Uses LINQ to distinct UNIQUE languages from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private async Task SeparateLanguage(List<Country> countries)
        {
            List<Language> Langs = new List<Language>();

            await Task.Run(() =>
            {
                foreach (Country item in countries)
                {
                    foreach (Language lang in item.Languages)
                    {
                        Langs.Add(lang);
                    }
                }
                var distinct = Langs.DistinctBy(b => b.Iso639_2).ToList();

                Langs = distinct;
            });
            await SaveLanguage(Langs);
        }

        /// <summary>
        /// Separates regionalBlocs from countries list &
        /// Uses LINQ to distinct UNIQUE regionalBlocs from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private async Task SeparateRegionalBloc(List<Country> countries)
        {
            List<Regionalbloc> Blocs = new List<Regionalbloc>();

            await Task.Run(() =>
            {
                foreach (Country item in countries)
                {
                    foreach (Regionalbloc bloc in item.RegionalBlocs)
                    {
                        Blocs.Add(bloc);
                    }
                }
                var distinct = Blocs.DistinctBy(b => b.Acronym).ToList();

                Blocs = distinct;
            });
            await SaveRegionalBloc(Blocs);
        }

        /// <summary>
        /// Separates the curency objects from the countries list &
        /// Uses LINQ to distinct UNIQUE currencies from the list & 
        /// Saves the list to DB
        /// </summary>
        /// <param name="countries"></param>
        private async Task SeparateCurrencies(List<Country> countries)
        {
            List<Currency> Currencies = new List<Currency>();

            await Task.Run(() =>
            {
                foreach (Country item in countries)
                {
                    foreach (Currency currency in item.Currencies)
                    {
                        Currencies.Add(currency);
                    }
                }
                var distinc = Currencies.DistinctBy(c => c.Code).ToList();

                Currencies = distinc;

            });
            await SaveCurrency(Currencies);
        }

        /// <summary>
        /// Saves DISTINCT data from SeparateRegionalBloc function to the regionalBloc table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private async Task SaveRegionalBloc(List<Regionalbloc> regionalbloc)
        {
            string insertCmd = "INSERT INTO regionalBloc VALUES(@acronym, @name, @otherAcronym, @otherNames)";

            string otherAcronym = string.Empty, otherNames = string.Empty;

            saved = 0;
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                sqlConnection.Open();

                await Task.Run(() =>
                {
                    foreach (Regionalbloc bloc in regionalbloc)
                    {
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

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / regionalbloc.Count;
                        Report.ItemName = $"Updating Economic Group: {bloc.Name}";

                        ReportProgress.Report(Report);
                        #endregion

                        otherAcronym = string.Empty;
                        otherNames = string.Empty;
                    }
                });

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            //regional bloc
            
        }

        /// <summary>
        /// Saves data to the translations table in SQL
        /// </summary>
        /// <param name="countries"></param>
        private async Task SaveTranslations(Translations language, string a3c)
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

                await command.ExecuteNonQueryAsync();                
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
        private async Task SaveLanguage(List<Language> languages)
        {
            string insertCmd = "INSERT INTO language VALUES(@iso639_2, @iso639_1, @name, @nativeLanguageName)";
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                sqlConnection.Open();

                saved = 0;
                await Task.Run(() =>
                {
                    foreach (Language language in languages)
                    {
                        #region Parameters & Values
                        command.Parameters.AddWithValue("@iso639_2", language.Iso639_2);
                        command.Parameters.AddWithValue("@iso639_1", language.Iso639_1);
                        command.Parameters.AddWithValue("@name", language.Name);
                        command.Parameters.AddWithValue("@nativeLanguageName", language.NativeName);
                        #endregion

                        command.ExecuteNonQuery();

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / languages.Count;
                        Report.ItemName = $"Updating Language: {language.Name}";

                        ReportProgress.Report(Report);
                        #endregion
                    }
                });

                sqlConnection.Close();
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
        private async Task SaveCurrency(List<Currency> currencies)
        {
            string insertCmd = "INSERT INTO currency VALUES(@code, @name, @symbol)";
            try
            {
                command = new SQLiteCommand(insertCmd, sqlConnection);

                sqlConnection.Open();

                saved = 0;
                await Task.Run(() =>
                {
                    foreach (Currency currency in currencies)
                    {
                        #region Parameters & Values
                        command.Parameters.AddWithValue("@code", currency.Code);
                        command.Parameters.AddWithValue("@name", currency.Name);
                        command.Parameters.AddWithValue("@symbol", currency.Symbol);
                        #endregion

                        command.ExecuteNonQuery();

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / currencies.Count;
                        Report.ItemName = $"Updating Currency: {currency.Name}";

                        ReportProgress.Report(Report);
                        #endregion
                    }
                });

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// async downloads flags in SVG format
        /// </summary>
        /// <param name="Countries"></param>
        /// <returns></returns>
        private async Task DownloadFlags(List<Country> countries, bool firstRun)
        {
            WebClient client = new WebClient();

            if (!Directory.Exists("Data/LocalFlags"))
            {
                Directory.CreateDirectory("Data/LocalFlags");
            }

            if (firstRun)
            {
                saved = 0;
                await Task.Run(() =>
                {
                    foreach (Country country in countries)
                    {
                        string path = $"Data/LocalFlags/{country.Alpha3Code}.svg";

                        client.DownloadFile(country.Flag, path);

                        #region Progress Bar
                        saved++;

                        Report.CompletedPercent = (saved * 100) / countries.Count;
                        Report.ItemName = $"Saving {country.Alpha3Code}.svg to disk";

                        ReportProgress.Report(Report);
                        #endregion
                    }
                });
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
                sqlConnection.Open();

                using (command = new SQLiteCommand(sqlConnection))
                {
                    foreach (string cmd in SQLCmds)
                    {
                        command.CommandText = cmd;

                        command.ExecuteNonQuery();
                    }
                }

                sqlConnection.Close();
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
            SQLCmds.Add("CREATE table IF NOT EXISTS rate ('RateId' INT, 'Code' VARCHAR(5), 'TaxRate' REAL, 'Name' VARCHAR(250))");
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
            SQLCmds.Add("DELETE FROM rate");
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
