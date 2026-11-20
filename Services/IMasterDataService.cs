using CrmLeadManagement.Models.Dtos;

namespace CrmLeadManagement.Services;

public interface IMasterDataService
{
    Task<IReadOnlyList<MasterDataResponse>> GetAsync(
        string? masterType,
        int? parentId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<MasterDataResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<MasterDataResponse> CreateAsync(CreateMasterDataRequest request, CancellationToken cancellationToken);

    Task<MasterDataResponse> CreateRequirementItemAsync(
        CreateRequirementItemRequest request,
        CancellationToken cancellationToken);

    Task<MasterDataResponse?> UpdateAsync(int id, UpdateMasterDataRequest request, CancellationToken cancellationToken);

    Task<MasterDataResponse?> DeactivateAsync(int id, CancellationToken cancellationToken);

    Task<RequirementMasterResolution> ResolveRequirementSelectionsAsync(
        IReadOnlyCollection<RequirementMasterSelectionIds> selections,
        CancellationToken cancellationToken);
}
