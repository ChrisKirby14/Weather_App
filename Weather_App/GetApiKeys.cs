using System.Text.Json;
using Weather_App.Models;

namespace Weather_App
{
    public class GetApiKeys
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GetApiKeys(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        /*private async Task<string?> FetchImageAsBase64Async(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Attach the API key in the header, exactly like we did for the JSON
            request.Headers.Add("apikey", _config["Weather_Map_Images:ServiceApiKey"]);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                // Return it as a data URI that the HTML <img> tag can read directly
                return $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            }

            return null;
        }*/
        
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

        public async Task<string> GetSunriseSunsetJsonAsync(WeatherLocation location, string startDate, string endDate)
        {
            string cleanStart = startDate.Split('T')[0];
            string cleanEnd = endDate.Split('T')[0];

            string url = $"https://api.sunrise-sunset.org/v2?lat={location.Latitude}&lng={location.Longitude}&date_start={cleanStart}&date_end={cleanEnd}&tz={location.Timezone}";

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
        
        /*
        public async Task<MapImageViewModel?> GetLatestMapImageUrlAsync()
        {
            var orderId = _config["Weather_Map_OrderId:OrderId"];
            if (string.IsNullOrEmpty(orderId)) return null;

            string latestUrl = $"https://data.hub.api.metoffice.gov.uk/map-images/1.0.0/orders/{orderId}/latest";

            string jsonString = await FetchWeatherJsonAsync(latestUrl, "Weather_Map_Images:ServiceApiKey");

            if (string.IsNullOrEmpty(jsonString)) return null;

            using var doc = JsonDocument.Parse(jsonString);

            // 1. Extract ALL file IDs for each parameter
            var tempFiles = ExtractFileIdsForParameter(doc.RootElement, "temp");
            //var rainFiles = ExtractFileIdsForParameter(doc.RootElement, "precip");
            //var cloudFiles = ExtractFileIdsForParameter(doc.RootElement, "cloud");
            //var pressureFiles = ExtractFileIdsForParameter(doc.RootElement, "pressure");

            var viewModel = new MapImageViewModel();

            int framesToAnimate = 12;

            async Task<List<string>> DownloadFramesFast(IEnumerable<string> fileIds)
            {
                // Fire off all download tasks at the exact same time
                var tasks = fileIds.Take(framesToAnimate).Select(async fileId =>
                {
                    string url = $"https://data.hub.api.metoffice.gov.uk/map-images/1.0.0/orders/{orderId}/latest/{Uri.EscapeDataString(fileId)}/data";
                    return await FetchImageAsBase64Async(url);
                });

                // Wait for all of them to finish at once, then filter out any nulls
                var results = await Task.WhenAll(tasks);
                return results.Where(r => r != null).ToList()!;
            }

            viewModel.TemperatureMapUrls = await DownloadFramesFast(tempFiles);
            //viewModel.RainfallMapUrls = await DownloadFramesFast(rainFiles);
            //viewModel.CloudMapUrls = await DownloadFramesFast(cloudFiles);
            //viewModel.PressureMapUrls = await DownloadFramesFast(pressureFiles);

            return viewModel;
        }
            
        private List<string> ExtractFileIdsForParameter(JsonElement rootElement, string keyword)
        {
            var fileIds = new List<string>();

            if (rootElement.TryGetProperty("orderDetails", out JsonElement orderDetails))
            {
                if (orderDetails.TryGetProperty("files", out JsonElement filesArray))
                {
                    foreach (var file in filesArray.EnumerateArray())
                    {
                        if (file.TryGetProperty("fileId", out JsonElement fileIdElement))
                        {
                            string? fileId = fileIdElement.GetString();
                            if (fileId != null && fileId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                fileIds.Add(fileId);
                            }
                        }
                    }
                }
            }
            return fileIds;
        }*/
    }
}
