﻿using System.Data;
using Microsoft.Extensions.Configuration;

namespace DataIntegration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /* var inputStr = getCurrentValutaString().Result;
            var currentUpdateDate = getUpdateDate(inputStr);
            var conversions = GetConversions(inputStr, currentUpdateDate); */
            var config = LoadConfiguration();
            var connectionStr = config.GetConnectionString("ValutaKurser");
            var repository = new SqlValutaConversionRepository(new SqlConnection(connectionStr));
            
            repository.ReadAll();
            /* var date = repository.getLastUpdateDate();
            Console.WriteLine(date); */
        
            /* foreach(var conversion in conversions) 
            {
                repository.Create(conversion);
            }
            
            repository.ReadAll(); */
                
            
        }
        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>();

            return builder.Build();
        }
        private static async Task<string> getCurrentValutaString()
        {
            HttpClient httpClient = new HttpClient();
            var http = httpClient.GetAsync("https://valutakurser.azurewebsites.net/ValutaKurs");
            var result = http.Result;
            if (result.StatusCode.ToString() != "OK")
            {
                Console.WriteLine("Could not connect to API. Exiting with StatusCode: " + result.StatusCode.ToString());
                System.Environment.Exit(0);
            }

            var content = await result.Content.ReadAsStringAsync();
            return content;
        }

        public static IEnumerable<ValutaConversionDTO> GetConversions(string apiResult, DateTime updateDate)
        {
            var pattern = @"[a-zA-Z]+.{3}([A-Z]{3}).{3}[a-zA-Z]+.{3}([A-Z]{3}).{3}[a-zA-Z]+.{2}(\d+.\d+)";
            int currentId = 0;
            foreach (Match m in Regex.Matches(apiResult, pattern))
            {
                var valutaConversion = new ValutaConversionDTO(currentId, m.Groups[1].ToString(), m.Groups[2].ToString(), updateDate, double.Parse(m.Groups[3].ToString()));
                currentId++;
                yield return valutaConversion;
            }
        }

        public static DateTime getUpdateDate(string apiResult)
        {
            var datePattern = @"[a-zA-Z]+.{3}(\d+).(\d+).(\d+).(\d+).(\d+).(\d+).(\d+)[A-Z]";
            var match = Regex.Match(apiResult, datePattern);
            var updateDate = new DateTime(
                Int32.Parse(match.Groups[1].ToString()),
                Int32.Parse(match.Groups[2].ToString()),
                Int32.Parse(match.Groups[3].ToString()),
                Int32.Parse(match.Groups[4].ToString()),
                Int32.Parse(match.Groups[5].ToString()),
                Int32.Parse(match.Groups[6].ToString()));
            return updateDate;
        }
    }
}
