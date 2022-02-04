using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Debex.Convert.Views.Controls.FieldsMatchingPageControls
{
    /// <summary>
    /// Interaction logic for FieldsList.xaml
    /// </summary>
    public partial class FieldsList : UserControl
    {
        public FieldsList()
        {
            InitializeComponent();
        }
        
        public static readonly DependencyProperty FieldsToMatchProperty = DependencyProperty.Register(
            "FieldsToMatch", typeof(ObservableCollection<BaseFieldToMatch>), typeof(FieldsList), new PropertyMetadata(default(ObservableCollection<BaseFieldToMatch>)));

        public ObservableCollection<BaseFieldToMatch> FieldsToMatch
        {
            get => (ObservableCollection<BaseFieldToMatch>) GetValue(FieldsToMatchProperty);
            set => SetValue(FieldsToMatchProperty, value);
        }

        public event Action<string> FieldSelected;
        public event Action<string> OnFieldChange; 

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(FieldsList), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        
        private void OnFieldSelected(string obj)
        {
            FieldSelected?.Invoke(obj);
        }

        private void OnFieldChangeClick(string obj)
        {
            OnFieldChange?.Invoke(obj);
        }
    }
}
