using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Frierun.Server;

public class CloudflareClient
{
    private readonly HttpClient _httpClient;

    public CloudflareClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
    }

    /// <summary>
    /// Verifies token
    /// </summary>
    /// <see href="https://developers.cloudflare.com/api/resources/user/subresources/tokens/" />
    public bool VerifyToken()
    {
        var result = _httpClient.GetFromJsonAsync<JsonNode>("user/tokens/verify").Result;
        return result?["success"]?.GetValue<bool>() == true;
    }
}