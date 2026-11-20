using CrmLeadManagement.Models;
using CrmLeadManagement.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CrmLeadManagement.Services;

public sealed class MasterDataService : IMasterDataService
{
    private static readonly string[] MasterTypes = { "Category", "Item", "Type", "Topology", "Glass" };
    private readonly AppDbContext _db;

    public MasterDataService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MasterDataResponse>> GetAsync(
        string? masterType,
        int? parentId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var canonicalType = string.IsNullOrWhiteSpace(masterType) ? null : CanonicalizeMasterType(masterType);
        if (parentId.HasValue && canonicalType is null)
            throw new ArgumentException("Master type is required when filtering by parent.");

        IQueryable<MasterData> query = _db.MasterData.AsNoTracking();
        if (!includeInactive)
            query = query.Where(master => master.IsActive);
        if (canonicalType is not null)
            query = query.Where(master => master.MasterType == canonicalType);
        if (parentId.HasValue)
            query = query.Where(master => master.ParentId == parentId);

        var masters = await query.OrderBy(master => master.Name).ToListAsync(cancellationToken);
        return masters.Select(ToResponse).ToList();
    }

    public async Task<MasterDataResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var master = await _db.MasterData.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        return master is null ? null : ToResponse(master);
    }

    public async Task<MasterDataResponse> CreateAsync(CreateMasterDataRequest request, CancellationToken cancellationToken)
    {
        var masterType = CanonicalizeMasterType(request.MasterType);
        var name = NormalizeName(request.Name);
        await ValidateParentAsync(masterType, request.ParentId, cancellationToken);
        await EnsureNameIsUniqueAsync(masterType, request.ParentId, name, null, cancellationToken);

        var master = new MasterData
        {
            MasterType = masterType,
            Name = name,
            ParentId = request.ParentId,
            IsActive = true
        };
        _db.MasterData.Add(master);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(master);
    }

    public async Task<MasterDataResponse> CreateRequirementItemAsync(
        CreateRequirementItemRequest request,
        CancellationToken cancellationToken)
    {
        var itemName = NormalizeName(request.ItemName);
        var newCategoryName = string.IsNullOrWhiteSpace(request.NewCategoryName)
            ? null
            : NormalizeName(request.NewCategoryName);
        if (request.CategoryId.HasValue == (newCategoryName is not null))
            throw new ArgumentException("Select an existing category or enter a new category, not both.");

        var typeNames = NormalizeNames(request.Types);
        var topologyNames = NormalizeNames(request.Topologies);
        var glassNames = NormalizeNames(request.Glass);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        MasterData category;
        if (request.CategoryId.HasValue)
        {
            category = await _db.MasterData.FirstOrDefaultAsync(
                master => master.Id == request.CategoryId && master.MasterType == "Category" && master.IsActive,
                cancellationToken)
                ?? throw new ArgumentException("Select an active category.");
        }
        else
        {
            await EnsureNameIsUniqueAsync("Category", null, newCategoryName!, null, cancellationToken);
            category = new MasterData
            {
                MasterType = "Category",
                Name = newCategoryName!,
                IsActive = true
            };
            _db.MasterData.Add(category);
            await _db.SaveChangesAsync(cancellationToken);
        }

        await EnsureNameIsUniqueAsync("Item", category.Id, itemName, null, cancellationToken);
        var item = new MasterData
        {
            MasterType = "Item",
            Name = itemName,
            ParentId = category.Id,
            IsActive = true
        };
        _db.MasterData.Add(item);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var name in typeNames)
        {
            await EnsureNameIsUniqueAsync("Type", item.Id, name, null, cancellationToken);
            _db.MasterData.Add(new MasterData
            {
                MasterType = "Type",
                Name = name,
                ParentId = item.Id,
                IsActive = true
            });
        }

        foreach (var name in topologyNames)
        {
            await EnsureNameIsUniqueAsync("Topology", item.Id, name, null, cancellationToken);
            _db.MasterData.Add(new MasterData
            {
                MasterType = "Topology",
                Name = name,
                ParentId = item.Id,
                IsActive = true
            });
        }

        foreach (var name in glassNames)
        {
            var existingGlass = await _db.MasterData.FirstOrDefaultAsync(
                master => master.MasterType == "Glass" && master.ParentId == null &&
                          master.Name.ToLower() == name.ToLower(),
                cancellationToken);
            if (existingGlass?.IsActive == true)
                continue;
            if (existingGlass is not null)
                throw new MasterDataConflictException($"Glass option \"{name}\" is inactive and cannot be re-added.");

            _db.MasterData.Add(new MasterData
            {
                MasterType = "Glass",
                Name = name,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResponse(item);
    }

    public async Task<MasterDataResponse?> UpdateAsync(
        int id,
        UpdateMasterDataRequest request,
        CancellationToken cancellationToken)
    {
        var master = await _db.MasterData.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (master is null)
            return null;

        var name = NormalizeName(request.Name);
        await EnsureNameIsUniqueAsync(master.MasterType, master.ParentId, name, id, cancellationToken);
        master.Name = name;
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(master);
    }

    public async Task<MasterDataResponse?> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var master = await _db.MasterData.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (master is null)
            return null;

        if (master.IsActive && await _db.MasterData.AnyAsync(
                child => child.ParentId == id && child.IsActive, cancellationToken))
        {
            throw new MasterDataConflictException("Deactivate active child options before deactivating this option.");
        }

        master.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(master);
    }

    public async Task<RequirementMasterResolution> ResolveRequirementSelectionsAsync(
        IReadOnlyCollection<RequirementMasterSelectionIds> selections,
        CancellationToken cancellationToken)
    {
        var ids = selections
            .SelectMany(selection => new[]
            {
                selection.CategoryId, selection.ItemId, selection.TypeId, selection.TopologyId, selection.GlassId
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var masters = await _db.MasterData.AsNoTracking()
            .Where(master => ids.Contains(master.Id) && master.IsActive)
            .ToDictionaryAsync(master => master.Id, cancellationToken);

        var resolved = new List<RequirementMasterSelectionNames>(selections.Count);
        foreach (var selection in selections)
        {
            if (!selection.CategoryId.HasValue && !selection.ItemId.HasValue &&
                !selection.TypeId.HasValue && !selection.TopologyId.HasValue && !selection.GlassId.HasValue)
            {
                resolved.Add(new RequirementMasterSelectionNames("", "", "", "", ""));
                continue;
            }

            if (!selection.CategoryId.HasValue || !selection.ItemId.HasValue ||
                !TryGetMaster(masters, selection.CategoryId, "Category", null, out var category) ||
                !TryGetMaster(masters, selection.ItemId, "Item", selection.CategoryId, out var item))
            {
                return InvalidResolution(resolved, selections.Count, "Select a valid Category and Item.");
            }

            if ((selection.TypeId.HasValue &&
                 !TryGetMaster(masters, selection.TypeId, "Type", selection.ItemId, out _)) ||
                (selection.TopologyId.HasValue &&
                 !TryGetMaster(masters, selection.TopologyId, "Topology", selection.ItemId, out _)) ||
                (selection.GlassId.HasValue &&
                 !TryGetMaster(masters, selection.GlassId, "Glass", null, out _)))
            {
                return InvalidResolution(resolved, selections.Count, "One or more selected options are invalid for this item.");
            }

            resolved.Add(new RequirementMasterSelectionNames(
                category.Name,
                item.Name,
                selection.TypeId.HasValue ? masters[selection.TypeId.Value].Name : "",
                selection.TopologyId.HasValue ? masters[selection.TopologyId.Value].Name : "",
                selection.GlassId.HasValue ? masters[selection.GlassId.Value].Name : ""));
        }

        return new RequirementMasterResolution(true, null, resolved);
    }

    public static bool IsValidMasterType(string? masterType)
    {
        return !string.IsNullOrWhiteSpace(masterType) &&
               MasterTypes.Any(type => string.Equals(type, masterType.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string CanonicalizeMasterType(string masterType)
    {
        var canonicalType = MasterTypes.FirstOrDefault(type =>
            string.Equals(type, masterType.Trim(), StringComparison.OrdinalIgnoreCase));
        return canonicalType ?? throw new ArgumentException("Master type must be Category, Item, Type, Topology, or Glass.");
    }

    private async Task ValidateParentAsync(string masterType, int? parentId, CancellationToken cancellationToken)
    {
        var expectedParentType = masterType switch
        {
            "Item" => "Category",
            "Type" or "Topology" => "Item",
            _ => null
        };

        if (expectedParentType is null)
        {
            if (parentId.HasValue)
                throw new ArgumentException($"{masterType} options cannot have a parent.");
            return;
        }

        if (!parentId.HasValue)
            throw new ArgumentException($"{masterType} options require a parent {expectedParentType}.");

        var parent = await _db.MasterData.AsNoTracking()
            .FirstOrDefaultAsync(master => master.Id == parentId && master.IsActive, cancellationToken);
        if (parent is null || parent.MasterType != expectedParentType)
            throw new ArgumentException($"Select an active parent {expectedParentType}.");
    }

    private async Task EnsureNameIsUniqueAsync(
        string masterType,
        int? parentId,
        string name,
        int? exceptId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToLower();
        var duplicate = await _db.MasterData.AnyAsync(master =>
            master.MasterType == masterType &&
            master.ParentId == parentId &&
            master.Name.ToLower() == normalizedName &&
            (!exceptId.HasValue || master.Id != exceptId.Value), cancellationToken);
        if (duplicate)
            throw new MasterDataConflictException("An option with this name already exists under the selected parent.");
    }

    private static string NormalizeName(string name)
    {
        var normalized = name?.Trim() ?? "";
        if (normalized.Length == 0)
            throw new ArgumentException("Name is required.");
        if (normalized.Length > 150)
            throw new ArgumentException("Name cannot exceed 150 characters.");
        return normalized;
    }

    private static IReadOnlyList<string> NormalizeNames(IReadOnlyList<string> names)
    {
        var normalizedNames = new List<string>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var normalized = NormalizeName(name);
            if (seenNames.Add(normalized))
                normalizedNames.Add(normalized);
        }

        return normalizedNames;
    }

    private static bool TryGetMaster(
        IReadOnlyDictionary<int, MasterData> masters,
        int? id,
        string expectedType,
        int? expectedParentId,
        out MasterData master)
    {
        master = null!;
        return id.HasValue &&
               masters.TryGetValue(id.Value, out master!) &&
               master.MasterType == expectedType &&
               master.ParentId == expectedParentId;
    }

    private static RequirementMasterResolution InvalidResolution(
        IReadOnlyCollection<RequirementMasterSelectionNames> resolved,
        int totalCount,
        string error)
    {
        var items = resolved.Concat(Enumerable.Repeat(
            new RequirementMasterSelectionNames("", "", "", "", ""), totalCount - resolved.Count)).ToList();
        return new RequirementMasterResolution(false, error, items);
    }

    private static MasterDataResponse ToResponse(MasterData master)
    {
        return new MasterDataResponse(master.Id, master.MasterType, master.Name, master.ParentId, master.IsActive);
    }
}

public sealed class MasterDataConflictException : Exception
{
    public MasterDataConflictException(string message) : base(message) { }
}
