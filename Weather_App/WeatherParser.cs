using System.Text.Json;
using Weather_App.Models;

namespace Weather_App
{
    public class WeatherParser
    {
        public static List<WeatherForcastViewModel> ParseHourly(string json)
        {
            // 1. Create a local list to hold our upcoming hours
            var forecasts = new List<WeatherForcastViewModel>();

            using var weatherDoc = JsonDocument.Parse(json);
            var weatherRoot = weatherDoc.RootElement;

            if (weatherRoot.TryGetProperty("features", out JsonElement features) && features.GetArrayLength() > 0)
            {
                var properties = features[0].GetProperty("properties");
                if (properties.TryGetProperty("timeSeries", out JsonElement timeSeries) && timeSeries.GetArrayLength() > 0)
                {
                    foreach (var hour in timeSeries.EnumerateArray())
                    {
                        forecasts.Add(new WeatherForcastViewModel
                        {
                            Time = hour.GetProperty("time").GetString() ?? "",
                            ScreenTemperature = hour.GetProperty("screenTemperature").GetDouble(),
                            FeelsLikeTemperature = hour.GetProperty("feelsLikeTemperature").GetDouble(),
                            WindSpeed = hour.GetProperty("windSpeed10m").GetDouble(),
                            RainChance = hour.TryGetProperty("probOfPrecipitation", out JsonElement rainEl) ? rainEl.GetDouble() : 0
                        });
                    }
                }
            }

            // 3. Return the fully populated list back to OnGetAsync
            return forecasts;
        }

        public static List<DailyForecastViewModel> ParseDaily(string json)
        {
            var forecasts = new List<DailyForecastViewModel>();

            using var weatherDoc = JsonDocument.Parse(json);
            var weatherRoot = weatherDoc.RootElement;

            if (weatherRoot.TryGetProperty("features", out JsonElement features) && features.GetArrayLength() > 0)
            {
                var properties = features[0].GetProperty("properties");
                if (properties.TryGetProperty("timeSeries", out JsonElement timeSeries) && timeSeries.GetArrayLength() > 0)
                {
                    // Loop through the 7 days provided by the daily API
                    foreach (var day in timeSeries.EnumerateArray())
                    {
                        // Safely grab the values. If the API misses a day, we default to 0 to prevent a crash.
                        double maxTemp = day.TryGetProperty("dayMaxScreenTemperature", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
                        double minNightTemp = day.TryGetProperty("nightMinScreenTemperature", out JsonElement minEl) ? minEl.GetDouble() : 0;
                        double dailyRainChance = day.TryGetProperty("dayProbabilityOfRain", out JsonElement rainEL) ? rainEL.GetDouble() : 0;


                        forecasts.Add(new DailyForecastViewModel
                        {
                            Date = day.GetProperty("time").GetString() ?? "",
                            MaxUpperTemperature = maxTemp,
                            MinNightTemperature = minNightTemp,
                            DailyRainChance = dailyRainChance,
                        });
                    }
                }
            }

            return forecasts;
        }

        public static SunriseSunsetViewModel ParseSunriseSunset(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("results", out JsonElement results))
            {
                return new SunriseSunsetViewModel
                {
                    Sunrise = results.GetProperty("sunrise").GetString() ?? "",
                    Sunset = results.GetProperty("sunset").GetString() ?? ""
                };
            }

            return new SunriseSunsetViewModel();
        }
    }
}
