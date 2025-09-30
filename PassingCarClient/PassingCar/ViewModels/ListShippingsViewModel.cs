using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace PassingCar.ViewModels
{
    public class ListShippingsViewModel : BaseViewModel
    {
        public ObservableCollection<ShippingsDetailsExtened> Shippingss { get; set; }
        private ObservableCollection<ShippingsDetailsExtened> allShippings;
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
                    //UpdateShippings();
                    OnPropertyChanged(nameof(OnlyActive));
                    OnPropertyChanged(nameof(Shippingss));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }
        private readonly ContentView smallLoading;
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                try
                {
                    //handle exception
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }
        public ICommand RefreshCommand => new Command(async () =>
        {
            IsRefreshing = true;

            await CustomLoad();

            IsRefreshing = false;
        });

        public ListShippingsViewModel(ContentView smallLoading)
        {
            Shippingss = new ObservableCollection<ShippingsDetailsExtened>();
            this.smallLoading = smallLoading;
            _onlyActive = true;
        }
        public async Task CustomLoad()
        {
            try
            {
                //handle exception
                smallLoading.IsVisible = true; // Show loading indicator
                var tempShippings = new ObservableCollection<ShippingsDetailsExtened>();
                ShippingFromListModel lastLoaded = new ShippingFromListModel()
                {
                    Id = int.MinValue
                };
                DateTime reqTime = DateTime.Now;
                while (lastLoaded != null)
                {
                    // Removed artificial delay for instant response
                    GetNextShippingResponse result = await Api.GetNextShipping(new GetNextShippingInput()
                    {
                        FirstRequestTime = reqTime,
                        LastShippingLoaded = lastLoaded.Id,
                    });
                    if (result != null)
                    {
                        if (result.Success)
                        {
                            if (result.ShippingItems != null && result.ShippingItems.Count() > 0)
                            {
                                lastLoaded = new ShippingFromListModel()
                                {
                                    Id = result.ShippingItems.Select(a => a.Id).Max()
                                };
                                foreach (ShippingFromListModel item in result.ShippingItems)
                                {
                                    ShippingsDetailsExtened ship = new ShippingsDetailsExtened(tempShippings, item);
                                    tempShippings.Add(ship);
                                }
                            }
                            else
                            {
                                smallLoading.IsVisible = false;
                                lastLoaded = null;
                            }
                        }
                        else
                        {
                            smallLoading.IsVisible = false;
                            lastLoaded = null;
                        }
                    }
                    else
                    {
                        smallLoading.IsVisible = false;
                        lastLoaded = null;
                    }
                }

                // Update collections without replacing the bound collection to prevent UI flicker
                if (allShippings == null)
                {
                    allShippings = new ObservableCollection<ShippingsDetailsExtened>();
                }
                else
                {
                    allShippings.Clear();
                }

                foreach (var item in tempShippings)
                {
                    allShippings.Add(item);
                }

                // Only assign to Shippingss if it's null (first load), otherwise update existing collection
                if (Shippingss == null)
                {
                    Shippingss = allShippings;
                }
                else
                {
                    // Clear and repopulate existing collection to maintain binding
                    Shippingss.Clear();
                    foreach (var item in allShippings)
                    {
                        Shippingss.Add(item);
                    }
                }

                OnlyActive = _onlyActive;
                smallLoading.IsVisible = false; // Hide loading indicator when done
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                smallLoading.IsVisible = false; // Hide loading indicator on error
            }
        }
        private void UpdateShippings()
        {
            try
            {
                //handle exception
                if (_onlyActive)
                {
                    try
                    {
                        // Filter existing collection without creating new one to prevent UI flicker
                        var filteredItems = allShippings?.Where(item => item.State < Models.ShippingState.Finalizado).ToList() ?? new List<ShippingsDetailsExtened>();

                        // Update existing collection instead of replacing it
                        if (Shippingss == null)
                        {
                            Shippingss = new ObservableCollection<ShippingsDetailsExtened>(filteredItems);
                        }
                        else
                        {
                            Shippingss.Clear();
                            foreach (var item in filteredItems)
                            {
                                Shippingss.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.OpeErrorPopUp("Unable to filter", JsonConvert.SerializeObject(ex), "Ok");
                        // Fallback: show all items if filtering fails
                        if (allShippings != null && Shippingss != null)
                        {
                            Shippingss.Clear();
                            foreach (var item in allShippings)
                            {
                                Shippingss.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    // Show all items without filtering
                    if (allShippings != null && Shippingss != null)
                    {
                        Shippingss.Clear();
                        foreach (var item in allShippings)
                        {
                            Shippingss.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public void KeepSync()
        {
            //if (App.ShippingsHub != null)
            //{
            //    App.ShippingsHub.On<ShippingsDetails>("NewShippings", (item) =>
            //    {
            //        if (!Shippingss.Any(a => a.Equals(item)))
            //        {
            //            Shippingss.Add(new ShippingsDetailsExtened(Shippingss)
            //            {
            //                User = item.User,
            //                Shippings = item.Shippings,
            //                IsFavorite = item.IsFavorite,
            //            });
            //        }
            //    });
            //}
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ListShippingsViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    Shippingss?.Clear();
                    allShippings?.Clear();
                    
                    System.Diagnostics.Debug.WriteLine("[ListShippingsViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ListShippingsViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
