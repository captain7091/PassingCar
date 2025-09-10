using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.ViewModels;
using System;



namespace PassingCar.Views
{
    
    public partial class ForgotCheckValidationCode : ContentPage
    {

        User user_;
        public ForgotCheckValidationCode(User user)
        {
            try
            {
                InitializeComponent();
                _ = step1.Focus();
                step2.IsEnabled = false;
                step3.IsEnabled = false;
                step4.IsEnabled = false;
                step5.IsEnabled = false;


                BindingContext = new ValidationCodeViewModel(user , isregistration: false);

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
                    if (string.IsNullOrEmpty(step3.Text))
                    {
                        _ = step3.Focus();
                        step3.IsEnabled = true;
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
                    _ = step4.Focus();
                    step4.IsEnabled = true;
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
                    _ = step5.Focus();
                    step5.IsEnabled = true;
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
                if (e.NewTextValue.Length == 0)
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
                _ = step1.Focus();
                step1.Text = string.Empty;
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
                _ = step2.Focus();
                step2.Text = string.Empty;
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
                _ = step3.Focus();
                step3.Text = string.Empty;
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
                _ = step4.Focus();
                step4.Text = string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {




               var Code = step1.Text + step2.Text + step3.Text + step4.Text + step5.Text;

                var bind = BindingContext as ValidationCodeViewModel;

                bind.ValidationCode = Code;
                bind.CheckCodeClicked();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Backward(object sender, EventArgs e)
        {
            try
            {
                _ = await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                GoHome();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoHome()
        {
            try
            {
                _ = await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void GoHelpPage(object sender, EventArgs e)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new SupportPage("modal"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            var bind = BindingContext as ValidationCodeViewModel;
            bind.SendAgainCommand.Execute(null);
        }
    }
}