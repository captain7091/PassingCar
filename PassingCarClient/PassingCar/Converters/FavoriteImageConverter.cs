using System;
using System.Globalization;

namespace PassingCar.Converters
{
    public class FavoriteImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Check if the value is a boolean and return the correct image source based on its value
            if (value is bool isFavorite)
            {
                return isFavorite ? "a_star_filled1.png" : "a_star1.png";
            }

            // Fallback image in case value is null or not a boolean
            return "a_star1.png";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // We don't need to implement this for one-way binding (View to ViewModel)
            throw new NotImplementedException();
        }
    }
}

