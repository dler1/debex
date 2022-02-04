using Debex.Convert.BL.FieldsChecking;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Data
{
    public class CheckField : IHasErrors
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public int Total { get; set; }
        public int Filled { get; set; }
        public int Empty { get; set; }
        public int Correct { get; set; }
        public int Errors { get; set; }

       
        public bool HasData { get; set; }

        public RepeatOutputTable OutputTable { get; set; }
    }

    public class CheckFieldState : BaseVm, IHasErrors
    {
        public string Name { get => Get<string>(); set => Set(value); }
        public string ColumnName { get => Get<string>(); set => Set(value); }
        public int Total { get => Get<int>(); set => Set(value); }
        public int Filled { get => Get<int>(); set => Set(value); }
        public int Empty { get => Get<int>(); set => Set(value); }
        public int Correct { get => Get<int>(); set => Set(value); }
        public int Errors { get => Get<int>(); set => Set(value); }

        

        public bool HasData
        {
            get => Get<bool>();
            set => Set(value);
        }

        public RepeatOutputTable OutputTable { get; set; }
    }
}