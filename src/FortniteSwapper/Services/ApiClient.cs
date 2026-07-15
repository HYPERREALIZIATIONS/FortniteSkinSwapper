using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FortniteSwapper.Models;
using FortniteSwapper.Models.Api;

namespace FortniteSwapper.Services;

public interface IApiClient
{
    Task<List<Cosmetic>> GetCosmeticsAsync();
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private const string Url = "https://fortnite-api.com/v2/cosmetics/br?language=en";

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = System.TimeSpan.FromSeconds(30);
    }

    public async Task<List<Cosmetic>> GetCosmeticsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ApiCosmetic>>>(Url, JsonDefaults.Options);
        if (response?.Data is null)
        {
            throw new System.InvalidOperationException("Fortnite-API returned an empty response.");
        }

        var list = new List<Cosmetic>(response.Data.Count);
        foreach (var c in response.Data)
        {
            list.Add(c.ToCosmetic());
        }

        return list;
    }
}
