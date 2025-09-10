using PassingCar.Extensions;
using PassingCar.Models;

namespace PassingCar.Controls
{
    public partial class CountryControl : ContentView
    {
        public static readonly BindableProperty CountryProperty = BindableProperty.Create(
            nameof(Country),
            typeof(CountryModel),
            typeof(CountryControl),
            default,
            BindingMode.TwoWay,
            propertyChanged: (bindable, value, newValue) => (bindable as CountryControl)?.UpdateCountry(newValue as CountryModel));

        public CountryModel Country
        {
            get => (CountryModel)GetValue(CountryProperty);
            set => SetValue(CountryProperty, value);
        }

        public CountryControl()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void UpdateCountry(CountryModel model)
        {
            try
            {
                CountryCodeLabel.Text = $"(+{model?.CountryCode})";
                CountryNameLabel.Text = model?.CountryName;
                if (!string.IsNullOrEmpty(model?.FlagUrl))
                {
                    FlagImage.Source = ImageSource.FromUri(new Uri(model?.FlagUrl));
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
