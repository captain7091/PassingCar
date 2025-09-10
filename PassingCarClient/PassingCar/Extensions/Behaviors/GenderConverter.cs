using System;


namespace PassingCar.Extensions.Behaviors
{
    internal class GenderConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string? gender = value.ToString();
            if (gender == "Male")
            {
                return "You're handsome!";
            }
            else if (gender == "Female")
            {
                return "You're a beauty!";
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
