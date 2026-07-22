using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weather_App.Models;

namespace Weather_App.Pages.SevenDayPage
{
    public class SevenDayForecastModel : PageModel
    {
        private readonly GetApiKeys _apiKeys;
        public List<DailyForecastViewModel>? Forecasts { get; set; }

        public SunriseSunsetViewModel? SunTimes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        public WeatherLocation? CurrentLocation { get; set; }

        public SevenDayForecastModel(IConfiguration config)
        {
            _apiKeys = new GetApiKeys(config);
        }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(SearchTerm)) return;

            var location = await _apiKeys.GetLocationCoordinatesAsync(SearchTerm);
            if (location == null) return;

            CurrentLocation = location;

            Forecasts = await _apiKeys.GetDailyForecastAsync(CurrentLocation);

            if (Forecasts != null && Forecasts.Any())
            {
                string startDate = Forecasts.First().Date!;
                string endDate = Forecasts.Last().Date!;

                string sunriseSunsetJson = await _apiKeys.GetSunriseSunsetJsonAsync(CurrentLocation, startDate, endDate);

                WeatherParser.ParseAndAssignSunriseSunset(sunriseSunsetJson, Forecasts);
            }
        }
    }
}
