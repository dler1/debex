using Debex.Convert.BL;
using Debex.Convert.BL.CalculatingFields;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.FieldsChecking;
using Debex.Convert.BL.RegionsMatching;
using Debex.Convert.BL.Stages.CleanFormatsStage;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Debex.Convert.Views.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Debex.Convert.BL.FieldsMatching;
using Color = System.Windows.Media.Color;
using Page = System.Windows.Controls.Page;

namespace Debex.Convert.ViewModels.Controls
{
    public class SidebarViewModel : BaseVm
    {
        private static readonly Color Gray = Color.FromRgb(170, 170, 170);
        private static readonly Color Orange = Color.FromRgb(245, 154, 35);
        private static readonly Color Green = Color.FromRgb(112, 183, 3);

        private readonly Navigator navigator;
        private readonly AutomaticFieldsMatcher fieldsMatcher;
        public SidebarViewModel(Storage storage, Navigator navigator, AutomaticFieldsMatcher fieldsMatcher) : base(storage)
        {
            this.navigator = navigator;
            this.fieldsMatcher = fieldsMatcher;
        }

        public SolidColorBrush MatchingColor
        {
            get => Get<SolidColorBrush>();
            set => Set(value);
        }

        public string FilePath
        {
            get => Get<string>();
            set => Set(value);
        }

        public string MatchedText
        {
            get => Get<string>();
            set => Set(value);
        }



        public MenuItemState MatchItem
        {
            get => Get<MenuItemState>();
            set => Set(value);
        }

        public MenuItemState CorrectFormatsItem
        {
            get => Get<MenuItemState>();
            set => Set(value);
        }

        public MenuItemState RegionMatchItem
        {
            get => Get<MenuItemState>();
            set => Set(value);
        }

        public MenuItemState CalculateItem
        {
            get => Get<MenuItemState>();
            set => Set(value);
        }

        public MenuItemState CheckFieldsItem
        {
            get => Get<MenuItemState>();
            set => Set(value);
        }

        public bool IsEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public void CheckAll()
        {
            CorrectFormatsItem.IsSelected = true;
            RegionMatchItem.IsSelected = true;
            CalculateItem.IsSelected = true;
            CheckFieldsItem.IsSelected = true;
        }

        public async Task RunAll()
        {
            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            var loading = Storage.GetValue<bool>(Storage.StorageKey.Loading);

            if (excel == null || fields.All(x => !x.IsMatched) || loading) return;

            navigator.Navigate<EmptyPage>();
            Storage.SetValue(Storage.StorageKey.Loading, true);
            var sw = Stopwatch.StartNew();

            try
            {
                if (CorrectFormatsItem.IsSelected)
                {
                    var cleanFields = await IoC.Get<FormatCleaner>().FormatData(excel, fields);

                    excel.UpdateHeaders();

                    Storage.SetValue(Storage.StorageKey.ClearedFieldsState, cleanFields);
                    Storage.SetValue(Storage.StorageKey.ClearedFieldsResult, cleanFields.GetResult());
                }

                if (RegionMatchItem.IsSelected)
                {
                    var regions = await IoC.Get<RegionsMatcher>().FillRegions(excel, fields);
                    excel.UpdateHeaders();
                    Storage.SetValue(Storage.StorageKey.RegionMatch, regions);
                    Storage.SetValue(Storage.StorageKey.RegionMatchResult, regions.GetResult());
                }

                if (CalculateItem.IsSelected)
                {
                    var calculate = await IoC.Get<FieldsCalculator>().CalculateFields(excel, fields);
                    excel.UpdateHeaders();
                    Storage.SetValue(Storage.StorageKey.CalculatedFields, calculate);
                    Storage.SetValue(Storage.StorageKey.CalculateFieldsResult, calculate.GetResult());
                }

                if (CheckFieldsItem.IsSelected)
                {
                    var check = await IoC.Get<FieldsChecker>().CheckFields(excel, fields);
                    excel.UpdateHeaders();
                    Storage.SetValue(Storage.StorageKey.CheckFields, check);
                    Storage.SetValue(Storage.StorageKey.CheckFieldsResult, check.GetResult());
                }

                Storage.SetValue(Storage.StorageKey.Excel, excel);

                var elapsed = TimeSpan.FromMilliseconds(Math.Max(sw.ElapsedMilliseconds, 1000)).ToString(@"hh\:mm\:ss");
                MessageBox.Show($"Обработка файла завершена успешно.\n" +
                                $"Время обработки файла: {elapsed}");
            }
            catch (Exception e)
            {
                MessageBox.Show("Произошла непредвиденная ошибка");
            }

            sw.Stop();

            Storage.SetValue(Storage.StorageKey.Loading, false);
            NavigateMatching();

        }


