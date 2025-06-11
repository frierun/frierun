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
                    name,
                    config_src = "cloudflare",
                }
            )
            .Result
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

    public JsonObject GetTunnelConfiguration(string accountId, string tunnelId)
    {
        var result = _httpClient
            .GetFromJsonAsync<JsonNode>($"accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations")
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to get tunnel configuration: {result?["errors"]?.ToJsonString()}");
        }

        return result["result"]?["config"]?.DeepClone().AsObject() ?? new JsonObject();
    }

    public void UpdateTunnelConfiguration(string accountId, string tunnelId, JsonNode config)
    {
        var result = _httpClient
            .PutAsJsonAsync(
                $"accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations",
                new JsonObject
                {
                    ["config"] = config
                }
            )
            .Result
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to update tunnel configuration: {result?["errors"]?.ToJsonString()}");
        }
    }

    public void DeleteTunnel(string accountId, string tunnelId)
    {
        var result = _httpClient.DeleteAsync($"accounts/{accountId}/cfd_tunnel/{tunnelId}")
            .Result
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to delete tunnel: {result?["errors"]?.ToJsonString()}");
        }
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

    public void CreateDnsRecord(string zoneId, JsonObject record)
    {
        var result = _httpClient
            .PostAsJsonAsync($"zones/{zoneId}/dns_records", record)
            .Result
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to create DNS record: {result?["errors"]?.ToJsonString()}");
        }
    }

    public IEnumerable<JsonObject> GetDnsRecords(string zoneId)
    {
        const int perPage = 100;
        var page = 0;
        var totalCount = 1;

        while(totalCount > page * perPage)
        {
            page++;
            var result = _httpClient
                .GetFromJsonAsync<JsonNode>($"zones/{zoneId}/dns_records?page={page}&per_page={perPage}")
                .Result;

            if (result?["success"]?.GetValue<bool>() != true)
            {
                throw new Exception($"Failed to list DNS records: {result?["errors"]?.ToJsonString()}");
            }

            if (result["result"] is not JsonArray records)
            {
                throw new Exception("Unexpected response format: 'result' is null or not an array.");
            }

            foreach (var record in records)
            {
                if (record is not JsonObject jsonObject)
                {
                    throw new Exception("Wrong response format: 'result' contains non-object items.");
                }

                yield return jsonObject;
            }

            totalCount = result["result_info"]?["total_count"]?.GetValue<int>() ?? 0;
        }
    }

    public void DeleteDnsRecord(string zoneId, string recordId)
    {
        var result = _httpClient
            .DeleteAsync($"zones/{zoneId}/dns_records/{recordId}")
            .Result
            .Content
            .ReadFromJsonAsync<JsonNode>()
            .Result;

        if (result?["success"]?.GetValue<bool>() != true)
        {
            throw new Exception($"Failed to delete DNS record: {result?["errors"]?.ToJsonString()}");
        }
    }
}