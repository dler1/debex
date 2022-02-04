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
    /// Interaction logic for CalculateFieldsPage.xaml
    /// </summary>
    public partial class CalculateFieldsPage : Page
    {
        [HasViewModel]
        private CalculatedFieldsViewModel vm;
        public CalculateFieldsPage()
        {
            InitializeComponent();
        }

        private async void RefreshClick(object sender, MouseButtonEventArgs e)
        {
            await vm.Calculate();
        }

        private void OnErrorsClick(object sender, MouseButtonEventArgs e)
        {
            var col = GetColumnByClick(sender);
            if(col.IsNullOrEmpty()) return;

            vm.SortByErrors(col);
        }

        private void OnCalculatedClick(object sender, MouseButtonEventArgs e)
        {
        }

        private string GetColumnByClick(object sender)
        {
            var field = sender as FrameworkElement;
            if (field == null) return string.Empty;

            var stateField = field.DataContext as CalculatedFieldState;
            if (stateField == null) return string.Empty;


            var currentCol = stateField.Name;
            return currentCol;
        }
    }
}
