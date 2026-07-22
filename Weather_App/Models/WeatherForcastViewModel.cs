namespace Weather_App.Models
{
    public class WeatherForcastViewModel
    {
        public string Time { get; set; } = string.Empty;
        public double ScreenTemperature { get; set; }
        public double FeelsLikeTemperature { get; set; }
        public double WindSpeed { get; set; }
        public double RainChance { get; set; }
    }
}
