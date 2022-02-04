using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Debex.Convert.BL;
using Debex.Convert.Environment;
using Microsoft.Extensions.DependencyInjection;

namespace Debex.Convert.ViewModels
{
    public class BaseVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> values = new();
        private readonly List<Storage.StorageKey> keys = new();

        protected readonly Storage Storage;
        public BaseVm(Storage storage)
        {
            Storage = storage;
        }

        public BaseVm()
        {
            Storage = IoC.Get<Storage>();
        }

        protected void Set<T>(T value, [CallerMemberName] string property = null)
        {
            values[property] = value;
            OnPropertyChanged(property);
        }

        protected T Get<T>([CallerMemberName] string property = null)
        {
            if (values.ContainsKey(property)) return (T)values[property];
            return default;
        }

        public virtual void Initialize()
        {

        }

        protected virtual void OnLoaded()
        {

        }

        protected virtual void OnUnloaded()
        {
        }

        public void Loaded(object sender, RoutedEventArgs e) { OnLoaded(); }

        public void Unloaded(object sender, RoutedEventArgs e)
        {

            foreach (var key in keys.ToList()) Unsubscribe(key);
            OnUnloaded();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(T value, out T oldValue, [CallerMemberName] string propertyName = null)
        {
            oldValue = value;
            OnPropertyChanged(propertyName);
        }





        protected void Subscribe<T>(Storage.StorageKey key, Action<T> act)
        {
            keys.Add(key);
            Storage.Subscribe(key, this, act);
        }

        protected void Unsubscribe(Storage.StorageKey key)
        {
            Storage.Unsubscribe(key, this);
            keys.Remove(key);
        }
    }
}
