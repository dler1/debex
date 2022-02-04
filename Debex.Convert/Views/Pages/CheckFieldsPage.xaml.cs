using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Debex.Convert.ViewModels.Pages;

namespace Debex.Convert.Views.Pages
{
    /// <summary>
    /// Interaction logic for CheckFieldsPage.xaml
    /// </summary>
    public partial class CheckFieldsPage : Page
    {
        [HasViewModel] private CheckFieldsViewModel vm;

        public CheckFieldsPage()
        {
            InitializeComponent();
        }
        private async void RefreshClick(object sender, MouseButtonEventArgs e)
        {
            await vm.Check();
        }
        private void OnErrorsClick(object sender, MouseButtonEventArgs e)
        {
            var col = GetColumnByClick(sender);
            if (col.IsNullOrEmpty()) return;
            vm.SortByErrors(col);
        }
        private string GetColumnByClick(object sender)
        {
            var field = sender as FrameworkElement;
            if (field == null) return string.Empty;

            var stateField = field.DataContext as CheckFieldState;
            if (stateField == null) return string.Empty;
            if (!stateField.HasData) return string.Empty;

            var currentCol = stateField.Name;
            return currentCol;
        }

    }
}
