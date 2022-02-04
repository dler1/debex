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
    /// Interaction logic for EmptyPage.xaml
    /// </summary>
    public partial class EmptyPage : Page
    {
        [HasViewModel] private EmptyPageViewModel vm;
        public EmptyPage()
        {
            InitializeComponent();
        }
    }
}
