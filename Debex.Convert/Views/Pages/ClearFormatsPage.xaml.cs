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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Debex.Convert.ViewModels.Pages;

namespace Debex.Convert.Views.Pages
{
    /// <summary>
    /// Interaction logic for ClearFormatsPage.xaml
    /// </summary>
    public partial class ClearFormatsPage : Page
    {
        [HasViewModel]
        private ClearFormatsViewModel vm;

        public ClearFormatsPage()
        {
            InitializeComponent();
        }


        private void RefreshClick(object sender, MouseButtonEventArgs e)
        {
            vm.Process();
        }

        private void OnErrorClick(object sender, MouseButtonEventArgs e)
        {
            var currentCol = GetColumnByClick(sender);
            if (currentCol.IsNullOrEmpty()) return;
            vm.ErrorSort(currentCol);
        }

        private void OnCorrectedClick(object sender, MouseButtonEventArgs e)
        {
            var col = GetColumnByClick(sender);
            if(col.IsNullOrEmpty()) return;
            vm.CorrectedSort(col);

        }

        private string GetColumnByClick(object sender)
        {
            var field = sender as FrameworkElement;
            if (field == null) return string.Empty;

            var stateField = field.DataContext as CleanFieldState;
            if (stateField == null) return string.Empty;


            var currentCol = stateField.Name;
            return currentCol;
        }
    }
}
