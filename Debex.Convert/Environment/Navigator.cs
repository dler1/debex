using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Debex.Convert.ViewModels;

namespace Debex.Convert.Environment
{
    public class Navigator
    {
        private NavigationService navigationService;

        public event Func<Type, Type, bool> BeforeNavigate;
        public event Action<Type> AfterNavigate;

        public Type CurrentPage { get; private set; }

        public void SetFrame(Frame frame)
        {
            this.navigationService = frame.NavigationService;
        }
        
        
        public void Navigate<T>() where T : Page, new()
        {
            if (navigationService.Content?.GetType() == typeof(T)) return;

            var shouldNavigate = BeforeNavigate?.Invoke(navigationService.Content?.GetType(), typeof(T)) ?? true;
            if (!shouldNavigate) return;

            var view = new T();

            var vmField = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.GetCustomAttribute(typeof(HasViewModelAttribute)) != null);

            if (vmField != null)
            {
                var vm = (BaseVm)IoC.Get(vmField.FieldType);
                vmField.SetValue(view, vm);
                view.DataContext = vm;

                if (vm != null)
                {
                    vm.Initialize();
                    view.Loaded += vm.Loaded;
                    view.Unloaded += vm.Unloaded;

                }
            }

            navigationService.Navigate(view);
            navigationService.RemoveBackEntry();
            CurrentPage = typeof(T);
            AfterNavigate?.Invoke(typeof(T));

        }
    }
}
