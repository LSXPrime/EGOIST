using System.ComponentModel;
using System.Net.Http;
using Microsoft.SemanticKernel;

using Newtonsoft.Json;

namespace EGOIST.Data.Plugins;

[Description("WeatherPlugin Gets weather information for a specified location")]
public class WeatherPlugin
{
    [KernelFunction("GetWeather")]
    public async Task<string> GetWeatherAsync(string location)
    {
        using HttpClient client = new();
        // Get location Geocoding using OpenStreetMap Nominatim API
        var response = await client.GetAsync($"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(location)}");
        if (response.IsSuccessStatusCode)
        {
            string responseLocation = await response.Content.ReadAsStringAsync();
            var city = JsonConvert.DeserializeObject<CityGeocoding>(responseLocation);


            // Get weather using Open-Meteo API
            var weatherResponse = await client.GetAsync($"https://api.open-meteo.com/v1/forecast?latitude={city.Latitude}&longitude={city.Longitude}&current=temperature_2m,wind_speed_10m");

            if (weatherResponse.IsSuccessStatusCode)
            {
                string weatherResponseBody = await weatherResponse.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(weatherResponseBody);

                return $"Current Weather in {location}: Time - {weatherData.current.Time}, Temperature - {weatherData.current.Temperature}°C, Wind Speed - {weatherData.current.WindSpeed} m/s";
            }
        }

        return "Failed to retrieve weather data.";
    }

    public class CityGeocoding
    {
        [JsonProperty("lat")]
        public double Latitude;
        [JsonProperty("lon")]
        public double Longitude;
    }

    public class WeatherResponse
    {
        public CurrentWeather current { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("time")]
        public DateTime Time { get; set; }
        [JsonProperty("temperature_2m")]
        public double Temperature { get; set; }
        [JsonProperty("wind_speed_10m")]
        public double WindSpeed { get; set; }
    }
}

