using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Models.API.Ads;
using PassingCar.Models.API.Chat;
using PassingCar.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;


namespace PassingCar.ViewModels
{
    public class SingleAdsViewModel : BaseViewModel
    {
        private bool isFavorite;
        public bool IsFavorite
        {
            get => isFavorite;
            set
            {
                if (isFavorite != value)
                {
                    isFavorite = value;
                    OnPropertyChanged(nameof(IsFavorite)); // Notify the UI of changes
                }
            }
        }
        public ICommand MoreCommand { get; }
        private AdsState _state;
        public string State
        {
            get => _state.Espana();
            set
            {
                try
                {
                    //handle exception
                    _state = (AdsState)Enum.Parse(typeof(AdsState), value);
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(RepostBtnVisibility));
                    OnPropertyChanged(nameof(DisableBtnVisibility));
                    OnPropertyChanged(nameof(NewOfferVisibility));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }
        public AdsDetailsExtened AdsDetails { get; set; }
        public AdsMoreDetails AdsMoreDetails { get; set; }
        private ObservableCollection<GroupOffersDetailsExtended> _offersGroup;
        public ObservableCollection<GroupOffersDetailsExtended> OffersGroup
        {
            get => _offersGroup;
            set
            {
                try
                {
                    //handle exception
                    _offersGroup = value;
                    OnPropertyChanged(nameof(OffersGroup));
                    OnPropertyChanged(nameof(OffersHeader));
                    OnPropertyChanged(nameof(OpenChatVisibility));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }

        public ObservableCollection<OfferDetailsExtended> SentOffers { get; set; }
        public bool OpenChatVisibility => IsNotMyAds && SentOffers != null && SentOffers.Count > 0;
        public string OffersHeader
        {
            get
            {
                try
                {
                    //handle exception
                    return IsNotMyAds
                        ? SentOffers != null && SentOffers.Count > 0 ? $"Ofertas enviadas" : string.Empty
                        : OffersGroup != null && OffersGroup.Count > 0 ? $"Ofertas" : $"No tienes ofertas";
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                    return $"No tienes ofertas";
                }
            }
        }
        private ObservableCollection<GroupOffersDetailsExtended> hiddenOffersGroup;
        private ObservableCollection<OfferDetailsExtended> hiddenSentOffers;
        public Command ShowHideCommand => new Command(ShowHideOffers);
        public Command OpenChatCommand => new Command(OpenChat);
        public bool IsNotMyAds => AdsMoreDetails != null && !AdsMoreDetails.IsMyAds;
        public bool NewOfferVisibility => !(SentOffers != null && SentOffers.Count > 0 && SentOffers.Where(o => o.State <= OfferState.Seen || o.State == OfferState.PaymentPending || o.State == OfferState.Accepted).Count() > 0);
        private async void UpdateSentOffers()
        {
            try
            {
                if (SentOffers != null && SentOffers.Count == 1 && reloadOffers)
                {
                    this.OpenPopUp();
                    await AsyncLoad();
                    this.ClosePopUp();
                }
                OnPropertyChanged(nameof(OffersGroup));
                OnPropertyChanged(nameof(SentOffers));
                OnPropertyChanged(nameof(OffersHeader));
                OnPropertyChanged(nameof(NewOfferVisibility));
                OnPropertyChanged(nameof(OpenChatVisibility));
                //handle exception
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public string UserTypeText
        {
            get
            {
                if (AdsDetails.UserProfile == Utils.ProfileType.Fisica)
                    return $"Persona Física";
                else
                    return $"Persona Juridica";
            }
        }
        private bool reloadOffers;
        public string JuridicCIF
        {
            get
            {
                try
                {

                    if (AdsDetails.UserProfile == Utils.ProfileType.Fisica)
                        return string.Empty;
                    else
                    {  //handle exception
                        if (AdsMoreDetails != null && !string.IsNullOrEmpty(AdsMoreDetails.JuridicDetails))
                        {
                            JuridicPersonDetails juridicDetails = JsonConvert.DeserializeObject<JuridicPersonDetails>(AdsMoreDetails.JuridicDetails);
                            return juridicDetails != null && !string.IsNullOrEmpty(juridicDetails.NIF) ? $"NIF: {juridicDetails.NIF}" : string.Empty;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                    return string.Empty;
                }
            }
        }
        public bool DescriptionIsLonger
        {
            get
            {
                try
                {
                    //handle exception
                    return AdsMoreDetails != null && !string.IsNullOrEmpty(AdsMoreDetails.Description) && AdsMoreDetails.Description.Length > 100;
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                    return false;
                }
            }
        }
        public bool DescriptionIsNoLonger
        {
            get
            {
                try
                {
                    //handle exception
                    return AdsMoreDetails == null || string.IsNullOrEmpty(AdsMoreDetails.Description)
|| !(AdsMoreDetails.Description.Length > 100);
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                    return false;
                }
            }
        }
        public ICommand FavoriteImageSingle => new Command(AddToFavoriteAction);
        public ICommand TakeTheOfferCommand => new Command(TakeTheOffer);
        public ICommand NewOfferCommand => new Command(NewOffer);
        public ICommand DisableCommand => new Command(DisableAdd);
        public ICommand RepostCommand => new Command(RepostAdd);
        public ICommand ShareCommand => new Command(ShareAd);
        public ObservableCollection<CarouselItem> CarouselItems { get; set; }
        public string NewOfferValue { get; set; }
        public SingleAdsViewModel(AdsDetailsExtened adsToShow)
        {
            try
            {
                //handle exception
                reloadOffers = true;
                this.OpenPopUp();
                AdsDetails = adsToShow;
                State = adsToShow.State.ReplaceEspana();
                MoreCommand = new Command(() =>
                {
                    this.OpeErrorPopUp("HEY", "Run your custom logic here", "Ok");
                });
                CarouselItems = new ObservableCollection<CarouselItem>();
                if (AdsDetails != null)
                {
                    CarouselItems.Add(new CarouselItem()
                    {
                        ImageSource = AdsDetails.FirstImage
                    });
                }
                else
                {
                    CarouselItems.Add(new CarouselItem()
                    {
                        ImageSource = "empty.jpg"
                    });
                }
                OffersGroup = new ObservableCollection<GroupOffersDetailsExtended>();
                IsFavorite = AdsDetails.IsFavorite;
                SentOffers = new ObservableCollection<OfferDetailsExtended>();
             
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void UpdateEvent()
        {
            PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.AdsStateChanged, (input) =>
            {
                ChangeAdsStateInput item = input as ChangeAdsStateInput;
                State = item.State.ToString();
            });
            PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.NewOffer, (input) =>
            {
                SendOfferInput item = input as SendOfferInput;
                if (item.AdId == AdsDetails.AdsId)
                {
                    if (!IsNotMyAds)
                    {
                        AsyncOffersReload();
                    }
                }
            });
            PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.OfferStateChanged, (input) =>
            {
                ChangeOfferStateInput item = input as ChangeOfferStateInput;


                if (OffersGroup.Any(oex => oex.Offers.Any(o => o.Id == 0 || o.Id.Equals(item.OfferId))) || SentOffers.Any(o => o.Id == 0 || o.Id.Equals(item.OfferId)))
                {
                    AsyncOffersReload();
                }
            });
        }
        private async void AddToFavoriteAction()
        {
            try
            {
                //handle exception
                AdsDetails.IsFavorite = !AdsDetails.IsFavorite;
                _ = !AdsDetails.IsFavorite
                    ? await App.LocalDatabase.RemoveFromFavorite(AdsDetails.AdsId)
                    : await App.LocalDatabase.AddToFavorite(AdsDetails.AdsId);
                IsFavorite = AdsDetails.IsFavorite;

            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void OpenChat()
        {
            try
            {
                //handle exception
                try
                {
                    this.OpenPopUp();
                    GetChatDetailsResponse chatDetails = await Api.GetChatDetails(new GetChatDetailsInput()
                    {
                        ChatId = AdsMoreDetails.ChatIdForViewer.Value
                    });
                    chatDetails.Result.Chat.Id = AdsMoreDetails.ChatIdForViewer.Value;
                    ChatDetailsExtended chatDetailsExtended = new ChatDetailsExtended(chatDetails.Result);
                    ChatPage chatPage = new ChatPage(chatDetailsExtended);
                    this.ClosePopUp();
                   await App.Current.MainPage.Navigation.PushAsync(chatPage);
                    await chatPage.AsyncLoad();
                }
                catch (Exception)
                {
                    this.ClosePopUp();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public bool DisableBtnVisibility => !IsNotMyAds && _state < AdsState.PaymentPending;
        private async void DisableAdd()
        {
            try
            {
                //handle exception
                State = AdsState.Disabled.ToString();
                ApiBaseResponse result = await Api.ChangeAdsState(new ChangeAdsStateInput()
                {
                    AdId = AdsDetails.AdsId,
                    State = AdsState.Disabled
                });
                if (result != null && result.Success == false)
                {
                    this.OpeErrorPopUp($"Failed to disable your add", result.ErrorMessage, "Ok");
                }
                else if (result == null)
                {
                    this.OpeErrorPopUp($"Failed to disable your add", $"Server error", "Ok");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public bool RepostBtnVisibility => !IsNotMyAds && _state == AdsState.Disabled;
        private async void RepostAdd()
        {
            try
            {
                //handle exception
                State = AdsState.Posted.ToString();
                ApiBaseResponse result = await Api.ChangeAdsState(new ChangeAdsStateInput()
                {
                    AdId = AdsDetails.AdsId,
                    State = AdsState.Posted
                });
                if (result != null && result.Success == false)
                {
                    this.OpeErrorPopUp($"Failed to repost your add", result.ErrorMessage, "Ok");
                }
                else if (result == null)
                {
                    this.OpeErrorPopUp($"Failed to repost your add", $"Server error", "Ok");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void ShareAd()
        {
            try
            {
                //handle exception
                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = $"https://api.passingcar.com/Open/Ads?adID={AdsDetails.AdsId}",
                    Title = $"Compartir {AdsDetails.AdsTitle} con amig@s"
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void TakeTheOffer()
        {
            try
            {
                //handle exception
                SendOfferResult result = await Api.SendOffer(new SendOfferInput()
                {
                    AdId = AdsDetails.AdsId,
                    Message = $"{AdsDetails.AdsPrice:0.00}".Replace(",",".")
                });
                if (result != null && result.Success == true && result.OfferId > 0)
                {
                    SentOffers.Add(new OfferDetailsExtended(new OfferDetails()
                    {
                        Id = result.OfferId,
                        State = OfferState.Sent,
                        CreatedAt = DateTime.Now,
                        Price = AdsDetails.AdsPrice
                    }, UpdateSentOffers));
                    UpdateSentOffers();
                }
                else if (result != null && result.Success == false)
                {
                    this.OpeErrorPopUp($"Failed to send your offer", result.ErrorMessage, "Okay");
                }
                else if (result == null)
                {
                    this.OpeErrorPopUp($"Failed to send your offer", $"Server error", "Okay");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void NewOffer()
        {
            try
            {
                //handle exception
                if (!string.IsNullOrEmpty(NewOfferValue))
                {
                    if (!NewOfferValue.Replace(",", ".").Contains("."))
                    {
                        NewOfferValue = $"{NewOfferValue}.00";
                    }
                    string temp_number = NewOfferValue.Replace(",", ".");
                    if (ValidateString(temp_number))
                    {

                        SendOfferResult result = await Api.SendOffer(new SendOfferInput()
                        {
                            AdId = AdsDetails.AdsId,
                            Message = temp_number
                        });

                        if (result != null && result.Success == true && result.OfferId > 0)
                        {
                            SentOffers.Add(new OfferDetailsExtended(new OfferDetails()
                            {
                                Id = result.OfferId,
                                State = OfferState.Sent,
                                CreatedAt = DateTime.Now,
                                Price = decimal.Parse(temp_number)
                            }, UpdateSentOffers));
                            UpdateSentOffers();
                        }
                        else if (result != null && result.Success == false)
                        {
                            this.OpeErrorPopUp($"Failed to send your offer", result.ErrorMessage, "Okay");
                        }
                        else if (result == null)
                        {
                            this.OpeErrorPopUp($"Failed to send your offer", $"Server error", "Okay");
                        }
                    }
                    else
                    {
                        this.OpeErrorPopUp($"Failed to send your offer", $"Invalid number", "Okay");
                    }
                }
                else
                {
                    this.OpeErrorPopUp($"Failed to send your offer", $"Invalid co-offer value", "Okay");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public bool ValidateString(string input)
        {
            // Define the regular expression pattern
            string pattern = @"^[^.]*\.[^.]*$";

            // Create a Regex object and check if the input string matches the pattern
            return Regex.IsMatch(input, pattern);
        }
        public void AsyncOffersReload()
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    //handle exception
                    GetAdsDetailsResponse getAdsDetailsResponse = await Api.GetAdsDetailsAds(new GetAdsDetailsInput()
                    {
                        AdsId = AdsDetails.AdsId
                    });
                    OffersGroup?.Clear();
                    SentOffers?.Clear();
                    if (getAdsDetailsResponse != null && getAdsDetailsResponse.AdsMoreDetails != null)
                    {
                        AdsMoreDetails = getAdsDetailsResponse.AdsMoreDetails;
                        if (AdsMoreDetails.OffersGroups != null && AdsMoreDetails.OffersGroups.Count > 0)
                        {
                            AdsMoreDetails.OffersGroups = AdsMoreDetails.OffersGroups.OrderByDescending(og => og.Offers.Max(o => o.CreatedAt)).ToList();
                            foreach (GroupOfOffers off in AdsMoreDetails.OffersGroups)
                            {
                                OffersGroup.Add(new GroupOffersDetailsExtended(off, UpdateSentOffers));
                            }
                        }
                        if(AdsMoreDetails.SentOffers != null && AdsMoreDetails.SentOffers.Count > 0)
                        {
                            AdsMoreDetails.SentOffers = AdsMoreDetails.SentOffers.OrderBy(o => o.CreatedAt).ToList();
                            foreach (OfferDetails item in AdsMoreDetails.SentOffers)
                            {
                                reloadOffers = false;
                                SentOffers.Add(new OfferDetailsExtended(item, UpdateSentOffers));
                            }
                        }
                    }
                    else
                    {
                        this.OpeErrorPopUp("Failed to load ADS Details", $"Intentar otra vez later", "INTENTALO DE NUEVO");
                    }
                    await Task.Delay(200);
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task AsyncLoad()
        {
            try
            {
                //handle exception
                GetNextAdsResponse updates = await Api.CheckAdsUpdates(new System.Collections.Generic.List<LocalDatabase.LocalAd> { new LocalDatabase.LocalAd(AdsDetails, _state, AdsDetails.UserId, AdsDetails.ModifiedAt) });
                if (updates != null && updates.Success && updates.AdsItem != null && updates.AdsItem.Any())
                {
                    AdFromAdsListModel item = updates.AdsItem.First();
                    AdsDetails = new AdsDetailsExtened(false)
                    {
                        FirstAdsImage = item.FirstAdsImage,
                        AdsTitle = item.AdsTitle,
                        IsFavorite = item.IsFavorite,
                        AdsId = item.AdsId,
                        AdsFrom = item.AdsFrom,
                        AdsTo = item.AdsTo,
                        AdsPrice = item.AdsPrice,
                        State = item.State.ToString(),
                        UserProfilePhoto = item.UserProfilePhoto,
                        UserName = item.UserName,
                        UserRating = item.UserRating,
                        PostedTime = item.PostedTime,
                        UserId = item.UserId,
                        UserProfile = item.UserProfile,
                        ModifiedAt = item.ModifiedAt,
                    };
                }
                GetAdsDetailsResponse getAdsDetailsResponse = await Api.GetAdsDetailsAds(new GetAdsDetailsInput()
                {
                    AdsId = AdsDetails.AdsId
                });
                OffersGroup.Clear();
                SentOffers.Clear();
                if (getAdsDetailsResponse != null && getAdsDetailsResponse.AdsMoreDetails != null)
                {
                    AdsMoreDetails = getAdsDetailsResponse.AdsMoreDetails;
                    if (AdsMoreDetails.NextPhotos != null && AdsMoreDetails.NextPhotos.Count > 0)
                    {
                        foreach (byte[] item in AdsMoreDetails.NextPhotos)
                        {
                            CarouselItems.Add(new CarouselItem()
                            {
                                ImageSource = ImageSource.FromStream(() => new MemoryStream(item))
                            });
                        }
                    }
                    if (AdsMoreDetails.OffersGroups != null && AdsMoreDetails.OffersGroups.Count > 0)
                    {
                        reloadOffers = false;
                        AdsMoreDetails.OffersGroups = AdsMoreDetails.OffersGroups.OrderByDescending(og => og.Offers.Max(o => o.CreatedAt)).ToList();
                        foreach (GroupOfOffers off in AdsMoreDetails.OffersGroups)
                        {
                            OffersGroup.Add(new GroupOffersDetailsExtended(off, UpdateSentOffers));
                        }
                    }
                    if (AdsMoreDetails.SentOffers != null && AdsMoreDetails.SentOffers.Count > 0)
                    {
                        AdsMoreDetails.SentOffers = AdsMoreDetails.SentOffers.OrderBy(o => o.CreatedAt).ToList();
                        foreach (OfferDetails item in AdsMoreDetails.SentOffers)
                        {
                            reloadOffers = false;
                            SentOffers.Add(new OfferDetailsExtended(item, UpdateSentOffers));
                        }
                    }
                    //if (SendOffersBtnsSP != null)
                    //{
                    //    SendOffersBtnsSP.IsVisible = IsNotMyAds && NewOfferVisibility;
                    //}
                }
                else
                {
                    this.OpeErrorPopUp("Failed to load ADS Details", $"Intentar otra vez later", "INTENTALO DE NUEVO");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void ShowHideOffers()
        {
            try
            {
                //handle exception
                if (hiddenSentOffers != null)
                {
                    foreach (OfferDetailsExtended item in hiddenSentOffers)
                    {
                        SentOffers.Add(item);
                    }
                    hiddenSentOffers = null;
                }
                else
                {
                    hiddenSentOffers = new ObservableCollection<OfferDetailsExtended>();
                    foreach (OfferDetailsExtended item in SentOffers)
                    {
                        hiddenSentOffers.Add(item);
                    }
                    SentOffers.Clear();
                }
                if (hiddenOffersGroup != null)
                {
                    foreach (GroupOffersDetailsExtended item in hiddenOffersGroup)
                    {
                        OffersGroup.Add(item);
                    }
                    hiddenOffersGroup = null;
                }
                else
                {
                    hiddenOffersGroup = new ObservableCollection<GroupOffersDetailsExtended>();
                    foreach (GroupOffersDetailsExtended item in OffersGroup)
                    {
                        hiddenOffersGroup.Add(item);
                    }
                    OffersGroup.Clear();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
    public class CarouselItem
    {
        public ImageSource ImageSource { get; set; }
    }
}
