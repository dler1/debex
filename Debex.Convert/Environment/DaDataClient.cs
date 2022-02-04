using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Polly;
using RestSharp;
using RestSharp.Serialization;

namespace Debex.Convert.Environment
{
    public class DaDataClient
    {
        private const string Token = "79d902e33e5bc5034b8150b2a77551797790ef56";
        private const string Secret = "5f6fa46e10a8ba7c74bf124ed0ebae733924b8d7";
        private const string Url = "https://cleaner.dadata.ru/api/v1/clean/address";
        public const int MaxBatchSize = 50;

        private readonly IRestClient http;
        public DaDataClient(IRestClient http)
        {
            this.http = http;
        }

        public async Task<Dictionary<string, Address>> LoadRegions(List<string> addressList)
        {

            var dict = new Dictionary<string, Address>();
            var resultList = new List<Address>();

            var handled = 0;
            
            while (handled < addressList.Count)
            {
                var request = new RestRequest(Url, Method.POST, DataFormat.Json)
                    .AddHeader("X-Secret", Secret)
                    .AddHeader("Authorization", $"Token {Token}")
                    .AddJsonBody(addressList.Skip(handled).Take(MaxBatchSize), ContentType.Json);


                var result = await Policy
                    .HandleResult<IRestResponse<List<Address>>>(r => !r.IsSuccessful)
                    .WaitAndRetryAsync(4, v => TimeSpan.FromSeconds(v))
                    .ExecuteAsync(() => http.ExecutePostAsync<List<Address>>(request));

                if (!result.IsSuccessful || result.Data == null) throw new HttpRequestException("Не удалось получить доступ к DaData");
                
                var addresses = result.Data;
                handled += MaxBatchSize;
                resultList.AddRange(addresses);
            }

            for (int i = 0; i < addressList.Count; i++)
            {
                dict[addressList[i]] = resultList[i];
            }

            return dict;
        }
    }

    public class Address
    {
        [JsonPropertyName("region_with_type")]
        public string Region { get; set; }
        [JsonPropertyName("federal_district")]
        public string District { get; set; }
    }
}
