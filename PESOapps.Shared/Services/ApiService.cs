using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Beneficiary>> GetProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Beneficiary>>("http://localhost:5167/api/Tupad/getall");
    }
}