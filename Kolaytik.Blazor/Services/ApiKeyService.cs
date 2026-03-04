using Kolaytik.Blazor.Models.ApiKeys;

namespace Kolaytik.Blazor.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly ApiClient _api;

    public ApiKeyService(ApiClient api) => _api = api;

    public async Task<IList<ApiKeyResponse>?> GetApiKeysAsync()
    {
        var result = await _api.GetAsync<IList<ApiKeyResponse>>("api/api-keys");
        return result?.Data;
    }

    public async Task<ApiKeyResponse?> GetApiKeyAsync(Guid id)
    {
        var result = await _api.GetAsync<ApiKeyResponse>($"api/api-keys/{id}");
        return result?.Data;
    }

    public async Task<CreateApiKeyResponse?> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        var result = await _api.PostAsync<CreateApiKeyResponse>("api/api-keys", request);
        return result?.Data;
    }

    public async Task<ApiKeyResponse?> UpdateApiKeyAsync(Guid id, UpdateApiKeyRequest request)
    {
        var result = await _api.PutAsync<ApiKeyResponse>($"api/api-keys/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteApiKeyAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/api-keys/{id}");
        return result?.Success ?? false;
    }
}
