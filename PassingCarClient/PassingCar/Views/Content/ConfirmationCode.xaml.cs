using PassingCar.Extensions;


namespace PassingCar.Views.Content
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmationCode : ContentView
    {
        public static BindableProperty Code1Property = BindableProperty.Create(nameof(Code1), typeof(string), typeof(string), default(string), defaultBindingMode: BindingMode.TwoWay);
        public string Code1 { get => (string)GetValue(Code1Property); set => SetValue(Code1Property, value); }
        public static BindableProperty Code2Property = BindableProperty.Create(nameof(Code2), typeof(string), typeof(string), default(string), defaultBindingMode: BindingMode.TwoWay);
        public string Code2 { get => (string)GetValue(Code2Property); set => SetValue(Code2Property, value); }
        public static BindableProperty Code3Property = BindableProperty.Create(nameof(Code3), typeof(string), typeof(string), default(string), defaultBindingMode: BindingMode.TwoWay);
        public string Code3 { get => (string)GetValue(Code3Property); set => SetValue(Code3Property, value); }
        public static BindableProperty Code4Property = BindableProperty.Create(nameof(Code4), typeof(string), typeof(string), default(string), defaultBindingMode: BindingMode.TwoWay);
        public string Code4 { get => (string)GetValue(Code4Property); set => SetValue(Code4Property, value); }
        public static BindableProperty Code5Property = BindableProperty.Create(nameof(Code5), typeof(string), typeof(string), default(string), defaultBindingMode: BindingMode.TwoWay);
        public string Code5 { get => (string)GetValue(Code5Property); set => SetValue(Code5Property, value); }
        public ConfirmationCode()
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
        private void step1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length == 1)
                {
                    Code1 = e.NewTextValue;
                    SetValue(Code1Property, e.NewTextValue);
                    if (string.IsNullOrEmpty(step2.Text))
                    {
                        step2.IsEnabled = true;
                        _ = step2.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void step2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length == 1)
                {
                    Code2 = e.NewTextValue;
                    SetValue(Code2Property, e.NewTextValue);
                    if (string.IsNullOrEmpty(step3.Text))
                    {
                        step3.IsEnabled = true;
                        _ = step3.Focus();
                    }
                }
                if (e.NewTextValue.Length == 0)
                {
                    step2.OnBackspace += EntryBackspaceEventHandler2;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void step3_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length == 1)
                {
                    Code3 = e.NewTextValue;
                    SetValue(Code3Property, e.NewTextValue);
                    step4.IsEnabled = true;
                    _ = step4.Focus();
                }
                if (e.NewTextValue.Length == 0)
                {
                    step3.OnBackspace += EntryBackspaceEventHandler3;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void step4_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length == 1)
                {
                    Code4 = e.NewTextValue;
                    SetValue(Code4Property, e.NewTextValue);
                    step5.IsEnabled = true;
                    _ = step5.Focus();
                }
                if (e.NewTextValue.Length == 0)
                {
                    step4.OnBackspace += EntryBackspaceEventHandler4;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void step5_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length == 1)
                {
                    Code5 = e.NewTextValue;
                    SetValue(Code5Property, e.NewTextValue);
                }
                else if (e.NewTextValue.Length == 0)
                {
                    step5.OnBackspace += EntryBackspaceEventHandler5;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void EntryBackspaceEventHandler2(object sender, EventArgs e)
        {
            try
            {
                step1.Text = string.Empty;
                _ = step1.Focus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void EntryBackspaceEventHandler3(object sender, EventArgs e)
        {
            try
            {
                step2.Text = string.Empty;
                _ = step2.Focus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void EntryBackspaceEventHandler4(object sender, EventArgs e)
        {
            try
            {
                step3.Text = string.Empty;
                _ = step3.Focus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void EntryBackspaceEventHandler5(object sender, EventArgs e)
        {
            try
            {
                step4.Text = string.Empty;
                _ = step4.Focus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}