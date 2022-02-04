using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;

namespace Debex.Convert.BL.RegionsMatching
{

    public class RegionMatchToDaData
    {
        public int BaseColumnIndex { get; set; }
        public string BaseFieldName { get; set; }
        public Dictionary<string, List<int>> Regions { get; set; }

        public RegionMatch Match { get; set; }
    }

    public class RegionsMatcher
    {
        private GoogleDocsLoader googleLoader;
        private readonly DaDataClient daDataClient;

        public RegionsMatcher(GoogleDocsLoader googleLoader, DaDataClient daDataClient)
        {
            this.googleLoader = googleLoader;
            this.daDataClient = daDataClient;
        }

        public async Task<List<RegionMatch>> FillRegions(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return await Task.Run(() => Run(excel, fields.Where(x => x.IsMatched && x.RegionCheck).ToList()));
        }

        private async Task<List<RegionMatch>> Run(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            if (fields.IsNullOrEmpty()) return new List<RegionMatch>();

            var entries = await googleLoader.GetRegionEntries();

            CreateColumns(excel, fields);

            var toDaData = await fields
                .Select(x => Match(excel, x, entries))
                .ToList()
                .WhenAll();
            var regions = toDaData
                .Select(x => x.Regions)
                .SelectMany(x => x.Keys)
                .Distinct()
                .ToList();

            if (regions.Count == 0)
            {
                var cols = new List<DataColumn>();
                foreach (DataColumn col in excel.Data.Columns)
                {
                    if (col.ColumnName?.Contains("Debex.info / DaData", StringComparison.InvariantCultureIgnoreCase) ?? false)
                        cols.Add(col);
                }

                foreach (var dataColumn in cols)
                {
                    excel.Data.Columns.Remove(dataColumn);
                }

            }
            else
            {
                var daDataResults = await daDataClient.LoadRegions(regions);
                await googleLoader.AppendDaDaResults(daDataResults);

                await toDaData.Select(x => FillDaData(excel, x, daDataResults))
                    .ToList()
                    .WhenAll();
            }

            excel.UpdateHeaders();

            return toDaData.Select(x => x.Match).ToList();
        }

        private void CreateColumns(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            foreach (var field in fields)
            {
                var column = excel.Headers[field.MatchedName];

                var regRegionName = $"Debex.Info/{field.Name}";
                var districtRegionName = $"Debex.Info/ {field.Name.Replace("регион", "ФО", StringComparison.InvariantCultureIgnoreCase)}";

                var daDataName = $"Debex.info / DaData / {field.Name}";
                var daDataDistrictName = $"Debex.info / DaData / {field.Name.Replace("регион", "ФО", StringComparison.InvariantCultureIgnoreCase)}";

                var regRegionIndex = column + 1;
                var districtRegionIndex = column + 2;
                var daDataIndex = column + 3;
                var daDataDistrictIndex = column + 4;

                if (!excel.Headers.ContainsKey(regRegionName))
                {
                    var regRegion = excel.Data.Columns.Add(regRegionName, typeof(RowData));
                    var districtRegion = excel.Data.Columns.Add(districtRegionName, typeof(RowData));
                    var daDataCol = excel.Data.Columns.Add(daDataName, typeof(RowData));
                    var daDataDistrictCol = excel.Data.Columns.Add(daDataDistrictName, typeof(RowData));

                    regRegion.SetOrdinal(regRegionIndex);
                    districtRegion.SetOrdinal(districtRegionIndex);
                    daDataCol.SetOrdinal(daDataIndex);
                    daDataDistrictCol.SetOrdinal(daDataDistrictIndex);
                }

                excel.UpdateHeaders();
            }

        }

        private async Task<RegionMatchToDaData> Match(ReadExcelResult excel, BaseFieldToMatch field, Dictionary<string, RegionInfo> entries)
        {

            var toDaData = new Dictionary<string, List<int>>();
            if (field.MatchedName.IsNullOrEmpty()) return null;

            var data = excel.Data;
            var column = excel.Headers[field.MatchedName];


            var regRegionIndex = column + 1;
            var districtRegionIndex = column + 2;

            var filledRows = 0;
            var foundRows = 0;

            for (int row = 0; row < excel.Data.Rows.Count; row++)
            {
                data.Rows[row][regRegionIndex] = new RowData { Value = "нет данных" };
                data.Rows[row][districtRegionIndex] = new RowData { Value = "нет данных" };

                var value = data.GetValue(row, column)?.ToString()?.Trim()?.ToUpperInvariant();

                if (value.IsNullOrEmpty())
                {
                    data.ChangeValue(row, regRegionIndex, "");
                    data.ChangeValue(row, districtRegionIndex, "");
                    continue;

                }

                filledRows++;

                var hasKey = entries.TryGetValue(value, out var regionInfo);
                if (hasKey)
                {

                    data.ChangeValue(row, regRegionIndex, regionInfo.Region);
                    data.ChangeValue(row, districtRegionIndex, regionInfo.District);
                    data.SetRegionMatched(row, column);
                    foundRows++;
                    continue;
                }



                if (toDaData.ContainsKey(value)) toDaData[value].Add(row);
                else toDaData[value] = new() { row };

            }

            excel.UpdateHeaders();

            return new RegionMatchToDaData
            {
                BaseColumnIndex = column,
                BaseFieldName = field.Name,
                Regions = toDaData,
                Match = new RegionMatch
                {
                    Total = data.Rows.Count,
                    Filled = filledRows,
                    Found = foundRows,
                    Name = field.Name
                }
            };
        }

        private async Task FillDaData(ReadExcelResult excel, RegionMatchToDaData regionMatch, Dictionary<string, Address> addresses)
        {
            var daDataIndex = regionMatch.BaseColumnIndex + 3;
            var daDataDistrictIndex = regionMatch.BaseColumnIndex + 4;

            var data = excel.Data;
            for (int row = 0; row < data.Rows.Count; row++)
            {
                data.Rows[row][daDataIndex] = new RowData { Value = "нет данных" };
                data.Rows[row][daDataDistrictIndex] = new RowData { Value = "нет данных" };

                var value = data.GetValue(row, regionMatch.BaseColumnIndex)?.ToString()?.Trim()?.ToUpperInvariant();
                if (value.IsNullOrEmpty())
                {
                    data.ChangeValue(row, daDataIndex, "");
                    data.ChangeValue(row, daDataDistrictIndex, "");
                    continue;
                };

                var hasKey = addresses.TryGetValue(value, out var adr);

                if (!hasKey)
                {
                    data.ChangeValue(row, daDataIndex, "");
                    data.ChangeValue(row, daDataDistrictIndex, "");
                    continue;
                }
                if (adr?.Region != null)
                {
                    data.ChangeValue(row, daDataIndex, adr.Region);
                    data.ChangeValue(row, daDataDistrictIndex, adr.District);
                    data.SetRegionMatched(row, regionMatch.BaseColumnIndex);

                    regionMatch.Match.Found++;
                }
            }

        }


    }
}
