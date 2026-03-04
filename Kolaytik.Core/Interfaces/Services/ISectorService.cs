using Kolaytik.Core.DTOs.Sector;

namespace Kolaytik.Core.Interfaces.Services;

public interface ISectorService
{
    Task<IList<SectorResponse>> GetSectorsAsync();
    Task<SectorResponse> GetSectorAsync(Guid id);
    Task<SectorResponse> CreateSectorAsync(CreateSectorRequest request);
    Task<SectorResponse> UpdateSectorAsync(Guid id, UpdateSectorRequest request);
    Task DeleteSectorAsync(Guid id);
}
