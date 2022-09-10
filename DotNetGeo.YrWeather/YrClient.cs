using DotNetGeo.Yr.Structures;
using System.Text.Json;

namespace DotNetGeo.Yr.Core;

public class YrClient
{
    private HttpClient HttpClient {get; set; }

    public YrClient()
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://www.yr.no/api/"),
        };
        HttpClient.DefaultRequestHeaders.UserAgent.Add(new("DotNetGeo YrClient; https://github.com/julian94/DotNetGeo; langlo94@gmail.com;"));
    }

    public async Task<WeatherForecast> GetForecast(string id)
    {
        // To get forecast for specific coordinate use Lat Lon ordering.
        var response = await HttpClient.GetAsync($"v0/locations/{id}/forecast");

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStreamAsync();

        var forecast = JsonSerializer.Deserialize<WeatherForecast>(data);
        if (forecast == null) throw new FormatException("Unable to parse the forecast from Yr. Has the API been updated?");

        return forecast;
    }
}
