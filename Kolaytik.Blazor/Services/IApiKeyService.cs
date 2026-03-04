using Kolaytik.Blazor.Models.ApiKeys;

namespace Kolaytik.Blazor.Services;

public interface IApiKeyService
{
    Task<IList<ApiKeyResponse>?> GetApiKeysAsync();
    Task<CreateApiKeyResponse?> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<ApiKeyResponse?> UpdateApiKeyAsync(Guid id, UpdateApiKeyRequest request);
    Task<bool> DeleteApiKeyAsync(Guid id);
}
