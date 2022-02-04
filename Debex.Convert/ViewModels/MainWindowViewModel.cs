using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Ribbon;
using Debex.Convert.BL;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Debex.Convert.Views.Pages;
using String = System.String;

namespace Debex.Convert.ViewModels
{
    public class MainWindowViewModel : BaseVm
    {
        private readonly DialogService dialogService;
        private readonly ExcelReader excelReader;
        private readonly ConfigurationLoader configurationLoader;
        private readonly Navigator navigator;

        private string filePath;
        public string FilePath
        {
            get => filePath;
            set => Set(value, out filePath);
        }

        public bool IsFileSelected
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsProcessing
        {
            get => Get<bool>();
            set => Set(value);
        }

        public DataTable Data
        {
            get => Get<DataTable>();
            set => Set(value);
        }
        public ObservableCollection<BaseFieldToMatch> BaseFields
        {
            get => Get<ObservableCollection<BaseFieldToMatch>>();
            set => Set(value);
        }

        public Dictionary<string, BaseField.BaseFieldType> Types
        {
            get => Get<Dictionary<string, BaseField.BaseFieldType>>();
            set => Set(value);
        }

        public event Action<string> OnSorted;


        public MainWindowViewModel(DialogService dialogService, ExcelReader excelReader, ConfigurationLoader configurationLoader, Storage storage, Navigator navigator) : base(storage)
        {
            this.dialogService = dialogService;
            this.excelReader = excelReader;
            this.configurationLoader = configurationLoader;
            this.navigator = navigator;
        }

        public async void SelectFile()
        {
            FilePath = dialogService.SelectFile();
            if (FilePath.IsNullOrEmpty()) return;

            navigator.Navigate<EmptyPage>();

            Storage.SetValue(Storage.StorageKey.FilePath, FilePath);
            Storage.SetValue(Storage.StorageKey.Loading, true);

            var excel = await excelReader.ReadExcel(FilePath);

            var fileFieldsToMatch = excel.Headers.Keys.Select(x => new FileFieldToMatch { Name = x }).ToList();

            IsFileSelected = true;
            Initialize();

            Storage.SetValue(Storage.StorageKey.FileFieldsToMatch, fileFieldsToMatch);
            Storage.SetValue(Storage.StorageKey.Excel, excel);
            Storage.SetValue(Storage.StorageKey.Loading, false);
            Storage.SetValue<List<CleanFieldState>>(Storage.StorageKey.ClearedFieldsState, null);
            Storage.SetValue<string>(Storage.StorageKey.ConfigurationMatchFile, null);

            Storage.SetValue<List<RegionMatch>>(Storage.StorageKey.RegionMatch, null);
            Storage.SetValue<List<CalculatedField>>(Storage.StorageKey.CalculatedFields, null);
            Storage.SetValue<List<CheckField>>(Storage.StorageKey.CheckFields, null);

            Storage.SetValue(Storage.StorageKey.ClearedFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.CheckFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.CalculateFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.RegionMatchResult, RunResult.NotRun);

            navigator.Navigate<FieldsMatchingPage>();
        }



        public override void Initialize()
        {
            var baseFields = configurationLoader.BaseFields;

            var fieldsToMatch = baseFields.Select(baseField => new BaseFieldToMatch
            {
                Id = baseField.Id,
                Name = baseField.Name,
                IsMatched = false,
                Type = baseField.Type,
                Required = baseField.Required ?? false,
                RegionCheck = baseField.RegionCheck ?? false
            }).ToList();


            Storage.SetValue(Storage.StorageKey.ClearedFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.CheckFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.CalculateFieldsResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.RegionMatchResult, RunResult.NotRun);
            Storage.SetValue(Storage.StorageKey.BaseFields, baseFields);
            Storage.SetValue(Storage.StorageKey.BaseFieldsToMatch, fieldsToMatch);
            Storage.SetValue(Storage.StorageKey.FieldsMatchState, Consts.FieldsMatchState.None);
            Storage.SetValue(Storage.StorageKey.Loading, false);



        }
        public void SortByRegionMatched(string name)
        {
            Sort(name, MatchedComparison);
        }
        public void SortByRegionUnmatched(string name)
        {
            Sort(name, NotMatchedComparison);
        }
        public void SortByErrors(string name)
        {
            Sort(name, ErrorComparison);
        }
        public void SortByCorrected(string name)
        {
            Sort(name, CorrectedComparison);
        }

        public void SortByCalculatedErrors(string name)
        {
            Sort(name, ErrorComparison);

        }

        public void SortByNames(string name, bool reverse = false)
        {
            if (!reverse) Sort(name, NameComparison);
            else Sort(name, ReverseNameComparison);
        }

        private void SortyByCheckedErrors(string name)
        {
            Sort(name, ErrorComparison);
        }

