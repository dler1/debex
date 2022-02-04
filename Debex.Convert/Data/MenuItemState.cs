using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class MenuItemState : BaseVm
    {
        public string MainText
        {
            get => Get<string>();
            set => Set(value);
        }

        public bool IsSelected
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool CanSelect
        {
            get => Get<bool>();
            set => Set(value);
        }
        public bool IsActive
        {
            get => Get<bool>(); set => Set(value);
        }

        public string AdditionalText
        {
            get => Get<string>();
            set => Set(value);
        }

        public SolidColorBrush CircleColor
        {
            get => Get<SolidColorBrush>();
            set => Set(value);
        }
    }
}
