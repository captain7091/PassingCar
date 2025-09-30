using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.ViewModels;

namespace PassingCar.Models.API.Ads
{
    public class ShippingsDetailsExtened : BaseViewModel
    {
        public int Id { get; set; }
        private DateTime _createdAt;
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set 
            { 
                _createdAt = value; 
                OnPropertyChanged(nameof(CreatedAt)); 
            } 
        }
        private ShippingState _state = ShippingState.PendingForPickup;
        public ShippingState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(CancelBtnVisibilitiy));
                OnPropertyChanged(nameof(PickUpVisibility));
                OnPropertyChanged(nameof(ConfirmPickUpVisibility));
                OnPropertyChanged(nameof(DropOffVisibility));
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(History));
                OnPropertyChanged(nameof(HistoryVisibility));
                OnPropertyChanged(nameof(InfoMessage));
                OnPropertyChanged(nameof(InfoMessageVisibility));
                OnPropertyChanged(nameof(ReviewVisibility));
                OnPropertyChanged(nameof(GenerateProofBtnVisibilitiy));
                OnPropertyChanged(nameof(ViewProofBtnVisibilitiy));
                OnPropertyChanged("Shippingss");
            }
        }
        public AddressDetails From { get; set; }
        public AddressDetails To { get; set; }
        public bool InfoMessageVisibility => !string.IsNullOrEmpty(InfoMessage);
        public string InfoMessage
        {
            get
            {
                switch (_state)
                {
                    case ShippingState.PendingForPickup:
                        return IsMyAds ? $"Wait for uber to pick up your package" : string.Empty;
                    case ShippingState.UberPickupConfirmed:
                        return IsMyAds ? string.Empty : $"Wait for client to confirm your picked up";
                    case ShippingState.ClientPickupConfirmed:
                        return IsMyAds
                            ? string.IsNullOrEmpty(ConfirmationCode)
                                ? $"Wait for uber to deliver your package. Confirmation code was sent to destination phone number."
                                : $"Wait for uber to deliver your package. Confirmation code was sent to destination phone number. Validation code is: {ConfirmationCode}"
                            : string.Empty;
                    case ShippingState.Delivered:
                        return $"Paquete entregado";
                    case ShippingState.Finalizado:
                        return $"Paquete entregado";
                    case ShippingState.Canceled:
                        return $"Envío cancelado";
                    default:
                        return string.Empty;
                }
            }
        }
        public int OfferId { get; set; }
        private int _adsId;
        public int AdsId 
        { 
            get => _adsId; 
            set 
            { 
                _adsId = value; 
                OnPropertyChanged(nameof(AdsId)); 
            } 
        }
        public string Code1 { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public string Code4 { get; set; }
        public string Code5 { get; set; }
        public bool IsMyAds { get; set; }
        public string ConfirmationCode { get; set; }

        // Add image properties for displaying ad images
        private byte[]? _firstAdsImage;
        public byte[]? FirstAdsImage
        {
            get => _firstAdsImage;
            set
            {
                _firstAdsImage = value;
                OnPropertyChanged(nameof(FirstImage));
            }
        }

        public ImageSource FirstImage
        {
            get
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] FirstImage getter called for shipping {Id}, AdsId: {AdsId}, FirstAdsImage: {(FirstAdsImage == null ? "null" : $"{FirstAdsImage.Length} bytes")}");
                    
                    if (FirstAdsImage != null && FirstAdsImage.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Returning image stream for AdsId {AdsId}");
                        return ImageSource.FromStream(() => new MemoryStream(FirstAdsImage));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Returning fallback image for AdsId {AdsId}");
                        return "empty.jpg";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Error loading image for ads {AdsId}: {ex.Message}");
                    return "empty.jpg";
                }
            }
        }
        public ObservableCollection<ShippmentHistoryModel> History { get; set; }
        private readonly ObservableCollection<ShippingsDetailsExtened> parent;
        public ICommand FromLocationCommand { get; set; }
        public ICommand FromPhoneCommand { get; set; }
        public ICommand ToLocationCommand { get; set; }
        public ICommand ToPhoneCommand { get; set; }
        public ICommand ViewAdsCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand PickUpCommand { get; set; }
        public ICommand ConfirmPickUpCommand { get; set; }
        public ICommand GenerateProofCommand { get; set; }
        public ICommand ViewProofCommand { get; set; }
        public ICommand ReviewCommand { get; set; }
        public ICommand DropOffCommand { get; set; }
        private bool IsShippmentCompany;
        public ShippingsDetailsExtened(ObservableCollection<ShippingsDetailsExtened> parent, ShippingFromListModel item)
        {
            this.parent = parent;
            
            // Handle null item for hardcoded testing
            if (item != null)
            {
                Id = item.Id;
                CreatedAt = item.CreatedAt;
                State = item.State;
                From = item.From;
                To = item.To;
                OfferId = item.OfferId;
                AdsId = item.AdsId;
            }
            else
            {
                // Default values for hardcoded testing
                Id = 0;
                CreatedAt = DateTime.Now;
                State = ShippingState.PendingForPickup;
                From = null;
                To = null;
                OfferId = 0;
                AdsId = 0;
            }
            IsMyAds = item?.IsMyAds ?? false;
            ConfirmationCode = item?.ConfirmationCode ?? string.Empty;
            History = new ObservableCollection<ShippmentHistoryModel>();
            if (item?.History != null)
            {
                foreach (ShippmentHistoryModel hist in item.History)
                {
                    History.Add(hist);
                }
            }
            FromLocationCommand = new Command(FromLocation);
            ToLocationCommand = new Command(ToLocation);
            FromPhoneCommand = new Command(FromPhone);
            ToPhoneCommand = new Command(ToPhone);
            ViewAdsCommand = new Command(ViewAds);
            CancelCommand = new Command(Cancel);
            PickUpCommand = new Command(PickUp);
            ConfirmPickUpCommand = new Command(ConfirmPickUp);
            GenerateProofCommand = new Command(GenerateProof);
            ViewProofCommand = new Command(ViewProof);
            ReviewCommand = new Command(LeaveReview);
            DropOffCommand = new Command(DropOff);
            PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, (input) =>
            {
                ChangesShippingStateInput shipItem = input as ChangesShippingStateInput;
                if (shipItem.ShippingId == Id)
                {
                    State = shipItem.State;
                }
            });
            IsShippmentCompany = false;
            LoadShippentCompany();
            // LoadAdsImage() will be called from MyShippingPage.EnsureImagesLoaded()
        }
        public async void LoadShippentCompany()
        {
            Models.User userLogged = await Api.GetUserData();
            IsShippmentCompany = userLogged != null && userLogged.JuridicDetails != null && userLogged.JuridicDetails.IsShippingCompany;
        }

        public async Task LoadAdsImage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] LoadAdsImage called for shipping {Id}, AdsId: {AdsId}");
                
                if (AdsId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Loading image for AdsId {AdsId}");
                    var imageData = await AdsId.GetAdsPhoto();
                    if (imageData != null && imageData.Length > 0)
                    {
                        FirstAdsImage = imageData;
                        System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Successfully loaded image for AdsId {AdsId}: {imageData.Length} bytes");
                        
                        // Force property change notification
                        OnPropertyChanged(nameof(FirstAdsImage));
                        OnPropertyChanged(nameof(FirstImage));
                        System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Property change notifications sent for AdsId {AdsId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] No image data returned for AdsId {AdsId}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Invalid AdsId: {AdsId} for shipping {Id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Error loading ads image for AdsId {AdsId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ShippingsDetailsExtened] Stack trace: {ex.StackTrace}");
            }
        }
        public bool CancelBtnVisibilitiy => State < ShippingState.UberPickupConfirmed;
        public bool GenerateProofBtnVisibilitiy => !IsMyAds && IsShippmentCompany && State == ShippingState.Delivered;
        public bool ViewProofBtnVisibilitiy => !IsMyAds && IsShippmentCompany && State == ShippingState.Finalizado;
        public bool ReviewVisibility => State == ShippingState.Delivered;
        public bool PickUpVisibility => !IsMyAds && State < ShippingState.UberPickupConfirmed;
        public bool ConfirmPickUpVisibility => IsMyAds && State == ShippingState.UberPickupConfirmed;
        public bool DropOffVisibility => !IsMyAds && (State == ShippingState.ClientPickupConfirmed);
        public bool HistoryVisibility => History != null && History.Count > 0;
        private void FromLocation()
        {
            string sourceAddress = string.Empty;
            if (From != null)
            {
                sourceAddress += $"Ciudad: {From.City}{System.Environment.NewLine}";
                sourceAddress += $"Código postal: {From.ZIPCode}{System.Environment.NewLine}";
                sourceAddress += $"Dirección: {From.Address}{System.Environment.NewLine}";
            }
           // this.OpeErrorPopUp("Dirección de Origen", sourceAddress, "Ok");
        }
        private async void ViewProof()
        {
            await Browser.OpenAsync($"https://api.passingcar.com/Home/GeneratePaymentProofByOrderId?offerId={OfferId}");
        }
        private void FromPhone()
        {
            if (From != null && !string.IsNullOrEmpty(From.PhoneNumber))
            {
                try
                {
                    PhoneDialer.Open(From.PhoneNumber);
                }
                catch (ArgumentNullException anEx)
                {
                    //this.OpeErrorPopUp("Unable to call source", anEx.Message, "Ok");
                }
                catch (FeatureNotSupportedException ex)
                {
                   // this.OpeErrorPopUp("Unable to call source", ex.Message, "Ok");
                }
                catch (Exception ex)
                {
                  //  this.OpeErrorPopUp("Unable to call source", ex.Message, "Ok");
                }
            }
            else
            {
               // this.OpeErrorPopUp("Unable to call source", $"No phone number completed", "Ok");
            }
        }
        private void ToLocation()
        {
            string sourceAddress = string.Empty;
            if (From != null)
            {
                sourceAddress += $"Ciudad: {From.City}{System.Environment.NewLine}";
                sourceAddress += $"Código postal: {From.ZIPCode}{System.Environment.NewLine}";
                sourceAddress += $"Dirección: {From.Address}{System.Environment.NewLine}";
            }
            //this.OpeErrorPopUp("Dirección de Destino", sourceAddress, "Ok");
        }
        private void ToPhone()
        {
            if (To != null && !string.IsNullOrEmpty(To.PhoneNumber))
            {
                try
                {
                    PhoneDialer.Open(To.PhoneNumber);
                }
                catch (ArgumentNullException anEx)
                {
                   // this.OpeErrorPopUp("Unable to call destinatar", anEx.Message, "Ok");
                }
                catch (FeatureNotSupportedException ex)
                {
                   // this.OpeErrorPopUp("Unable to call destinatar", ex.Message, "Ok");
                }
                catch (Exception ex)
                {
                  //  this.OpeErrorPopUp("Unable to call destinatar", ex.Message, "Ok");
                }
            }
            else
            {
              //  this.OpeErrorPopUp("Unable to call destinatar", $"No phone number completed", "Ok");
            }
        }
        private async void ViewAds()
        {
            try
            {
                await this.GoToAds(AdsId);
            }
            catch (Exception ex)
            {
                this.OpeErrorPopUp("Failed to redirect to ads", ex.Message, "Ok");
            }
        }
        private async void Cancel()
        {
            ChangeShippingStateResponse response = await Api.ChangeShippingState(new ChangesShippingStateInput()
            {
                ShippingId = Id,
                State = ShippingState.Canceled
            });
            if (response.Success)
            {
                State = ShippingState.Canceled;
                History.Clear();
                foreach (ShippmentHistoryModel item in response.History)
                {
                    History.Add(item);
                }
               // this.OpeErrorPopUp("Shipping canceled", $"Cancelaste este envío", "Ok");
            }
            else
            {
               // this.OpeErrorPopUp("Error en el envío cancelado", $"{response.ErrorMessage}", "Ok");
            }
        }

        private async void GenerateProof()
        {
            string result = await App.Current.MainPage.DisplayPromptAsync("Billing number", "Provide billing number. It can be set only one time");
            if (!string.IsNullOrEmpty(result))
            {
                ApiBaseResponse response = await Api.UploadNoFactura(new Payment.UploadNoFacturaRequest()
                {
                    OfferId = OfferId,
                    NoFactura = result
                });
                if (response.Success)
                {
                    response = await Api.ChangeShippingState(new ChangesShippingStateInput()
                    {
                        ShippingId = Id,
                        State = ShippingState.Finalizado
                    });
                }
                if (!response.Success)
                {
                    await App.Current.MainPage.DisplayAlert("El número de facturación no se carga", response.ErrorMessage, "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Número de facturación SUBIDO", $"Ahora puedes ver el comprobante de pago adjunto.", "OK");
                }
            }
        }

        private async void LeaveReview()
        {
            //object result = await Application.Current.MainPage.Navigation.ShowPopupAsync(new UserReviewPopUp(Id));
            //if (result != null)
            //{
            //    if (result == "succes")
            //    {
            //        await App.Current.MainPage.DisplayAlert("Your review was submitted!", "Thank you!", "Go Back");
            //    }
            //}
        }

        private async void PickUp()
        {
            ChangeShippingStateResponse response = await Api.ChangeShippingState(new ChangesShippingStateInput()
            {
                ShippingId = Id,
                State = ShippingState.UberPickupConfirmed
            });
            if (response.Success)
            {
                State = ShippingState.UberPickupConfirmed;
                History.Clear();
                foreach (ShippmentHistoryModel item in response.History)
                {
                    History.Add(item);
                }
               // this.OpeErrorPopUp("Envío recogido", $"Espera a que el cliente confirme que recogiste", "Ok");
            }
            else
            {
               // this.OpeErrorPopUp("Error Envío recogido", $"{response.ErrorMessage}", "Ok");
            }
        }
        private async void ConfirmPickUp()
        {
            ChangeShippingStateResponse response = await Api.ChangeShippingState(new ChangesShippingStateInput()
            {
                ShippingId = Id,
                State = ShippingState.ClientPickupConfirmed
            });
            if (response.Success)
            {
                State = ShippingState.ClientPickupConfirmed;
                History.Clear();
                foreach (ShippmentHistoryModel item in response.History)
                {
                    History.Add(item);
                }
                //this.OpeErrorPopUp("Envío confirmado recogido", $"Confirmaste el retiro de ese envío.", "Ok");
            }
            else
            {
               // this.OpeErrorPopUp("Error envío confirmado recogido", $"{response.ErrorMessage}", "Ok");
            }
        }
        private async void DropOff()
        {
            if (!string.IsNullOrEmpty(ConfirmationCode) && ConfirmationCode.Equals($"{Code1}{Code2}{Code3}{Code4}{Code5}", StringComparison.CurrentCultureIgnoreCase))
            {
                ChangeShippingStateResponse response = await Api.ChangeShippingState(new ChangesShippingStateInput()
                {
                    ShippingId = Id,
                    State = ShippingState.Delivered
                });
                if (response.Success)
                {
                    State = ShippingState.Delivered;
                    History.Clear();
                    foreach (ShippmentHistoryModel item in response.History)
                    {
                        History.Add(item);
                    }
                   // this.OpeErrorPopUp("El paquete fue entregado exitosamente", $"Confirmaste la entrega", "Ok");
                }
                else
                {
                   // this.OpeErrorPopUp("Error de envío entregado actualización", $"{response.ErrorMessage}", "Ok");
                }
            }
            else
            {
                //this.OpeErrorPopUp("Código de confirmación inválido", $"El código fue enviado a {To.PhoneNumber}", "Ok");
            }
        }
    }
}

