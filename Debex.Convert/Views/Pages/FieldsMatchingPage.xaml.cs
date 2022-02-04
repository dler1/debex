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
using Debex.Convert.Environment;
using Debex.Convert.ViewModels.Pages;

namespace Debex.Convert.Views.Pages
{
    /// <summary>
    /// Interaction logic for FieldsMatchingPage.xaml
    /// </summary>
    public partial class FieldsMatchingPage : Page
    {
        [HasViewModel] private FieldsMatchingViewModel vm;
        public FieldsMatchingPage()
        {
            InitializeComponent();

        }


        private void FileFieldSelected(string obj)
        {
            vm.SelectFileField(obj);
        }

        private void BaseFieldSelected(string obj)
        {
            vm.SelectBaseField(obj);
        }

        private void SaveAsClick(object sender, MouseButtonEventArgs e)
        {

            vm.SaveConfigurationAs();
        }

        private void OpenConfigurationClick(object sender, MouseButtonEventArgs e)
        {
            vm.OpenConfiguration();
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            vm.SaveConfiguration();
        }

        private void OnFieldChange(string obj)
        {
            vm.ChangeField(obj);
        }

        private async void MatchAuto(object sender, MouseButtonEventArgs e)
        {
           await vm.MatchAuto();
        }
    }
}
