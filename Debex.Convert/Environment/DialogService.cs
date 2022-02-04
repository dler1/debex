using Debex.Convert.Views.Windows;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Path = System.IO.Path;

namespace Debex.Convert.Environment
{
    public class DialogService
    {
        public string SelectFile(string extension = "Excel|*.xlsx")
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = extension;

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        public string SelectFileToSave(string extension = "Excel|*.xlsx")
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = extension;

            if (dialog.ShowDialog() == true) return dialog.FileName;
            return null;
        }
        public string SelectCfgToSave(string fileName)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "DebexSettings|*.debex";
            dialog.FileName = Path.GetFileNameWithoutExtension(fileName) + "-" + DateTime.Now.ToString("dd.MM.yyyy");
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string SelectCfgToOpen()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "DebexSettings|*.debex";
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public enum SelectFieldDialogResult
        {
            Cancel,
            Select,
            Remove
        }

        public SelectFieldDialogResult SelectField(bool canSelect = true, bool canRemove = true)
        {

            var dialog = new FieldsMatchingActions();
            Point mousePositionInApp = Mouse.GetPosition(Application.Current.MainWindow);
            Point mousePositionInScreenCoordinates = Application.Current.MainWindow.PointToScreen(mousePositionInApp);
            dialog.Owner = Application.Current.MainWindow;
            dialog.Top = mousePositionInScreenCoordinates.Y - 40;
            dialog.Left = mousePositionInScreenCoordinates.X - 40;
            dialog.CanRemove = canRemove;
            dialog.CanSelect = canSelect;
            dialog.ShowDialog();
            return dialog.Result;
        }

        public bool SaveBeforeLeave()
        {

            return MessageBox.Show("Конфигурация не сохранена. Сохранить конфигурацию?", "Сохранение",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes;

        }

        public bool NotifyRequiredFieldsNotFilled()
        {
            return MessageBox.Show(
                "Вы не сопоставили все обязательные поля. Не забудье сопоставить их перед запуском, они выделены красным цветом",
                "Внимание",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation) == MessageBoxResult.OK;
        }
    }
}
