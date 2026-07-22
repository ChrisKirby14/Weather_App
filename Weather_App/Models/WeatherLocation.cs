using System.ComponentModel.DataAnnotations;

namespace Weather_App.Models
{
    public class WeatherLocation
    {
        public int Id { get; set; }
        public string? Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
