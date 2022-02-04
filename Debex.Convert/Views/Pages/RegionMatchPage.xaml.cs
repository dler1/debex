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
    /// Interaction logic for RegionMatchPage.xaml
    /// </summary>
    public partial class RegionMatchPage : Page
    {
        [HasViewModel]
        private RegionMatchViewModel vm;
        public RegionMatchPage()
        {
            InitializeComponent();
        }

        private async void RefreshClick(object sender, MouseButtonEventArgs e)
        {
            await vm.Process();
        }

        private void OnFoundClick(object sender, MouseButtonEventArgs e)
        {
            var col = GetColumnByClick(sender);
            if (col.IsNullOrEmpty()) return;

            vm.SortMatched(col);
        }

        private void OnNotFoundClick(object sender, MouseButtonEventArgs e)
        {
            var col = GetColumnByClick(sender);
            if(col.IsNullOrEmpty())return;
            
            vm.SortUnMatched(col);
        }

        private string GetColumnByClick(object sender)
        {
            var field = sender as FrameworkElement;
            if (field == null) return string.Empty;

            var stateField = field.DataContext as RegionMatchState;
            if (stateField == null) return string.Empty;


            var currentCol = stateField.Name;
            return currentCol;
        }
    }
}