        private async void Sort(string name, Func<string, Comparison<DataRow>> comparer)
        {
            var targetField = BaseFields.FirstOrDefault(x => x.Name == name)?.MatchedName;
            if (targetField == null) targetField = name;

            if (!Data.Columns.Contains(targetField)) return;

            Storage.SetValue(Storage.StorageKey.Processing, true);

            var result = await Task.Run(() =>
            {
                var table = Data.Clone();
                var rows = new List<DataRow>();

                for (int i = 0; i < Data.Rows.Count; i++)
                {
                    rows.Add(Data.Rows[i]);
                }

                rows.Sort(comparer(targetField));

                for (int i = 0; i < rows.Count; i++)
                {
                    table.Rows.Add(rows[i].ItemArray);
                }

                return table;
            });

            Data = result;

            OnSorted?.Invoke(targetField);

            Storage.SetValue(Storage.StorageKey.Processing, false);
        }

        private Comparison<DataRow> NameComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];

            if (f.Value is null || s.Value is null || f.Value == DBNull.Value || s.Value == DBNull.Value) return f.Value != null ? 1 : -1;
            if (f.Value is string fs && s.Value is string ss) return String.Compare(fs, ss, StringComparison.Ordinal);
            else if (f.Value is int fi && s.Value is int si) return fi.CompareTo(si);
            else if (f.Value is DateTime dtf && s.Value is DateTime dt2) return dtf.CompareTo(dt2);
            else if (f.Value is decimal df && s.Value is decimal sf) return df.CompareTo(sf);
            else if (f.Value is double dbf && s.Value is double dbs) return dbf.CompareTo(dbs);

            return Comparer.Default.Compare(f.Value.ToString(), s.Value.ToString());
        };
        private Comparison<DataRow> ReverseNameComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];

            if (f.Value is null || s.Value is null || f.Value == DBNull.Value || s.Value == DBNull.Value) return f.Value != null ? -1 : 1;
            if (f.Value is string fs && s.Value is string ss) return -String.Compare(fs, ss, StringComparison.Ordinal);
            else if (f.Value is int fi && s.Value is int si) return -fi.CompareTo(si);
            else if (f.Value is DateTime dtf && s.Value is DateTime dt2) return -dtf.CompareTo(dt2);
            else if (f.Value is decimal df && s.Value is decimal sf) return -df.CompareTo(sf);
            else if (f.Value is double dbf && s.Value is double dbs) return -dbf.CompareTo(dbs);

            return -Comparer.Default.Compare(f.Value.ToString(), s.Value.ToString());
        };

        private Comparison<DataRow> ErrorComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];
            return s.IsError.CompareTo(f.IsError);
        };
        private Comparison<DataRow> CorrectedComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];
            return s.IsCorrected.CompareTo(f.IsCorrected);
        };

        private Comparison<DataRow> MatchedComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];
            return s.IsRegionMatched.CompareTo(f.IsRegionMatched);
        };

        private Comparison<DataRow> NotMatchedComparison(string targetField) => (r, r2) =>
        {
            var f = (RowData)r[targetField];
            var s = (RowData)r2[targetField];
            var fVal = f.Value?.ToString() ?? string.Empty;
            var sVal = s.Value?.ToString() ?? string.Empty;

            return f.IsRegionMatched || s.IsRegionMatched
                ? f.IsRegionMatched.CompareTo(s.IsRegionMatched)
                : sVal.CompareTo(fVal);
        };

        protected override void OnLoaded()
        {
            Subscribe<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch, f =>
            {
                BaseFields = new ObservableCollection<BaseFieldToMatch>(f);

                var types = BaseFields?
                    .Where(x => x.MatchedName.NotNullOrEmpty())
                    .ToDictionary(x => x.MatchedName, v => v.Type);

                Storage.SetValue(Storage.StorageKey.BaseFieldsToMatchMapping, types);
            });

            Subscribe<ReadExcelResult>(Storage.StorageKey.Excel, OnExcelChanged);

            Subscribe<bool>(Storage.StorageKey.Processing, v => IsProcessing = v);

            Subscribe<SortInfo>(Storage.StorageKey.SortByColumnName, (v) =>
            {
                if (v?.Type == SortType.Error) SortByErrors(v.ColumnName);
                else if (v?.Type == SortType.Corrected) SortByCorrected(v.ColumnName);
                else if (v?.Type == SortType.RegionMatched) SortByRegionMatched(v.ColumnName);
                else if (v?.Type == SortType.RegionNotMatched) SortByRegionUnmatched(v.ColumnName);
                else if (v?.Type == SortType.CalculatedError) SortByCalculatedErrors(v.ColumnName);
                else if (v?.Type == SortType.CheckedError) SortyByCheckedErrors(v.ColumnName);
            });
        }


        private void OnExcelChanged(ReadExcelResult r)
        {
            if (r == null) return;
            Data = r.Data;
        }
    }
}
