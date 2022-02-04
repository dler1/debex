using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Debex.Convert.Environment;

namespace Debex.Convert.Views.Windows
{
    /// <summary>
    /// Interaction logic for FieldsMatchingActions.xaml
    /// </summary>
    public partial class FieldsMatchingActions : Window
    {
        public FieldsMatchingActions()
        {
            InitializeComponent();

        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            Debug.WriteLine($"{Top} {Left}");
        }

        public DialogService.SelectFieldDialogResult Result { get; set; } =
            DialogService.SelectFieldDialogResult.Cancel;


        public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
            "CanSelect", typeof(bool), typeof(FieldsMatchingActions), new PropertyMetadata(default(bool)));

        public bool CanSelect
        {
            get => (bool)GetValue(CanSelectProperty);
            set => SetValue(CanSelectProperty, value);
        }

        public static readonly DependencyProperty CanRemoveProperty = DependencyProperty.Register(
            "CanRemove", typeof(bool), typeof(FieldsMatchingActions), new PropertyMetadata(default(bool)));

        public bool CanRemove
        {
            get => (bool)GetValue(CanRemoveProperty);
            set => SetValue(CanRemoveProperty, value);
        }
        private void OnSelect(object sender, RoutedEventArgs e)
        {

            Result = DialogService.SelectFieldDialogResult.Select;
            Close();
        }

        private void OnRemove(object sender, RoutedEventArgs e)
        {
            Result = DialogService.SelectFieldDialogResult.Remove;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {

            Result = DialogService.SelectFieldDialogResult.Cancel;
            Close();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CanSelect)
            {
                Result = DialogService.SelectFieldDialogResult.Select;
                Close();
            }
        }
        
    }
}
