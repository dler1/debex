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

namespace Debex.Convert.Views.Controls.SidebarControls
{
    /// <summary>
    /// Interaction logic for SidebarMenuItem.xaml
    /// </summary>
    public partial class SidebarMenuItem : UserControl
    {
        public static readonly DependencyProperty MainTextProperty = DependencyProperty.Register(nameof(MainText), typeof(string), typeof(SidebarMenuItem), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(SidebarMenuItem), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsChoosenProperty = DependencyProperty.Register(
            "IsChoosen", typeof(bool), typeof(SidebarMenuItem), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty AdditionalTextProperty = DependencyProperty.Register(
            "AdditionalText", typeof(string), typeof(SidebarMenuItem), new PropertyMetadata(default(string)));

        public event Action<object> Click;
        public string AdditionalText
        {
            get => (string)GetValue(AdditionalTextProperty);
            set => SetValue(AdditionalTextProperty, value);
        }

        public string MainText
        {
            get => (string)GetValue(MainTextProperty);
            set => SetValue(MainTextProperty, value);
        }

        public bool IsChoosen
        {
            get => (bool)GetValue(IsChoosenProperty);
            set => SetValue(IsChoosenProperty, value);
        }


        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty CircleColorProperty = DependencyProperty.Register(
            "CircleColor", typeof(SolidColorBrush), typeof(SidebarMenuItem), new PropertyMetadata(default(SolidColorBrush)));

        public SolidColorBrush CircleColor
        {
            get => (SolidColorBrush)GetValue(CircleColorProperty);
            set => SetValue(CircleColorProperty, value);
        }

        public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
            "CanSelect", typeof(bool), typeof(SidebarMenuItem), new PropertyMetadata(default(bool)));

        public bool CanSelect
        {
            get => (bool)GetValue(CanSelectProperty);
            set => SetValue(CanSelectProperty, value);
        }
        public SidebarMenuItem()
        {
            InitializeComponent();
        }

        private bool MouseWasDown;


        private void LeftDown(object sender, MouseButtonEventArgs e)
        {
            MouseWasDown = true;
        }

        private void LeftUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseWasDown) Click?.Invoke(this);
            MouseWasDown = false;
        }

        
        private void CheckboxClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        
    }
}
