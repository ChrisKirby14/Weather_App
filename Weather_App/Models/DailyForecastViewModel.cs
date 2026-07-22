namespace Weather_App.Models
{
    public class DailyForecastViewModel
    {
        public string Date { get; set; } = string.Empty;
        public double MaxUpperTemperature { get; set; }
        public double MaxLowerTemperature { get; set; }
        public double MinNightTemperature { get; set; }
        public double DailyRainChance { get; set; }
    }
}
