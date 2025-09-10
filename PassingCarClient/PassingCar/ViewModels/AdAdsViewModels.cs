
using System.Windows.Input;
using Mopups.Services;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API;
using PassingCar.Popups;
using PassingCar.Utils;

namespace PassingCar.ViewModels
{
    public class AdAdsViewModels : BaseViewModel
    {
        public string Title { get; set; }
        public byte[] Photo1 { get; set; }
        public byte[] Photo2 { get; set; }
        public byte[] Photo3 { get; set; }
        public byte[] Photo4 { get; set; }
        public string Description { get; set; }
        public string FromCity { get; set; }
        public string FromZIPCode { get; set; }
        public string FromAddress { get; set; }
        public string FromProvince { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToCity { get; set; }
        public string ToZIPCode { get; set; }
        public string ToAddress { get; set; }
        public string ToProvince { get; set; }
        public string ToPhoneNumber { get; set; }
        public decimal Price { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Weigth { get; set; }
        public bool IsFragile { get; set; }
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsFormEnabled));
            }
        }
        
        public bool IsFormEnabled => !IsLoading;
        
        public ICommand InsertPhoto1Commad { get; set; }
        public ICommand InsertPhoto2Commad { get; set; }
        public ICommand InsertPhoto3Commad { get; set; }
        public ICommand InsertPhoto4Commad { get; set; }
        private ImageSource _img1;
        public ImageSource ImageSource1
        {
            get => _img1;
            set
            {
                _img1 = value;
                OnPropertyChanged(nameof(ImageSource1));
            }
        }
        private ImageSource _img2;
        public ImageSource ImageSource2
        {
            get => _img2;
            set
            {
                _img2 = value;
                OnPropertyChanged(nameof(ImageSource2));
            }
        }
        private ImageSource _img3;
        public ImageSource ImageSource3
        {
            get => _img3;
            set
            {
                _img3 = value;
                OnPropertyChanged(nameof(ImageSource3));
            }
        }
        private ImageSource _img4;
        public ImageSource ImageSource4
        {
            get => _img4;
            set
            {
                _img4 = value;
                OnPropertyChanged(nameof(ImageSource4));
            }
        }

        [Obsolete]
        public AdAdsViewModels()
        {
            try
            {
                IsLoading = true; // Start with loading state
                _img1 = "upload_photo.png";
                _img2 = "upload_photo.png";
                _img3 = "upload_photo.png";
                _img4 = "upload_photo.png";
                InsertPhoto1Commad = new Command(InsertPhoto1Clicked);
                InsertPhoto2Commad = new Command(InsertPhoto2Clicked);
                InsertPhoto3Commad = new Command(InsertPhoto3Clicked);
                InsertPhoto4Commad = new Command(InsertPhoto4Clicked);
                RequestCameraAndGalleryPermission();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                
                // Simulate initialization time (e.g., loading data, setting up form)
                await Task.Delay(1000);
                
                // Any additional initialization logic can go here
                
                IsLoading = false;
            }
            catch (Exception ex)
            {
                IsLoading = false;
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
        
        [Obsolete]
        private async void InsertPhoto1Clicked()
        {
            try
            {
                // Create a TaskCompletionSource to await the result from the popup
                var tcs = new TaskCompletionSource<string>();

                // Show the popup and pass the TaskCompletionSource
                var result = await App.Current.MainPage.DisplayActionSheet("Subir Foto Desde:", "Cancel", null, "Camera", "Gallery");


                if (!string.IsNullOrEmpty(result))
                {
                    if (result == "Camera")
                    {
                        await TakePhoto(1);
                    }
                    else if (result == "Gallery")
                    {
                        await SelectPhoto(1);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle(); // Assuming you have a global error handler
            }
        }

        [Obsolete]
        private async void InsertPhoto2Clicked()
        {
            try
            {
                // Create a TaskCompletionSource to await the result from the popup
                var tcs = new TaskCompletionSource<string>();

                var result = await App.Current.MainPage.DisplayActionSheet("Subir Foto Desde:", "Cancel", null, "Camera", "Gallery");

                if (!string.IsNullOrEmpty(result))
                {
                    if (result == "Camera")
                    {
                        await TakePhoto(2);
                    }
                    else if (result == "Gallery")
                    {
                        await SelectPhoto(2);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void InsertPhoto3Clicked()
        {
            try
            {
                // Create a TaskCompletionSource to await the result from the popup
                var tcs = new TaskCompletionSource<string>();

                // Show the popup and pass the TaskCompletionSource
                var result = await App.Current.MainPage.DisplayActionSheet("Subir Foto Desde:", "Cancel", null, "Camera", "Gallery");


               

                if (!string.IsNullOrEmpty(result))
                {
                    if (result == "Camera")
                    {
                        await TakePhoto(3);
                    }
                    else if (result == "Gallery")
                    {
                        await SelectPhoto(3);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void InsertPhoto4Clicked()
        {
            try
            {
                // Create a TaskCompletionSource to await the result from the popup
                var tcs = new TaskCompletionSource<string>();

                // Show the popup and pass the TaskCompletionSource
              // await MopupService.Instance.PushAsync(new MediaChoosePopUp(tcs));
              var result = await App.Current.MainPage.DisplayActionSheet("Subir Foto Desde:", "Cancel", null, "Camera", "Gallery");
                // Await the result from the popup
              //  string result = await tcs.Task;

                if (!string.IsNullOrEmpty(result))
                {
                    if (result == "Camera")
                    {
                        await TakePhoto(4);
                    }
                    else if (result == "Gallery")
                    {
                        await SelectPhoto(4);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

       
        private async Task SelectPhoto(int index)
        {
            try
            {
                // Request photo selection from the gallery
                var result = await PickPhotoAsync();
 
                if (result != null)
                {
                   using var stream = await result.OpenReadAsync();
                    byte[] imageBytes = new byte[stream.Length];
                    await stream.ReadAsync(imageBytes, 0, (int)stream.Length);
                    
                    string filePath = result.FullPath;
                    
                    // Assign photo and image source based on the index
                    switch (index)
                    {
                        case 1:
                            Photo1 = imageBytes;
                            ImageSource1 = ImageSource.FromFile(filePath);
                            break;
                        case 2:
                            Photo2 = imageBytes;
                            ImageSource2 = ImageSource.FromFile(filePath);
                            break;
                        case 3:
                            Photo3 = imageBytes;
                            ImageSource3 = ImageSource.FromFile(filePath);
                            break;
                        case 4:
                            Photo4 = imageBytes;
                            ImageSource4 = ImageSource.FromFile(filePath);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to pick photo: {ex.Message}", "OK");
            }
        }
        public async Task<FileResult> PickPhotoAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    // Request permission if not granted
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();

                    // If permission is still not granted, show an alert and exit
                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied", "Storage permission is required to pick a photo.", "Ok");
                        return null;
                    }
                }

                
                var photo = await Microsoft.Maui.Media.MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (photo != null)
                {
                    return photo;
                }

                return null;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
                return null;
            }
            
        }

        [Obsolete]
        private async Task TakePhoto(int index)
        {
            try
            {
                
              
                var result = await CapturePhotoAsync();
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    byte[] imageBytes = new byte[stream.Length];
                    await stream.ReadAsync(imageBytes, 0, (int)stream.Length);

                    string filePath = result.FullPath;

                    // Assign photo and image source based on the index
                    switch (index)
                    {
                        case 1:
                            Photo1 = imageBytes;
                            ImageSource1 = ImageSource.FromFile(filePath);
                            break;
                        case 2:
                            Photo2 = imageBytes;
                            ImageSource2 = ImageSource.FromFile(filePath);
                            break;
                        case 3:
                            Photo3 = imageBytes;
                            ImageSource3 = ImageSource.FromFile(filePath);
                            break;
                        case 4:
                            Photo4 = imageBytes;
                            ImageSource4 = ImageSource.FromFile(filePath);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
            }
        }
        public async Task<FileResult> CapturePhotoAsync()
        {
            try
            {
                if (!MediaPicker.IsCaptureSupported)
                {
                    await Application.Current.MainPage.DisplayAlert("","Camera is not available","Ok");
                    return null;
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
                        return null;
                    }
                }

                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a photo"
                });

                if (photo != null)
                {
                
                    return photo;
                }

                return null;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
                return null;
            }
           
        }

        [Obsolete]
        public async Task<bool> RequestCameraAndGalleryPermissions()
        {
            try
            {
                // Request camera permission
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                }

                // Request storage permission (optional for MediaPicker)
                var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (storageStatus != PermissionStatus.Granted)
                {
                    storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                // If both are granted, return true
                return cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Permission Error", $"Failed to get permissions: {ex.Message}", "OK");
                return false;
            }
        }


        private void ShowInvalidInputErrorMessage(string message)
        {
            try
            {
                //handle exception
                this.OpeErrorPopUp("Datos completados no válidos", message, "OK");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task Submit(string from_prov, string to_prov)
        {
            bool popupOpened = false;
            try
            {
                System.Diagnostics.Debug.WriteLine("[AddAds] Starting ad submission process...");
                System.Diagnostics.Debug.WriteLine($"[AddAds] Title: '{Title}', Price: {Price}, From: {FromCity}, To: {ToCity}");

                // Validate required parameters
                if (string.IsNullOrWhiteSpace(from_prov) || string.IsNullOrWhiteSpace(to_prov))
                {
                    this.OpeErrorPopUp("Error de validación", "Las provincias de origen y destino son requeridas.", "OK");
                    return;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(Title))
                {
                    this.OpeErrorPopUp("Error de validación", "El título es requerido.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Description))
                {
                    this.OpeErrorPopUp("Error de validación", "La descripción es requerida.", "OK");
                    return;
                }

                if (Price <= 0)
                {
                    this.OpeErrorPopUp("Error de validación", "El precio debe ser mayor a 0.", "OK");
                    return;
                }

                // Additional validation for dimensions if provided
                if (Length < 0 || Width < 0 || Height < 0)
                {
                    this.OpeErrorPopUp("Error de validación", "Las dimensiones no pueden ser negativas.", "OK");
                    return;
                }

                if (!string.IsNullOrEmpty(Weigth) && decimal.TryParse(Weigth, out decimal weight) && weight < 0)
                {
                    this.OpeErrorPopUp("Error de validación", "El peso no puede ser negativo.", "OK");
                    return;
                }

                //handle exception
                this.OpenPopUp();
                popupOpened = true;
                FromProvince = from_prov;
                ToProvince = to_prov;

                try
                {
                    // Get profile type with null check
                    ProfileType profileType = ProfileType.Fisica;
                    try
                    {
                        profileType = await Api.GetProfile();
                    }
                    catch (Exception profileEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AddAds] Warning: Could not get profile type: {profileEx.Message}");
                        profileType = ProfileType.Fisica; // Default fallback
                    }

                    var adModel = new Models.API.Ads.AddAdsModel()
                    {
                        Title = Title ?? string.Empty,
                        Photo1 = Photo1 != null ? Convert.ToBase64String(Photo1) : null,
                        Photo2 = Photo2 != null ? Convert.ToBase64String(Photo2) : null,
                        Photo3 = Photo3 != null ? Convert.ToBase64String(Photo3) : null,
                        Photo4 = Photo4 != null ? Convert.ToBase64String(Photo4) : null,
                        Description = Description ?? string.Empty,
                        From = new Models.API.Ads.AddressDetails()
                        {
                            City = FromCity ?? string.Empty,
                            ZIPCode = FromZIPCode ?? string.Empty,
                            Address = FromAddress ?? string.Empty,
                            PhoneNumber = FromPhoneNumber ?? string.Empty,
                            Provincie = FromProvince ?? string.Empty,
                        },
                        To = new Models.API.Ads.AddressDetails()
                        {
                            City = ToCity ?? string.Empty,
                            ZIPCode = ToZIPCode ?? string.Empty,
                            Address = ToAddress ?? string.Empty,
                            PhoneNumber = ToPhoneNumber ?? string.Empty,
                            Provincie = ToProvince ?? string.Empty,
                        },
                        Price = Price,
                        Size = new Models.API.Ads.SizeDetails()
                        {
                            Length = Length,
                            Width = Width,
                            Height = Height,
                        },
                        Weigth = Weigth,
                        IsFragile = IsFragile,
                        ProfileType = profileType
                    };

                    System.Diagnostics.Debug.WriteLine($"[AddAds] Calling API with ad model...");
                    InsertResponse response = await Api.AddAds(adModel);

                    System.Diagnostics.Debug.WriteLine($"[AddAds] API Response received:");
                    System.Diagnostics.Debug.WriteLine($"[AddAds] - Success: {response?.Success}");
                    System.Diagnostics.Debug.WriteLine($"[AddAds] - Id: {response?.Id}");
                    System.Diagnostics.Debug.WriteLine($"[AddAds] - ErrorMessage: '{response?.ErrorMessage}'");

                    if (response != null)
                    {
                        if (!response.Success)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AddAds] ERROR: Ad submission failed - {response.ErrorMessage}");
                            this.OpeErrorPopUp("Error al publicar anuncio", response.ErrorMessage ?? "Error desconocido del servidor", "Ok");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AddAds] SUCCESS: Ad created with ID {response.Id}");

                            try
                            {
                                // Save the newly created ad to local database immediately
                                await SaveNewAdToLocalDatabase(response.Id, adModel);
                            }
                            catch (Exception saveEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[AddAds] Warning: Failed to save ad to local database: {saveEx.Message}");
                                // Continue with navigation even if local save fails
                            }


                            try
                            {
                                await Task.Delay(1000);
                                _ = await Shell.Current.Navigation.PopAsync();
                                await Shell.Current.GoToAsync("//MyAdsPage");
                            }
                            catch (Exception navEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[AddAds] Warning: Navigation failed: {navEx.Message}");
                                this.OpeErrorPopUp("Anuncio publicado", "El anuncio se publicó correctamente, pero hubo un problema con la navegación. Por favor, actualiza la página.", "Ok");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[AddAds] ERROR: Null response from API");
                        this.OpeErrorPopUp("Error de conexión", "No se pudo conectar con el servidor. Verifica tu conexión a internet e inténtalo de nuevo.", "Ok");
                    }
                }
                catch (TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[AddAds] ERROR: Task was cancelled (timeout)");
                    this.OpeErrorPopUp("Tiempo agotado", "La operación tardó demasiado tiempo. Verifica tu conexión e inténtalo de nuevo.", "Ok");
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddAds] ERROR: HTTP request failed: {httpEx.Message}");
                    this.OpeErrorPopUp("Error de red", "Problema de conexión con el servidor. Verifica tu conexión a internet.", "Ok");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddAds] ERROR: Unexpected exception: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[AddAds] Stack trace: {ex.StackTrace}");
                    this.OpeErrorPopUp("Error inesperado", $"Ocurrió un error inesperado: {ex.Message}", "Ok");
                    _ = ex.Handle();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddAds] CRITICAL ERROR: Outer exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AddAds] Stack trace: {ex.StackTrace}");
                this.OpeErrorPopUp("Error crítico", "Ocurrió un error crítico durante la publicación del anuncio. Por favor, inténtalo de nuevo.", "Ok");
                _ = ex.Handle();
            }
            finally
            {
                // Ensure popup is always closed
                if (popupOpened)
                {
                    try
                    {
                        this.ClosePopUp();
                    }
                    catch (Exception closeEx)
                    {
                        System.Diagnostics.Debug.WriteLine("[AddAds] Warning: Failed to close popup: " + closeEx.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Save newly created ad to local database immediately after successful API creation
        /// </summary>
        private async Task SaveNewAdToLocalDatabase(int adId, Models.API.Ads.AddAdsModel adModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AddAds] Saving newly created ad (ID: {adId}) to local database...");

                // Get current user info
                var currentUser = await Api.GetUserData();
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddAds] ERROR: Could not get current user data");
                    return;
                }

                // DEBUG: Log profile information during ad creation
                System.Diagnostics.Debug.WriteLine($"[AddAds] Creating ad with ProfileType: {adModel.ProfileType} (from adModel)");
                System.Diagnostics.Debug.WriteLine($"[AddAds] Current user: ID={currentUser.Id}, Name='{currentUser.Name}'");
                
                // Create LocalAd object from the submitted ad model
                var adsDetails = new Models.API.Ads.AdsDetailsExtened(false)
                {
                    AdsId = adId,
                    AdsTitle = adModel.Title,
                    AdsFrom = Newtonsoft.Json.JsonConvert.SerializeObject(adModel.From),
                    AdsTo = Newtonsoft.Json.JsonConvert.SerializeObject(adModel.To),
                    AdsPrice = adModel.Price,
                    FirstAdsImage = !string.IsNullOrEmpty(adModel.Photo1) ? Convert.FromBase64String(adModel.Photo1) : 
                                   !string.IsNullOrEmpty(adModel.Photo2) ? Convert.FromBase64String(adModel.Photo2) :
                                   !string.IsNullOrEmpty(adModel.Photo3) ? Convert.FromBase64String(adModel.Photo3) :
                                   !string.IsNullOrEmpty(adModel.Photo4) ? Convert.FromBase64String(adModel.Photo4) : null,
                    IsFavorite = false,
                    PostedTime = DateTime.Now,
                    UserName = currentUser.Name,
                    UserProfilePhoto = currentUser.Photo,
                    UserRating = 0, // Default rating since User model doesn't have Rating
                    UserId = currentUser.Id,
                    UserProfile = adModel.ProfileType,
                    ModifiedAt = DateTime.Now,
                    State = "Disponible" // Posted state in Spanish
                };
                
                System.Diagnostics.Debug.WriteLine($"[AddAds] AdsDetails created with UserProfile: {adsDetails.UserProfile}");

                var localAd = new LocalDatabase.LocalAd(adsDetails, Models.AdsState.Posted, currentUser.Id, DateTime.Now);

                bool saved = await App.LocalDatabase.SaveAds(new List<LocalDatabase.LocalAd> { localAd });

                if (saved)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddAds] SUCCESS: New ad saved to local database with ID {adId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AddAds] WARNING: Failed to save new ad to local database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddAds] ERROR saving ad to local database: {ex.Message}");
                _ = ex.Handle();
            }
        }
    }
}
