using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Chat;
using PassingCar.Models.API.Payment;
using PassingCar.ViewModels;
using PassingCar.Views;
//using Plugin.XamarinFormsSaveOpenPDFPackage;

using System.Collections.ObjectModel;

using System.Windows.Input;

using static PassingCar.Models.API.Ads.OfferDetailsExtended;

namespace PassingCar.Models.API.Ads
{
    public class GetAdsDetailsResponse : ApiBaseResponse
    {
        public AdsMoreDetails AdsMoreDetails { get; set; }
    }
    public class AdsMoreDetails
    {
        public List<byte[]> NextPhotos { get; set; }
        public AdsState State { get; set; }
        public string Description { get; set; }
        public string JuridicDetails { get; set; }
        public bool IsMyAds { get; set; }
        public List<GroupOfOffers> OffersGroups { get; set; }
        public List<OfferDetails> SentOffers { get; set; }
        public int? ChatIdForViewer { get; set; }
        public int ViewCounter { get; set; }
        public int? ChatId { get; set; }
    }
    public class GroupOfOffers
    {
        public byte[] UserProfilePhoto { get; set; }
        public double UserRating { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public int? ChatId { get; set; }
        public int UserId { get; set; }
        public List<OfferDetails> Offers { get; set; }
    }
    public class OfferDetails
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public OfferState State { get; set; }
    }
    public class AdsPhotosModel
    {
        public byte[] Photo1 { get; set; }
        public byte[] Photo2 { get; set; }
        public byte[] Photo3 { get; set; }
        public byte[] Photo4 { get; set; }
    }
    public class GroupOffersDetailsExtended : BaseViewModel
    {
        private byte[] _userProfilePhoto;
        public byte[] UserProfilePhoto
        {
            get => _userProfilePhoto;
            set
            {
                _userProfilePhoto = value;
                if (value != null)
                {
                    OnPropertyChanged(nameof(UserProfilePhotoImg));
                }
            }
        }
        public double UserRating { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public int? ChatId { get; set; }
        public int UserId { get; set; }
        public List<OfferDetails> Offers { get; set; }
        public GroupOffersDetailsExtended(GroupOfOffers offerDetails, OnOffersChange onOffersChange = null)
        {
            UserRating = offerDetails.UserRating;
            UserProfilePhoto = offerDetails.UserProfilePhoto;
            UserName = offerDetails.UserName;
            UserType = offerDetails.UserType;
            ChatId = offerDetails.ChatId;
            UserId = offerDetails.UserId;
            Offers = offerDetails.Offers;
            if (Offers != null && Offers.Count > 0)
            {
                GroupOffers = new ObservableCollection<OfferDetailsExtended>();
                Offers = Offers.OrderByDescending(o => o.CreatedAt).ToList();
                foreach (OfferDetails item in Offers)
                {
                    GroupOffers.Add(new OfferDetailsExtended(item, onOffersChange));
                }
            }
        }
        public ObservableCollection<OfferDetailsExtended> GroupOffers { get; set; }
        private ObservableCollection<OfferDetailsExtended> hiddenOffersGroup;
        public Command ShowHideCommand => new Command(ShowHideOffers);
        public Command OpenChatCommand => new Command(OpenChat);
        private async void OpenChat()
        {
            try
            {
                GetChatDetailsResponse chatDetails = await Api.GetChatDetails(new GetChatDetailsInput()
                {
                    ChatId = ChatId.Value
                });
                chatDetails.Result.Chat.Id = ChatId.Value;
            //    ChatPage chatPage = new ChatPage(new ChatDetailsExtended(chatDetails.Result));
               // await Application.Current.MainPage.Navigation.PushAsync(chatPage);
              //  await chatPage.AsyncLoad();
            }
            catch (Exception ex)
            {
                this.OpeErrorPopUp("Cannot open this page", ex.Message, "Ok");
            }
        }
    private void ShowHideOffers()
    {
      try
      {
        if (hiddenOffersGroup != null && hiddenOffersGroup.Count > 0)
        {
          // Show offers (from hidden to visible)
          foreach (OfferDetailsExtended item in hiddenOffersGroup)
          {
            if (item != null) // Null check
            {
              GroupOffers.Add(item);
            }
          }
          hiddenOffersGroup = null;
        }
        else
        {
          // Hide offers (from visible to hidden)
          hiddenOffersGroup = new ObservableCollection<OfferDetailsExtended>();
          foreach (OfferDetailsExtended item in GroupOffers)
          {
            if (item != null) // Null check
            {
              hiddenOffersGroup.Add(item);
            }
          }
          GroupOffers.Clear();
        }
      }
      catch (Exception ex)
      {
        _ = ex.Handle();
      }
    }
    public ImageSource UserProfilePhotoImg => UserProfilePhoto != null && UserProfilePhoto.Length > 0
                    ? ImageSource.FromStream(() => new MemoryStream(UserProfilePhoto))
                    : ImageSource.FromResource($"PassingCar.Resources.Images.img_account.png");
        public ImageSource UserStar1 => GetStarImage(1);
        public ImageSource UserStar2 => GetStarImage(2);
        public ImageSource UserStar3 => GetStarImage(3);
        public ImageSource UserStar4 => GetStarImage(4);
        public ImageSource UserStar5 => GetStarImage(5);
        private ImageSource GetStarImage(int countStar)
        {
            return !(UserRating > 0)
                ? null
                : (UserRating - countStar) >= 0
                    ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star_filled.png")
                    : (UserRating - countStar) >= -0.5
                                    ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star_half.png")
                                    : ImageSource.FromResource($"PassingCar.Resources.Images.a_star.png");
        }

    }
    public class OfferDetailsExtended : BaseViewModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public OfferState State { get; set; }
        public delegate void OnOffersChange();
        public OnOffersChange OnOffersChanged { get; set; }
        public OfferDetailsExtended(OfferDetails offer, OnOffersChange onOffersChange)
        {
            OnOffersChanged = onOffersChange;
            Id = offer.Id;
            Price = offer.Price;
            State = offer.State;
            CreatedAt = offer.CreatedAt;
            OnOffersChanged += UpdateBtnVisibility;
        }
        public ICommand AcceptCommand => new Command(AcceptOffer);
        private async void AcceptOffer()
        {
            this.OpenPopUp();
            ApiBaseResponse response = await Api.ChangeOfferState(new ChangeOfferStateInput()
            {
                OfferId = Id,
                State = OfferState.PaymentPending
            });
            if (response != null)
            {
                if (response.Success == false)
                {
                    this.OpeErrorPopUp("Failed to Accept offer", response.ErrorMessage, "INTENTALO DE NUEVO");
                }
                else
                {
                    State = OfferState.PaymentPending;
                    OnOffersChanged?.Invoke();
                }
            }
            else
            {
                this.OpeErrorPopUp("Failed to Accept offer", $"Tal vez perdiste la conexión a Internet.", "INTENTALO DE NUEVO");
            }
            this.ClosePopUp();
        }

