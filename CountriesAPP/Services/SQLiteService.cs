namespace CountriesAPP.Services
{
    using API_Models;
    using SQLite;
    using System;
    using System.IO;
    using System.Windows;

    class SQLiteService
    {
        private SQLiteConnection connection;

        public SQLiteService()
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = $@"{Directory.GetCurrentDirectory()}\Data\CountriesDB.sqlite";

            try
            {
                connection = new SQLiteConnection(path);

                connection.CreateTable<Country>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
