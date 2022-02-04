using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Debex.Convert.BL;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.RegionsMatching;
using Debex.Convert.BL.Stages.CleanFormatsStage;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Microsoft.VisualBasic.FileIO;

namespace Debex.Convert.ViewModels.Pages
{
    public class ClearFormatsViewModel : BaseVm
    {
        private readonly UiService uiService;
        private readonly FormatCleaner formatCleaner;
        public ObservableCollection<CleanFieldState> State { get => Get<ObservableCollection<CleanFieldState>>(); set => Set(value); }

        public ClearFormatsViewModel(Storage storage, FormatCleaner formatCleaner, UiService uiService) : base(storage)
        {
            this.formatCleaner = formatCleaner;
            this.uiService = uiService;

            State = new ObservableCollection<CleanFieldState>();
        }


        protected override void OnLoaded()
        {
            OnBaseFieldToMatchChanged();

            uiService.ChangeDataVisibility(true);
        }

        public async void Process()
        {
            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            var baseFields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);

            Storage.SetValue(Storage.StorageKey.Processing, true);
            var results = await formatCleaner.FormatData(excel, baseFields);
            
            
            var state = results.Map<List<CleanFieldState>>();
            State = new ObservableCollection<CleanFieldState>(state);
            Storage.SetValue(Storage.StorageKey.ClearedFieldsState, results);
            Storage.SetValue(Storage.StorageKey.Processing, false);
            Storage.SetValue(Storage.StorageKey.ClearedFieldsResult, ShortResult());

        }

        public void ErrorSort(string fieldName)
        {
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(fieldName, SortType.Error));
        }

        public void CorrectedSort(string col)
        {
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(col, SortType.Corrected));
        }

        private void OnBaseFieldToMatchChanged()
        {
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            var savedState = Storage.GetValue<List<CleanField>>(Storage.StorageKey.ClearedFieldsState);

            if (savedState?.Count > 0)
            {
                State = new ObservableCollection<CleanFieldState>(savedState.Map<List<CleanFieldState>>());
                OnPropertyChanged(nameof(State));
                return;
            }

            State.Clear();

            foreach (var field in fields.Where(x => x.IsMatched))
            {
                var stateField = new CleanFieldState
                {
                    Name = field.Name
                };

                State.Add(stateField);
            }
        }

        private RunResult ShortResult()
        {
            return State.Any(x => x.Errors > 0) ? RunResult.WithErrors : RunResult.Success;
        }

    }
}
