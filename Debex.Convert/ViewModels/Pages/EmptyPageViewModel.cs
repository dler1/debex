using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL;
using Debex.Convert.Environment;

namespace Debex.Convert.ViewModels.Pages
{
    public class EmptyPageViewModel: BaseVm
    {
        private readonly UiService uiService;
        public EmptyPageViewModel(Storage storage, UiService uiService) :base(storage)
        {
            this.uiService = uiService;
        }

        public bool IsLoading
        {
            get => Get<bool>();
            set => Set(value);
        }
        protected override void OnLoaded()
        {
            Subscribe<bool>(Storage.StorageKey.Loading, (v) => IsLoading = v);
            uiService.ChangeDataVisibility(false);
        }

        protected override void OnUnloaded()
        {
            uiService.ChangeDataVisibility(true);
        }
    }
}
