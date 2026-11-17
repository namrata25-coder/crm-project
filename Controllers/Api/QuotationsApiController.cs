using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrmLeadManagement.Models;
using CrmLeadManagement.Models.Dtos;
using CrmLeadManagement.Models.Dtos.App;
using CrmLeadManagement.Security;
using CrmLeadManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmLeadManagement.Controllers.Api;

/// <summary>
/// Web REST API for Quotations supporting Notes and PDF upload.
/// Base Route: /api/quotations
/// </summary>
[ApiController]
[Route("api/quotations")]
[Produces("application/json")]
[AllowAnonymous]
public class QuotationsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<QuotationsApiController> _logger;

    public QuotationsApiController(AppDbContext db, ILogger<QuotationsApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/quotations
    /// Retrieves all quotations or filters by leadId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? leadId)
    {
        var query = _db.Leads.AsQueryable();
        if (!string.IsNullOrWhiteSpace(leadId))
        {
            var trimmed = leadId.Trim();
            query = query.Where(l => l.LeadId == trimmed || l.Id.ToString() == trimmed);
        }

        var leads = await query.ToListAsync();
        var dtos = leads
            .SelectMany(l => l.Quotations.Select(q => FinanceDtoHelper.ToDto(q, l, Request)))
            .OrderByDescending(d => d.Id)
            .ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// GET /api/quotations/{id}
    /// Retrieves a single quotation by numeric Id, Number (e.g. QUO-2001), or LeadId.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ApiError { Message = "Quotation identifier cannot be empty." });

        var trimmed = id.Trim();
        var isNum = int.TryParse(trimmed, out var numericId);

        var leads = await _db.Leads.ToListAsync();
        foreach (var lead in leads)
        {
            var match = lead.Quotations.FirstOrDefault(q =>
                string.Equals(q.Number, trimmed, StringComparison.OrdinalIgnoreCase) ||
                (isNum && q.Id == numericId));

            if (match is not null)
            {
                return Ok(FinanceDtoHelper.ToDto(match, lead, Request));
            }
        }

        var leadMatch = leads.FirstOrDefault(l => string.Equals(l.LeadId, trimmed, StringComparison.OrdinalIgnoreCase));
        if (leadMatch is not null && leadMatch.Quotations.Count > 0)
        {
            var latest = leadMatch.Quotations.Last();
            return Ok(FinanceDtoHelper.ToDto(latest, leadMatch, Request));
        }

        return NotFound(new ApiError { Message = $"Quotation '{id}' was not found." });
    }

    /// <summary>
    /// POST /api/quotations (multipart/form-data or application/x-www-form-urlencoded)
    /// Saves Notes and optional PDF attachment for a Quotation.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public async Task<IActionResult> SaveQuotationForm([FromForm] AppFinanceCreateForm form)
    {
        IFormFile? file = form.ResolvedFile;
        if (file is null && Request.HasFormContentType && Request.Form.Files.Count > 0)
        {
            file = Request.Form.Files[0];
        }

        return await ProcessSave(form, file);
    }

    /// <summary>
    /// POST /api/quotations (application/json)
    /// Saves Notes and fields for a Quotation via JSON.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> SaveQuotationJson([FromBody] AppFinanceCreateJsonDto dto)
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

        return await ProcessSave(form, null);
    }

    private async Task<IActionResult> ProcessSave(AppFinanceCreateForm form, IFormFile? uploadedFile)
    {
        Lead? lead = null;
        QuotationEntry? existing = null;

        if (!string.IsNullOrWhiteSpace(form.Number))
        {
            var leads = await _db.Leads.ToListAsync();
            foreach (var l in leads)
            {
                var found = l.Quotations.FirstOrDefault(q => string.Equals(q.Number, form.Number.Trim(), StringComparison.OrdinalIgnoreCase));
                if (found is not null)
                {
                    lead = l;
                    existing = found;
                    break;
                }
            }
        }

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

        var (attPath, attName, attError) = await FinanceAttachmentService.SavePdfAsync(uploadedFile, "quotations");
        if (attError is not null)
        {
            return BadRequest(new ApiError { Message = attError });
        }

        var entry = existing ?? new QuotationEntry
        {
            Number = LeadStore.NextQuotationNumber()
        };

        if (!string.IsNullOrWhiteSpace(attPath))
        {
            FinanceAttachmentService.Delete(entry.AttachmentPath);
            entry.AttachmentPath = attPath;
            entry.AttachmentFileName = attName;
        }

        entry.Notes = form.ResolvedNotes;
        entry.Date = !string.IsNullOrWhiteSpace(form.Date) ? form.Date : (!string.IsNullOrWhiteSpace(entry.Date) ? entry.Date : DateTime.UtcNow.ToString("yyyy-MM-dd"));
        entry.LeadCustomer = $"{lead.LeadName} ({lead.CompanyName})";
        if (!string.IsNullOrWhiteSpace(form.ProjectRequirement)) entry.ProjectRequirement = form.ProjectRequirement;
        if (!string.IsNullOrWhiteSpace(form.Status)) entry.Status = form.Status;
        if (!string.IsNullOrWhiteSpace(form.ValidUntil)) entry.ValidUntil = form.ValidUntil;
        if (!string.IsNullOrWhiteSpace(form.TermsConditions)) entry.TermsConditions = form.TermsConditions;
        if (!string.IsNullOrWhiteSpace(form.Discount)) entry.Discount = form.Discount;
        if (!string.IsNullOrWhiteSpace(form.Tax)) entry.Tax = form.Tax;
        if (!string.IsNullOrWhiteSpace(form.Total)) entry.GrandTotal = form.Total;
        if (string.IsNullOrWhiteSpace(entry.Status)) entry.Status = "Draft";

        if (existing is null)
        {
            lead.Quotations.Add(entry);
        }

        await _db.SaveChangesAsync();

        var dto = FinanceDtoHelper.ToDto(entry, lead, Request);
        return Ok(dto);
    }

    /// <summary>
    /// GET /api/quotations/{id}/download
    /// Streams the attached PDF document for download.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadAttachment(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest(new ApiError { Message = "Invalid quotation id." });

        var trimmed = id.Trim();
        var isNum = int.TryParse(trimmed, out var numericId);

        var leads = await _db.Leads.ToListAsync();
        foreach (var lead in leads)
        {
            var match = lead.Quotations.FirstOrDefault(q =>
                string.Equals(q.Number, trimmed, StringComparison.OrdinalIgnoreCase) ||
                (isNum && q.Id == numericId));

            if (match is not null)
            {
                if (string.IsNullOrWhiteSpace(match.AttachmentPath))
                {
                    return NotFound(new ApiError { Message = "No attachment found for this quotation." });
                }

                var physicalPath = FinanceAttachmentService.GetPhysicalPath(match.AttachmentPath);
                if (physicalPath == null || !System.IO.File.Exists(physicalPath))
                {
                    return NotFound(new ApiError { Message = "Attachment file is missing or not accessible on server." });
                }

                var fileName = !string.IsNullOrWhiteSpace(match.AttachmentFileName) ? match.AttachmentFileName : $"{match.Number}.pdf";
                return PhysicalFile(physicalPath, "application/pdf", fileName);
            }
        }

        return NotFound(new ApiError { Message = $"Quotation '{id}' was not found." });
    }
}
