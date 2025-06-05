using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Frierun.Server;

public class CloudflareClient : ICloudflareClient
{
    private readonly HttpClient _httpClient;

    public CloudflareClient(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _httpClient.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
    }

    public bool VerifyToken()
    {
        var result = _httpClient.GetFromJsonAsync<JsonNode>("user/tokens/verify").Result;
        return result?["success"]?.GetValue<bool>() == true;
    }

    public IEnumerable<(string id, string name)> GetAccounts()
    {
        var result = _httpClient.GetFromJsonAsync<JsonNode>("accounts").Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to get accounts: {result?["errors"]?.ToJsonString()}");
        }

        if (result["result"] is not JsonArray accounts)
        {
            throw new Exception("Unexpected response format: 'result' is null or not an array.");
        }

        return accounts.Select(
            account => (account?["id"]?.GetValue<string>() ?? "", account?["name"]?.GetValue<string>() ?? "")
        );
    }

    public (string id, string token) CreateTunnel(string accountId, string name)
    {
        var result = _httpClient
            .PostAsJsonAsync(
                $"accounts/{accountId}/cfd_tunnel",
                new
                {
                    name = name,
                    config_src = "cloudflare",
                }
            )
            .Result
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to create tunnel: {result?["errors"]?.ToJsonString()}");
        }

        result = result["result"];
        if (result == null)
        {
            throw new Exception("Unexpected response format: 'result' is null.");
        }

        return (
            result["id"]?.GetValue<string>() ?? throw new Exception("Tunnel ID not found in response."),
            result["token"]?.GetValue<string>() ?? throw new Exception("Tunnel token not found in response.")
        );
    }
    
    public bool DeleteTunnel(string accountId, string tunnelId)
    {
        var result = _httpClient.DeleteAsync($"accounts/{accountId}/cfd_tunnel/{tunnelId}")
            .Result
            .EnsureSuccessStatusCode()
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        return result?["success"]?.GetValue<bool>() == true;
    }

    public IEnumerable<(string id, string name)> GetZones()
    {
        var result = _httpClient.GetFromJsonAsync<JsonNode>("zones").Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to get zones: {result?["errors"]?.ToJsonString()}");
        }

        if (result["result"] is not JsonArray zones)
        {
            throw new Exception("Unexpected response format: 'result' is null or not an array.");
        }

        return zones.Select(
            zone => (zone?["id"]?.GetValue<string>() ?? "", zone?["name"]?.GetValue<string>() ?? "")
        );
    }
}