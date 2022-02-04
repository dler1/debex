using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Extensions;

namespace Debex.Convert.BL
{
    public class Storage
    {
        public enum StorageKey
        {
            FilePath,
            Loading,

            ConfigurationMatchFile,
            BaseFields,
            BaseFieldsToMatch,
            FileFieldsToMatch,
            FieldsMatchState,
            MatchedFields,
            BaseFieldsToMatchMapping,
            ClearedFieldsState,
            ClearedFieldsResult,

            RegionMatch,
            RegionMatchResult,
            
            CalculatedFields,
            CalculateFieldsResult,
            
            CheckFields,
            CheckFieldsResult,
            
            Excel,
            Processing,

            
            
            SortByColumnName,
        }
        
        private readonly Dictionary<StorageKey, object> values = new();
        private readonly Dictionary<StorageKey, List<SubscriberInfo>> subscribers = new();



        public T Subscribe<T>(StorageKey key, object subscriber, Action<T> onChange)
        {
            var info = new SubscriberInfo(subscriber, onChange);
            if (subscribers.ContainsKey(key)) subscribers[key].Add(info);
            else subscribers[key] = new() { info };
            var currentValue = GetValue<T>(key);
            onChange(currentValue);
            return currentValue;
        }

        public void Unsubscribe(StorageKey key, object subscriber)
        {
            var target = subscribers[key];
            if (target.IsNullOrEmpty()) return;

            var targetSubscriber = target.FirstOrDefault(x => x.Subscriber == subscriber);
            if (targetSubscriber == null) return;

            target.Remove(targetSubscriber);
        }

        public T GetValue<T>(StorageKey key)
        {
            if (!values.ContainsKey(key)) return default;
            return (T)values[key];
        }

        public void SetValue<T>(StorageKey key, T value)
        {
            values[key] = value;
            if (!subscribers.ContainsKey(key)) return;
            var toNotify = subscribers[key];

            foreach (var subscriberInfo in toNotify)
            {
                var act = (Action<T>)subscriberInfo.OnChange;
                act(value);
            }
        }




    }

    record SubscriberInfo(object Subscriber, object OnChange);
}
