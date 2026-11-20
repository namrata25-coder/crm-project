using CrmLeadManagement.Models;
using CrmLeadManagement.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmLeadManagement.Controllers.Api;

/// <summary>
/// JSON REST API for Clients / Customers lookup.
/// Base route: /api/clients
/// </summary>
[ApiController]
[Route("api/clients")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + BearerTokenAuthenticationHandler.SchemeName)]
public class ClientsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ClientsApiController> _logger;

    public ClientsApiController(AppDbContext db, ILogger<ClientsApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/clients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientLookupDto>>> GetAll([FromQuery] string? search)
    {
        try
        {
            var leads = await _db.Leads.AsNoTracking().ToListAsync();
            var projects = await _db.WorkProjects.AsNoTracking().ToListAsync();

            var clientMap = new Dictionary<string, ClientLookupDto>(StringComparer.OrdinalIgnoreCase);

            // Populate from Leads
            foreach (var lead in leads)
            {
                var name = !string.IsNullOrWhiteSpace(lead.LeadName) ? lead.LeadName : lead.CompanyName;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var key = $"lead:{(!string.IsNullOrWhiteSpace(lead.LeadId) ? lead.LeadId : lead.Id.ToString())}";
                if (!clientMap.ContainsKey(key))
                {
                    clientMap[key] = new ClientLookupDto
                    {
                        ClientId = !string.IsNullOrWhiteSpace(lead.LeadId) ? lead.LeadId : lead.Id.ToString(),
                        LeadId = lead.LeadId ?? "",
                        ClientName = lead.LeadName ?? "",
                        CompanyName = lead.CompanyName ?? "",
                        Email = lead.Email ?? "",
                        MobileNumber = lead.MobileNumber ?? "",
                        ProjectsCount = projects.Count(p => 
                            string.Equals(p.ClientName, lead.LeadName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(p.ClientCompany, lead.CompanyName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(p.ClientName, lead.CompanyName, StringComparison.OrdinalIgnoreCase))
                    };
                }
            }

            // Populate from Projects (if any client was created directly in Work module)
            foreach (var p in projects)
            {
                var name = !string.IsNullOrWhiteSpace(p.ClientName) ? p.ClientName : p.ClientCompany;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var hasLead = leads.Any(lead =>
                    (!string.IsNullOrWhiteSpace(p.ClientName) &&
                     (string.Equals(lead.LeadName, p.ClientName, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(lead.CompanyName, p.ClientName, StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrWhiteSpace(p.ClientCompany) &&
                     (string.Equals(lead.CompanyName, p.ClientCompany, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(lead.LeadName, p.ClientCompany, StringComparison.OrdinalIgnoreCase))));
                if (hasLead) continue;

                var key = $"project:{name.Trim()}|{p.ClientCompany?.Trim()}";
                if (!clientMap.ContainsKey(key))
                {
                    clientMap[key] = new ClientLookupDto
                    {
                        ClientId = !string.IsNullOrWhiteSpace(p.ProjectCode) ? p.ProjectCode : $"CLIENT-{p.Id}",
                        LeadId = "",
                        ClientName = p.ClientName ?? "",
                        CompanyName = p.ClientCompany ?? "",
                        Email = "",
                        MobileNumber = "",
                        ProjectsCount = projects.Count(x => 
                            string.Equals(x.ClientCompany, p.ClientCompany, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.ClientName, p.ClientName, StringComparison.OrdinalIgnoreCase))
                    };
                }
            }

            var list = clientMap.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                list = list.Where(c => 
                    c.ClientName.ToLower().Contains(term) ||
                    c.CompanyName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.MobileNumber.ToLower().Contains(term));
            }

            return Ok(list.OrderBy(c => c.ClientName).ThenBy(c => c.CompanyName).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/clients failed");
            return StatusCode(500, new { message = "Failed to retrieve clients", error = ex.Message });
        }
    }

    // GET /api/clients/{clientName}/projects
    [HttpGet("{clientName}/projects")]
    public async Task<ActionResult<IEnumerable<WorkProject>>> GetClientProjects(string clientName)
    {
        try
        {
            var decoded = Uri.UnescapeDataString(clientName).Trim();
            var projects = await _db.WorkProjects
                .Include(p => p.Milestones)
                .Include(p => p.Invoices)
                .Where(p => 
                    string.Equals(p.ClientName, decoded, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.ClientCompany, decoded, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/clients/{ClientName}/projects failed", clientName);
            return StatusCode(500, new { message = "Failed to retrieve client projects", error = ex.Message });
        }
    }
}

public class ClientLookupDto
{
    public string ClientId { get; set; } = "";
    public string LeadId { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Email { get; set; } = "";
    public string MobileNumber { get; set; } = "";
    public int ProjectsCount { get; set; }
}
