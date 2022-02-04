using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.RegionsMatching;
using Debex.Convert.Data;
using Debex.Convert.Environment;

namespace Debex.Convert.ViewModels.Pages
{
    public class RegionMatchViewModel : BaseVm
    {

        private readonly RegionsMatcher matcher;
        private readonly UiService uiService;

        public RegionMatchViewModel(RegionsMatcher matcher, Storage storage, UiService uiService) : base(storage)
        {
            this.matcher = matcher;
            this.uiService = uiService;
        }


        public ObservableCollection<RegionMatchState> State
        {
            get => Get<ObservableCollection<RegionMatchState>>();
            set => Set(value);
        }

        public override void Initialize()
        {
            InitState();
            uiService.ChangeDataVisibility(true);

        }


        public async Task Process()
        {
            var baseFields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            if (baseFields == null || baseFields.Count == 0) return;

            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            if (excel == null) return;

            
            Storage.SetValue(Storage.StorageKey.Processing, true);

            var results = await matcher.FillRegions(excel, baseFields);
            State = new ObservableCollection<RegionMatchState>(results.Map<List<RegionMatchState>>());

            Storage.SetValue(Storage.StorageKey.RegionMatch, results);
            Storage.SetValue(Storage.StorageKey.RegionMatchResult, UpdateResult());
            Storage.SetValue(Storage.StorageKey.Excel, excel);

            Storage.SetValue(Storage.StorageKey.Processing, false);
        }

        public void SortMatched(string name)
        {
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(name, SortType.RegionMatched));
        }
        public void SortUnMatched(string col)
        {
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(col, SortType.RegionNotMatched));
        }

        private void InitState()
        {
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            var regions = Storage.GetValue<List<RegionMatch>>(Storage.StorageKey.RegionMatch);

            if (regions?.Count > 0)
            {
                State = new ObservableCollection<RegionMatchState>(regions.Map<List<RegionMatchState>>());
                return;
            }

            var newState = fields
                .Where(x => x.RegionCheck)
                .Select(x => new RegionMatchState { Name = x.Name });

            State = new ObservableCollection<RegionMatchState>(newState);
        }

        private RunResult UpdateResult()
        {
            return State.Any(x => x.NotFound > 0) ? RunResult.WithErrors : RunResult.Success;
        }

    }
}
