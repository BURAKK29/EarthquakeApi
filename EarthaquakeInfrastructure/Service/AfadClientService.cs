using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using EarthaquakeInfrastructure.Service;
using System.Text.Json;
public class AfadClientService : IAfadClientService
{
    private readonly HttpClient _httpClient;

    public AfadClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<EarthquakeModel>> GetEarthquakesAsync()
    {
        var nowUtc = DateTime.UtcNow;
        string start = Uri.EscapeDataString(nowUtc.AddMinutes(-120).ToString("yyyy-MM-dd HH:mm:ss"));
        string end   = Uri.EscapeDataString(nowUtc.ToString("yyyy-MM-dd HH:mm:ss"));

        string url = $"https://deprem.afad.gov.tr/apiv2/event/filter?start={start}&end={end}";

        Console.WriteLine($"[DEBUG] AFAD API çağrısı: {url}");

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)//http kodu 200 değilse
        {
            Console.WriteLine($"[ERROR] AFAD API başarısız: {response.StatusCode}");
            return new List<EarthquakeModel>();
        }

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[DEBUG] AFAD API yanıtı:");
        Console.WriteLine(json);

        try
        {
            var earthquakes = JsonSerializer.Deserialize<List<EarthquakeModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true//büyük küçük harf duyarlılığı
            });

            return earthquakes ?? new List<EarthquakeModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] JSON parse hatası: " + ex.Message);
            return new List<EarthquakeModel>();
        }
    }
}
