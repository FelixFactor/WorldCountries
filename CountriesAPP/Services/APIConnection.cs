namespace CountriesAPP.Services
{
    using CountriesAPP.Models.API_Models;
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class APIConnection
    {
        public async Task<Response> GetApiData(string host, string controller)
        {
            try
            {
                //opens a connection to the web
                var client = new HttpClient();

                //defines the host 
                client.BaseAddress = new Uri(host);

                //defines the api controller(the file itself) and waits for its response
                var response = await client.GetAsync(controller);

                //reads the content to a string
                var result = await response.Content.ReadAsStringAsync();

                //checks if the operation was successfull
                if (!response.IsSuccessStatusCode)
                {
                    //generates a new unsuccess response with the error that happened 
                    return new Response
                    {
                        Success = false,
                        Answer = result
                    };
                }

                //defines options for the JSON deserializer
                var options = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };

                //converts the Json to a List of country objects
                var countries = JsonConvert.DeserializeObject<List<Country>>(result, options);

                return new Response { Success = true, Result = countries };
            }
            catch (Exception ex)
            {
                return new Response { Success = false, Answer = ex.Message };
            }
        }

        public async Task<Response> GetRates()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://cambiosrafa.azurewebsites.net");

                var response = await client.GetAsync("/api/rates");

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        Success = false,
                        Answer = result
                    };
                }

                var rates = JsonConvert.DeserializeObject<List<Rate>>(result);

                return new Response { Success = true, Result = rates };
            }
            catch (Exception ex)
            {
                return new Response { Success = false, Answer = ex.Message };
            }
        }
    }
}
