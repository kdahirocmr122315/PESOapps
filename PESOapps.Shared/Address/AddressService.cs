using System.Net.Http.Json;

namespace PESOapps.Shared.Address;

public class AddressService(HttpClient httpClient) // Primary Constructor
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<Region>> GetRegionsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Region>>("_content/PESOapps.Shared/json/refregion.json") ?? [];
    }

    public async Task<List<Province>> GetProvincesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Province>>("_content/PESOapps.Shared/json/refprovince.json") ?? [];
    }

    public async Task<List<CityMunicipality>> GetCitiesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CityMunicipality>>("_content/PESOapps.Shared/json/refcitymun.json") ?? [];
    }

    public async Task<List<Barangay>> GetBarangaysAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Barangay>>("_content/PESOapps.Shared/json/refbrgy.json") ?? [];
    }
}