        public ICommand GenerateProofCommand => new Command(GenerateProof);
        private async void GenerateProof()
        {
            await Browser.OpenAsync($"https://api.passingcar.com/Home/PaymentProofByOrderId?offerId={Id}");
        }

        public ICommand StripeProofCommand => new Command(StripeProof);
        private async void StripeProof()
        {
            try
            {
                GetPaymentDetailsResponse details = await Api.GetPaymentDetails(Id);
                if (details != null && details.PaymentDetails != null && details.PaymentDetails.ChargeResponse != null && !string.IsNullOrEmpty(details.PaymentDetails.ChargeResponse.ReceiptUrl))
                {
                    await Browser.OpenAsync(details.PaymentDetails.ChargeResponse.ReceiptUrl);
                }
                else if (details != null && !details.Success && !string.IsNullOrEmpty(details.ErrorMessage))
                {
                    this.OpeErrorPopUp($"Cannot acces receipt from Stripe", details.ErrorMessage, "Ok");
                }
            }
            catch (Exception ex)
            {
                this.OpeErrorPopUp($"Cannot acces receipt from Stripe", ex.Message, "Ok");
            }
        }
        public ICommand RejectCommand => new Command(RejectOffer);
        private async void RejectOffer()
        {
            this.OpenPopUp();
            ApiBaseResponse response = await Api.ChangeOfferState(new ChangeOfferStateInput()
            {
                OfferId = Id,
                State = OfferState.Rejected
            });
            if (response != null)
            {
                if (response.Success == false)
                {
                    this.OpeErrorPopUp("Failed to Reject offer", response.ErrorMessage, "INTENTALO DE NUEVO");
                }
                else
                {
                    State = OfferState.Rejected;
                    OnOffersChanged?.Invoke();
                }
            }
            else
            {
                this.OpeErrorPopUp("Failed to Reject offer", $"Tal vez perdiste la conexión a Internet.", "INTENTALO DE NUEVO");
            }
            this.ClosePopUp();
        }
        public ICommand CancelCommand => new Command(CancelOffer);
        private async void CancelOffer()
        {
            this.OpenPopUp();
            ApiBaseResponse response = await Api.ChangeOfferState(new ChangeOfferStateInput()
            {
                OfferId = Id,
                State = OfferState.Canceled
            });
            if (response != null)
            {
                if (response.Success == false)
                {
                    this.OpeErrorPopUp("Failed to Cancel offer", response.ErrorMessage, "INTENTALO DE NUEVO");
                }
                else
                {
                    State = OfferState.Canceled;
                    OnOffersChanged?.Invoke();
                }
            }
            else
            {
                this.OpeErrorPopUp("Failed to Cancel offer", $"Tal vez perdiste la conexión a Internet.", "INTENTALO DE NUEVO");
            }
            this.ClosePopUp();
        }
        public ICommand AbortCommand => new Command(AbortOffer);
        private async void AbortOffer()
        {
            this.OpenPopUp();
            ApiBaseResponse response = await Api.ChangeOfferState(new ChangeOfferStateInput()
            {
                OfferId = Id,
                State = OfferState.Canceled
            });
            if (response != null)
            {
                if (response.Success == false)
                {
                    this.OpeErrorPopUp("Failed to Cancel offer", response.ErrorMessage, "INTENTALO DE NUEVO");
                }
                else
                {
                    State = OfferState.Canceled;
                    OnOffersChanged?.Invoke();
                }
            }
            else
            {
                this.OpeErrorPopUp("Failed to Cancel offer", $"Tal vez perdiste la conexión a Internet.", "INTENTALO DE NUEVO");
            }
            this.ClosePopUp();
        }
        public ICommand PaymentCommand => new Command(PaymentOffer);
        private async void PaymentOffer()
        {
            //navigation.OpenPopUp();
           PaymentPage paymentPage = new PaymentPage(Id, Price);
           await Application.Current.MainPage.Navigation.PushAsync(paymentPage);
            //navigation.ClosePopUp();
        }
        public bool RejectedOfferVisibility => State == OfferState.Rejected;
        public bool AcceptedOfferVisibility => State == OfferState.Accepted || State == OfferState.PaymentPending;
        public bool CanceledOfferVisibility => State == OfferState.Canceled;
        public bool AbortBtnVisibility => State == OfferState.Accepted;
        public bool PaymentBtnVisibility => State == OfferState.PaymentPending;
        public bool GeneratePDFVisibility => State == OfferState.Accepted;
        public bool AcceptRejectCancelBtnVisibility => State < OfferState.PaymentPending;
        private void UpdateBtnVisibility()
        {
            OnPropertyChanged(nameof(RejectedOfferVisibility));
            OnPropertyChanged(nameof(AcceptedOfferVisibility));
            OnPropertyChanged(nameof(CanceledOfferVisibility));
            OnPropertyChanged(nameof(AbortBtnVisibility));
            OnPropertyChanged(nameof(PaymentBtnVisibility));
            OnPropertyChanged(nameof(GeneratePDFVisibility));
            OnPropertyChanged(nameof(AcceptRejectCancelBtnVisibility));
        }
    }
}
