using Kolaytik.Core.DTOs.ApiKey;

namespace Kolaytik.Core.Interfaces.Services;

public interface IApiKeyService
{
    Task<IList<ApiKeyResponse>> GetApiKeysAsync();
    Task<ApiKeyResponse> GetApiKeyAsync(Guid id);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<ApiKeyResponse> UpdateApiKeyAsync(Guid id, UpdateApiKeyRequest request);
    Task DeleteApiKeyAsync(Guid id);
}
