using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Debex.Convert.Views.Controls.SidebarControls
{
    /// <summary>
    /// Interaction logic for OpenFileButton.xaml
    /// </summary>
    public partial class OpenFileButton : UserControl
    {
        public event Action Click;
        
        public OpenFileButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            "FilePath", typeof(string), typeof(OpenFileButton), new PropertyMetadata(default(string)));

        public string FilePath
        {
            get => (string) GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke();
        }
    }
}
