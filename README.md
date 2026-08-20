# Weather App

---

## Intro
This site is for checking the weather in your area. It shows up to 3 days of hourly weather, displaying temperatures, rain chance, UV index, and the Max/Min temperature for the day. 

There is also a 7-day forecast, displaying the Max/Min temperatures, possibility of rain, Max UV, and the times of sunrise and sunset.

### Hourly Forecast
<img width="800" alt="Hourly Forecast View" src="https://github.com/user-attachments/assets/4896c4c6-4aab-48a4-a28a-3034b293f989" />

### 7-Day Forecast
<img width="800" alt="7 Day Forecast View" src="https://github.com/user-attachments/assets/eb96ec1a-d669-4d0c-bfa3-fbcc80b73d1f" />


---

## Live Demo
**URL:** [Weather App Website on Azure](https://kirbyweather-f2bnejg9grb7hxe9.ukwest-01.azurewebsites.net)

---

## Technical Skills Demonstrated
*   **API Integration:** Engineered the backend to securely request and parse live data from external REST APIs.
*   **Cloud Hosting:** Deployed and hosted the application using Microsoft Azure App Services (Free Tier).
*   **Web Architecture:** Built using C# and ASP.NET Core Razor Pages, demonstrating a solid understanding of frontend/backend traffic and routing.

---

## 🌍 Global Timezone Architecture & Edge-Case Handling
* **Timezone Normalisation:** Integrated `TimeZoneConverter` (`TZConvert`) to handle global cities dynamically, converting server-side UTC into accurate local times for hourly forecasts, sunrises, sunsets, and date boundaries.
* **Defensive Parsing:** Replaced direct JSON property lookups with safe `TryGetProperty` patterns and graceful fallbacks (e.g., defaulting to UTC) to prevent unhandled exceptions on incomplete API payloads.
* **Boundary Testing:** Validated date-filtering logic against extreme global offsets (such as UTC+14 and negative offset zones) to ensure accurate calendar day rollovers across the International Date Line.
