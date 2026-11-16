using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace CrmLeadManagement.Models.Dtos.App;

/// <summary>
/// Data transfer object for Estimation, Quotation, and Proposal records in App and Web APIs.
/// Includes Notes, attachment metadata, full accessible URLs, and download endpoints.
/// </summary>
public class AppFinanceItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = "";

    [JsonPropertyName("module")]
    public string Module { get; set; } = ""; // "Estimation", "Quotation", or "Proposal"

    [JsonPropertyName("leadId")]
    public string LeadId { get; set; } = "";

    [JsonPropertyName("leadName")]
    public string LeadName { get; set; } = "";

    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = "";

    [JsonPropertyName("leadCustomer")]
    public string LeadCustomer { get; set; } = "";

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("projectRequirement")]
    public string? ProjectRequirement { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Draft";

    [JsonPropertyName("subtotal")]
    public string? Subtotal { get; set; }

    [JsonPropertyName("discount")]
    public string? Discount { get; set; }

    [JsonPropertyName("taxRate")]
    public string? TaxRate { get; set; }

    [JsonPropertyName("tax")]
    public string? Tax { get; set; }

    [JsonPropertyName("taxName")]
    public string? TaxName { get; set; }

    [JsonPropertyName("taxType")]
    public string? TaxType { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }

    [JsonPropertyName("validUntil")]
    public string? ValidUntil { get; set; }

    [JsonPropertyName("termsConditions")]
    public string? TermsConditions { get; set; }

    [JsonPropertyName("attachmentPath")]
    public string? AttachmentPath { get; set; }

    [JsonPropertyName("attachmentFileName")]
    public string? AttachmentFileName { get; set; }

    [JsonPropertyName("attachmentUrl")]
    public string? AttachmentUrl { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("hasAttachment")]
    public bool HasAttachment { get; set; }

    [JsonPropertyName("attachment")]
    public AppFinanceAttachmentDto? Attachment { get; set; }
}

/// <summary>
/// Detailed attachment info for an uploaded PDF.
/// </summary>
public class AppFinanceAttachmentDto
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = "";

    [JsonPropertyName("relativePath")]
    public string RelativePath { get; set; } = "";

    [JsonPropertyName("fileUrl")]
    public string FileUrl { get; set; } = "";

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = "";

    [JsonPropertyName("hasAttachment")]
    public bool HasAttachment { get; set; }
}

/// <summary>
/// Form-data request model for saving an Estimation, Quotation, or Proposal.
/// Supports Notes, PDF file upload (as attachment, file, or pdf), and leadId.
/// </summary>
public class AppFinanceCreateForm
{
    /// <summary>Target Lead ID (e.g., LD-1001 or numeric ID). Required when creating a new record if not editing.</summary>
    public string? LeadId { get; set; }
    public string? Lead_Id { get; set; }
    public string? CustomerId { get; set; }

    [JsonIgnore]
    public string? ResolvedLeadId => !string.IsNullOrWhiteSpace(LeadId) ? LeadId : (!string.IsNullOrWhiteSpace(Lead_Id) ? Lead_Id : CustomerId);

    /// <summary>Notes / Description text for the record.</summary>
    public string? Notes { get; set; }
    public string? Description { get; set; }
    public string? Remarks { get; set; }

    [JsonIgnore]
    public string ResolvedNotes => !string.IsNullOrWhiteSpace(Notes) ? Notes : (!string.IsNullOrWhiteSpace(Description) ? Description : (Remarks ?? ""));

    /// <summary>Uploaded PDF file.</summary>
    public IFormFile? Attachment { get; set; }

    /// <summary>Alternative parameter names for uploaded PDF file.</summary>
    public IFormFile? File { get; set; }
    public IFormFile? Pdf { get; set; }
    public IFormFile? AttachPdf { get; set; }
    public IFormFile? Attach_Pdf { get; set; }
    public IFormFile? PdfFile { get; set; }
    public IFormFile? Pdf_File { get; set; }
    public IFormFile? Document { get; set; }
    public IFormFile? Upload { get; set; }

    /// <summary>Existing number (e.g. EST-2001) if editing an existing record.</summary>
    public string? Number { get; set; }

    /// <summary>Document date in yyyy-MM-dd format (defaults to current UTC date).</summary>
    public string? Date { get; set; }

    /// <summary>Proposal Title (used by Proposal module).</summary>
    public string? Title { get; set; }

    /// <summary>Project or Requirement description.</summary>
    public string? ProjectRequirement { get; set; }

    /// <summary>Status (e.g. Draft, Sent, Accepted, Declined).</summary>
    public string? Status { get; set; }

    /// <summary>Validity date (yyyy-MM-dd).</summary>
    public string? ValidUntil { get; set; }

    /// <summary>Commercial terms and conditions.</summary>
    public string? TermsConditions { get; set; }

    /// <summary>Subtotal amount.</summary>
    public string? Subtotal { get; set; }

    /// <summary>Discount amount.</summary>
    public string? Discount { get; set; }

    /// <summary>Tax amount.</summary>
    public string? Tax { get; set; }

    /// <summary>Final/Grand Total amount.</summary>
    public string? Total { get; set; }

    /// <summary>Resolves the uploaded file from Attachment, File, Pdf, AttachPdf, etc.</summary>
    [JsonIgnore]
    public IFormFile? ResolvedFile => Attachment ?? File ?? Pdf ?? AttachPdf ?? Attach_Pdf ?? PdfFile ?? Pdf_File ?? Document ?? Upload;
}

/// <summary>
/// JSON request model for saving an Estimation, Quotation, or Proposal via application/json.
/// </summary>
public class AppFinanceCreateJsonDto
{
    [JsonPropertyName("leadId")]
    public string? LeadId { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("projectRequirement")]
    public string? ProjectRequirement { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("validUntil")]
    public string? ValidUntil { get; set; }

    [JsonPropertyName("termsConditions")]
    public string? TermsConditions { get; set; }

    [JsonPropertyName("subtotal")]
    public string? Subtotal { get; set; }

    [JsonPropertyName("discount")]
    public string? Discount { get; set; }

    [JsonPropertyName("tax")]
    public string? Tax { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }
}

