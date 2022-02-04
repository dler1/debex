using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.FieldsChecking;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Polly;

namespace Debex.Convert.ViewModels.Pages
{
    public class CheckFieldsViewModel : BaseVm
    {
        private readonly UiService uiService;
        private readonly FieldsChecker checker;

        public CheckFieldsViewModel(UiService uiService, Storage storage, FieldsChecker checker) : base(storage)
        {
            this.uiService = uiService;
            this.checker = checker;
        }

        public ObservableCollection<CheckFieldState> State
        {
            get => Get<ObservableCollection<CheckFieldState>>(); set => Set(value);
        }

        public override void Initialize()
        {
            uiService.ChangeDataVisibility(true);
            InitState();
        }

        public async Task Check()
        {
            Storage.SetValue(Storage.StorageKey.Processing, true);

            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);

            var result = await checker.CheckFields(excel, fields);

            State = new ObservableCollection<CheckFieldState>(result.Map<List<CheckFieldState>>());

            Storage.SetValue(Storage.StorageKey.CheckFields, result);
            Storage.SetValue(Storage.StorageKey.CheckFieldsResult, ShortResult());
            Storage.SetValue(Storage.StorageKey.Excel, excel);
            Storage.SetValue(Storage.StorageKey.Processing, false);
        }

        private void InitState()
        {
            var state = Storage.GetValue<List<CheckField>>(Storage.StorageKey.CheckFields);
            if (state?.Count > 0)
            {
                State = new ObservableCollection<CheckFieldState>(state.Map<List<CheckFieldState>>());
                return;
            }

            State = new ObservableCollection<CheckFieldState>();

        }

        private RunResult ShortResult()
        {
            return State.Any(x => x.Errors > 0) ? RunResult.WithErrors : RunResult.Success;
        }


        public void SortByErrors(string fieldName)
        {
            var colName = State.FirstOrDefault(x => x.Name == fieldName)?.ColumnName;
            if (colName == null) return;
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(colName, SortType.CheckedError));
        }
    }
}
