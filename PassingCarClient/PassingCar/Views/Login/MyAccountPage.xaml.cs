using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Models.API.User;
using PassingCar.Popups;
using PassingCar.ViewModels;
using PhoneNumbers;

using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Color = Microsoft.Maui.Graphics.Colors;
using Application = Microsoft.Maui.Controls.Application;
using Mopups.Services;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyAccountPage : ContentPage, INotifyPropertyChanged
    {
        private int check_if_enable = 0;
        private int check_if_photo = 0;
        private readonly INavigation navigation;
        public byte[] ProfilePhoto { get; set; }
        public ImageSource ProfileImageSource { get; private set; }

        private bool ratingloaded = false;
        private bool ratingLoadingStopped = false;
        private readonly PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
        public AccountPageViewModel AccountViewModel { get; set; }

        [Obsolete]
        public MyAccountPage()
        {
            try
            {
                InitializeComponent();
                DoRatingLoading();
                GetData();
                RequestCameraAndGalleryPermission();
                AccountViewModel = new AccountPageViewModel();
                BindingContext = AccountViewModel;
                up_context_image.IsVisible = false;
                List<string> province_pick = new List<string>
                {
                    "A Coruña",
                    "Álava",
                    "Albacete",
                    "Alicante",
                    "Almería",
                    "Asturias",
                    "Ávila",
                    "Badajoz",
                    "Islas Baleares",
                    "Barcelona",
                    "Vizcaya",
                    "Burgos",
                    "Cáceres",
                    "Cádiz",
                    "Cantabria",
                    "Castellón",
                    "Ciudad Real",
                    "Córdoba",
                    "Cuenca",
                    "Guipúzcoa",
                    "Gerona",
                    "Granada",
                    "Guadalajara",
                    "Huelva",
                    "Huesca",
                    "Jaén",
                    "La Rioja",
                    "Las Palmas",
                    "León",
                    "Lérida",
                    "Lugo",
                    "Madrid",
                    "Málaga",
                    "Murcia",
                    "Navarra",
                    "Orense",
                    "Palencia",
                    "Pontevedra",
                    "Salamanca",
                    "Santa Cruz de Tenerife",
                    "Segovia",
                    "Sevilla",
                    "Soria",
                    "Tarragona",
                    "Teruel",
                    "Toledo",
                    "Valencia",
                    "Valladolid",
                    "Zamora",
                    "Zaragoza"
                };
                provincie_picker.ItemsSource = province_pick;
                //provincie_picker.SelectedItem = "Barcelona";
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        public async void RequestCameraAndGalleryPermission()
        {
            try
            {
                _ = await RequestCameraAndGalleryPermissions();
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
                    hello_context.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    up_context_image.IsVisible = false;
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = "help_icon.png";
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
                        menu_context.Source = "back_arrow_white.png";
                        up_context_image.BackgroundColor = Microsoft.Maui.Graphics.Color.FromRgb(0, 0, 0);
                        help_context.BackgroundColor = Microsoft.Maui.Graphics.Color.FromRgb(0, 0, 0);
                        help_image_context.Source = "help_icon_border.png";
                        grid_context.BackgroundColor = Microsoft.Maui.Graphics.Color.FromRgb(0, 0, 0);
                    }
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    hello_context.TextColor = Microsoft.Maui.Graphics.Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    up_context_image.IsVisible = false;
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = "help_icon.png";
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

        public async void GetData()
        {
            try
            {
                GetUserRatingAndSoldResponse current_rating_sol = await Api.GetUserRatingAndSol();
                User user_data = await Api.GetUserData();
                login_type.Text = user_data.RegistrationType.ToString();
                profile_name.Text = user_data.Name + " " + user_data.Surname;
                if (user_data.Photo != null && user_data.Photo.Length > 0)
                {
                    profile_photo.Source = ImageSource.FromStream(() => new MemoryStream(user_data.Photo));
                }
                await LoadRating();

                account_number.Text = user_data.PhoneNumber;
                account_email.Text = user_data.Email;
                account_re_email.Text = "";
                account_oldpassword.Text = "**************";
                account_newpassword.Text = "";
                account_repassword.Text = "";

                general_firstname.Text = user_data.Name;
                general_lastname.Text = user_data.Surname;

                //address split 2 lines
                if (!string.IsNullOrEmpty(user_data.Address.Address))
                {
                    string adr = user_data.Address.Address;
                    string[] parts = adr.Split(' ');
                    string line1 = string.Join(" ", parts.Take(parts.Length / 2));
                    string line2 = string.Join(" ", parts.Skip(parts.Length / 2));
                    general_address.Text = line1;
                    general_address2.Text = line2;
                }

                general_city.Text = user_data.Address.City;
                general_zipcode.Text = user_data.Address.ZipCode;
                foreach (string item in provincie_picker.Items)
                {
                    if (item == user_data.Address.Provincie)
                    {
                        provincie_picker.SelectedItem = item;
                    }
                }
                general_dni.Text = user_data.PersonalInfo.DNI;

                if (string.IsNullOrEmpty(user_data.JuridicDetails.CompanyName))
                {
                    company_title.IsVisible = false;
                    company_bar.IsVisible = false;
                    company_name_content.IsVisible = false;
                    company_name_content2.IsVisible = false;
                    company_cif_content.IsVisible = false;
                    company_button_content.IsVisible = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(user_data.JuridicDetails.CompanyName))
                    {
                        string comp = user_data.JuridicDetails.CompanyName;
                        string[] parts_comp = comp.Split(' ');
                        string line1_comp = string.Join(" ", parts_comp.Take(parts_comp.Length / 2));
                        string line2_comp = string.Join(" ", parts_comp.Skip(parts_comp.Length / 2));
                        company_name.Text = line1_comp;
                        company_name2.Text = line2_comp;
                    }
                    company_cif.Text = user_data.JuridicDetails.NIF;
                }

                //current_balance.Text = current_rating_sol.Sold.ToString();
                bank_iban.Text = user_data.PersonalInfo.IBAN;
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
                current_balance.Text = (ratingAndSold != null && ratingAndSold.Sold.HasValue) ? $"{ratingAndSold.Sold:0.00} €" : $"0.00 €";
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
                                RatingStar1.Source = "a_star_filled1.png";
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar5.Source = "a_star1.png";
                                await Task.Delay(10);
                            });
                            break;
                        case 1:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar2.Source = "a_star_filled1.png";
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
                                RatingStar3.Source = "a_star_filled1.png";
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 3:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                RatingStar1.Source = "a_star1.png";
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar4.Source = "a_star_filled1.png";
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 4:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar2.Source = "a_star1.png";
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar5.Source = "a_star_filled1.png";
                                await Task.Delay(50);
                            });
                            break;
                        case 5:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar3.Source = "a_star1.png";
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
                                RatingStar4.Source = "a_star1.png";
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                if (RatingValue.Text.Contains("..."))
                                {
                                    RatingValue.Text = RatingValue.Text.Replace("...", "");
                                }
                                else
                                {
                                    RatingValue.Text += ".";
                                }
                                current_balance.Text = RatingValue.Text;
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
                    ? "a_star1.png"
                    : (userRating - countStar) >= 0
                        ? "a_star_filled1.png"
                        : (userRating - countStar) >= -0.5
                                            ? "a_star_half1.png"
                                            : "a_star1.png";
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return "a_star1.png";
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

        private bool IsValidNumber(string aNumber)
        {
            try
            {
                bool result = false;

                aNumber = aNumber.Trim();

                if (aNumber.StartsWith("00"))
                {
                    // Replace 00 at beginning with +
                    aNumber = "+" + aNumber.Remove(0, 2);
                }

                try
                {
                    result = phoneUtil.IsValidNumber(phoneUtil.Parse(aNumber, ""));
                }
                catch
                {
                    Console.WriteLine("errornotvalid");
                    // Exception means is no valid number
                }

                return result;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        [Obsolete]
        private async void InsertPhotoClicked(object sender, EventArgs e)
        {
            try
            {
    //            string result = DeviceInfo.Platform == DevicePlatform.Android
    //? await MopupService.Instance.PushAsync(new MediaChoosePopUp())
    //: await DisplayActionSheet("Seleccionar", "Cancelar", null, "Cámara", "Galería");
    //            if (result != null)
    //            {
    //                if (result == "Cámara")
    //                {
    //                    TakePhoto();
    //                }

    //                if (result == "Galería")
    //                {
    //                    SelectPhoto();
    //                }
    //            }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void SelectPhoto()
        {
            try
            {
                // Check if the device supports picking photos
                if (!MediaPicker.IsCaptureSupported)
                {
                    ShowInvalidInputErrorMessage("¡No se puede acceder a la galería!");
                    return;
                }

                // Request necessary permissions
                bool isPermissionGranted = await RequestCameraAndGalleryPermissions();
                if (!isPermissionGranted)
                {
                    return;
                }

                // Delay for iOS devices
                await Task.Delay(100);

                // Pick a photo from the gallery
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo != null)
                {
                    // Get the stream of the selected photo
                    using (var stream = await photo.OpenReadAsync())
                    {
                        // Convert the stream to byte array
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        ProfilePhoto = memoryStream.ToArray();
                    }

                    // Use the file path to set the image source
                    ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(ProfilePhoto));
                    profile_photo.Source = ProfileImageSource;
                    profile_name.Text = "Foto cargada!";
                    profile_name.TextColor = Colors.DarkGreen;
                    OnPropertyChanged(nameof(profile_photo));
                    OnPropertyChanged(nameof(profile_name));
                    check_if_photo = 1;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and show error popup
                navigation.OpeErrorPopUp("Insertar foto falló", JsonConvert.SerializeObject(ex), "OK");
            }
        }



        private async void TakePhoto()
        {
            try
            {
                // Check if the device has a camera available
                if (!MediaPicker.IsCaptureSupported)
                {
                    ShowInvalidInputErrorMessage("No hay cámara disponible.");
                    return;
                }

                // Request necessary permissions
                bool isPermissionGranted = await RequestCameraAndGalleryPermissions();
                if (!isPermissionGranted)
                {
                    return;
                }

                // Wait for iOS devices
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await Task.Delay(100);
                }

                // Pick a photo from the camera
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    // Get the stream of the photo
                    using (var stream = await photo.OpenReadAsync())
                    {
                        // Convert the stream to byte array
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        ProfilePhoto = memoryStream.ToArray();
                    }

                    // Use the file path to set the image source
                    ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(ProfilePhoto));
                    profile_photo.Source = ProfileImageSource;
                    profile_name.Text = "Foto cargada!";
                    profile_name.TextColor = Colors.DarkGreen;
                    OnPropertyChanged(nameof(profile_photo));
                    OnPropertyChanged(nameof(profile_name));
                    check_if_photo = 1;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and show error popup
                navigation.OpeErrorPopUp("TakePhoto failed", JsonConvert.SerializeObject(ex), "OK");
            }
        }

        public async Task<bool> RequestCameraAndGalleryPermissions()
        {
            try
            {
                // Check Camera Permission
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    var cameraRequestResult = await Permissions.RequestAsync<Permissions.Camera>();
                    if (cameraRequestResult != PermissionStatus.Granted)
                    {
                        return false; // Camera permission denied
                    }
                }

                // Check Photos Permission
                var photosStatus = await Permissions.CheckStatusAsync<Permissions.Photos>();
                if (photosStatus != PermissionStatus.Granted)
                {
                    var photosRequestResult = await Permissions.RequestAsync<Permissions.Photos>();
                    if (photosRequestResult != PermissionStatus.Granted)
                    {
                        return false; // Photos permission denied
                    }
                }

                // Check Storage Permission
                var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (storageStatus != PermissionStatus.Granted)
                {
                    var storageRequestResult = await Permissions.RequestAsync<Permissions.StorageRead>();
                    if (storageRequestResult != PermissionStatus.Granted)
                    {
                        return false; // Storage permission denied
                    }
                }

                return true; // All permissions granted
            }
            catch (Exception ex)
            {
                // Handle exceptions as necessary
                Console.WriteLine($"Error requesting permissions: {ex}");
                return false; // Return false on exception
            }
        }

        private void ShowInvalidInputErrorMessage(string message)
        {
            try
            {
                navigation.OpeErrorPopUp("Datos completados no válidos", message, "OK");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Update_account(object sender, EventArgs e)
        {
            try
            {
                //get user_data
                User user_data = await Api.GetUserData();
                //check validators
                int check_account_email = 0;
                int check_account_passw = 0;

                //check password && new_passw && re_passw
                if (account_oldpassword.Text != null && account_newpassword.Text != null && account_repassword.Text != null)
                {
                    if (old_password.IsValid && new_password.IsValid && re_password.IsValid)
                    {
                        account_oldpassword_head.TextColor = Color.Black;
                        account_newpassword_head.TextColor = Color.Black;
                        if (account_newpassword.Text == account_repassword.Text)
                        {
                            account_repassword_head.TextColor = Color.Black;
                        }
                    }
                    else
                    {
                        if (!new_password.IsValid)
                        {
                            account_newpassword_head.TextColor = Color.IndianRed;
                            PasswInvalidGo("new");
                        }
                        if (new_password != re_password)
                        {
                            account_repassword_head.TextColor = Color.IndianRed;
                            PasswInvalidGo("re");
                        }
                        if (!old_password.IsValid)
                        {
                            //do nothing
                            account_oldpassword_head.TextColor = Color.IndianRed;
                            PasswInvalidGo("old");
                        }
                        check_account_passw = 1;
                    }
                }
                else
                {
                    if (!new_password.IsValid)
                    {
                        account_newpassword_head.TextColor = Color.IndianRed;
                        PasswInvalidGo("new");
                    }
                    if (new_password != re_password)
                    {
                        account_repassword_head.TextColor = Color.IndianRed;
                        PasswInvalidGo("re");
                    }
                    if (!old_password.IsValid)
                    {
                        //do nothing
                        account_oldpassword_head.TextColor = Color.IndianRed;
                        PasswInvalidGo("old");
                    }
                    check_account_passw = 1;
                }

                //check re-type email
                if (account_re_email.Text != null)
                {
                    if (account_re_email.Text == account_email.Text)
                    {
                        //do nothing
                        account_re_email_head.TextColor = Color.Black;
                    }
                    else
                    {
                        account_re_email_head.TextColor = Color.IndianRed;
                        ReEmailInvalidGo();
                        check_account_email = 1;
                    }
                }
                else
                {
                    account_re_email_head.TextColor = Color.IndianRed;
                    ReEmailInvalidGo();
                    check_account_email = 1;
                }

                //check email
                if (account_email.Text != null)
                {
                    if (account_validator_email.IsValid)
                    {
                        //do nothing
                        account_email_head.TextColor = Color.Black;
                    }
                    else
                    {
                        account_email_head.TextColor = Color.IndianRed;
                        EmailInvalidGo();
                        check_account_email = 1;
                    }
                }
                else
                {
                    account_email_head.TextColor = Color.IndianRed;
                    EmailInvalidGo();
                    check_account_email = 1;
                }

                //check photo
                if (check_if_photo == 1)
                {

                }

                //check number phone
                //if (account_number.Text != null)
                //{
                //    if (IsValidNumber(account_number.Text.Replace("(", "").Replace(")", "")))
                //    {
                //Console.WriteLine(dest_number.Text);
                //        account_number_head.TextColor = Color.Black;
                //        account_number.TextColor = Color.Black;
                //    }
                //    else
                //    {
                //        check_account = 1;
                //        account_number_head.TextColor = Color.IndianRed;
                //        account_number.TextColor = Color.IndianRed;
                //        NumberPhoneInvalidGo();
                //    }
                //}
                //else
                //{
                //    check_account = 1;
                //    account_number_head.TextColor = Color.IndianRed;
                //    account_number.TextColor = Color.IndianRed;
                //    NumberPhoneInvalidGo();
                //}

                //if all_validators
                if ((check_account_passw == 0 && check_account_email == 1) ||
                    (check_account_passw == 1 && check_account_email == 0) ||
                    (check_account_passw == 0 && check_account_email == 0))
                {
                    bool result = await Application.Current.MainPage.DisplayAlert("Update Account", "Do you want to update your account information", "Yes", "No");
                    if (result)
                    {
                        if (check_account_email == 0)
                        {
                            user_data.Email = account_email.Text;
                        }
                        if (check_account_passw == 0)
                        {
                            if (account_oldpassword.Text.EncryptString() != user_data.HashedPassword)
                            {
                                await Application.Current.MainPage.DisplayAlert("Incorrect Password", "Your current password doesn't match!", "Ok");
                            }
                            else
                            {
                                user_data.HashedPassword = account_newpassword.Text.EncryptString();
                            }
                        }
                        try
                        {
                            if (check_if_photo == 1)
                            {
                                user_data.Photo = ProfilePhoto;
                                profile_name.Text = $"Photo updated!";
                                OnPropertyChanged(nameof(profile_name));
                            }
                            _ = await Api.UpdateUser(user_data);
                            account_newpassword.Text = "";
                            account_repassword.Text = "";
                            account_re_email.Text = "";
                            await SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(user_data));
                            HeaderContentView.HeaderContext.ProfileImageSource = ProfileImageSource;
                            OnPropertyChanged(nameof(HeaderContentView.HeaderContext.ProfileImageSource));
                            await Application.Current.MainPage.DisplayAlert("Updated succesfully!", "Your account information was updated!", "Ok");
                        }
                        catch
                        {
                            await Application.Current.MainPage.DisplayAlert("Error!", "Algo salió mal!", "Ok");

                        }
                    }
                    //Console.WriteLine("all_okk");
                    //DoSomethingAsync();
                }
                else if (check_if_photo == 1)
                {

                    bool result = await Application.Current.MainPage.DisplayAlert("Update Account", "Do you want to update your profile image", "Yes", "No");
                    if (result)
                    {
                        user_data.Photo = ProfilePhoto;
                        try
                        {
                            _ = await Api.UpdateUser(user_data);
                            account_newpassword.Text = "";
                            account_repassword.Text = "";
                            account_re_email.Text = "";
                            account_email_head.TextColor = Color.Black;
                            account_repassword_head.TextColor = Color.Black;
                            account_re_email_head.TextColor = Color.Black;
                            account_oldpassword_head.TextColor = Color.Black;
                            account_newpassword_head.TextColor = Color.Black;
                            profile_name.Text = $"Photo updated!";
                            OnPropertyChanged(nameof(profile_name));
                            await SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(user_data));
                            HeaderContentView.HeaderContext.ProfileImageSource = ProfileImageSource;
                            OnPropertyChanged(nameof(HeaderContentView.HeaderContext.ProfileImageSource));
                            await Application.Current.MainPage.DisplayAlert("Updated succesfully!", "Your profile image was updated!", "Ok");
                            PhotoGo();
                        }
                        catch
                        {
                            await Application.Current.MainPage.DisplayAlert("Error!", "Algo salió mal!", "Ok");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Update_general(object sender, EventArgs e)
        {
            try
            {
                //get user_data
                User user_data = await Api.GetUserData();
                //check validators
                int check_general = 0;

                //check DNI
                if (general_dni.Text != null)
                {
                    //if (general_dni_validator.IsValid)
                    //{
                    //do nothing
                    general_dni_head.TextColor = Color.Black;
                    //}
                    //else
                    //{
                    //    general_dni_head.TextColor = Color.IndianRed;
                    //    DNIInvalidGo();
                    //    check_general = 1;
                    //}
                }
                else
                {
                    general_dni_head.TextColor = Color.IndianRed;
                    DNIInvalidGo();
                    check_general = 1;
                }

                //check first and last name
                if (general_firstname.Text != null && general_lastname.Text != null)
                {
                    if (general_firstname.Text.Length > 1)
                    {
                        //Console.WriteLine(dest_number.Text);
                        general_firstname.TextColor = Color.Black;
                    }
                    else
                    {
                        check_general = 1;
                        general_firstname.TextColor = Color.IndianRed;
                        NameInvalidGo("first");
                    }

                    if (general_lastname.Text.Length > 1)
                    {
                        //Console.WriteLine(dest_number.Text);
                        general_lastname.TextColor = Color.Black;
                    }
                    else
                    {
                        check_general = 1;
                        general_lastname.TextColor = Color.IndianRed;
                        NameInvalidGo("last");
                    }
                }
                else
                {
                    check_general = 1;
                    general_firstname.TextColor = Color.IndianRed;
                    general_lastname.TextColor = Color.IndianRed;
                    NameInvalidGo("first");
                }

                //if all_validators
                if (check_general == 0)
                {
                    bool result = await Application.Current.MainPage.DisplayAlert("Update General", "Do you want to update your general information", "Yes", "No");
                    if (result)
                    {
                        user_data.Name = general_firstname.Text;
                        user_data.Surname = general_lastname.Text;
                        user_data.Address.Address = general_address.Text + " " + general_address2.Text;
                        user_data.Address.City = general_city.Text;
                        user_data.Address.ZipCode = general_zipcode.Text;
                        user_data.Address.Provincie = provincie_picker.SelectedItem.ToString().Trim();
                        user_data.PersonalInfo.DNI = general_dni.Text;
                        try
                        {
                            ApiBaseResponse response = await Api.UpdateUser(user_data);
                            if (response != null && response.Success)
                            {
                                await SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(user_data));
                                await Application.Current.MainPage.DisplayAlert("Updated succesfully!", "Your general information was updated!", "Ok");
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Error!", response.ErrorMessage, "Ok");
                            }
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error!", JsonConvert.SerializeObject(ex), "Ok");

                        }
                    }
                    //Console.WriteLine("all_okk");
                    //DoSomethingAsync();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Update_company(object sender, EventArgs e)
        {
            try
            {
                //get user_data
                User user_data = await Api.GetUserData();
                //check validators
                int check_company = 0;

                //check cif_nif
                if (company_cif.Text != null)
                {
                    if (validator_nif_cif.IsValid)
                    {
                        //do nothing
                        company_cif_head.TextColor = Color.Black;
                    }
                    else
                    {
                        company_cif_head.TextColor = Color.IndianRed;
                        CIFInvalidGo();
                        check_company = 1;
                    }
                }
                else
                {
                    company_cif_head.TextColor = Color.IndianRed;
                    CIFInvalidGo();
                    check_company = 1;
                }

                //check company_name
                if ((company_name.Text + company_name2.Text) != null)
                {
                    if ((company_name.Text + company_name2.Text).Length > 4)
                    {
                        //do nothing
                        company_name_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        company_name_head.TextColor = Colors.IndianRed;
                        CompanyNameInvalidGo();
                        check_company = 1;
                    }
                }
                else
                {
                    company_name_head.TextColor = Colors.IndianRed;
                    CompanyNameInvalidGo();
                    check_company = 1;
                }

                //if all_validators
                if (check_company == 0)
                {
                    bool result = await App.Current.MainPage.DisplayAlert("Update Company", "Do you want to update your company information", "Yes", "No");
                    if (result)
                    {
                        user_data.JuridicDetails.CompanyName = company_name.Text + " " + company_name2.Text;
                        user_data.JuridicDetails.NIF = company_cif.Text;
                        try
                        {
                            _ = await Api.UpdateUser(user_data);
                            await SecureStorage.SetAsync("UserLogged", JsonConvert.SerializeObject(user_data));
                            await App.Current.MainPage.DisplayAlert("Updated succesfully!", "Your company information was updated!", "Ok");
                        }
                        catch
                        {
                            await App.Current.MainPage.DisplayAlert("Error!", "Algo salió mal!", "Ok");

                        }
                    }
                    //Console.WriteLine("all_okk");
                    //DoSomethingAsync();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Update_bank(object sender, EventArgs e)
        {
            try
            {
                //check validators
                int check_bank = 0;

                //check cvc
                if (bank_cvc.Text != null)
                {
                    if (cvc_validator.IsValid)
                    {
                        //do nothing
                        bank_cvc_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_cvc_head.TextColor = Colors.IndianRed;
                        CVCInvalidGo();
                        check_bank = 1;
                    }
                }
                else
                {
                    bank_cvc_head.TextColor = Colors.IndianRed;
                    CVCInvalidGo();
                    check_bank = 1;
                }

                //check exp_date
                if (bank_card_date.Text != null && bank_card_year.Text != null)
                {
                    if (cardnumber_validator.IsValid && bank_card_year.Text.Length > 3)
                    {
                        try
                        {
                            int year = int.Parse(bank_card_year.Text);
                            if (year < 2090 && year >= DateTime.Now.Year)
                            {
                                bank_expirationdate_head.TextColor = Colors.Black;
                            }
                            else
                            {
                                bank_expirationdate_head.TextColor = Colors.IndianRed;
                                CardExpDateInvalidGo();
                            }
                        }
                        catch
                        {
                            bank_expirationdate_head.TextColor = Colors.IndianRed;
                            CardExpDateInvalidGo();
                        }
                        //do nothing
                        //bank_expirationdate_head.TextColor = Color.Black;
                    }
                    else
                    {
                        bank_expirationdate_head.TextColor = Colors.IndianRed;
                        CardExpDateInvalidGo();
                        check_bank = 1;
                    }
                }
                else
                {
                    bank_expirationdate_head.TextColor = Colors.IndianRed;
                    CardExpDateInvalidGo();
                    check_bank = 1;
                }

                //check card_name
                if (bank_name.Text != null)
                {
                    if (bank_name.Text.Length > 1)
                    {
                        //do nothing
                        bank_name_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_name_head.TextColor = Colors.IndianRed;
                        CardNameInvalidGo();
                        check_bank = 1;
                    }
                }
                else
                {
                    bank_name_head.TextColor = Colors.IndianRed;
                    CardNameInvalidGo();
                    check_bank = 1;
                }

                //check card_number
                if (bank_cardnumber.Text != null)
                {
                    if (cardnumber_validator.IsValid)
                    {
                        //do nothing
                        bank_cardnumber_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_cardnumber_head.TextColor = Colors.IndianRed;
                        CardNumberInvalidGo();
                        check_bank = 1;
                    }
                }
                else
                {
                    bank_cardnumber_head.TextColor = Colors.IndianRed;
                    CardNumberInvalidGo();
                    check_bank = 1;
                }

                //if all_validators
                if (check_bank == 0)
                {
                    //Console.WriteLine("all_okk");
                    //DoSomethingAsync();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CardExpDateInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(bank_expirationdate_head, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CVCInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(bank_cvc, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void PhotoGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(profile_photo, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CardNameInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(bank_name, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void BankInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(bank_iban_head, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CardNumberInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(bank_cardnumber, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void DNIInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(general_dni, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CIFInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(company_cif_head, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void CompanyNameInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(company_name_head, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void NumberPhoneInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(account_number, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void EmailInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(account_email, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void NameInvalidGo(string name)
        {
            try
            {
                switch (name)
                {
                    case "first":
                        await myscrollview.ScrollToAsync(general_firstname, ScrollToPosition.Center, true);
                        break;
                    case "last":
                        await myscrollview.ScrollToAsync(general_lastname, ScrollToPosition.Center, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void PasswInvalidGo(string name)
        {
            try
            {
                switch (name)
                {
                    case "old":
                        await myscrollview.ScrollToAsync(account_oldpassword_head, ScrollToPosition.Center, true);
                        break;
                    case "new":
                        await myscrollview.ScrollToAsync(account_newpassword_head, ScrollToPosition.Center, true);
                        break;
                    case "re":
                        await myscrollview.ScrollToAsync(account_repassword_head, ScrollToPosition.Center, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void ReEmailInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(account_re_email, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Delete_Account(object sender, EventArgs e)
        {
            try
            {
                bool result = await DisplayAlert("Confirmation", "Your account will be deleted", "Confirm", "Cancel");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void GetToken_Btn(object sender, EventArgs e)
        {
            try
            {
                //get user_data
                //User user_data = await Api.GetUserData();

                //check validators
                int check_bankdata = 0;
                //check cvc
                if (bank_cvc.Text != null)
                {
                    if (cvc_validator.IsValid)
                    {
                        //do nothing
                        bank_cvc_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_cvc_head.TextColor = Colors.IndianRed;
                        CVCInvalidGo();
                        check_bankdata = 1;
                    }
                }
                else
                {
                    bank_cvc_head.TextColor = Colors.IndianRed;
                    CVCInvalidGo();
                    check_bankdata = 1;
                }

                //check exp_date
                if (bank_card_date.Text != null && bank_card_year.Text != null)
                {
                    if (cardnumber_validator.IsValid && bank_card_year.Text.Length > 3)
                    {
                        try
                        {
                            int year = int.Parse(bank_card_year.Text);
                            if (year < 2090 && year >= DateTime.Now.Year)
                            {
                                bank_expirationdate_head.TextColor = Colors.Black;
                            }
                            else
                            {
                                bank_expirationdate_head.TextColor = Colors.IndianRed;
                                CardExpDateInvalidGo();
                            }
                        }
                        catch
                        {
                            bank_expirationdate_head.TextColor = Colors.IndianRed;
                            CardExpDateInvalidGo();
                        }
                        //do nothing
                        //bank_expirationdate_head.TextColor = Color.Black;
                    }
                    else
                    {
                        bank_expirationdate_head.TextColor = Colors.IndianRed;
                        CardExpDateInvalidGo();
                        check_bankdata = 1;
                    }
                }
                else
                {
                    bank_expirationdate_head.TextColor = Colors.IndianRed;
                    CardExpDateInvalidGo();
                    check_bankdata = 1;
                }

                //check card_name
                if (bank_name.Text != null)
                {
                    if (bank_name.Text.Length > 1)
                    {
                        //do nothing
                        bank_name_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_name_head.TextColor = Colors.IndianRed;
                        CardNameInvalidGo();
                        check_bankdata = 1;
                    }
                }
                else
                {
                    bank_name_head.TextColor = Colors.IndianRed;
                    CardNameInvalidGo();
                    check_bankdata = 1;
                }

                //check card_number
                if (bank_cardnumber.Text != null)
                {
                    if (cardnumber_validator.IsValid)
                    {
                        //do nothing
                        bank_cardnumber_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        bank_cardnumber_head.TextColor = Colors.IndianRed;
                        CardNumberInvalidGo();
                        check_bankdata = 1;
                    }
                }
                else
                {
                    bank_cardnumber_head.TextColor = Colors.IndianRed;
                    CardNumberInvalidGo();
                    check_bankdata = 1;
                }

                check_bankdata = 0;
                if (check_bankdata == 1)
                {
                    await App.Current.MainPage.DisplayAlert("Info!", "The Token is generated based on the bank card account! Please fill Card Data.", "Next");
                    await App.Current.MainPage.DisplayAlert("Info!", "The token will appear in the 'Your Token' field.", "Next");
                    await App.Current.MainPage.DisplayAlert("Info!", "You can withdraw money based on the token.", "Next");
                    await App.Current.MainPage.DisplayAlert("Info!", "The Token can be used only once.", "Finish.");
                }

                if (check_bankdata == 0)
                {
                    try
                    {
                        User userLogged = await Api.GetUserData();
                        TokenWithdrawViewModel gettoken = new TokenWithdrawViewModel();
                        TokenCardOptions cardOptions = new TokenCardOptions
                        {
                            //Number = cardModel.Number,
                            Number = bank_cardnumber.Text,
                            //ExpMonth = cardModel.ExpMonth,
                            ExpMonth = bank_card_date.Text,
                            //ExpYear = cardModel.ExpYear,
                            ExpYear = bank_card_year.Text,
                            //Cvc = cardModel.Cvc,
                            Cvc = bank_cvc.Text,
                            //Currency = "EUR",
                            Currency = "ron",
                            //Name = cardModel.Name,
                            Name = bank_name.Text,
                            //AddressCity = cardModel.AddressCity,
                            AddressCity = (userLogged != null && userLogged.Address != null) ? userLogged.Address.City : string.Empty,
                            //AddressZip = cardModel.AddressZip,
                            AddressZip = (userLogged != null && userLogged.Address != null) ? userLogged.Address.ZipCode : string.Empty,
                            //AddressLine1 = cardModel.AddressLine1,
                            AddressLine1 = (userLogged != null && userLogged.Address != null) ? userLogged.Address.Address : string.Empty,
                            //AddressCountry = cardModel.AddressCountry
                            //AddressCountry = "RO"
                        };
                        bank_iban.Text = await gettoken.GetToken(cardOptions);
                        if (String.IsNullOrEmpty(bank_iban.Text.ToString()))
                        {
                            await App.Current.MainPage.DisplayAlert("Alert!", "We encountered some problems when generating the Token. Intentar otra vez later!", "Ok");
                        }
                    }
                    catch (Exception)
                    {
                        await App.Current.MainPage.DisplayAlert("Alert!", "Algo salió mal. Intentar otra vez later or contact the Administrator!", "Ok");
                    }

                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void CheckIBAN_Btn(object sender, EventArgs e)
        {
            try
            {
                if (bank_iban.Text != null)
                {
                    User user_data = await Api.GetUserData();

                    TokenWithdrawViewModel getcheck = new TokenWithdrawViewModel();
                    var get_code = await getcheck.CheckIBAN(bank_iban.Text, user_data);
                    if (get_code == "200")
                    {
                        withdraw.IsEnabled = true;
                        withdraw.BackgroundColor =Microsoft.Maui.Graphics.Color.FromHex("#03a4ed");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Alert!", "Please insert IBAN Account", "Ok");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Alert!", "Algo salió mal. Intentar otra vez later or contact the Administrator! " + ex.ToString(), "Ok");
                _ = ex.Handle();
            }
        }

        private async void Withdraw_Btn(object sender, EventArgs e)
        {
            try
            {
                //check iban
                if (bank_iban.Text != null)
                {
                    try
                    {
                        //get user_data
                        User user_data = await Api.GetUserData();
                        GetUserRatingAndSoldResponse ratingAndSold = await Api.GetUserRatingAndSol();

                        TokenWithdrawViewModel getwithdraw = new TokenWithdrawViewModel();
                        await getwithdraw.Payout(user_data, ratingAndSold.Sold);
                    }
                    catch (Exception ex)
                    {
                        await App.Current.MainPage.DisplayAlert("Alert!", "Algo salió mal. Intentar otra vez later or contact the Administrator! " + ex.ToString(), "Ok");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Info!", "Please insert IBAN", "Ok");
                    bank_iban_head.TextColor = Colors.IndianRed;
                    BankInvalidGo();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}