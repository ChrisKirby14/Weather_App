using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weather_App.Models;

namespace Weather_App.Pages
{
    public class IndexModel : PageModel
    {
        private GetApiKeys _apiKeys;
        
        [BindProperty(SupportsGet=true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int DaysToDisplay { get; set; } = 7;

        public WeatherLocation? CurrentLocation { get; set; }
        public List<WeatherForcastViewModel>? HourlyForecastList { get; set; }
        public List<DailyForecastViewModel>? DailyForecastList { get; set; }

        public string? RawWeatherJson { get; set; }

        public IndexModel(IConfiguration config)
        {
            _apiKeys = new GetApiKeys(config);
        }

        public async Task OnGetAsync()
        {
            // 1. If there's no search term, do nothing.
            if (string.IsNullOrEmpty(SearchTerm)) return;

            var location = await _apiKeys.GetLocationCoordinatesAsync(SearchTerm);
            if (location == null) return;

            CurrentLocation = location;

            // 3. Go get the Met Office weather
            HourlyForecastList = await _apiKeys.GetHourlyForecastAsync(CurrentLocation);
            DailyForecastList = await _apiKeys.GetDailyForecastAsync(CurrentLocation);

            // FUTURE PHASES:
            // CurrentMap = await GetWeatherMapAsync(CurrentLocation);
            // CurrentObservations = await GetObservationsAsync(CurrentLocation);
        }
    }
}
