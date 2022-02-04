using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class CleanFieldState : BaseVm, IHasErrors
    {
        public string Name { get => Get<string>(); set => Set(value); }
        public int Total { get => Get<int>(); set => Set(value); }
        public int Filled { get => Get<int>(); set => Set(value); }
        public int Empty { get => Get<int>(); set => Set(value); }
        public int Corrected { get => Get<int>(); set => Set(value); }
        public int Errors { get => Get<int>(); set => Set(value); }
    }

    public class CleanField: IHasErrors
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Filled { get; set; }
        public int Empty { get; set; }
        public int Corrected { get; set; }
        public int Errors { get; set; }
    }
}
