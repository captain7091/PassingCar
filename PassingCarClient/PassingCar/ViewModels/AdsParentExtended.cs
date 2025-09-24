using PassingCar.Extensions;
using PassingCar.Models.API.Ads;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PassingCar.ViewModels
{
    public class AdsParentExtended : BaseViewModel
    {
        public int AdsId { get; set; }
        private byte[] _firstPhoto;
        public byte[] FirstPhoto
        {
            get => _firstPhoto;
            set
            {
                _firstPhoto = value;
                OnPropertyChanged(nameof(FirstPhoto));
            }
        }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupOfOffers> OffersGroups { get; set; }
        public int? ChatIdForViewer { get; set; }
        public List<OfferDetails> SentOffers { get; set; }
        public AdsParentExtended()
        {
            ViewAds = new Command(ViewThisAds);
        }
        public ObservableCollection<GroupOffersDetailsExtended> OffersGroupEx { get; set; }
        public ObservableCollection<OfferDetailsExtended> SentOffersEx { get; set; }
        private ObservableCollection<GroupOffersDetailsExtended> hiddenOffersGroup;
        private ObservableCollection<OfferDetailsExtended> hiddenSentOffers;
        public Command ViewAds { get; set; }
        public Command ShowHideCommand => new Command(ShowHideOffers);
        public ImageSource FirstImage
        {
            get
            {
                try
                {
                    //handle exception
                    return FirstPhoto != null && FirstPhoto.Length > 0
                        ? ImageSource.FromStream(() => new MemoryStream(FirstPhoto))
                        : ImageSource.FromResource($"PassingCar.Resources.Images.empty.jpg");
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                    return ImageSource.FromResource($"PassingCar.Resources.Images.empty.jpg");
                }

            }
        }
        private void ShowHideOffers()
        {
            try
            {
                //handle exception
                if (hiddenOffersGroup != null)
                {
                    // Show offers (from hidden to visible)
                    foreach (GroupOffersDetailsExtended item in hiddenOffersGroup)
                    {
                        if (item != null)
                        {
                            OffersGroupEx.Add(item);
                        }
                    }
                    hiddenOffersGroup = null;
                }
                else
                {
                    // Hide offers (from visible to hidden)
                    hiddenOffersGroup = new ObservableCollection<GroupOffersDetailsExtended>();
                    foreach (GroupOffersDetailsExtended item in OffersGroupEx)
                    {
                        if (item != null)
                        {
                            hiddenOffersGroup.Add(item);
                        }
                    }
                    OffersGroupEx.Clear();
                }
                if (hiddenSentOffers != null)
                {
                    foreach (OfferDetailsExtended item in hiddenSentOffers)
                    {
                        if (item != null)
                        {
                            SentOffersEx.Add(item);
                        }
                    }
                    hiddenSentOffers = null;
                }
                else
                {
                    hiddenSentOffers = new ObservableCollection<OfferDetailsExtended>();
                    foreach (OfferDetailsExtended item in SentOffersEx)
                    {
                        hiddenSentOffers.Add(item);
                    }
                    SentOffersEx.Clear();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        // New method to ensure offers are always visible by default
        public void EnsureOffersVisible()
        {
            try
            {
                if (hiddenOffersGroup != null && hiddenOffersGroup.Count > 0)
                {
                    // Move offers from hidden to visible
                    foreach (GroupOffersDetailsExtended item in hiddenOffersGroup)
                    {
                        if (item != null)
                        {
                            OffersGroupEx.Add(item);
                        }
                    }
                    hiddenOffersGroup = null;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        private async void ViewThisAds()
        {
            try
            {
                //handle exception
                await this.GoToAds(AdsId);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
