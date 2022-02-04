using AirtableApiClient;
using Debex.Convert.Data;
using Debex.Convert.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Debex.Convert.BL.FieldsMatching
{
    public class AutomaticFieldsMatcher
    {
        private readonly string baseId = "appwSXSLaxIwGHvyV";
        private readonly string apiKey = "keyACBG5LG3ZqFjix";

        private Dictionary<string, List<string>> nameToRegistryNames;
        private Dictionary<string, string> baseFieldsIds = new();
        public async Task Init()
        {
            if (nameToRegistryNames != null) return;

            using var api = new AirtableBase(apiKey, baseId);

            var records = await api.ListRecords("tblCYXboM539UM6K4", view: "viwMKWBBMpssCek6W");
            var baseFields = await GetBaseFields();


            if (!records.Success)
            {
                //todo error
            }
            var mappings = new Dictionary<string, List<string>>();
            foreach (var record in records.Records)
            {
                if (!record.Fields.ContainsKey("alter_name")) continue;

                var recordField = ((string)record.Fields["name_ru"]).ToLowerInvariant();
                baseFieldsIds[recordField] = record.Id;
                var alterNames = new List<string>();
                var ids = (IEnumerable<JToken>)record.Fields["alter_name"];

                foreach (var token in ids)
                {
                    var id = token.Value<string>();
                    alterNames.Add(baseFields[id].ToLowerInvariant());
                }

                mappings[recordField.ToLowerInvariant()] = alterNames;
            }

            nameToRegistryNames = mappings;
        }

        public async Task SaveMatched(List<BaseFieldToMatch> matchedFields)
        {
            await Init();

            var matchedNames = nameToRegistryNames.SelectMany(x => x.Value)
                .Select(x => x.ToLowerInvariant().Trim())
                .ToHashSet();

            var hasChanges = false;

            foreach (var field in matchedFields)
            {
                if (!field.IsMatched || field.MatchedName.IsNullOrEmpty()) continue;

                var matchedLower = field.MatchedName.ToLowerInvariant();
                var nameLower = field.Name.ToLower();


                if (!nameToRegistryNames.ContainsKey(nameLower)) continue;
                if (nameToRegistryNames[nameLower].Contains(matchedLower)) continue;
                if (matchedNames.Contains(matchedLower)) continue;
                if (!baseFieldsIds.ContainsKey(nameLower)) continue;

                hasChanges = true;

                using var api = new AirtableBase(apiKey, baseId);

                var record = await api.CreateRecord("tbljkM2ZWEwVWU2fx",
                    new()
                    {
                        FieldsCollection = new()
                        {
                            ["name"] = field.MatchedName,
                            ["is_useless"] = false,
                            ["system_name"] = new List<string>()
                            {
                                baseFieldsIds[nameLower]
                            }
                        }
                    }
                    );


            }

            if (hasChanges)
            {
                nameToRegistryNames = null;
                baseFieldsIds.Clear();
            }


        }


        public void Match(ICollection<BaseFieldToMatch> baseFields, ICollection<FileFieldToMatch> fileFields)
        {

            if (nameToRegistryNames == null)
            {
                return;
            }

            foreach (var baseField in baseFields)
            {
                if (baseField.IsMatched) continue;
                if (!nameToRegistryNames.ContainsKey(baseField.Name.ToLower())) continue;

                var availableFields = nameToRegistryNames[baseField.Name.ToLowerInvariant()];

                var matchedField = availableFields
                    .Join(fileFields, m => m.ToLower(), f => f.Name.ToLower(), (m, f) => f)
                    .FirstOrDefault(x => x != null && !x.IsMatched);

                if (matchedField == null) continue;

                baseField.MatchedName = matchedField.Name;
                baseField.IsMatched = true;

                matchedField.IsMatched = true;
                matchedField.MatchedName = baseField.Name;


            }

        }


        private async Task<Dictionary<string, string>> GetBaseFields()
        {
            using var api = new AirtableBase(apiKey, baseId);
            var offset = string.Empty;
            var mapper = new Dictionary<string, string>();
            do
            {
                var records = await api.ListRecords("tbljkM2ZWEwVWU2fx", offset: offset);
                if (!records.Success)
                {

                }

                foreach (var airtableRecord in records.Records)
                {
                    var name = (string)airtableRecord.GetField("name");
                    mapper[airtableRecord.Id] = name;
                }

                offset = records.Offset;


            } while (!offset.IsNullOrEmpty());

            return mapper;
        }
    }
}