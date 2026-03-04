using Kolaytik.Blazor.Models.Sectors;

namespace Kolaytik.Blazor.Services;

public class SectorService : ISectorService
{
    private readonly ApiClient _api;

    public SectorService(ApiClient api) => _api = api;

    public async Task<IList<SectorResponse>?> GetSectorsAsync()
    {
        var result = await _api.GetAsync<IList<SectorResponse>>("api/sectors");
        return result?.Data;
    }

    public async Task<SectorResponse?> GetSectorAsync(Guid id)
    {
        var result = await _api.GetAsync<SectorResponse>($"api/sectors/{id}");
        return result?.Data;
    }

    public async Task<SectorResponse?> CreateSectorAsync(CreateSectorRequest request)
    {
        var result = await _api.PostAsync<SectorResponse>("api/sectors", request);
        return result?.Data;
    }

    public async Task<SectorResponse?> UpdateSectorAsync(Guid id, UpdateSectorRequest request)
    {
        var result = await _api.PutAsync<SectorResponse>($"api/sectors/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteSectorAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/sectors/{id}");
        return result?.Success ?? false;
    }
}
