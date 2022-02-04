using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Debex.Convert.Extensions;
using RestSharp;

namespace Debex.Convert.Environment
{

    public class RegionsTable
    {
        public List<List<string>> Values { get; set; }
    }

    public class GoogleDocsLoader
    {
        private const string RegionsPath = "https://sheets.googleapis.com/v4/spreadsheets/1suMt4feNC3t-zdtLMKGv-Won9QaNFuP2POa8qEhULyE/values/A1:C99999?key=AIzaSyAdqX-6fJDFmnf6EzT-PnES_F__tbgoHCQ";

        private const string PushDataPath =
            "https://sheets.googleapis.com/v4/spreadsheets/1suMt4feNC3t-zdtLMKGv-Won9QaNFuP2POa8qEhULyE/values/regions-dadata!B1:D1:append?key=AIzaSyAdqX-6fJDFmnf6EzT-PnES_F__tbgoHCQ&valueInputOption=USER_ENTERED";


        private readonly IRestClient http;

        private Dictionary<string, RegionInfo> regions;

        public GoogleDocsLoader(IRestClient http)
        {
            this.http = http;
        }


        public async Task AppendDaDaResults(Dictionary<string, Address> pushData)
        {
            if (!pushData.Any()) return;


            var vals = new
            {
                range = "regions-dadata!B1:D1",
                majorDimension = "ROWS",
                values = pushData.Select(x => new[] { x.Key, x.Value.Region, x.Value.District }).ToArray()
            };

            var request = new RestRequest(PushDataPath, Method.POST).AddJsonBody(vals);
            await http.ExecuteAsync(request, Method.POST);


        }

        public async Task<Dictionary<string, RegionInfo>> GetRegionEntries()
        {
            return await LoadDoc();
        }

        private async Task<Dictionary<string, RegionInfo>> LoadDoc()
        {
            if (regions?.Any() ?? true)
            {
                var result = await http.ExecuteAsync(new RestRequest(RegionsPath));

                var dict = result.Content.FromJson<RegionsTable>()?.Values
                    .Skip(1)
                    .Select(x => new { Key = x[0].ToUpperInvariant().Trim(), Region = x[1], District = x[2] })
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, v => new RegionInfo { Region = v.First().Region, District = v.First().District });

                regions = dict;
            }

            return regions;
        }
    }

    public class RegionInfo
    {
        public string Region { get; set; }
        public string District { get; set; }
    }
}