        public async Task Save()
        {

            var excel = Storage.GetValue<ReadExcelResult>(Storage.StorageKey.Excel);
            var fields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch);
            var loading = Storage.GetValue<bool>(Storage.StorageKey.Loading);

            var cleanFields = Storage.GetValue<List<CleanField>>(Storage.StorageKey.ClearedFieldsState) ?? new List<CleanField>();
            var calculatedFields = Storage.GetValue<List<CalculatedField>>(Storage.StorageKey.CalculatedFields) ?? new List<CalculatedField>();
            var checkFields = Storage.GetValue<List<CheckField>>(Storage.StorageKey.CheckFields) ?? new List<CheckField>();

            if (excel == null || !fields.Any(x => x.IsMatched) || loading) return;

            navigator.Navigate<EmptyPage>();
            Storage.SetValue(Storage.StorageKey.Loading, true);

            try
            {
                await fieldsMatcher.SaveMatched(fields);
                if (await IoC.Get<ExcelSaver>().Save(excel, fields, cleanFields, calculatedFields, checkFields))
                    MessageBox.Show("Файл сохранен");
            }
            catch (Exception e)
            {
                MessageBox.Show("Не удалось сохранить файл. Возможно выбранный файл открыт в другой программе.");
            }

            Storage.SetValue(Storage.StorageKey.Loading, false);
            NavigateMatching();
        }


        protected override void OnLoaded()
        {
            MatchItem = new MenuItemState { IsActive = true, CanSelect = false, IsSelected = true };
            CorrectFormatsItem = new MenuItemState { CanSelect = true, IsSelected = true };
            RegionMatchItem = new MenuItemState
            {
                CanSelect = true,
                IsSelected = true,
                MainText = "Сопоставить с справочниками",
                AdditionalText = "Не выполнялось",
                CircleColor = new SolidColorBrush(Gray)
            };

            CalculateItem = new MenuItemState
            {
                CanSelect = true,
                IsSelected = true,
                MainText = "Добавить расчетные столбцы",
                AdditionalText = "Не выполнялось",
                CircleColor = new SolidColorBrush(Gray)
            };

            CheckFieldsItem = new MenuItemState
            {
                CanSelect = true,
                IsSelected = true,
                MainText = "Проверить логику данных",
                AdditionalText = "Не выполнялось",
                CircleColor = new SolidColorBrush(Gray)
            };

            Subscribe<Consts.FieldsMatchState>(Storage.StorageKey.FieldsMatchState, OnFieldsMatchStateChanged);
            Subscribe<string>(Storage.StorageKey.FilePath, p => FilePath = p);
            Subscribe<(int, int)>(Storage.StorageKey.MatchedFields, OnMatchedFieldsChanged);

            Subscribe<bool>(Storage.StorageKey.Loading, (l) => IsEnabled = !l);
            Subscribe<bool>(Storage.StorageKey.Processing, p => IsEnabled = !p);

            Subscribe<RunResult>(Storage.StorageKey.ClearedFieldsResult, OnClearFieldsResultChanged);
            Subscribe<RunResult>(Storage.StorageKey.RegionMatchResult, OnRegionMatchResultChanged);
            Subscribe<RunResult>(Storage.StorageKey.CalculateFieldsResult, OnCalculateResultChanged);
            Subscribe<RunResult>(Storage.StorageKey.CheckFieldsResult, OnCheckFieldsResultChanged);


            navigator.AfterNavigate += AfterNavigate;
        }



