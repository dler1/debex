using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class RegionMatchState : BaseVm
    {
        public string Name { get => Get<string>(); set => Set(value); }
        public int Total
        {
            get => Get<int>();
            set => Set(value);
        }

        public int Filled
        {
            get => Get<int>();
            set => Set(value);
        }

        public int Found
        {
            get => Get<int>();
            set => Set(value);
        }

        public int NotFound
        {
            get => Get<int>();
            set => Set(value);
        }
    }

    public class RegionMatch
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Filled { get; set; }
        public int Found { get; set; }
        public int NotFound => Filled - Found;
    }
}
