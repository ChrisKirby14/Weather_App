using System.Text.Json;
using Weather_App.Models;

namespace Weather_App
{
    public class GetApiKeys
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GetApiKeys(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        private async Task<string> FetchWeatherJsonAsync(string url, string configKeyName)
        {
            var apikey = _config[configKeyName];

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", apikey);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        public async Task<WeatherLocation?> GetLocationCoordinatesAsync(string city)
        {
            string geocodeUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={city}&count=1&language=en&format=json";
            var response = await _httpClient.GetAsync(geocodeUrl);

            if (!response.IsSuccessStatusCode) return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            using var locDoc = JsonDocument.Parse(jsonString);

            if (!locDoc.RootElement.TryGetProperty("results", out JsonElement results) || results.GetArrayLength() == 0)
                return null;

            var firstResult = results[0];
            return new WeatherLocation
            {
                Location = firstResult.GetProperty("name").GetString(),
                Latitude = firstResult.GetProperty("latitude").GetDouble(),
                Longitude = firstResult.GetProperty("longitude").GetDouble(),
                Timezone = firstResult.GetProperty("timezone").GetString() ?? "UTC"
            };
        }

        public async Task<string> GetSunriseSunSetJsonAsync(WeatherLocation location)
        {
            string url = $"https://api.sunrise-sunset.org/json?lat={location.Latitude}&lng={location.Longitude}&formatted=1&tzid={location.Timezone}";

            var response = await _httpClient.GetAsync(url);

            if ( response.IsSuccessStatusCode )
            {
                return await response.Content.ReadAsStringAsync() ;
            }

            return string.Empty;
        }

        public async Task<List<WeatherForcastViewModel>> GetHourlyForecastAsync(WeatherLocation location)
        {
            string hourlyJson = await GetHourlyWeatherJsonAsync(location);
            return WeatherParser.ParseHourly(hourlyJson);
        }

        public async Task<List<DailyForecastViewModel>> GetDailyForecastAsync(WeatherLocation location)
        {
            string dailyJson = await GetDailyWeatherJsonAsync(location);
            return WeatherParser.ParseDaily(dailyJson);
        }

        private async Task<string> GetHourlyWeatherJsonAsync(WeatherLocation location)
        {
            string url = $"https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/hourly?latitude={location.Latitude}&longitude={location.Longitude}";

            return await FetchWeatherJsonAsync(url, "Weather_Site_Specific:ServiceApiKey");
        }

        private async Task<string> GetDailyWeatherJsonAsync(WeatherLocation location)
        {
            string url = $"https://data.hub.api.metoffice.gov.uk/sitespecific/v0/point/daily?latitude={location.Latitude}&longitude={location.Longitude}";

            return await FetchWeatherJsonAsync(url, "Weather_Site_Specific:ServiceApiKey");
        }
    }
}
