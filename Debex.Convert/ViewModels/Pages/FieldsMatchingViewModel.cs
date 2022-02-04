using Debex.Convert.BL;
using Debex.Convert.BL.FieldsMatching;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace Debex.Convert.ViewModels.Pages
{
    public class FieldsMatchingViewModel : BaseVm
    {
        private readonly DialogService dialogService;
        private readonly UiService uiService;
        private readonly AutomaticFieldsMatcher automaticFieldsMatcher;

        public FieldsMatchingViewModel(Storage storage, DialogService dialogService, UiService uiService, AutomaticFieldsMatcher automaticFieldsMatcher) : base(storage)
        {
            BaseFields = new ObservableCollection<BaseFieldToMatch>();
            FileFields = new ObservableCollection<FileFieldToMatch>();
            IsEnabled = true;
            this.dialogService = dialogService;
            this.uiService = uiService;
            this.automaticFieldsMatcher = automaticFieldsMatcher;
        }

        public ObservableCollection<BaseFieldToMatch> BaseFields { get => Get<ObservableCollection<BaseFieldToMatch>>(); set => Set(value); }
        public ObservableCollection<FileFieldToMatch> FileFields { get => Get<ObservableCollection<FileFieldToMatch>>(); set => Set(value); }

        public bool IsFileSelected
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsBaseFieldSelected
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsEdited
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string ConfigurationFilePath
        {
            get => Get<string>();
            set => Set(value);
        }

        public void SaveConfigurationAs()
        {
            var file = dialogService.SelectCfgToSave(Storage.GetValue<string>(Storage.StorageKey.FilePath));
            if (file.IsNullOrEmpty()) return;

            ConfigurationFilePath = file;
            Storage.SetValue<string>(Storage.StorageKey.ConfigurationMatchFile, ConfigurationFilePath);

            var json = BaseFields.ToJson();
            File.WriteAllText(file, json);
            IsEdited = false;

            MessageBox.Show("Изменения успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Asterisk);

        }

        public void SaveConfiguration()
        {
            if (ConfigurationFilePath.IsNullOrEmpty()) return;

            var json = BaseFields.ToJson();
            File.WriteAllText(ConfigurationFilePath, json);
            IsEdited = false;
            MessageBox.Show("Изменения успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        public void OpenConfiguration()
        {
            var file = dialogService.SelectCfgToOpen();
            if (file == null) return;

            ConfigurationFilePath = file;
            Storage.SetValue<string>(Storage.StorageKey.ConfigurationMatchFile, ConfigurationFilePath);


            var fields = File.ReadAllText(ConfigurationFilePath).FromJson<List<BaseFieldToMatch>>();

            foreach (var bf in BaseFields)
            {
                bf.IsMatched = false;
                bf.MatchedName = null;
            }

            foreach (var ff in FileFields)
            {
                ff.IsMatched = false;
                ff.MatchedName = null;
            }

            fields.ForEach(f =>
            {
                var fileField = FileFields.FirstOrDefault(x => x.Name == f.MatchedName);
                var baseField = BaseFields.FirstOrDefault(x => x.Name == f.Name);

                if (baseField == null) return;

                if (fileField != null)
                {
                    fileField.IsMatched = f.IsMatched;
                    fileField.MatchedName = f.Name;

                    baseField.MatchedName = f.MatchedName;
                    baseField.IsMatched = f.IsMatched;
                }

            });

            SetMatchedState();
            IsEdited = false;
        }


        public async Task MatchAuto()
        {
            Storage.SetValue(Storage.StorageKey.Processing, true);
            uiService.ChangeLoadingVisibility(true);
            IsEnabled = false;

            try
            {

                await Task.Run(automaticFieldsMatcher.Init);
                automaticFieldsMatcher.Match(BaseFields, FileFields);

                BaseFields = new(BaseFields.OrderBy(x => x.IsMatched));
                FileFields = new(FileFields.OrderBy(x => x.IsMatched));

            }
            catch (Exception e)
            {
                MessageBox.Show("Произошла непредвиденная ошибка с источником данных. Обратитесь в службу поддержки. Код ошибки 509");
            }

            IsEnabled = true;
            uiService.ChangeLoadingVisibility(false);
            uiService.ChangeDataVisibility(true);
            Storage.SetValue(Storage.StorageKey.Processing, false);
        }

        public void SelectBaseField(string fieldName)
        {
            var baseField = BaseFields.First(x => x.Name == fieldName);

            if (IsBaseFieldSelected)
            {
                baseField.IsSelected = false;
                IsBaseFieldSelected = false;
                foreach (var field in BaseFields) field.IsSelected = false;
                return;
            }

            if (IsFileSelected)
            {
                var fileField = FileFields.First(x => x.IsSelected);

                var dialogResult = dialogService.SelectField(true, fileField.IsMatched);
                if (dialogResult == DialogService.SelectFieldDialogResult.Cancel)
                {
                    fileField.IsHighlighted = false;
                    return;
                }

                Unselect(baseField, fileField);

                if (dialogResult == DialogService.SelectFieldDialogResult.Remove) Remove(fileField);
                else Match(fileField.Name, baseField.Name);

                return;
            }

            baseField.IsSelected = true;
            IsBaseFieldSelected = true;
        }

        public void SelectFileField(string fileField)
        {
            var targetFileField = FileFields.First(x => x.Name == fileField);

            if (IsFileSelected)
            {
                IsFileSelected = false;
                targetFileField.IsSelected = false;
                targetFileField.IsHighlighted = false;
                foreach (var field in FileFields) field.IsSelected = false;
                return;
            }

            if (IsBaseFieldSelected)
            {

                var baseField = BaseFields.First(x => x.IsSelected);
                targetFileField.IsHighlighted = true;
                var dialogResult = dialogService.SelectField(true, baseField.IsMatched);

                if (dialogResult == DialogService.SelectFieldDialogResult.Cancel)
                {
                    targetFileField.IsHighlighted = false;
                    return;
                }

                Unselect(baseField, targetFileField);

                if (dialogResult == DialogService.SelectFieldDialogResult.Remove) Remove(baseField);
                else Match(targetFileField.Name, baseField.Name);

                return;
            }


            targetFileField.IsSelected = true;
            IsFileSelected = true;
            IsEdited = false;
        }

        public void ChangeField(string fieldName)
        {
            var field = BaseFields.FirstOrDefault(x => x.Name == fieldName);
            if (field == null) return;

            var dialog = dialogService.SelectField(false, true);
            if (dialog != DialogService.SelectFieldDialogResult.Remove) return;

            Remove(field);
        }

        private void Remove(FileFieldToMatch fileField)
        {
            fileField.IsMatched = false;
            fileField.MatchedName = null;

            var baseField = BaseFields.FirstOrDefault(x => x.MatchedName == fileField.Name);
            if (baseField == null) return;

            baseField.IsMatched = false;
            baseField.MatchedName = null;

            SetMatchedState();
            Unselect(baseField, fileField);
        }

        private void Remove(BaseFieldToMatch baseField)
        {
            baseField.IsMatched = false;
            baseField.MatchedName = null;

            var fileField = FileFields.FirstOrDefault(x => x.MatchedName == baseField.Name);
            if (fileField == null) return;

            fileField.MatchedName = null;
            fileField.IsMatched = false;

            SetMatchedState();
            Unselect(baseField, fileField);

        }

        private void Unselect(BaseFieldToMatch baseField, FileFieldToMatch fileField)
        {
            baseField.IsSelected = false;
            fileField.IsSelected = false;
            IsBaseFieldSelected = false;
            IsFileSelected = false;
            fileField.IsHighlighted = false;
        }

        private void Match(string fileFieldName, string baseFieldName)
        {
            var fileField = FileFields.First(x => x.Name == fileFieldName);
            var baseField = BaseFields.First(x => x.Name == baseFieldName);

            BaseFields.Where(x => x.MatchedName == fileField.Name).ToList().ForEach(x =>
            {
                x.MatchedName = null;
                x.IsMatched = false;
            });

            FileFields.Where(x => x.MatchedName == baseField.Name).ToList().ForEach(x =>
            {
                x.MatchedName = null;
                x.IsMatched = false;
            });

            fileField.IsMatched = true;
            baseField.IsMatched = true;

            baseField.MatchedName = fileField.Name;
            fileField.MatchedName = baseField.Name;

            SetMatchedState();

        }

        private void SetMatchedState()
        {
            IsEdited = true;

            var allFields = BaseFields.Count;
            var matchedFields = BaseFields.Count(x => x.IsMatched);

            Storage.SetValue(Storage.StorageKey.MatchedFields, (allFields, matchedFields));

            if (allFields == matchedFields) Storage.SetValue(Storage.StorageKey.FieldsMatchState, Consts.FieldsMatchState.All);
            else if (matchedFields > 0) Storage.SetValue(Storage.StorageKey.FieldsMatchState, Consts.FieldsMatchState.Partial);
            else Storage.SetValue(Storage.StorageKey.FieldsMatchState, Consts.FieldsMatchState.None);

            Storage.SetValue(Storage.StorageKey.BaseFieldsToMatch, BaseFields.Map<List<BaseFieldToMatch>>());
        }

        protected override void OnLoaded()
        {
            InitFields();

            ConfigurationFilePath = Storage.GetValue<string>(Storage.StorageKey.ConfigurationMatchFile);

            uiService.ChangeDataVisibility(true);
        }

        protected override void OnUnloaded()
        {
            Storage.SetValue(Storage.StorageKey.BaseFieldsToMatch, BaseFields.Map<List<BaseFieldToMatch>>());

            if (IsEdited && dialogService.SaveBeforeLeave())
            {
                if (ConfigurationFilePath.IsNullOrEmpty()) SaveConfigurationAs();
                else SaveConfiguration();
            }

            if (BaseFields.Any(x => x.Required && !x.IsMatched)) dialogService.NotifyRequiredFieldsNotFilled();


        }

        private void InitFields()
        {
            var baseFields = Storage.GetValue<List<BaseFieldToMatch>>(Storage.StorageKey.BaseFieldsToMatch) ?? new List<BaseFieldToMatch>();
            var fileFields = Storage.GetValue<List<FileFieldToMatch>>(Storage.StorageKey.FileFieldsToMatch) ?? new List<FileFieldToMatch>();

            BaseFields = new ObservableCollection<BaseFieldToMatch>(baseFields.Map<List<BaseFieldToMatch>>());
            FileFields = new ObservableCollection<FileFieldToMatch>(fileFields.Map<List<FileFieldToMatch>>());

            CorrectFileFields();
            SetMatchedState();

            IsEdited = false;

        }

        private void CorrectFileFields()
        {
            if (BaseFields.Count == 0 || FileFields.Count == 0) return;

            foreach (var baseField in BaseFields)
            {
                var fileField = FileFields.FirstOrDefault(x => x.Name == baseField.MatchedName);
                if (fileField != null)
                {
                    fileField.IsMatched = baseField.IsMatched;
                    fileField.MatchedName = baseField.Name;
                }
                else
                {
                    baseField.MatchedName = null;
                    baseField.IsMatched = false;
                }

            }
        }

    }
}