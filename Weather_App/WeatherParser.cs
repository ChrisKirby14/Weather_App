using System.Text.Json;
using Weather_App.Models;

namespace Weather_App
{
    public class WeatherParser
    {
        public static List<WeatherForcastViewModel> ParseHourly(string json)
        {
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
                            WindDirection = hour.GetProperty("windDirectionFrom10m").GetDouble(),
                            UVIndex = hour.GetProperty("uvIndex").GetInt32(),
                            RainChance = hour.TryGetProperty("probOfPrecipitation", out JsonElement rainEl) ? rainEl.GetDouble() : 0
                        });
                    }
                }
            }

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
                    foreach (var day in timeSeries.EnumerateArray())
                    {
                        string dateString = day.GetProperty("time").GetString() ?? "";

                        if (DateTime.TryParse(dateString, out DateTime parsedDate))
                        {
                            if (parsedDate.Date < DateTime.Today)
                            {
                                continue;
                            }
                        }

                        // Safely grab the values. If the API misses a day, we default to 0 to prevent a crash.
                        double maxTemp = day.TryGetProperty("dayMaxScreenTemperature", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
                        double minNightTemp = day.TryGetProperty("nightMinScreenTemperature", out JsonElement minEl) ? minEl.GetDouble() : 0;
                        double dailyRainChance = day.TryGetProperty("dayProbabilityOfRain", out JsonElement rainEL) ? rainEL.GetDouble() : 0;
                        int maxUvIndex = day.TryGetProperty("maxUvIndex", out JsonElement maxUv) ? maxUv.GetInt32() : 0;


                        forecasts.Add(new DailyForecastViewModel
                        {
                            Date = dateString,
                            MaxUpperTemperature = maxTemp,
                            MinNightTemperature = minNightTemp,
                            DailyRainChance = dailyRainChance,
                            MaxUvIndex = maxUvIndex,
                        });
                    }
                }
            }

            return forecasts;
        }

        public static void ParseAndAssignSunriseSunset(string json, List<DailyForecastViewModel> forecasts)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("days", out JsonElement daysArray))
            {
                foreach (var dayElement in daysArray.EnumerateArray())
                {
                    string dateStr = dayElement.GetProperty("date").GetString() ?? "";

                    string rawSunrise = dayElement.GetProperty("sunrise").GetString() ?? "";
                    string rawSunset = dayElement.GetProperty("sunset").GetString() ?? "";

                    string sunriseTime = ExtractPureTime(rawSunrise);
                    string sunsetTime = ExtractPureTime(rawSunset);

                    var match = forecasts.FirstOrDefault(f => f.Date != null && f.Date.StartsWith(dateStr));
                    if (match != null)
                    {
                        match.SunTimes = new SunriseSunsetViewModel
                        {
                            Sunrise = sunriseTime,
                            Sunset = sunsetTime
                        };
                    }
                }
            }
        }

        private static string ExtractPureTime(string isoString)
        {
            if (string.IsNullOrEmpty(isoString)) return string.Empty;

            var parts = isoString.Split('T');
            if (parts.Length < 2) return isoString;

            var timePart = parts[1].Split(new char[] { '+', '-', 'Z' })[0];
            return timePart;
        }
    }
}
