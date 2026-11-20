using System.ComponentModel.DataAnnotations;

namespace CrmLeadManagement.Models.Dtos;

/// <summary>Active or inactive master data returned by the master data API.</summary>
public sealed record MasterDataResponse(
    int Id,
    string MasterType,
    string Name,
    int? ParentId,
    bool IsActive);

/// <summary>Payload for creating a master data option.</summary>
public sealed record CreateMasterDataRequest
{
    [Required, StringLength(20)]
    public required string MasterType { get; init; }

    [Required, StringLength(150)]
    public required string Name { get; init; }

    public int? ParentId { get; init; }
}

/// <summary>Payload for creating a requirement category and its item options.</summary>
public sealed record CreateRequirementItemRequest
{
    public int? CategoryId { get; init; }

    [StringLength(150)]
    public string? NewCategoryName { get; init; }

    [Required, StringLength(150)]
    public required string ItemName { get; init; }

    public IReadOnlyList<string> Types { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Topologies { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Glass { get; init; } = Array.Empty<string>();
}

/// <summary>Payload for renaming a master data option.</summary>
public sealed record UpdateMasterDataRequest
{
    [Required, StringLength(150)]
    public required string Name { get; init; }
}

/// <summary>Selected master IDs for one requirement item.</summary>
public sealed record RequirementMasterSelectionIds(
    int? CategoryId,
    int? ItemId,
    int? TypeId,
    int? TopologyId,
    int? GlassId);

/// <summary>Names resolved from the selected master IDs for one requirement item.</summary>
public sealed record RequirementMasterSelectionNames(
    string Category,
    string Item,
    string Type,
    string Topology,
    string Glass);

/// <summary>Result of validating selected requirement master IDs.</summary>
public sealed record RequirementMasterResolution(
    bool IsValid,
    string? Error,
    IReadOnlyList<RequirementMasterSelectionNames> Items);
