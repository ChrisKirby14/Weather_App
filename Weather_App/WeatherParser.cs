using System.Text.Json;
using Weather_App.Models;

namespace Weather_App
{
    public class WeatherParser
    {
        private static List<T> ExtractTimeSeries<T>(string Json, Func<JsonElement, T?> parseItem) where T : class
        {
            var result = new List<T>();
            using var weatherDoc = JsonDocument.Parse(Json);
            var root = weatherDoc.RootElement;

            if (root.TryGetProperty("features", out JsonElement features) && features.GetArrayLength() > 0)
            {
                var properties = features[0].GetProperty("properties");
                if (properties.TryGetProperty("timeSeries", out var timeSeries))
                    foreach (var item in timeSeries.EnumerateArray())
                    {
                        var parsedItem = parseItem(item);

                        if (parsedItem != null)
                        {
                            result.Add(parsedItem);
                        }
                    }
            }

            return result;
        }

        public static List<WeatherForcastViewModel> ParseHourly(string json)
        {
            var now = DateTimeOffset.Now;
            var startOfCurrentHour = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Offset);

            return ExtractTimeSeries(json, hour =>
            {
                string timeString = hour.GetProperty("time").GetString() ?? "";

                if (DateTimeOffset.TryParse(timeString, out DateTimeOffset parsedDate))
                {
                    if (parsedDate < startOfCurrentHour)
                    {
                        return null;
                    }

                    return new WeatherForcastViewModel
                    {
                        Time = parsedDate.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),

                        ScreenTemperature = hour.GetProperty("screenTemperature").GetDouble(),
                        FeelsLikeTemperature = hour.GetProperty("feelsLikeTemperature").GetDouble(),
                        WindSpeed = hour.GetProperty("windSpeed10m").GetDouble(),
                        WindDirection = hour.GetProperty("windDirectionFrom10m").GetDouble(),
                        UVIndex = hour.GetProperty("uvIndex").GetInt32(),
                        RainChance = hour.TryGetProperty("probOfPrecipitation", out var rainEl) ? rainEl.GetDouble() : 0
                    };
                }

                return null;
            });
        }

        public static List<DailyForecastViewModel> ParseDaily(string json)
        {
            return ExtractTimeSeries(json, day =>
            {
                string dateString = day.GetProperty("time").GetString() ?? "";

                if (DateTime.TryParse(dateString, out DateTime parsedDate))
                {
                    if (parsedDate.Date < DateTime.Today)
                    {
                        return null;
                    }
                }

                return new DailyForecastViewModel
                {
                    Date = dateString,
                    MaxUpperTemperature = day.TryGetProperty("dayMaxScreenTemperature", out var maxEl) ? maxEl.GetDouble() : 0,
                    MinNightTemperature = day.TryGetProperty("nightMinScreenTemperature", out var minEl) ? minEl.GetDouble() : 0,
                    DailyRainChance = day.TryGetProperty("dayProbabilityOfRain", out var rainEl) ? rainEl.GetDouble() : 0,
                    MaxUvIndex = day.TryGetProperty("maxUvIndex", out var maxUv) ? maxUv.GetInt32() : 0,
                };
            });
        }

        public static void ParseAndAssignSunriseSunset(string json, List<DailyForecastViewModel> forecasts)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("days", out JsonElement daysArray))
            {
                return;
            }

            foreach (var dayElement in daysArray.EnumerateArray())
            {
                string dateStr = dayElement.GetProperty("date").GetString() ?? "";

                var match = forecasts.FirstOrDefault(f => f.Date != null && f.Date.StartsWith(dateStr));
                if (match == null) continue;

                string rawSunrise = dayElement.GetProperty("sunrise").GetString() ?? "";
                string rawSunset = dayElement.GetProperty("sunset").GetString() ?? "";

                match.SunTimes = new SunriseSunsetViewModel
                {
                    Sunrise = FormatLocalTime(rawSunrise),
                    Sunset = FormatLocalTime(rawSunset)
                };
            }
        }

        private static string FormatLocalTime(string rawTime)
        {
            if (DateTimeOffset.TryParse(rawTime, out DateTimeOffset parsedTime))
            {
                return parsedTime.ToLocalTime().ToString("HH:mm");
            }

            return rawTime;
        }

    }
}
