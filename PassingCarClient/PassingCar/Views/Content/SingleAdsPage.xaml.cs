using PassingCar.Extensions;
using PassingCar.ViewModels;

namespace PassingCar.Views.Content
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SingleAdsPage : ContentPage
    {
        private readonly SingleAdsViewModel SingleAdsViewModel;
        private int check_if_enable = 0;
        private bool changedEntry;
        public SingleAdsPage(SingleAdsViewModel model)
        {
            try
            {
                SingleAdsViewModel = model;
                InitializeComponent();
                //SingleAdsViewModel.FavImage = FavImage;
                BindingContext = SingleAdsViewModel;
                button_offer.IsEnabled= false;
                button_offer.BackgroundColor = Color.FromHex("#EBEBE4");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override void OnAppearing()
        {
            SingleAdsViewModel.UpdateEvent();
        }

        private void offer_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            try
            {
                if(!String.IsNullOrEmpty(entry.Text)&&entry.Text.StartsWith("0"))
                {
                    entry.Text = entry.Text.Remove(0, 1);
                }
                {
                    changedEntry = true;
                }
                // Check if input is not empty and starts with '0'
              

                // Ensure only numbers are entered
                if (!System.Text.RegularExpressions.Regex.IsMatch(e.NewTextValue, @"^\d+$"))
                {
                    // Remove non-numeric characters
                    entry.Text = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
                }
                {
                    changedEntry = true;
                }
                decimal number = 0;
                if (!string.IsNullOrEmpty(entry.Text)) 
                {
                    button_offer.IsEnabled = false;
                    number = decimal.Parse(entry.Text);
                }              
                if (number >= 10)
                {
                    button_offer.IsEnabled= true;
                    button_offer.BackgroundColor = Color.FromHex("#1892df");
                }
                else
                {
                    button_offer.IsEnabled = false;
                    button_offer.BackgroundColor = Color.FromHex("#EBEBE4");
                }

                string strName = entry.Text;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex);
            }

            finally
            {
                //
            }
        }

        private async void Backward(object sender, EventArgs e)
        {
            try
            {
                //await Shell.Current.GoToAsync($"//InboxPage");
                _ = await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void go_to_maps_start(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync("http://maps.google.com/?q=" + label_start.Text);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void go_to_maps_end(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync("http://maps.google.com/?q=" + label_end.Text);
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
                GoBack();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoBack()
        {
            try
            {
                //await Shell.Current.GoToAsync($"//InboxPage");
                _ = await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Colors.Transparent;
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                    return;
                }
                double scrollingSpace = myscrollview.ContentSize.Height - myscrollview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    if (check_if_enable == 0)
                    {
                        menu_context.Source = "back_arrow_white.png";
                    }
                    grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Colors.Transparent;
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}