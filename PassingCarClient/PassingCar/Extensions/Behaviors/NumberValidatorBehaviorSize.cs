

namespace PassingCar.Extensions.Behaviors
{
    public class NumberValidatorBehaviorSize : Behavior<Entry>
    {
        // Creating BindableProperties with Limited write access: http://iosapi.xamarin.com/index.aspx?link=M%3AXamarin.Forms.BindableObject.SetValue(Xamarin.Forms.BindablePropertyKey%2CSystem.Object) 

        private static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(NumberValidatorBehavior), false);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            private set => SetValue(IsValidPropertyKey, value);
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            //bool not_zero;
            //not_zero = decimal.TryParse(e.NewTextValue, out result);
            //if (not_zero)
            //{
            //    if (result == 0)
            //    {
            //        IsValid = false;
            //    }
            //    else
            //        IsValid = true;
            //}
            IsValid = decimal.TryParse(e.NewTextValue, out _);
            ((Entry)sender).TextColor = IsValid ? Colors.Black : Colors.Red;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;
        }
    }
}
