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
using Debex.Convert.ViewModels.Controls;

namespace Debex.Convert.Views.Controls
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        [HasViewModel]
        private SidebarViewModel vm;

        public event Action SelectFile;
        public Sidebar()
        {
            DataContext = vm = IoC.Get<SidebarViewModel>();

            Loaded += vm.Loaded;
            Unloaded += vm.Unloaded;

            InitializeComponent();

            vm.Initialize();
        }

        private void OnSelectFileClick()
        {
            SelectFile?.Invoke();
        }

        public static readonly DependencyProperty SelectedFileProperty = DependencyProperty.Register(
            "SelectedFile", typeof(string), typeof(Sidebar), new PropertyMetadata(default(string)));

        public string SelectedFile
        {
            get => (string)GetValue(SelectedFileProperty);
            set => SetValue(SelectedFileProperty, value);
        }

        private void NavigateClearFormats(object sender)
        {
            vm.NavigateClearFormats();
        }

        private void NavigateMatch(object sender)
        {

            vm.NavigateMatching();
        }

        private void NavigateRegionMatch(object sender)
        {
            vm.NavigateRegionMatch();
        }

        private void NavigateCalculate(object sender)
        {
            vm.NavigateCalculate();
        }

        private void NavigateToCheckFields(object obj)
        {
            vm.NavigateCheckFields();

        }

        private async void RunAll(object sender, RoutedEventArgs e)
        {
            await vm.RunAll();
        }

        private async void SaveClick(object sender, RoutedEventArgs e)
        {
           await vm.Save();
        }
    }
}
