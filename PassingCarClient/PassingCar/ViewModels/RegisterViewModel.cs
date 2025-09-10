using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Popups;
using PassingCar.Views;

using System.ComponentModel;


namespace PassingCar.ViewModels
{
    public class RegisterViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public Command BackCommand { get; }
        public Command InsertProfilePhotoCommand { get; }
        public Command SignUpCommand { get; }
        public string FirstName { get; set; }
        public string Genre { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Email { get; set; }
        public string DNI { get; set; }
        public string Password { get; set; }
        public string SecoundPassword { get; set; }
        public byte[] ProfilePhoto { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string InfoProfilePhotoTxt { get; set; }
        public Color InsertPhotoColor { get; set; }
        public string NIF { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public ImageSource ProfileImageSource { get; private set; }

        private User user;
        private readonly bool isShippingCompany;
        private readonly string legal_entity;

        [Obsolete]
        public RegisterViewModel(User newuser, string legal_entity, bool isShippingCompany, string password)
        {
            try
            {
                //handle exception

                user = newuser;

                Birthday = DateTime.Now.AddYears(18);
                BackCommand = new Command(OnBackClicked);
                InsertProfilePhotoCommand = new Command(InsertPhotoClicked);
                SignUpCommand = new Command(SignUpClicked);
                PhoneNumber = user.PhoneNumber;
                Password = password;
                SecoundPassword = password;
                Email = newuser.Email;
                FirstName = newuser.Name;
                LastName = newuser.Surname;
                if (newuser.PersonalInfo != null)
                {
                    Birthday = newuser.PersonalInfo.Birthday;
                    DNI = newuser.PersonalInfo.DNI;

                }
                if (newuser.Address != null)
                {
                    City = newuser.Address.City;
                    ZipCode = newuser.Address.ZipCode;
                    Address = newuser.Address.Address;
                    Province = newuser.Address.Provincie;
                }
                if (newuser.JuridicDetails != null)
                {
                    CompanyName = newuser.JuridicDetails.CompanyName;
                    NIF = newuser.JuridicDetails.NIF;

                }
                Password = string.IsNullOrEmpty(newuser.HashedPassword) ? string.Empty : newuser.HashedPassword;
                if (newuser.Photo != null && newuser.Photo.Length > 0)
                {
                    ProfilePhoto = newuser.Photo;
                    ProfileImageSource = ImageSource.FromStream(() =>
                    {
                        return new MemoryStream(newuser.Photo);
                    });
                    InfoProfilePhotoTxt = $"Foto cargda!";
                    InsertPhotoColor = Colors.DarkGreen;
                }
                else
                {
                    ProfileImageSource = ImageSource.FromResource($"PassingCar.Resources.Images.img_account.png");
                    InfoProfilePhotoTxt = $"Sube tu foto de perfil";
                    InsertPhotoColor = Colors.DarkOrange;
                }
                user = newuser;
                this.isShippingCompany = isShippingCompany;
                this.legal_entity = legal_entity;
               // RequestCameraAndGalleryPermission();
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
                //handle exception
                _ = await RequestCameraAndGalleryPermissions();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnBackClicked()
        {
            try
            {
                //handle exception
                Application.Current.MainPage = new LoginInput();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void InsertPhotoClicked()
        {
            try
            {
                //handle exception
                string result = await App.Current.MainPage.DisplayActionSheet("Seleccionar", "Cancelar", null, "Cámara", "Galería");
                if (result != null)
                {
                    if (result == "Cámara")
                    {
                        TakePhoto();
                    }

                    if (result == "Galería")
                    {
                        SelectPhoto();
                    }
                }
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
                // Check if the gallery is accessible
                if (!MediaPicker.IsCaptureSupported)
                {
                    ShowInvalidInputErrorMessage("Unable to access gallery!");
                    return;
                }

                // Request camera and gallery permissions
                bool isPermissionGranted = await RequestCameraAndGalleryPermissions();
                if (!isPermissionGranted)
                {
                    return;
                }

                // Delay for iOS to ensure UI update
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await Task.Delay(100);
                }

                // Use the MediaPicker to pick a photo
                var photo = await MediaPicker.PickPhotoAsync();

                // Process the photo if available
                if (photo != null)
                {
                     var stream = await photo.OpenReadAsync();

                    // Read the stream into a byte array and set the ProfilePhoto property
                    ProfilePhoto = await ConvertStreamToByteArrayAsync(stream); // Read the photo into a byte array
                    ProfileImageSource = ImageSource.FromStream(() => stream);
                    InfoProfilePhotoTxt = "Foto cargada!";
                    InsertPhotoColor = Colors.DarkGreen;

                    // Notify property changes
                    OnPropertyChanged(nameof(ProfileImageSource));
                    OnPropertyChanged(nameof(InfoProfilePhotoTxt));
                    OnPropertyChanged(nameof(InsertPhotoColor));
                }
            }
            catch (Exception ex)
            {
                // OpeErrorPopUp("InsertPhoto failed", JsonConvert.SerializeObject(ex), "OK");
            }
        }

        // Helper method to convert a stream to a byte array
        private async Task<byte[]> ConvertStreamToByteArrayAsync(Stream input)
        {
            using (var memoryStream = new MemoryStream())
            {
                await input.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void TakePhoto()
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // Check if the camera is available
                    if (!MediaPicker.IsCaptureSupported)
                    {
                        ShowInvalidInputErrorMessage("No camera available.");
                        return;
                    }

                    // Request camera permissions

                    var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (cameraStatus != PermissionStatus.Granted)
                    {
                        if (Permissions.ShouldShowRationale<Permissions.Camera>())
                        {
                            await Application.Current.MainPage.DisplayAlert("Permiso requerido", "Por favor, permita el acceso a la cámara.", "OK");
                        }

                        var cameraRequestResult = await Permissions.RequestAsync<Permissions.Camera>();
                        if (cameraRequestResult != PermissionStatus.Granted)
                        {
                            bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                                                "Permiso denegado",
                                                "Debe habilitar el acceso a la cámara desde la configuración del dispositivo.",
                                                "Ir a configuración",
                                                "Cancelar"
                                            );
                            if (goToSettings)
                            {
                                AppInfo.ShowSettingsUI(); 
                            }
                            return; // Camera permission denied
                        }
                    }
                    // Delay for iOS to ensure UI update
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        await Task.Delay(100);
                    }

                    // Use the MediaPicker to take a photo
                    var photo = await MediaPicker.CapturePhotoAsync();

                    // Process the photo if available
                    if (photo != null)
                    {
                        var stream = await photo.OpenReadAsync();
                        ProfilePhoto = ReadFully(stream); // Read the photo into a byte array
                        ProfileImageSource = ImageSource.FromStream(() => stream);
                        InfoProfilePhotoTxt = "Foto cargada!";
                        InsertPhotoColor = Colors.DarkGreen;

                        // Notify property changes
                        OnPropertyChanged(nameof(ProfileImageSource));
                        OnPropertyChanged(nameof(InfoProfilePhotoTxt));
                        OnPropertyChanged(nameof(InsertPhotoColor));
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Take Photo failed", ex.Message, "OK");
                }
            });
        }

        // Helper method to read a stream to a byte array
        private byte[] ReadFully(Stream input)
        {
            using (var memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
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

                return true; // Both permissions granted
            }
            catch (Exception ex)
            {
                // Handle exceptions as necessary
                Console.WriteLine(ex);
                return false; // Return false on exception
            }
        }
        public async void SignUpClicked()
        {
            try
            {
                //handle exception
                try
                {
                    if (string.IsNullOrEmpty(FirstName))
                    {
                        ShowInvalidInputErrorMessage($"FirstName is required!");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(LastName))
                        {
                            ShowInvalidInputErrorMessage($"LastName is required!");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Email))
                            {
                                ShowInvalidInputErrorMessage($"Email is required!");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(Password))
                                {
                                    ShowInvalidInputErrorMessage($"Password is required!");
                                }
                                else
                                {
                                    if (!Password.Equals(SecoundPassword))
                                    {
                                        ShowInvalidInputErrorMessage($"Passwords didn't match!");
                                    }
                                    else
                                    {
                                        if (ProfilePhoto == null || ProfilePhoto.Length <= 0)
                                        {
                                            ShowInvalidInputErrorMessage($"Profile photo is required!");
                                        }
                                        else
                                        {
                                            try
                                            {
                                                RegistrationType type = user.RegistrationType;
                                                string fbId = user.FacebookId;
                                                string googleId = user.GoogleId;
                                                string appleId = user.AppleId;
                                                user = new User()
                                                {
                                                    Id = user.Id,
                                                    RegistrationType = type,
                                                    FacebookId = fbId,
                                                    GoogleId = googleId,
                                                    AppleId = appleId,
                                                    Name = FirstName,
                                                    Surname = LastName,
                                                    PhoneNumber = PhoneNumber,
                                                    Email = Email,
                                                    EmailVerified = false,
                                                    Photo = ProfilePhoto,
                                                    HashedPassword = Password,
                                                    Address = new AddressInfo()
                                                    {
                                                        Address = Address,
                                                        City = City,
                                                        ZipCode = ZipCode,
                                                        Provincie = Province
                                                    },
                                                    Customer = legal_entity.Equals("fisica"),
                                                    JuridicPerson = legal_entity.Equals("juridica") && !isShippingCompany,
                                                    Uber = legal_entity.Equals("juridica") && isShippingCompany,
                                                    JuridicDetails = new JuridicDetails()
                                                    {
                                                        CompanyName = CompanyName,
                                                        NIF = NIF,
                                                        IsShippingCompany = isShippingCompany
                                                    },
                                                    PersonalInfo = new PersonalInfo()
                                                    {
                                                        Birthday = Birthday,
                                                        DNI = DNI,
                                                        Genre = Genre,
                                                    },
                                                    CreatedAt = DateTime.Now,
                                                };
                                                this.OpenPopUp();
                                                InsertResponse response = await Api.Register(user);
                                                if (response != null)
                                                {
                                                    if (response.Success)
                                                    {
                                                        this.ClosePopUp();
                                                        this.OpeErrorPopUp($"Welcome, {FirstName} {LastName}",
                                                            $"Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                                                            "OK",
                                                            new Action(() =>
                                                            {
                                                                Application.Current.MainPage = new LoginInput();

                                                            }));
                                                        //LoginResponse loginResponse = await Api.Login(new LoginRequest()
                                                        //{
                                                        //    Email = user.Email,
                                                        //    FacebookId = user.FacebookId,
                                                        //    GoogleId = user.GoogleId,
                                                        //    Password = Password
                                                        //}, false);
                                                        //if (loginResponse == null || !loginResponse.Success)
                                                        //{
                                                        //    Application.Current.MainPage = new LoginInput();
                                                        //}
                                                        //else
                                                        //{
                                                        //    LoginViewModel lwm = new LoginViewModel(navigation);
                                                        //    await lwm.InitMenuAfterLogin(loginResponse);

                                                        //    Application.Current.MainPage = new MainMenu();
                                                        //}
                                                    }
                                                    else
                                                    {
                                                        this.ClosePopUp();
                                                        this.OpeErrorPopUp("Register failed", response.ErrorMessage, "OK");
                                                    }
                                                }
                                                else
                                                {
                                                    this.ClosePopUp();
                                                    this.OpeErrorPopUp("Register failed", $"Server didn't respond!", "OK");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                this.OpeErrorPopUp("Register failed", ex.Message, "OK");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("Register failed", JsonConvert.SerializeObject(ex), "OK");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void ShowInvalidInputErrorMessage(string message)
        {
            try
            {
                //handle exception
                this.OpeErrorPopUp("Invalid completed data", message, "OK");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
