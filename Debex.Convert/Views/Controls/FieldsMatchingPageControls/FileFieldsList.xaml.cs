using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Debex.Convert.Data;

namespace Debex.Convert.Views.Controls.FieldsMatchingPageControls
{
    /// <summary>
    /// Interaction logic for FileFieldsList.xaml
    /// </summary>
    public partial class FileFieldsList : UserControl
    {
        public static readonly DependencyProperty FileFieldsProperty = DependencyProperty.Register(
            "FileFields", typeof(ObservableCollection<FileFieldToMatch>), typeof(FileFieldsList), new PropertyMetadata(default(ObservableCollection<FileFieldToMatch>)));

        public ObservableCollection<FileFieldToMatch> FileFields
        {
            get => (ObservableCollection<FileFieldToMatch>)GetValue(FileFieldsProperty);
            set => SetValue(FileFieldsProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(FileFieldsList), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public FileFieldsList()
        {
            InitializeComponent();
        }

        public event Action<string> FieldSelected;

        private void FileFieldClick(string obj)
        {
            FieldSelected?.Invoke(obj);
        }
    }
}