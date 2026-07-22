using Microsoft.EntityFrameworkCore;

public class Weather_AppContext(DbContextOptions<Weather_AppContext> options) : DbContext(options)
{
    public DbSet<Weather_App.Models.WeatherLocation> Weather { get; set; } = default!;
}
