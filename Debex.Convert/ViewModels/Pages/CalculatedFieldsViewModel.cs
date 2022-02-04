using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL;
using Debex.Convert.BL.CalculatingFields;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Environment;

namespace Debex.Convert.ViewModels.Pages
{
    public class CalculatedFieldsViewModel : BaseVm
    {
        private readonly UiService uiService;
        private readonly FieldsCalculator calculator;
        public CalculatedFieldsViewModel(UiService uiService, Storage storage, FieldsCalculator calculator) : base(storage)
        {
            this.uiService = uiService;
            this.calculator = calculator;
        }

        public ObservableCollection<CalculatedFieldState> State
        {
            get => Get<ObservableCollection<CalculatedFieldState>>();
            set => Set(value);
        }


        public override void Initialize()
        {
            InitState();
            uiService.ChangeDataVisibility(true);
        }


        public async Task Calculate()
        {
            Storage.SetValue(Storage.StorageKey.Processing, true);

            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);

            var result = await calculator.CalculateFields(excel, fields);

            State = new ObservableCollection<CalculatedFieldState>(result.Map<List<CalculatedFieldState>>());

            Storage.SetValue(Storage.StorageKey.CalculatedFields, result);
            Storage.SetValue(Storage.StorageKey.CalculateFieldsResult, ShortResult());
            Storage.SetValue(Storage.StorageKey.Excel, excel);
            Storage.SetValue(Storage.StorageKey.Processing, false);
        }

        private void InitState()
        {
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            var calculated = Storage.GetValue<List<CalculatedField>>(Storage.StorageKey.CalculatedFields);

            if (calculated?.Count > 0)
            {
                State = new ObservableCollection<CalculatedFieldState>(calculated.Map<List<CalculatedFieldState>>());
                return;
            }

            State = new()
            {

            };
        }

        public RunResult ShortResult()
        {
            return State.Any(x => x.Errors > 0) ? RunResult.WithErrors : RunResult.Success;
        }

        public void SortByErrors(string fieldName)
        {
            var colName = State.FirstOrDefault(x => x.Name == fieldName)?.ColumnName;
            if(colName == null) return;
            Storage.SetValue(Storage.StorageKey.SortByColumnName, new SortInfo(colName, SortType.CalculatedError));
        }
    }
}
