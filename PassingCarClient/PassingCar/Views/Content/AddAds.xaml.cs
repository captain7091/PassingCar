using PassingCar.Extensions;
using PassingCar.ViewModels;
using PhoneNumbers;
using System;
using System.Collections.Generic;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddAds : ContentPage
    {
        private int check_if_enable = 0;
        public string gobackto_;
        const string prefix = "+34 ";
        private readonly AdAdsViewModels vmodel;
        private readonly PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

        [Obsolete]
        public AddAds(string frompage)
        {
            try
            {
                InitializeComponent();
                vmodel = new AdAdsViewModels();
                BindingContext = vmodel;
                if (frompage == "MyAdsPage")
                {
                    gobackto_ = "MyAdsPage";
                }
                else if (frompage == "AdsPage")
                {
                    gobackto_ = "AdsPage";
                }

                //provincie
                List<string> province_pick_from = new List<string>();
                List<string> province_pick_to = new List<string>();
                province_pick_from.Add("A Coruña");
                province_pick_to.Add("A Coruña");
                province_pick_from.Add("Álava");
                province_pick_to.Add("Álava");
                province_pick_from.Add("Albacete");
                province_pick_to.Add("Albacete");
                province_pick_from.Add("Alicante");
                province_pick_to.Add("Alicante");
                province_pick_from.Add("Almería");
                province_pick_to.Add("Almería");
                province_pick_from.Add("Asturias");
                province_pick_to.Add("Asturias");
                province_pick_from.Add("Ávila");
                province_pick_to.Add("Ávila");
                province_pick_from.Add("Badajoz");
                province_pick_to.Add("Badajoz");
                province_pick_from.Add("Islas Baleares");
                province_pick_to.Add("Islas Baleares");
                province_pick_from.Add("Barcelona");
                province_pick_to.Add("Barcelona");
                province_pick_from.Add("Vizcaya");
                province_pick_to.Add("Vizcaya");
                province_pick_from.Add("Burgos");
                province_pick_to.Add("Burgos");
                province_pick_from.Add("Cáceres");
                province_pick_to.Add("Cáceres");
                province_pick_from.Add("Cádiz");
                province_pick_to.Add("Cádiz");
                province_pick_from.Add("Cantabria");
                province_pick_to.Add("Cantabria");
                province_pick_from.Add("Castellón");
                province_pick_to.Add("Castellón");
                province_pick_from.Add("Ciudad Real");
                province_pick_to.Add("Ciudad Real");
                province_pick_from.Add("Córdoba");
                province_pick_to.Add("Córdoba");
                province_pick_from.Add("Cuenca");
                province_pick_to.Add("Cuenca");
                province_pick_from.Add("Guipúzcoa");
                province_pick_to.Add("Guipúzcoa");
                province_pick_from.Add("Gerona");
                province_pick_to.Add("Gerona");
                province_pick_from.Add("Granada");
                province_pick_to.Add("Granada");
                province_pick_from.Add("Guadalajara");
                province_pick_to.Add("Guadalajara");
                province_pick_from.Add("Huelva");
                province_pick_to.Add("Huelva");
                province_pick_from.Add("Huesca");
                province_pick_to.Add("Huesca");
                province_pick_from.Add("Jaén");
                province_pick_to.Add("Jaén");
                province_pick_from.Add("La Rioja");
                province_pick_to.Add("La Rioja");
                province_pick_from.Add("Las Palmas");
                province_pick_to.Add("Las Palmas");
                province_pick_from.Add("León");
                province_pick_to.Add("León");
                province_pick_from.Add("Lérida");
                province_pick_to.Add("Lérida");
                province_pick_from.Add("Lugo");
                province_pick_to.Add("Lugo");
                province_pick_from.Add("Madrid");
                province_pick_to.Add("Madrid");
                province_pick_from.Add("Málaga");
                province_pick_to.Add("Málaga");
                province_pick_from.Add("Murcia");
                province_pick_to.Add("Murcia");
                province_pick_from.Add("Navarra");
                province_pick_to.Add("Navarra");
                province_pick_from.Add("Orense");
                province_pick_to.Add("Orense");
                province_pick_from.Add("Palencia");
                province_pick_to.Add("Palencia");
                province_pick_from.Add("Pontevedra");
                province_pick_to.Add("Pontevedra");
                province_pick_from.Add("Salamanca");
                province_pick_to.Add("Salamanca");
                province_pick_from.Add("Santa Cruz de Tenerife");
                province_pick_to.Add("Santa Cruz de Tenerife");
                province_pick_from.Add("Segovia");
                province_pick_to.Add("Segovia");
                province_pick_from.Add("Sevilla");
                province_pick_to.Add("Sevilla");
                province_pick_from.Add("Soria");
                province_pick_to.Add("Soria");
                province_pick_from.Add("Tarragona");
                province_pick_to.Add("Tarragona");
                province_pick_from.Add("Teruel");
                province_pick_to.Add("Teruel");
                province_pick_from.Add("Toledo");
                province_pick_to.Add("Toledo");
                province_pick_from.Add("Valencia");
                province_pick_to.Add("Valencia");
                province_pick_from.Add("Valladolid");
                province_pick_to.Add("Valladolid");
                province_pick_from.Add("Zamora");
                province_pick_to.Add("Zamora");
                province_pick_from.Add("Zaragoza");
                province_pick_to.Add("Zaragoza");
                provincie_picker_from.ItemsSource = province_pick_from;
                provincie_picker_to.ItemsSource = province_pick_from;
                AddNumberOnEntry(from_number);
                AddNumberOnEntry(dest_number);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                
                // Show loading indicator
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsVisible = true;
                }
                
                // Initialize the view model asynchronously
                if (vmodel != null)
                {
                    await vmodel.InitializeAsync();
                }
                
                // Hide loading indicator
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                // Hide loading indicator on error
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsVisible = false;
                }
                _ = ex.Handle();
            }
        }

        private bool IsValidNumber(string aNumber, bool dest = false, bool from = true)
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
                catch (Exception ex)
                {
                    dest_number_head.TextColor = Colors.IndianRed;
                    dest_number.TextColor = Colors.IndianRed;
                    if (dest) 
                    {
                        dest_number_warning.IsVisible = true;
                    }
                    if (from)
                    {
                        from_number_warning.IsVisible = true;
                    }
                    NumberPhoneInvalidGo();
                }

                return result;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        private void PushAdd(object sender, EventArgs e)
        {
            try
            {
                //check validators
                int check_all = 0;

                // Null safety checks for UI elements
                if (terms_cond == null || provincie_picker_from == null || provincie_picker_to == null ||
                    dimension_weight == null || dimension_length == null || dimension_width == null ||
                    dimension_height == null || price == null || dest_number == null || from_number == null ||
                    description == null || title == null)
                {
                    DisplayAlert("Error de validación", "Algunos campos requeridos no están disponibles. Por favor, reinicie la página.", "OK");
                    return;
                }

                //terms_conditions
                if (terms_cond.IsChecked == true)
                {
                    //works done
                    terms_cond_desc.TextColor = Colors.Black;
                }
                else
                {
                    check_all = 1;
                    terms_cond_desc.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await TermsCondInvalidGo());
                }

                //provincia from
                if (provincie_picker_from.SelectedIndex >= 0)
                {
                    //works done
                    provincia_head.TextColor = Colors.Black;
                }
                else
                {
                    check_all = 1;
                    provincia_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await PickerFromInvalidGo());
                }

                //provincia to
                if (provincie_picker_to.SelectedIndex >= 0)
                {
                    //works done
                    provincia_to_head.TextColor = Colors.Black;
                }
                else
                {
                    check_all = 1;
                    provincia_to_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await PickerToInvalidGo());
                }

                //Weight
                if (!string.IsNullOrWhiteSpace(dimension_weight.Text))
                {
                    if (validator_weight?.IsValid == true)
                    {
                        //works done
                        dimension_weight_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        check_all = 1;
                        dimension_weight_head.TextColor = Colors.IndianRed;
                        _ = Task.Run(async () => await WeightInvalidGo());
                    }
                }
                else
                {
                    check_all = 1;
                    dimension_weight_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await WeightInvalidGo());
                }

                //L x l x H
                if (!string.IsNullOrWhiteSpace(dimension_length.Text) && !string.IsNullOrWhiteSpace(dimension_width.Text) && !string.IsNullOrWhiteSpace(dimension_height.Text))
                {
                    if (validator_length?.IsValid == true && validator_height?.IsValid == true && validator_width?.IsValid == true)
                    {
                        //works done
                        dimension_head_0.TextColor = Colors.Black;
                        dimension_head_1.TextColor = Colors.Black;
                        dimension_head_2.TextColor = Colors.Black;
                    }
                    else
                    {
                        check_all = 1;
                        dimension_head_0.TextColor = Colors.IndianRed;
                        dimension_head_1.TextColor = Colors.IndianRed;
                        dimension_head_2.TextColor = Colors.IndianRed;
                        _ = Task.Run(async () => await WLInvalidGo());
                    }
                }
                else
                {
                    check_all = 1;
                    dimension_head_0.TextColor = Colors.IndianRed;
                    dimension_head_1.TextColor = Colors.IndianRed;
                    dimension_head_2.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await WLInvalidGo());
                }

                //Price           
                if (!string.IsNullOrWhiteSpace(price.Text))
                {
                    if (validator_price?.IsValid == true)
                    {
                        //works done
                        price_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        check_all = 1;
                        price_head.TextColor = Colors.IndianRed;
                        _ = Task.Run(async () => await PriceInvalidGo());
                    }
                }
                else
                {
                    check_all = 1;
                    price_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await PriceInvalidGo());
                }

                //check destination number phone
                if (!string.IsNullOrWhiteSpace(dest_number.Text))
                {
                    if (IsValidNumber(dest_number.Text.Replace("(", "").Replace(")", ""),true))
                    {
                        //Console.WriteLine(dest_number.Text);
                        dest_number_head.TextColor = Colors.Black;
                        dest_number.TextColor = Colors.Black;
                        dest_number_warning.IsVisible = false;
                    }
                    else
                    {
                        check_all = 1;
                        dest_number_head.TextColor = Colors.IndianRed;
                        dest_number.TextColor = Colors.IndianRed;
                        NumberPhoneInvalidGo();
                    }
                }
                else
                {
                    check_all = 1;
                    dest_number_head.TextColor = Colors.IndianRed;
                    dest_number.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await NumberPhoneInvalidGo());
                }

                //check from number phone
                if (!string.IsNullOrWhiteSpace(from_number.Text))
                {
                    if (IsValidNumber(from_number.Text.Replace("(", "").Replace(")", ""),false,true))
                    {
                        //Console.WriteLine(from_number.Text);
                        from_number_head.TextColor = Colors.Black;
                        from_number.TextColor = Colors.Black;
                        from_number_warning.IsVisible = false;
                    }
                    else
                    {
                        check_all = 1;
                        from_number_head.TextColor = Colors.IndianRed;
                        from_number.TextColor = Colors.IndianRed;
                        _ = Task.Run(async () => await NumberPhoneInvalidGo());
                    }
                }
                else
                {
                    check_all = 1;
                    from_number_head.TextColor = Colors.IndianRed;
                    from_number.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await NumberPhoneInvalidGo());
                }

                //Description
                if (!string.IsNullOrWhiteSpace(description.Text))
                {
                    // if (validator_description.IsValid)
                    // {
                    //     description_head.TextColor = Colors.Black;
                    // }
                    // else
                    // {
                    //     check_all = 1;
                    //     description_head.TextColor = Colors.IndianRed;
                    //     DescriptonInvalidGo();
                    // }
                }
                else
                {
                    check_all = 1;
                    description_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await DescriptonInvalidGo());
                }


                //Title
                if (!string.IsNullOrWhiteSpace(title.Text))
                {
                    // if (validator_title.IsValid)
                    // {
                    //     title_head.TextColor = Colors.Black;
                    // }
                    // else
                    // {
                    //     check_all = 1;
                    //     title_head.TextColor = Colors.IndianRed;
                    //     TitleInvalidGo();
                    // }
                }
                else
                {
                    check_all = 1;
                    title_head.TextColor = Colors.IndianRed;
                    _ = Task.Run(async () => await TitleInvalidGo());
                }

                //if all_validators
                if (check_all == 0)
                {
                    //Console.WriteLine("all_okk");
                    _ = Task.Run(async () => await SubmitAsync());
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task SubmitAsync()
        {
            try
            {
                if (vmodel == null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () => 
                        await DisplayAlert("Error", "Error interno: modelo de vista no disponible. Por favor, reinicie la aplicación.", "OK"));
                    return;
                }

                // Show loading indicator on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (LoadingIndicator != null)
                    {
                        LoadingIndicator.IsVisible = true;
                    }
                });

                await vmodel.Submit(provincie_picker_from.SelectedItem.ToString(), provincie_picker_to.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => 
                    await DisplayAlert("Error", "Error al publicar el anuncio. Por favor, verifique su conexión e intente nuevamente.", "OK"));
                _ = ex.Handle();
            }
            finally
            {
                // Hide loading indicator on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (LoadingIndicator != null)
                    {
                        LoadingIndicator.IsVisible = false;
                    }
                });
            }
        }

        public async Task DescriptonInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(description_head, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task TitleInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(title_head, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task TermsCondInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(terms_cond, ScrollToPosition.MakeVisible, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task NumberPhoneInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(dest_number, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task PriceInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(price_head, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task WLInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(dimension_length, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task WeightInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(dimension_length, ScrollToPosition.MakeVisible, true));
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
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    grid_footer_context.BackgroundColor = Colors.Transparent;
                    check_if_enable = 0;
                    return;
                }
                //double scrollingSpace = this.ContentSize.Height - offers_listview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    hello_context.TextColor = Colors.White;
                    if (check_if_enable == 0)
                    {
                        menu_context.Source = "back_arrow_white.png";
                    }

                    grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    grid_footer_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    grid_footer_context.BackgroundColor = Colors.Transparent;
                    check_if_enable = 0;
                }
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
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.GoToAsync($"//" + gobackto_));
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
                _ = Task.Run(async () => await GoHome());
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async Task GoHome()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.GoToAsync($"//" + gobackto_));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void HelpIcon(object sender, EventArgs e)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.Navigation.PushAsync(new SupportPage()));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void TermsCond(object sender, EventArgs e)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Browser.OpenAsync(new Uri("https://www.passingcar.com"), BrowserLaunchMode.SystemPreferred));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task PickerFromInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(provincia_head, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task PickerToInvalidGo()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await myscrollview.ScrollToAsync(provincia_to_head, ScrollToPosition.Center, true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void from_number_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddNumberOnEntry(from_number);
        }

        private void dest_number_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddNumberOnEntry(dest_number);
        }
        private void AddNumberOnEntry(Entry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Text) || !entry.Text.StartsWith(prefix))
            {
                string digitsOnly = entry.Text?.Replace(prefix, "") ?? string.Empty;
                entry.Text = prefix + digitsOnly;
                entry.CursorPosition = entry.Text.Length;
            }

            if (entry.Text.Length < prefix.Length)
            {
                entry.Text = prefix;
                entry.CursorPosition = prefix.Length;
            }
        }
    }
}