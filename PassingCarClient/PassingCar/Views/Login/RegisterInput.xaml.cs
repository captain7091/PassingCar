
using MauiEx;
using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.ViewModels;
using System;
using System.Collections.Generic;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterInput : ContentPage
    {
        private int check_if_enable = 0;
        public string current_legal_entity;
        private readonly bool isShippingCompany;
        public RegisterViewModel ViewModel { get; set; }
        public GooglePlacesGetData gData { get; set; }

        [Obsolete]
        public RegisterInput(User user, string legal_entity = null, bool isShippingCompany = false, string password = null)
        {
            try
            {
                InitializeComponent();
                current_legal_entity = legal_entity;
                companyInfoContent.IsVisible = false;
                companyInfoGrid.IsVisible = false;
                ViewModel = new RegisterViewModel(user, legal_entity, isShippingCompany, password);
                BindingContext = ViewModel;
                gData = new GooglePlacesGetData();
                startDatePicker.MaximumDate = DateTime.Now;
                topbar_register_logo.IsVisible = false;
                personalInfoGrid.AddHideGesture(personalInfoArrow, personalInfoContent);
                accountInfoGrid.AddHideGesture(accountInfoArrow, accountInfoContent);
                addressInfoGrid.AddHideGesture(addressInfoArrow, addressInfoContent);
                companyInfoGrid.AddHideGesture(companyInfoArrow, companyInfoContent);
                this.isShippingCompany = isShippingCompany;
                //legal_entity
                if (legal_entity != null)
                {
                    if (legal_entity == "juridica")
                    {
                        companyInfoContent.IsVisible = true;
                        companyInfoGrid.IsVisible = true;
                    }
                    else
                    {
                        companyInfoContent.IsVisible = false;
                        companyInfoGrid.IsVisible = false;
                    }
                }


                //provincie
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
                    topbar_register_logo.IsVisible = false;
                    register_logo.IsVisible = true;
                    back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                    return;
                }
                //double scrollingSpace = this.ContentSize.Height - offers_listview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 100)
                {
                    register_logo.IsVisible = false;
                    topbar_register_logo.IsVisible = true;
                    if (check_if_enable == 0)
                    {
                        back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                        grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    topbar_register_logo.IsVisible = false;
                    register_logo.IsVisible = true;
                    back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void ClickRegister(object sender, EventArgs e)
        {
            try
            {
                //check validators
                int check_register = 0;

                //terms_conditions
                if (terms_cond.IsChecked == true)
                {
                    //works done
                    terms_cond_desc.TextColor = Colors.Black;
                }
                else
                {
                    check_register = 1;
                    terms_cond_desc.TextColor = Colors.IndianRed;
                    TermsCondInvalidGo();
                }

                //check cif_nif
                if (current_legal_entity == "juridica")
                {
                    if (register_cif.Text != null)
                    {
                        if (validator_nif_cif.IsValid)
                        {
                            //do nothing
                            register_cif_head.TextColor = Colors.Black;
                        }
                        else
                        {
                            register_cif_head.TextColor = Colors.IndianRed;
                            CIFInvalidGo();
                            check_register = 1;
                        }
                    }
                    else
                    {
                        register_cif_head.TextColor = Colors.IndianRed;
                        CIFInvalidGo();
                        check_register = 1;
                    }
                }

                //check company_name
                if (current_legal_entity == "juridica")
                {
                    if (register_company_name.Text != null)
                    {
                        if (register_company_name.Text.Length > 4)
                        {
                            //do nothing
                            register_company_name_head.TextColor = Colors.Black;
                        }
                        else
                        {
                            register_company_name_head.TextColor = Colors.IndianRed;
                            CompanyNameInvalidGo();
                            check_register = 1;
                        }
                    }
                    else
                    {
                        register_company_name_head.TextColor = Colors.IndianRed;
                        CompanyNameInvalidGo();
                        check_register = 1;
                    }
                }

                //check password
                if (register_password.Text != null && register_repassword != null)
                {
                    if (passwordValidator.IsValid)
                    {
                        register_password_head.TextColor = Colors.Black;
                        if (register_password.Text == register_repassword.Text)
                        {
                            register_repassword_head.TextColor = Colors.Black;
                        }
                    }
                    else
                    {
                        if (register_password.Text != register_repassword.Text)
                        {
                            register_repassword_head.TextColor = Colors.IndianRed;
                            PasswInvalidGo("passw");
                        }
                        if (!passwordValidator.IsValid)
                        {
                            register_password_head.TextColor = Colors.IndianRed;
                            PasswInvalidGo("re_passw");
                        }
                        check_register = 1;
                    }
                }
                else
                {
                    if (register_password.Text != register_repassword.Text)
                    {
                        register_repassword_head.TextColor = Colors.IndianRed;
                        PasswInvalidGo("passw");
                    }
                    if (!passwordValidator.IsValid)
                    {
                        register_password_head.TextColor = Colors.IndianRed;
                        PasswInvalidGo("re_passw");
                    }
                    check_register = 1;
                }

                //check email
                if (register_email.Text != null)
                {
                    if (emailValidator.IsValid)
                    {
                        //do nothing
                        register_email_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        register_email_head.TextColor = Colors.IndianRed;
                        EmailInvalidGo();
                        check_register = 1;
                    }
                }
                else
                {
                    register_email_head.TextColor = Colors.IndianRed;
                    EmailInvalidGo();
                    check_register = 1;
                }

                //check DNI
                if (!string.IsNullOrWhiteSpace(register_dni.Text))
                {
                    register_dni_head.TextColor = Colors.Black;
                }
                else
                {
                    register_dni_head.TextColor = Colors.IndianRed;
                    DNIInvalidGo();
                    check_register = 1;
                }

                //check first and last name
                if (register_firstname.Text != null && register_lastname.Text != null)
                {
                    if (register_firstname.Text.Length > 1)
                    {
                        //Console.WriteLine(dest_number.Text);
                        register_firstname.TextColor = Colors.Black;
                    }
                    else
                    {
                        check_register = 1;
                        register_firstname.TextColor = Colors.IndianRed;
                        NameInvalidGo("first");
                    }

                    if (register_lastname.Text.Length > 1)
                    {
                        //Console.WriteLine(dest_number.Text);
                        register_lastname.TextColor = Colors.Black;
                    }
                    else
                    {
                        check_register = 1;
                        register_lastname.TextColor = Colors.IndianRed;
                        NameInvalidGo("last");
                    }
                }
                else
                {
                    check_register = 1;
                    register_firstname.TextColor = Colors.IndianRed;
                    register_lastname.TextColor = Colors.IndianRed;
                    NameInvalidGo("first");
                }

                //check genre
                if (genre_picker.SelectedIndex != -1)
                {
                    register_genre_head.TextColor = Colors.Black;
                }
                else
                {
                    register_genre_head.TextColor = Colors.IndianRed;
                    GenreInvalidGo();
                    check_register = 1;
                }

                //if all_validators
                if (check_register == 0)
                {
                    ViewModel.SignUpClicked();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void GenreInvalidGo()
        {
            try
            {
                personalInfoContent.IsVisible = true;
                personalInfoArrow.Source = personalInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                await myscrollview.ScrollToAsync(register_genre_head, ScrollToPosition.Center, true);
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
                personalInfoContent.IsVisible = true;
                personalInfoArrow.Source = personalInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                switch (name)
                {
                    case "first":
                        await myscrollview.ScrollToAsync(register_firstname, ScrollToPosition.Center, true);
                        break;
                    case "last":
                        await myscrollview.ScrollToAsync(register_lastname, ScrollToPosition.Center, true);
                        break;
                }
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
                personalInfoContent.IsVisible = true;
                personalInfoArrow.Source = personalInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                await myscrollview.ScrollToAsync(register_dni_head, ScrollToPosition.Center, true);
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
                accountInfoContent.IsVisible = true;
                accountInfoArrow.Source = accountInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                await myscrollview.ScrollToAsync(register_email_head, ScrollToPosition.Center, true);
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
                accountInfoContent.IsVisible = true;
                accountInfoArrow.Source = accountInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                switch (name)
                {
                    case "passw":
                        await myscrollview.ScrollToAsync(register_password_head, ScrollToPosition.Center, true);
                        break;
                    case "repassw":
                        await myscrollview.ScrollToAsync(register_repassword_head, ScrollToPosition.Center, true);
                        break;
                }
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
                companyInfoContent.IsVisible = true;
                companyInfoArrow.Source = companyInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                await myscrollview.ScrollToAsync(register_cif, ScrollToPosition.Center, true);
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
                companyInfoContent.IsVisible = true;
                companyInfoArrow.Source = companyInfoContent.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
                await myscrollview.ScrollToAsync(register_company_name_head, ScrollToPosition.Center, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void TermsCondInvalidGo()
        {
            try
            {
                await myscrollview.ScrollToAsync(terms_cond, ScrollToPosition.MakeVisible, true);
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
                await Browser.OpenAsync(new Uri("https://www.passingcar.com"), BrowserLaunchMode.SystemPreferred);
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
                await Application.Current.MainPage.Navigation.PushModalAsync(new LoginInput());
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
                await Application.Current.MainPage.Navigation.PushModalAsync(new LoginInput());
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
                await Application.Current.MainPage.Navigation.PushModalAsync(new SupportPage("modal"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnTextChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(search_box.Text))
                {
                    //await ViewModel.GetPlacesPredictionsAsync();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            try
            {
                // Only get results when it was a user typing, 
                // otherwise assume the value got filled in by TextMemberPath 
                // or the handler for SuggestionChosen.
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    if (!string.IsNullOrWhiteSpace(search_box.Text))
                    {
                        await gData.GetPlacesPredictionsAsync(search_box.Text, sender);
                        await gData.GetDataAsync();
                    }
                    //if(search_box.ItemsSource != null)
                    //    search_box.ItemsSource.Clear();
                    //await GetPlacesPredictionsAsync();
                    //Set the ItemsSource to be your filtered dataset
                    //var list = new List<string>();
                    //list.Add("tst");
                    //sender.ItemsSource = ViewModel.Addresses;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            try
            {
                Console.WriteLine(sender.Text);
                // Set sender.Text. You can use args.SelectedItem to build your text string.
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                if (args.ChosenSuggestion != null)
                {
                    // User selected an item from the suggestion list, take an action on it here.
                }
                else
                {
                    // User hit Enter from the search box. Use args.QueryText to determine what to do.
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}