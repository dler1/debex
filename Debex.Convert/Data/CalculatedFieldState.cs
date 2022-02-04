using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class CalculatedFieldState : BaseVm, IHasErrors
    {

        public string Name { get => Get<string>(); set => Set(value); }
        public string ColumnName { get => Get<string>(); set => Set(value); }
        public int Total { get => Get<int>(); set => Set(value); }
        public int Empty { get => Get<int>(); set => Set(value); }
        public int Added { get => Get<int>(); set => Set(value); }
        public int Errors { get => Get<int>(); set => Set(value); }

        public bool HasDetailedErrors { get => Get<bool>(); set => Set(value); }
        public int MinErrors { get => Get<int>(); set => Set(value); }
        public int MaxErrors { get => Get<int>(); set => Set(value); }
        public int AvgErrors { get => Get<int>(); set => Set(value); }
    }

    public class CalculatedField : IHasErrors
    {
        public string ColumnName { get; set; }
        public string Name { get; set; }
        public int Total { get; set; }
        public int Empty { get; set; }
        public int Added { get; set; }
        public int Errors { get; set; }

        public bool HasDetailedErrors { get; set; }
        public double MinErrors { get; set; }
        public double MaxErrors { get; set; }
        public double AvgErrors { get; set; }
    }
}