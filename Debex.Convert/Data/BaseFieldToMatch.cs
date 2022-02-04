using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class BaseFieldToMatch : BaseVm, IHasId  
    {
        public int Id { get => Get<int>(); set => Set(value); }
        public string Name { get => Get<string>(); set => Set(value); }
        public bool IsMatched { get => Get<bool>(); set => Set(value); }
        public string MatchedName { get => Get<string>(); set => Set(value); }
        public BaseField.BaseFieldType Type { get => Get<BaseField.BaseFieldType>(); set => Set(value); }
        public bool IsSelected
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool Required
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool RegionCheck
        {
            get => Get<bool>();
            set => Set(value);
        }

    }
}
