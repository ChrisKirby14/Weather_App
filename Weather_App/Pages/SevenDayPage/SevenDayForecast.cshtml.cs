using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weather_App.Models;

namespace Weather_App.Pages.SevenDayPage
{
    public class SevenDayForecastModel : PageModel
    {
        public List<DailyForecastViewModel>? Forecasts { get; set; }

        public void OnGet()
        {
        }
    }
}
