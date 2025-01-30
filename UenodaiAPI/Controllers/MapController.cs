using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UenodaiCommon.Models.DTOs;
using System.Text.Json.Serialization;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MapController> _logger;
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public MapController(HttpClient httpClient, ILogger<MapController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> ConvertCoordinatesToAddress(double lat, double lng)
    {
        await _semaphore.WaitAsync();
        try
        {
            var address = await GetAddressFromNominatimAsync(lat, lng);
            if (address == null)
            {
                _logger.LogWarning("サーバー側で住所が見つかりませんでした。座標: {Lat}, {Lng}", lat, lng);
                return NotFound("住所が見つかりませんでした");
            }
            _logger.LogInformation("サーバー側で住所を返しました: {Address}", address);
            return Ok(new AddressDTO { Address = address });
        }
        catch (Exception ex)
        {
            _logger.LogError("サーバー側で住所取得中にエラーが発生しました: {Message}", ex.Message);
            return StatusCode(500, "内部サーバーエラー");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string?> GetAddressFromNominatimAsync(double lat, double lng)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";

        try
        {
            _logger.LogInformation("Sending request to Nominatim API for coordinates: lat = {Lat}, lon = {Lng}", lat, lng);

            // User-Agent ヘッダーを追加
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("User-Agent", "YourAppName/1.0 (your@email.com)");

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<NominatimResponse>(responseBody);
                if (result != null)
                {
                    if (string.IsNullOrEmpty(result.DisplayName))
                    {
                        _logger.LogWarning("DisplayName is null or empty in Nominatim response.");
                    }
                    else
                    {
                        _logger.LogInformation("Successfully retrieved address: {DisplayName}", result.DisplayName);
                    }
                    return result.DisplayName;
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize the response body from Nominatim.");
                }
            }
            else
            {
                _logger.LogWarning("Received unsuccessful status code {StatusCode} from Nominatim.", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while making the request to Nominatim.");
        }

        return null;
    }
}

public class NominatimResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("address")]
    public Address? Address { get; set; }
}


public class Address
{
    [JsonPropertyName("tourism")]
    public string? Tourism { get; set; }

    [JsonPropertyName("road")]
    public string? Road { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("county")]
    public string? County { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
