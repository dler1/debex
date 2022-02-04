using Debex.Convert.BL;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Debex.Convert.Converters
{
    public record ExcelValueParameter(Dictionary<string, BaseField.BaseFieldType> Mapping, string PropertyName);

    public class ExcelValueToGridViewConverter : IValueConverter
    {
        private readonly Storage storage;
        public static ExcelValueToGridViewConverter Instance { get; } = new();

        public ExcelValueToGridViewConverter()
        {
            storage = IoC.Get<Storage>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date) return date.ToString("dd.MM.yyyy");

            if (parameter is not string propName) return value;
            var mapper =
                storage.GetValue<Dictionary<string, BaseField.BaseFieldType>>(Storage.StorageKey.BaseFieldsToMatchMapping);
            if (mapper == null) return value;

            if (!mapper.TryGetValue(propName, out var type)) return value;

            if (type == BaseField.BaseFieldType.Decimal)
            {
                return value is decimal d ? d.ToString(new NumberFormatInfo { NumberDecimalSeparator = "," }) : value;
            }

            if (type.IsOneOf(BaseField.BaseFieldType.Int, BaseField.BaseFieldType.String)) return value;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
