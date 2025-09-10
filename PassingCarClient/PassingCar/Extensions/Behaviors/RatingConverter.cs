using System;


namespace PassingCar.Extensions.Behaviors
{
    internal class RatingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            int rating = (int)value;
            return rating == 1
                ? "Disappointed!"
                : rating == 2
                ? "Not a fan!"
                : rating == 3 ? "It's Ok!" : rating == 4 ? "Like it!" : rating == 5 ? "Love it!" : (object)string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
