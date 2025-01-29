using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public MapController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchLocation(double lat, double lng)
    {
        // Nominatim APIにリクエスト
        await _semaphore.WaitAsync();
        try
        {
            var address = await GetAddressFromNominatimAsync(lat, lng);
            return Ok(new { Address = address });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> GetAddressFromNominatimAsync(double lat, double lng)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";
        var response = await _httpClient.GetStringAsync(url);

        // Nominatim APIからの応答を処理（JSONをパース）
        var result = JsonSerializer.Deserialize<NominatimResult>(response);
        return result?.DisplayName ?? "住所が見つかりませんでした";
    }
}

public class NominatimResult
{
    public string DisplayName { get; set; }
}
