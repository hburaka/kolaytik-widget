using Kolaytik.Blazor.Models.Sectors;

namespace Kolaytik.Blazor.Services;

public interface ISectorService
{
    Task<IList<SectorResponse>?> GetSectorsAsync();
    Task<SectorResponse?> CreateSectorAsync(CreateSectorRequest request);
    Task<SectorResponse?> UpdateSectorAsync(Guid id, UpdateSectorRequest request);
    Task<bool> DeleteSectorAsync(Guid id);
}
