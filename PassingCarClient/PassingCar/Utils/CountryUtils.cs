using PassingCar.Models;
using PhoneNumbers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PassingCar.Utils
{
    public static class CountryUtils
    {
        /// <summary>
        /// Gets the list of countries based on ISO 3166-1
        /// </summary>
        /// <returns>Returns the list of countries based on ISO 3166-1</returns>
        public static List<RegionInfo> GetCountriesByIso3166()
        {
            List<RegionInfo> countries = new List<RegionInfo>();

            // Get specific cultures
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    // Create a RegionInfo object from the culture
                    RegionInfo region = new RegionInfo(culture.Name); // Use culture.Name instead of culture.LCID

                    // Avoid duplicate regions by checking the Name property
                    if (!countries.Any(p => p.Name == region.Name))
                    {
                        countries.Add(region);
                    }
                }
                catch (ArgumentException ex)
                {
                    // Handle cultures that don't have valid region information
                  //  Debug.WriteLine($"Skipping culture '{culture.Name}' due to: {ex.Message}");
                }
            }

            // Order by EnglishName and return the list
            return countries.OrderBy(p => p.EnglishName).ToList();
        }

        /// <summary>
        /// Get Country Model by Country Name
        /// </summary>
        /// <param name="countryName">English Name of Country</param>
        /// <returns>Complete Country Model with Region, Flag, Name and Code</returns>
        public static CountryModel GetCountryModelByName(string countryName)
        {
            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
            List<RegionInfo> isoCountries = GetCountriesByIso3166();
            RegionInfo regionInfo = isoCountries.FirstOrDefault(c => c.EnglishName == countryName);
            return regionInfo != null
                ? new CountryModel
                {
                    CountryCode = phoneNumberUtil.GetCountryCodeForRegion(regionInfo.TwoLetterISORegionName).ToString(),
                    CountryName = regionInfo.EnglishName,
                    FlagUrl = $"https://hatscripts.github.io/circle-flags/flags/{regionInfo.TwoLetterISORegionName.ToLower()}.svg",
                }
                : new CountryModel
                {
                    CountryName = countryName
                };
        }
    }
}
