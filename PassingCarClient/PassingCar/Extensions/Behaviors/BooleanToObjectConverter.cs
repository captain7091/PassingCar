using System;
using System.Globalization;


namespace PassingCar.Extensions.Behaviors
{
    internal class BooleanToObjectConverter<T> : IValueConverter
    {
        public T? FalseObject { set; get; }

        public T? TrueObject { set; get; }

        public object? Convert(object? value, Type targetType,
                              object? parameter, CultureInfo culture)
        {
            if (value == null) return FalseObject;
            return (bool)value ? TrueObject : FalseObject;
        }

        public object? ConvertBack(object? value, Type targetType,
                                  object? parameter, CultureInfo culture)
        {
            if (value == null) return false;
            return ((T)value).Equals(TrueObject);
        }
    }
}
