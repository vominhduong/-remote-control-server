using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace remoteControllerApp.Firebase;

public class RealtimeDatabaseClient
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseSettings _settings;
    private readonly FirebaseAuthService _authService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RealtimeDatabaseClient(
        HttpClient httpClient,
        IOptions<FirebaseSettings> options,
        FirebaseAuthService authService)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _authService = authService;
    }

    public async Task SetAsync<T>(string path, T data)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl(path));

        await AddAuthHeaderAsync(request);

        var json = JsonSerializer.Serialize(data, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    public async Task PatchAsync<T>(string path, T data)
    {
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), BuildUrl(path));

        await AddAuthHeaderAsync(request);

        var json = JsonSerializer.Serialize(data, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(path));

        await AddAuthHeaderAsync(request);

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task DeleteAsync(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl(path));

        await AddAuthHeaderAsync(request);

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_settings.DatabaseUrl))
        {
            throw new InvalidOperationException("Firebase DatabaseUrl is missing.");
        }

        var databaseUrl = _settings.DatabaseUrl.TrimEnd('/');
        var cleanPath = path.Trim('/');

        return $"{databaseUrl}/{cleanPath}.json";
    }

    private async Task AddAuthHeaderAsync(HttpRequestMessage request)
    {
        var token = await _authService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();

        throw new HttpRequestException(
            $"Firebase Realtime Database request failed. Status: {(int)response.StatusCode}. Body: {body}"
        );
    }
}