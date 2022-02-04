using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Debex.Convert.Views.Controls.FieldsMatchingPageControls
{
    /// <summary>
    /// Interaction logic for FileField.xaml
    /// </summary>
    public partial class FileField : UserControl
    {
        public FileField()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            "FieldName", typeof(string), typeof(FileField), new PropertyMetadata(default(string)));

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(FileField), new PropertyMetadata(default(bool)));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsMatchedProperty = DependencyProperty.Register(
            "IsMatched", typeof(bool), typeof(FileField), new PropertyMetadata(default(bool)));

        public bool IsMatched
        {
            get => (bool)GetValue(IsMatchedProperty);
            set => SetValue(IsMatchedProperty, value);
        }

        public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.Register(
            "IsHighlighted", typeof(bool), typeof(FileField), new PropertyMetadata(default(bool)));

        public bool IsHighlighted
        {
            get => (bool)GetValue(IsHighlightedProperty);
            set => SetValue(IsHighlightedProperty, value);
        }

        public event Action<string> Click;

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(FieldName);
        }
    }
}