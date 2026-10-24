using System.Security.Claims;
using CrmLeadManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmLeadManagement.Security;

public static class ProjectAccessPolicy
{
    public static async Task<HashSet<int>> GetAccessibleProjectIdsAsync(
        AppDbContext db,
        ClaimsPrincipal principal)
    {
        var projects = await db.WorkProjects
            .AsNoTracking()
            .Select(project => new WorkProject
            {
                Id = project.Id,
                ProjectManager = project.ProjectManager,
                AssignedMembers = project.AssignedMembers
            })
            .ToListAsync();

        return FilterAccessible(db, principal, projects)
            .Select(project => project.Id)
            .ToHashSet();
    }

    public static bool CanAccess(AppDbContext db, ClaimsPrincipal principal, WorkProject project)
    {
        var user = PermissionChecker.GetCurrentUser(db, principal);
        if (PermissionChecker.IsAdminOrManager(principal, user)) return true;
        if (user is null) return false;

        var identities = new[] { user.Name, user.Email, user.UserId }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim());

        return identities.Any(identity =>
            string.Equals(project.ProjectManager?.Trim(), identity, StringComparison.OrdinalIgnoreCase) ||
            (project.AssignedMembers ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(member => string.Equals(member, identity, StringComparison.OrdinalIgnoreCase)));
    }

    public static List<WorkProject> FilterAccessible(
        AppDbContext db,
        ClaimsPrincipal principal,
        IEnumerable<WorkProject> projects)
    {
        var user = PermissionChecker.GetCurrentUser(db, principal);
        if (PermissionChecker.IsAdminOrManager(principal, user)) return projects.ToList();
        if (user is null) return new List<WorkProject>();

        var identities = new[] { user.Name, user.Email, user.UserId }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return projects.Where(project =>
            (!string.IsNullOrWhiteSpace(project.ProjectManager) &&
             identities.Contains(project.ProjectManager.Trim())) ||
            (project.AssignedMembers ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(identities.Contains)).ToList();
    }
}
