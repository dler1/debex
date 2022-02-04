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

namespace Debex.Convert.Views.Controls.FieldsMatchingPageControls
{
    /// <summary>
    /// Interaction logic for MatchingField.xaml
    /// </summary>
    public partial class MatchingField : UserControl
    {
        public MatchingField()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FieldTextProperty = DependencyProperty.Register(
            "FieldText", typeof(string), typeof(MatchingField), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty MatchedFieldTextProperty = DependencyProperty.Register(
            "MatchedFieldText", typeof(string), typeof(MatchingField), new PropertyMetadata(default(string)));

        public string FieldText
        {
            get => (string)GetValue(FieldTextProperty);
            set => SetValue(FieldTextProperty, value);
        }

        public string MatchedFieldText
        {
            get => (string)GetValue(MatchedFieldTextProperty);
            set => SetValue(MatchedFieldTextProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(MatchingField), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty RequiredProperty = DependencyProperty.Register(
            "Required", typeof(bool), typeof(MatchingField), new PropertyMetadata(default(bool)));

        public bool Required
        {
            get => (bool) GetValue(RequiredProperty);
            set => SetValue(RequiredProperty, value);
        }
        
        public event Action<string> Click;
        public event Action<string> ChangeClick;

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(FieldText);
        }

        private void OnChangeClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnChangeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ChangeClick?.Invoke(FieldText);
        }
    }
}