        protected override void OnUnloaded()
        {

            navigator.AfterNavigate -= AfterNavigate;
        }

        private void OnFieldsMatchStateChanged(Consts.FieldsMatchState state)
        {

            MatchItem.CircleColor = state switch
            {
                Consts.FieldsMatchState.None => new SolidColorBrush(Gray),
                Consts.FieldsMatchState.Partial => new SolidColorBrush(Orange),
                Consts.FieldsMatchState.All => new SolidColorBrush(Green),
                _ => new SolidColorBrush(Gray)
            };
        }
        private void OnClearFieldsResultChanged(RunResult result)
        {
            SetResult(CorrectFormatsItem, result);
        }

        private void OnRegionMatchResultChanged(RunResult result)
        {
            SetResult(RegionMatchItem, result);
        }

        private void OnCalculateResultChanged(RunResult result)
        {
            SetResult(CalculateItem, result);
        }
        private void OnCheckFieldsResultChanged(RunResult result)
        {
            SetResult(CheckFieldsItem, result);

        }
        private void SetResult(MenuItemState state, RunResult result)
        {
            state.AdditionalText = result.IsOneOf(default, RunResult.NotRun) ? "Не выполнялось" : "Выполнено";
            state.CircleColor = result switch
            {
                RunResult.NotRun => new SolidColorBrush(Gray),
                RunResult.Success => new SolidColorBrush(Green),
                RunResult.WithErrors => new SolidColorBrush(Orange),
                _ => new SolidColorBrush(Gray)
            };
        }

        private void OnMatchedFieldsChanged((int all, int matched) vals)
        {
            MatchItem.AdditionalText = $"Сопоставленно {vals.matched} из {vals.all} полей";
        }

        public void NavigateClearFormats()
        {
            Navigate<ClearFormatsPage>(() =>
            {
                CorrectFormatsItem.IsActive = true;
            });
        }

        public void NavigateMatching()
        {
            Navigate<FieldsMatchingPage>(() => { MatchItem.IsActive = true; });
        }

        public void NavigateRegionMatch()
        {
            Navigate<RegionMatchPage>(() => { RegionMatchItem.IsActive = true; });
        }
        public void NavigateCalculate()
        {
            Navigate<CalculateFieldsPage>(() => CalculateItem.IsActive = true);
        }

        public void NavigateCheckFields() => Navigate<CheckFieldsPage>(() => CheckFieldsItem.IsActive = true);

        private void Navigate<T>(Action selector) where T : Page, new()
        {
            if (FilePath.IsNullOrEmpty()) return;

            UnselectMenuItems();
            selector();
            navigator.Navigate<T>();
        }

        private void UnselectMenuItems()
        {
            MatchItem.IsActive = false;
            CorrectFormatsItem.IsActive = false;
            RegionMatchItem.IsActive = false;
            CalculateItem.IsActive = false;
            CheckFieldsItem.IsActive = false;
        }

        private void AfterNavigate(Type newPage)
        {
            UnselectMenuItems();
            if (newPage == typeof(ClearFormatsPage)) CorrectFormatsItem.IsActive = true;
            if (newPage == typeof(FieldsMatchingPage)) MatchItem.IsActive = true;
            if (newPage == typeof(RegionMatchPage)) RegionMatchItem.IsActive = true;
            if (newPage == typeof(CalculateFieldsPage)) CalculateItem.IsActive = true;
            if (newPage == typeof(CheckFieldsPage)) CheckFieldsItem.IsActive = true;
        }

    }
}
