namespace Weather_App.Models
{
    public class MapImageViewModel
    {
            public List<string> TemperatureMapUrls { get; set; } = new List<string>();
            public List<string> RainfallMapUrls { get; set; } = new List<string>();
            public List<string> CloudMapUrls { get; set; } = new List<string>();
            public List<string> PressureMapUrls { get; set; } = new List<string>();
        }
    }