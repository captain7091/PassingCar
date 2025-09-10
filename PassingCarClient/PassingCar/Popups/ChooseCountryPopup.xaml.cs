
using Mopups.Pages;
using Mopups.Services;
using PassingCar.Controls;
using PassingCar.Models;
using PassingCar.Utils;
using PassingCar.ViewModels;
using PhoneNumbers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;


namespace PassingCar.Popups
{
    public partial class ChooseCountryPopup : PopupPage
    {
        #region Fields

        private static List<CountryModel> _countries;
        private CountryModel _selectedCountry;

        #endregion Fields

        #region Constructors

        public ChooseCountryPopup(CountryModel selectedCountry)
        {
            InitializeComponent();
            if (_countries == null || !_countries.Any())
            {
                LoadCountries();
            }
            VisibleCountries = new ObservableCollection<CountryModel>(_countries);

            SelectedCountry = selectedCountry;
            CommonCountriesList.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(VisibleCountries), source: this));
            CurrentCountryControl.SetBinding(CountryControl.CountryProperty, new Binding(nameof(SelectedCountry), source: this));
        }

        #endregion Constructors

        #region Properties

        public ICommand CountrySelectedCommand { get; set; }

        public ObservableCollection<CountryModel> VisibleCountries { get; }

        public CountryModel SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                _selectedCountry = value;
                OnPropertyChanged(nameof(SelectedCountry));
            }
        }

        #endregion Properties

        #region Private Methods

        private  void CloseBtn_Clicked(object sender, EventArgs e)
        {
            _ =  MopupService.Instance.PopAsync();
        }

        private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            CountrySelectedCommand?.Execute(SelectedCountry);
            _ = MopupService.Instance.PopAsync();
        }

        private void LoadCountries()
        {
            //this is not Task, because it's really fast
            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
            _countries = new List<CountryModel>();
            List<System.Globalization.RegionInfo> isoCountries = CountryUtils.GetCountriesByIso3166();
            _countries.AddRange(isoCountries.Select(c => new CountryModel
            {
                CountryCode = phoneNumberUtil.GetCountryCodeForRegion(c.TwoLetterISORegionName).ToString(),
                CountryName = c.EnglishName,
                FlagUrl = $"https://hatscripts.github.io/circle-flags/flags/{c.TwoLetterISORegionName.ToLower()}.svg",
            }));
        }

        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear the visible countries list
            VisibleCountries.Clear();

            // Filter countries based on the search text
            IEnumerable<CountryModel> filteredCountries = string.IsNullOrWhiteSpace(e.NewTextValue)
                ? _countries
                : _countries.Where(country => country.CountryName.Contains(e.NewTextValue, StringComparison.InvariantCultureIgnoreCase));

            // Add the filtered countries to the visible collection
            foreach (var country in filteredCountries)
            {
                VisibleCountries.Add(country);
            }
        }

        private void CommonCountriesList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SelectedCountry = e.SelectedItem as CountryModel;
        }

        #endregion Private Methods
    }
}
