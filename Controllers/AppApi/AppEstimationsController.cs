using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrmLeadManagement.Models;
using CrmLeadManagement.Models.Dtos.App;
using CrmLeadManagement.Security;
using CrmLeadManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmLeadManagement.Controllers.AppApi;

/// <summary>
/// App API V1 endpoints for Estimations.
/// Routes: /api/app/v1/estimations and /api/v1/app/estimations
/// Supports Notes and PDF file attachment via multipart/form-data or application/json.
/// </summary>
[ApiController]
[Route("api/app/v1/estimations")]
[Route("api/v1/app/estimations")]
[Produces("application/json")]
[AllowAnonymous]
public class AppEstimationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppEstimationsController> _logger;

    public AppEstimationsController(AppDbContext db, ILogger<AppEstimationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/app/v1/estimations
    /// Retrieves all estimations or filters by leadId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? leadId)
    {
        try
        {
            var query = _db.Leads.AsQueryable();
            if (!string.IsNullOrWhiteSpace(leadId))
            {
                var trimmed = leadId.Trim();
                query = query.Where(l => l.LeadId == trimmed || l.Id.ToString() == trimmed);
            }

            var leads = await query.ToListAsync();
            var dtos = leads
                .SelectMany(l => l.Estimations.Select(e => FinanceDtoHelper.ToDto(e, l, Request)))
                .OrderByDescending(d => d.Id)
                .ToList();

            return Ok(AppApiResponse<List<AppFinanceItemDto>>.Ok(dtos, "Estimations retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve estimations.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AppApiErrorResponse.Fail("An unexpected error occurred while retrieving estimations."));
        }
    }

    /// <summary>
    /// GET /api/app/v1/estimations/{id}
    /// Retrieves a single estimation by numeric Id, Number (e.g. EST-2001), or LeadId.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(AppApiErrorResponse.Fail("Estimation identifier cannot be empty."));

        var trimmed = id.Trim();
        var isNum = int.TryParse(trimmed, out var numericId);

        var leads = await _db.Leads.ToListAsync();
        foreach (var lead in leads)
        {
            var match = lead.Estimations.FirstOrDefault(e =>
                string.Equals(e.Number, trimmed, StringComparison.OrdinalIgnoreCase) ||
                (isNum && e.Id == numericId));

            if (match is not null)
            {
                var dto = FinanceDtoHelper.ToDto(match, lead, Request);
                return Ok(AppApiResponse<AppFinanceItemDto>.Ok(dto, "Estimation retrieved successfully."));
            }
        }

        // If identifier matches a lead, return their latest estimation if any
        var leadMatch = leads.FirstOrDefault(l => string.Equals(l.LeadId, trimmed, StringComparison.OrdinalIgnoreCase));
        if (leadMatch is not null && leadMatch.Estimations.Count > 0)
        {
            var latest = leadMatch.Estimations.Last();
            var dto = FinanceDtoHelper.ToDto(latest, leadMatch, Request);
            return Ok(AppApiResponse<AppFinanceItemDto>.Ok(dto, "Estimation retrieved successfully."));
        }

        return NotFound(AppApiErrorResponse.Fail($"Estimation '{id}' was not found."));
    }

    /// <summary>
    /// POST /api/app/v1/estimations
    /// Saves Notes and optional PDF attachment for an Estimation (multipart/form-data or form-urlencoded).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public async Task<IActionResult> SaveEstimationForm([FromForm] AppFinanceCreateForm form)
    {
        IFormFile? file = form.ResolvedFile;
        if (file is null && Request.HasFormContentType && Request.Form.Files.Count > 0)
        {
            file = Request.Form.Files[0];
        }

        return await ProcessSaveEstimation(form, file);
    }

    /// <summary>
    /// POST /api/app/v1/estimations
    /// Saves Notes and fields for an Estimation (application/json).
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> SaveEstimationJson([FromBody] AppFinanceCreateJsonDto dto)
    {
        var form = new AppFinanceCreateForm
        {
            LeadId = dto.LeadId,
            Notes = dto.Notes ?? dto.Description ?? dto.Remarks,
            Number = dto.Number,
            Date = dto.Date,
            ProjectRequirement = dto.ProjectRequirement,
            Status = dto.Status,
            ValidUntil = dto.ValidUntil,
            TermsConditions = dto.TermsConditions,
            Subtotal = dto.Subtotal,
            Discount = dto.Discount,
            Tax = dto.Tax,
            Total = dto.Total
        };

        return await ProcessSaveEstimation(form, null);
    }

    /// <summary>
    /// Core persistence logic for Estimation with Notes and PDF attachment.
    /// </summary>
    private async Task<IActionResult> ProcessSaveEstimation(AppFinanceCreateForm form, IFormFile? uploadedFile)
    {
        try
        {
            Lead? lead = null;
            EstimationEntry? existing = null;

            // 1. If number is provided, search existing estimation across all leads
            if (!string.IsNullOrWhiteSpace(form.Number))
            {
                var leads = await _db.Leads.ToListAsync();
                foreach (var l in leads)
                {
                    var found = l.Estimations.FirstOrDefault(e => string.Equals(e.Number, form.Number.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (found is not null)
                    {
                        lead = l;
                        existing = found;
                        break;
                    }
                }
            }

            // 2. If not found by number, locate the lead by LeadId or fallback to default
            if (lead is null)
            {
                var targetLeadId = form.ResolvedLeadId?.Trim();
                if (!string.IsNullOrWhiteSpace(targetLeadId))
                {
                    lead = await _db.Leads.FirstOrDefaultAsync(l => l.LeadId == targetLeadId || l.Id.ToString() == targetLeadId);
                }

                if (lead is null)
                {
                    lead = await _db.Leads.OrderBy(l => l.Id).FirstOrDefaultAsync();
                }

                // If still no lead exists in the system, create a default client record
                if (lead is null)
                {
                    lead = new Lead
                    {
                        LeadId = "LD-1001",
                        LeadName = "General Customer",
                        CompanyName = "Direct Client",
                        CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        LeadStatus = "New"
                    };
                    _db.Leads.Add(lead);
                    await _db.SaveChangesAsync();
                }
            }

            // 3. Handle PDF attachment validation and storage
            var (attPath, attName, attError) = await FinanceAttachmentService.SavePdfAsync(uploadedFile, "estimations");
            if (attError is not null)
            {
                return BadRequest(AppApiErrorResponse.Fail(attError));
            }

            // 4. Create or update estimation
            var entry = existing ?? new EstimationEntry
            {
                Number = LeadStore.NextEstimationNumber()
            };

            if (!string.IsNullOrWhiteSpace(attPath))
            {
                // Delete old file if replacing
                FinanceAttachmentService.Delete(entry.AttachmentPath);
                entry.AttachmentPath = attPath;
                entry.AttachmentFileName = attName;
            }

            // Save Notes and metadata
            entry.Notes = form.ResolvedNotes;
            entry.Date = !string.IsNullOrWhiteSpace(form.Date) ? form.Date : (!string.IsNullOrWhiteSpace(entry.Date) ? entry.Date : DateTime.UtcNow.ToString("yyyy-MM-dd"));
            entry.LeadCustomer = $"{lead.LeadName} ({lead.CompanyName})";
            if (!string.IsNullOrWhiteSpace(form.ProjectRequirement)) entry.ProjectRequirement = form.ProjectRequirement;
            if (!string.IsNullOrWhiteSpace(form.Status)) entry.Status = form.Status;
            if (!string.IsNullOrWhiteSpace(form.ValidUntil)) entry.ValidUntil = form.ValidUntil;
            if (!string.IsNullOrWhiteSpace(form.TermsConditions)) entry.TermsConditions = form.TermsConditions;
            if (!string.IsNullOrWhiteSpace(form.Discount)) entry.Discount = form.Discount;
            if (!string.IsNullOrWhiteSpace(form.Tax)) entry.Tax = form.Tax;
            if (!string.IsNullOrWhiteSpace(form.Total)) entry.Total = form.Total;
            if (string.IsNullOrWhiteSpace(entry.Status)) entry.Status = "Draft";

            if (existing is null)
            {
                lead.Estimations.Add(entry);
            }

            await _db.SaveChangesAsync();

            var dto = FinanceDtoHelper.ToDto(entry, lead, Request);
            return Ok(AppApiResponse<AppFinanceItemDto>.Ok(dto, $"Estimation {entry.Number} saved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save estimation.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AppApiErrorResponse.Fail("An unexpected error occurred while saving the estimation."));
        }
    }

    /// <summary>
    /// GET /api/app/v1/estimations/{id}/download
    /// Streams the attached PDF document for download.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadAttachment(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Invalid estimation id.");

        var trimmed = id.Trim();
        var isNum = int.TryParse(trimmed, out var numericId);

        var leads = await _db.Leads.ToListAsync();
        foreach (var lead in leads)
        {
            var match = lead.Estimations.FirstOrDefault(e =>
                string.Equals(e.Number, trimmed, StringComparison.OrdinalIgnoreCase) ||
                (isNum && e.Id == numericId));

            if (match is not null)
            {
                if (string.IsNullOrWhiteSpace(match.AttachmentPath))
                {
                    return NotFound(AppApiErrorResponse.Fail("No attachment found for this estimation."));
                }

                var physicalPath = FinanceAttachmentService.GetPhysicalPath(match.AttachmentPath);
                if (physicalPath == null || !System.IO.File.Exists(physicalPath))
                {
                    return NotFound(AppApiErrorResponse.Fail("Attachment file is missing or not accessible on server."));
                }

                var fileName = !string.IsNullOrWhiteSpace(match.AttachmentFileName) ? match.AttachmentFileName : $"{match.Number}.pdf";
                return PhysicalFile(physicalPath, "application/pdf", fileName);
            }
        }

        return NotFound(AppApiErrorResponse.Fail($"Estimation '{id}' was not found."));
    }
}
