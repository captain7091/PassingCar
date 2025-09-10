using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using System;
using System.Collections.ObjectModel;


namespace PassingCar.ViewModels
{
    public class ListOffersViewModel : BaseViewModel
    {
        public ObservableCollection<AdsParentExtended> Ads { get; set; }
        private ObservableCollection<AdsParentExtended> allAds;
        private bool _onlyActive;
        public bool OnlyActive
        {
            get => _onlyActive;
            set
            {
                try
                {
                    //handle exception
                    _onlyActive = value;
                    UpdateAds();
                    OnPropertyChanged(nameof(OnlyActive));
                    OnPropertyChanged(nameof(Ads));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }
        public void UpdateEvent()
        {
            PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.NewOffer, async (input) =>
            {
                SendOfferInput item = input as SendOfferInput;
                System.Collections.Generic.IEnumerable<AdsParentExtended> adsWithThisId = allAds.Where(a => a.AdsId == item.AdId);
                if (adsWithThisId != null && adsWithThisId.Count() > 0)
                {
                    await AsyncLoad();
                    // Removed artificial delays for instant response
                    await LoadPhotos();
                    // Removed artificial delays for instant response
                    OnlyActive = _onlyActive;
                }
            });
            PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.OfferStateChanged, async (input) =>
            {
                ChangeOfferStateInput item = input as ChangeOfferStateInput;
                if (Ads.Any(a => a.OffersGroupEx.Any(oex => oex.Offers.Any(o => o.Id.Equals(item.OfferId))) || a.SentOffersEx.Any(o => o.Id.Equals(item.OfferId))))
                {
                    await AsyncLoad();
                    await Task.Delay(100);
                    await LoadPhotos();
                    await Task.Delay(100);
                }
            });
        }
        public ListOffersViewModel()
        {
            try
            {
                //handle exception
                //OnlyActive = true;
                Ads = new ObservableCollection<AdsParentExtended>();
                _onlyActive = false;
              
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
                GetOffersResponse offers = await Api.GetMyOffers();
                if (Ads is null)
                {
                    Ads = new ObservableCollection<AdsParentExtended>();
                }
                Ads.Clear();
                if (offers != null && offers.Success)
                {
                    foreach (AdsParent item in offers.Ads)
                    {
                        AdsParentExtended adsDet = new AdsParentExtended()
                        {
                            AdsId = item.AdsId,
                            CreatedAt = item.CreatedAt,
                            ChatIdForViewer = item.ChatIdForViewer,
                            FirstPhoto = item.FirstPhoto,
                            OffersGroupEx = new ObservableCollection<GroupOffersDetailsExtended>(),
                            SentOffersEx = new ObservableCollection<OfferDetailsExtended>(),
                            Title = item.Title
                        };
                        adsDet.FirstPhoto = await adsDet.AdsId.GetAdsPhoto();



                        if (item.OffersGroups != null && item.OffersGroups.Count > 0)
                        {
                            item.OffersGroups = item.OffersGroups.OrderByDescending(og => og.Offers.Any() ? og.Offers.Max(o => o.CreatedAt) : DateTime.MinValue).ToList();
                            adsDet.OffersGroupEx = new ObservableCollection<GroupOffersDetailsExtended>();
                            foreach (GroupOfOffers offer in item.OffersGroups)
                            {
                                GroupOffersDetailsExtended offerex = new GroupOffersDetailsExtended(offer, UpdateSentOffers);
                                offerex.UserProfilePhoto = await offerex.UserId.GetUserPhoto();
                                adsDet.OffersGroupEx.Add(offerex);
                            }
                            if (Ads is null)
                            {
                                Ads = new ObservableCollection<AdsParentExtended>();
                            }
                            Ads.Add(adsDet);
                        }

                        if (item.SentOffers != null && item.SentOffers.Count > 0)
                        {
                            item.SentOffers = item.SentOffers.OrderByDescending(o => o.CreatedAt).ToList();
                            adsDet.SentOffersEx = new ObservableCollection<OfferDetailsExtended>();
                            foreach (OfferDetails offer in item.SentOffers)
                            {
                                adsDet.SentOffersEx.Add(new OfferDetailsExtended(offer, UpdateSentOffers));
                            }
                            if (Ads is null)
                            {
                                Ads = new ObservableCollection<AdsParentExtended>();
                            }
                            Ads.Add(adsDet);
                        }
                    }
                }
                else
                {
                    this.OpeErrorPopUp("No se pudieron cargar las ofertas", $"Tal vez perdiste la conexión a Internet.", "INTENTALO DE NUEVO");
                }
                allAds = Ads;
                OnlyActive = _onlyActive;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task LoadPhotos()
        {
            try
            {
                //handle exception
                if (Ads != null && Ads.Count > 0)
                {
                    // Load photos for each ad in parallel
                    var photoTasks = new List<Task>();

                    foreach (AdsParentExtended item in Ads)
                    {
                        // Load the first photo for each ad
                        photoTasks.Add(LoadAdPhotoAsync(item));
                    }

                    // Wait for all photo loading tasks to complete
                    if (photoTasks.Any())
                    {
                        await Task.WhenAll(photoTasks);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async Task LoadAdPhotoAsync(AdsParentExtended ad)
        {
            try
            {
                if (ad != null && ad.AdsId > 0 && (ad.FirstPhoto == null || ad.FirstPhoto.Length == 0))
                {
                    System.Diagnostics.Debug.WriteLine($"[ListOffersViewModel] Loading photo for ad {ad.AdsId}");
                    
                    // Try to get photo from local database first, then from API if needed
                    byte[] photoBytes = await ad.AdsId.GetAdsPhoto();
                    
                    if (photoBytes != null && photoBytes.Length > 0)
                    {
                        ad.FirstPhoto = photoBytes;
                        System.Diagnostics.Debug.WriteLine($"[ListOffersViewModel] Successfully loaded photo for ad {ad.AdsId}, size: {photoBytes.Length} bytes");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ListOffersViewModel] No photo available for ad {ad.AdsId}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ListOffersViewModel] Error loading photo for ad {ad?.AdsId}: {ex.Message}");
                _ = ex.Handle();
            }
        }
        public void UpdateSentOffers()
        {
        }
        private void UpdateAds()
        {
            try
            {
                //handle exception
                if (_onlyActive)
                {
                    allAds = Ads;
                    try
                    {
                        Ads = new ObservableCollection<AdsParentExtended>();
                        foreach (AdsParentExtended item in allAds)
                        {
                            AdsParentExtended adsToAdd = new AdsParentExtended()
                            {
                                AdsId = item.AdsId,
                                FirstPhoto = item.FirstPhoto,
                                Title = item.Title,
                                CreatedAt = item.CreatedAt,
                                ChatIdForViewer = item.ChatIdForViewer,
                            };
                            if (item != null)
                            {
                                adsToAdd.OffersGroupEx = new ObservableCollection<GroupOffersDetailsExtended>();
                                if (item.OffersGroupEx != null && item.OffersGroupEx.Count > 0)
                                {
                                    foreach (GroupOffersDetailsExtended offerGroup in item.OffersGroupEx)
                                    {
                                        if (offerGroup != null && offerGroup.GroupOffers != null && offerGroup.GroupOffers.Count > 0)
                                        {
                                            System.Collections.Generic.IEnumerable<OfferDetailsExtended> activeOffers = offerGroup.GroupOffers.Where(o => o.State <= Models.OfferState.PaymentPending);
                                            ObservableCollection<OfferDetailsExtended> activeOffG = new ObservableCollection<OfferDetailsExtended>();
                                            if (activeOffers != null && activeOffers.Count() > 0)
                                            {
                                                foreach (OfferDetailsExtended offer in activeOffers)
                                                {
                                                    activeOffG.Add(offer);
                                                }
                                            }
                                            if (activeOffG != null && activeOffG.Count > 0)
                                            {
                                                GroupOffersDetailsExtended groupOffer = offerGroup;
                                                groupOffer.GroupOffers = activeOffG;
                                                adsToAdd.OffersGroupEx.Add(groupOffer);
                                            }
                                        }
                                    }
                                }
                                adsToAdd.SentOffersEx = new ObservableCollection<OfferDetailsExtended>();
                                if (item.SentOffersEx != null && item.SentOffersEx.Count > 0)
                                {
                                    System.Collections.Generic.IEnumerable<OfferDetailsExtended> mySent = item.SentOffersEx.Where(o => o.State < Models.OfferState.PaymentPending);
                                    if (mySent != null && mySent.Count() > 0)
                                    {
                                        foreach (OfferDetailsExtended myitem in mySent)
                                        {
                                            adsToAdd.SentOffersEx.Add(myitem);
                                        }
                                    }
                                }
                                if ((adsToAdd.SentOffersEx != null && adsToAdd.SentOffersEx.Count() > 0) || (adsToAdd.OffersGroupEx != null && adsToAdd.OffersGroupEx.Count() > 0))
                                {
                                    Ads.Add(adsToAdd);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.OpeErrorPopUp("Unable to filter", JsonConvert.SerializeObject(ex), "Ok");
                        Ads = allAds;
                    }
                }
                else
                {
                    Ads = allAds;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ListOffersViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    Ads?.Clear();
                    allAds?.Clear();
                    
                    // Note: Hub event subscriptions are managed globally by PassingCarHubs
                    // Individual ViewModels don't need to unsubscribe as the hub manages the lifecycle
                    
                    System.Diagnostics.Debug.WriteLine("[ListOffersViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ListOffersViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
