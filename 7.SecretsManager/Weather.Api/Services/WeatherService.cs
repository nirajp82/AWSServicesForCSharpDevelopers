using System.Net;
using Microsoft.Extensions.Options;
using Weather.Api.Models;

namespace Weather.Api.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    //IOptionsMonitor: Used for notifications when OpenWeatherApiSettings instances change.
    private readonly IOptionsMonitor<OpenWeatherApiSettings> _weatherApiOptions;

    public WeatherService(IHttpClientFactory httpClientFactory, 
        IOptionsMonitor<OpenWeatherApiSettings> weatherApiOptions)
    {
        _httpClientFactory = httpClientFactory;
        _weatherApiOptions = weatherApiOptions;
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_weatherApiOptions.CurrentValue.ApiKey}&units=metric";
        var httpClient = _httpClientFactory.CreateClient();
        
        var weatherResponse = await httpClient.GetAsync(url);
        if (weatherResponse.IsSuccessStatusCode)
        {
            var weather = await weatherResponse.Content.ReadFromJsonAsync<WeatherResponse>();
            return weather;
        }
        return null;
    }
}
