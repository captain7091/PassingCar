using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.User;
using System;
using System.ComponentModel;
using Application = Microsoft.Maui.Controls.Application;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserAccountPage : ContentPage, INotifyPropertyChanged
    {
        private int check_if_enable = 0;
        private bool ratingloaded = false;
        private bool ratingLoadingStopped = false;

        public ImageSource ProfileImageSource { get; }

        public UserAccountPage()
        {
            try
            {
                InitializeComponent();
                up_context_image.IsVisible = false;
                DoRatingLoading();
                GetData();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void GetData()
        {
            try
            {
                GetUserRatingAndSoldResponse current_rating_sol = await Api.GetUserRatingAndSol();
                User user_data = await Api.GetUserData();
                //MonthlyStatusInput user_state = await Api.GetMonthlyStatus();

                first_name.Text = user_data.Surname;
                legal_entity.Text = user_data.JuridicPerson ? "Persoana Juridica" : "Persoana Fizica";

                if (user_data.Photo != null && user_data.Photo.Length > 0)
                {
                    profile_photo.Source = ImageSource.FromStream(() => new MemoryStream(user_data.Photo));
                }
                await LoadRating();

                created_account.Text = user_data.CreatedAt.ToString("MM/dd/yyyy");
                done_order.Text = "";
                cancel_order.Text = "";
                //current_balance.Text = current_rating_sol.Sold.ToString();
                //bank_iban.Text = user_data.PersonalInfo.IBAN;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async Task LoadRating()
        {
            try
            {
                GetUserRatingAndSoldResponse ratingAndSold = await Api.GetUserRatingAndSol();

                ratingloaded = true;
                while (!ratingLoadingStopped)
                {
                    await Task.Delay(40);
                }
                await Task.Delay(40);
                double rating = ratingAndSold != null && ratingAndSold.Rating > 0 ? ratingAndSold.Rating : 0f;
                RatingStar1.Source = GetStarImage(rating, 1);
                RatingStar2.Source = GetStarImage(rating, 2);
                RatingStar3.Source = GetStarImage(rating, 3);
                RatingStar4.Source = GetStarImage(rating, 4);
                RatingStar5.Source = GetStarImage(rating, 5);
                RatingValue.Text = ratingAndSold != null && ratingAndSold.Rating > 0 ? $"{rating:0.00}" : $"Sin reseñas";
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void DoRatingLoading()
        {
            try
            {
                int count = 0;
                Device.StartTimer(TimeSpan.FromMilliseconds(350), () =>
                {
                    switch (count % 7)
                    {
                        case 0:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 1:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 2:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 3:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 4:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                await Task.Delay(50);
                            });
                            break;
                        case 5:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                await Task.Delay(50);
                            });
                            break;
                        case 6:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                if (RatingValue.Text.Contains("..."))
                                {
                                    RatingValue.Text = RatingValue.Text.Replace("...", "");
                                }
                                else
                                {
                                    RatingValue.Text += ".";
                                }
                                await Task.Delay(50);
                            });
                            break;
                        default:
                            break;
                    }
                    count++;
                    ratingLoadingStopped = ratingloaded;
                    return !ratingloaded;
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private ImageSource GetStarImage(double userRating, int countStar)
        {
            try
            {
                return !(userRating > 0)
                    ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star1.png")
                    : (userRating - countStar) >= 0
                        ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star_filled1.png")
                        : (userRating - countStar) >= -0.5
                                            ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star_half1.png")
                                            : ImageSource.FromResource($"PassingCar.Resources.Images.a_star1.png");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return ImageSource.FromResource($"PassingCar.Resources.Images.a_star1.png");
            }
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    up_context_image.IsVisible = false;
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.help_icon.png");
                    grid_context.BackgroundColor = Colors.Transparent;
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                    return;
                }
                double scrollingSpace = myscrollview.ContentSize.Height - myscrollview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    hello_context.TextColor = Colors.White;
                    if (check_if_enable == 0)
                    {
                        up_context_image.IsVisible = true;
                        menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                        up_context_image.BackgroundColor = Color.FromRgb(0, 0, 0);
                        help_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                        help_image_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.help_icon_border.png");
                        grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    up_context_image.IsVisible = false;
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.help_icon.png");
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

        public void ScrollUp(object sender, EventArgs e)
        {
            try
            {
                _ = myscrollview.ScrollToAsync(0, 0, true);
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
                _ = await Shell.Current.Navigation.PopAsync();
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
                _ = await Shell.Current.Navigation.PopAsync();
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
                await Shell.Current.Navigation.PushAsync(new SupportPage());
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}