namespace CountriesAPP.Services
{
    using System;
    using System.Net;

    public class NetworkService
    {
        public bool CheckNetConnection()
        {
            var connection = new WebClient();

            try
            {
                using (connection.OpenRead("http://client3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
