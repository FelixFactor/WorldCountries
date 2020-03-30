namespace CountriesAPP.Services
{
    using API_Models;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Windows;

    public class SQLService
    {
        private SQLiteCommand command;
        private SQLiteConnection sqlConnection;
        private List<string> SQLCmds = new List<string>();

        /// <summary>
        /// constructor that generates the data directory,
        /// creates the tables to hold data from API service
        /// </summary>
        public SQLService()
        {
            SQLCmds = AddCreateCommands();

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
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace + "\n" + ex.HelpLink);
            }
        }

        /// <summary>
        /// Load data from SQL DB
        /// </summary>
        public void GetData()
        {

        }

        /// <summary>
        /// Saves data to SQL DB
        /// </summary>
        public void SaveData(List<Country> Countries)
        {
            string latlng = string.Empty, borders = string.Empty, topLevelDomain = string.Empty, callingCodes = string.Empty, timezones = string.Empty, altSpellings = string.Empty;
            string inserts = "BEGIN\n";
            try
            {
                foreach (Country item in Countries)
                {
                    #region Foreach List
                    //list of borders
                    foreach (string border in item.Borders)
                    {
                        borders += $"{border},";
                    }
                    //list of latitude - longitude
                    foreach (double coord in item.Latlng)
                    {
                        latlng += $"{coord},";
                    }
                    //list of Internet Domain
                    foreach (string domain in item.TopLevelDomain)
                    {
                        topLevelDomain += $"{domain},";
                    }
                    //list of telephone international codes
                    foreach (string phone in item.CallingCodes)
                    {
                        callingCodes += $"{phone},";
                    }
                    //list of alternative country name spellings
                    foreach (string alt in item.AltSpellings)
                    {
                        altSpellings += $"'{alt}',";
                    }
                    //list of timezones
                    foreach (string time in item.Timezones)
                    {
                        timezones += $"{time},";
                    }
                    #endregion
                    #region Parameters and Values
                    command.Parameters.AddWithValue("@alpha3Code", item.Alpha3Code);
                    command.Parameters.AddWithValue("@name", item.Name);
                    command.Parameters.AddWithValue("@alpha2Code", item.Alpha2Code);
                    command.Parameters.AddWithValue("@capital", item.Capital);
                    command.Parameters.AddWithValue("@region", item.Region);
                    command.Parameters.AddWithValue("@subRegion", item.Subregion);
                    command.Parameters.AddWithValue("@population", item.Population);
                    command.Parameters.AddWithValue("@denonym", item.Demonym);
                    command.Parameters.AddWithValue("@area", item.Area);
                    command.Parameters.AddWithValue("@gini", item.Gini);
                    command.Parameters.AddWithValue("@nativeName", item.NativeName);
                    command.Parameters.AddWithValue("@numericCode", item.NumericCode);
                    command.Parameters.AddWithValue("@flag", item.Flag);
                    command.Parameters.AddWithValue("@cioc", item.Cioc);
                    command.Parameters.AddWithValue("@latlong", latlng);
                    command.Parameters.AddWithValue("@borders", borders);
                    command.Parameters.AddWithValue("@topLevelDomain", latlng);
                    command.Parameters.AddWithValue("@callingCodes", callingCodes);
                    command.Parameters.AddWithValue("@timezones", timezones);
                    command.Parameters.AddWithValue("@altSpellings", altSpellings);
                    #endregion

                    //HUGE sql insert command 
                    string insertCmd = "INSERT INTO country VALUES(@alpha3Code, @name, @alpha2Code, @capital, @region, @subRegion, @population, @denonym, @area, @gini, @nativeName, @numericCode, @flag, @cioc, @latlong, @borders, @topLevelDomain, @callingCodes, @timezones, @altSpellings)";
                    //string insertCmd = string.Format("INSERT INTO country VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}')",
                    //    item.Alpha3Code, item.Name, item.Alpha2Code, item.Capital, item.Region, item.Subregion, item.Population, item.Demonym, item.Area, item.Gini, item.NativeName, item.NumericCode, item.Flag, item.Cioc, latlng, borders, topLevelDomain, callingCodes, timezones, altSpellings);

                    inserts += $"{insertCmd}\n";

                    command.Parameters.Clear();
                }
                inserts += "COMMIT";

                command = new SQLiteCommand(inserts, sqlConnection);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// deletes the data from SQL DB
        /// </summary>
        public void DeleteData()
        {
            SQLCmds = AddDeleteCommands();

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
        /// command list to create the SQL tables
        /// </summary>
        /// <returns></returns>
        private List<string> AddCreateCommands()
        {
            SQLCmds.Clear();
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS country" +
                           "(alpha3Code CHAR(3) PRIMARY KEY,name VARCHAR NULL,alpha2Code CHAR(2) NULL,capital VARCHAR NULL," +
                           "region VARCHAR NULL,subregion VARCHAR NULL,population INT NULL,denonym VARCHAR NULL,area FLOAT NULL,giniIndex FLOAT NULL," +
                           "nativeName VARCHAR NULL,numericCode VARCHAR NULL,flag VARCHAR NULL,cioc CHAR(3) NULL,latlong VARCHAR NULL,borders VARCHAR NULL," +
                           "topLevelDomain VARCHAR NULL,callingCodes VARCHAR NULL,timezones VARCHAR NULL, altSpellings VARCHAR NULL)");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS currency(code CHAR(3) PRIMARY KEY,name VARCHAR NULL,symbol CHAR(5) NULL,idCountry CHAR(3) REFERENCES country(alpha3Code))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS language(iso639_2 CHAR(3) PRIMARY KEY,iso639_1 CHAR(2) NULL,name VARCHAR NULL,nativeName VARCHAR NULL,idCountry CHAR(3) REFERENCES country(alpha3Code))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS translations(idCountry CHAR(3) PRIMARY KEY REFERENCES country(alpha3Code),de VARCHAR(100),es VARCHAR(100),fr VARCHAR(100),ja VARCHAR(100),it VARCHAR(100),br VARCHAR(100),pt VARCHAR(100),nl VARCHAR(100),hr VARCHAR(100),fa VARCHAR(100))");
            SQLCmds.Add("CREATE TABLE IF NOT EXISTS regionalBloc(acronym VARCHAR(10) PRIMARY KEY,name VARCHAR(100) NULL,otherAcronyms VARCHAR NULL,otherNames VARCHAR NULL,idCountry CHAR(3) REFERENCES country(alpha3Code))");

            return SQLCmds;
        }

        /// <summary>
        /// creates de delete command string list
        /// </summary>
        /// <returns></returns>
        private List<string> AddDeleteCommands()
        {
            SQLCmds.Clear();
            SQLCmds.Add("DELETE FROM country");
            SQLCmds.Add("DELETE FROM currency");
            SQLCmds.Add("DELETE FROM language");
            SQLCmds.Add("DELETE FROM translations");
            SQLCmds.Add("DELETE FROM regionalBloc");

            return SQLCmds;
        }
    }
}
