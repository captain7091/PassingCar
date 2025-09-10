
using PassingCar.Extensions;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SupportPage : ContentPage
    {
        private int check_if_enable = 0;
        public string modal = "";

        public SupportPage()
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

        public SupportPage(string model)
        {
            try
            {
                InitializeComponent();
                modal = model;
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
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    add_image.BackgroundColor = Colors.LightGray;
                    menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //up_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.plus_white.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    add_image.BackgroundColor = Color.FromRgb(180, 180, 177);
                    check_if_enable = 0;
                    return;
                }
                //double scrollingSpace = this.ContentSize.Height - offers_listview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    hello_context.TextColor = Colors.White;
                    if (check_if_enable == 0)
                    {
                        menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                        //up_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.plus_white.png");
                        add_image.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }

                    grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    add_image.BackgroundColor = Colors.LightGray;
                    menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //up_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.plus_white.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    add_image.BackgroundColor = Color.FromRgb(180, 180, 177);
                    check_if_enable = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void BackButton(object sender, System.EventArgs e)
        {
            try
            {
                _ = modal != "" ? await Application.Current.MainPage.Navigation.PopModalAsync() : await Shell.Current.Navigation.PopAsync();
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
                _ = modal != "" ? await Application.Current.MainPage.Navigation.PopModalAsync() : await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}