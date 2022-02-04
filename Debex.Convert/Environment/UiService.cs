using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Environment
{
    public class UiService
    {
        public event Action OnDataHide;
        public event Action OnDataShow;
        public event Action OnLoadingShow;
        public event Action OnLoadingHide;

        public void ChangeDataVisibility(bool isVisible)
        {
            if (isVisible) OnDataShow?.Invoke();
            else OnDataHide?.Invoke();
        }
        public void ChangeLoadingVisibility(bool isVisible)
        {
            if (isVisible) OnLoadingShow?.Invoke();
            else OnLoadingHide?.Invoke(); 
        }
    }
}
