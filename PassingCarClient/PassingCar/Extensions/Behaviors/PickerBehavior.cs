using System;


namespace PassingCar.Extensions.Behaviors
{
    public class PickerBehavior : Behavior<Picker>
    {
        // Creating BindableProperties with Limited write access: http://iosapi.xamarin.com/index.aspx?link=M%3AXamarin.Forms.BindableObject.SetValue(Xamarin.Forms.BindablePropertyKey%2CSystem.Object) 

        public static readonly BindablePropertyKey SelectedItemPropertyKey = BindableProperty.CreateReadOnly("SelectedItem", typeof(string), typeof(PickerBehavior), default(string));

        public static readonly BindableProperty SelectedItemProperty = SelectedItemPropertyKey.BindableProperty;

        public string SelectedItem
        {
            get => (string)GetValue(SelectedItemProperty);
            private set => SetValue(SelectedItemPropertyKey, value);
        }


        protected override void OnAttachedTo(Picker bindable)
        {
            bindable.SelectedIndexChanged += bindable_SelectedIndexChanged;
        }

        private void bindable_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            SelectedItem = picker.SelectedIndex <= 0 || picker.SelectedIndex > picker.Items.Count - 1 ? null : picker.Items[picker.SelectedIndex];
        }

        protected override void OnDetachingFrom(Picker bindable)
        {
            bindable.SelectedIndexChanged -= bindable_SelectedIndexChanged;
        }
    }
}
