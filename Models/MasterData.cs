namespace CrmLeadManagement.Models;

public sealed class MasterData
{
    public int Id { get; set; }
    public string MasterType { get; set; } = "";
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public MasterData? Parent { get; set; }
    public List<MasterData> Children { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
