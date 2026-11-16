using CrmLeadManagement.Models;
using CrmLeadManagement.Models.Dtos.App;
using Microsoft.AspNetCore.Http;

namespace CrmLeadManagement.Services;

public static class FinanceDtoHelper
{
    public static string BuildAbsoluteUrl(HttpRequest request, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return "";
        var scheme = request.Scheme;
        var host = request.Host.Value;
        var cleanPath = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        return $"{scheme}://{host}{cleanPath}";
    }

    public static AppFinanceItemDto ToDto(EstimationEntry est, Lead lead, HttpRequest request)
    {
        var relPath = est.AttachmentPath ?? "";
        var hasAtt = !string.IsNullOrWhiteSpace(relPath);
        var fileUrl = hasAtt ? BuildAbsoluteUrl(request, relPath) : "";
        var dlUrl = hasAtt ? BuildAbsoluteUrl(request, $"/api/app/v1/estimations/{est.Number}/download") : "";

        return new AppFinanceItemDto
        {
            Id = est.Id,
            Number = est.Number ?? "",
            Module = "Estimation",
            LeadId = lead.LeadId,
            LeadName = lead.LeadName,
            CompanyName = lead.CompanyName ?? "",
            LeadCustomer = !string.IsNullOrWhiteSpace(est.LeadCustomer) ? est.LeadCustomer : $"{lead.LeadName} ({lead.CompanyName})",
            Date = est.Date ?? "",
            Notes = est.Notes ?? "",
            Description = est.Notes,
            ProjectRequirement = est.ProjectRequirement,
            Status = est.Status ?? "Draft",
            Subtotal = est.Subtotal,
            Discount = est.Discount,
            TaxRate = est.TaxRate,
            Tax = est.Tax,
            TaxName = est.TaxName,
            TaxType = est.TaxType,
            Total = est.Total,
            ValidUntil = est.ValidUntil,
            TermsConditions = est.TermsConditions,
            AttachmentPath = est.AttachmentPath,
            AttachmentFileName = est.AttachmentFileName,
            AttachmentUrl = fileUrl,
            DownloadUrl = dlUrl,
            HasAttachment = hasAtt,
            Attachment = hasAtt ? new AppFinanceAttachmentDto
            {
                FileName = est.AttachmentFileName ?? "estimation.pdf",
                RelativePath = relPath,
                FileUrl = fileUrl,
                DownloadUrl = dlUrl,
                HasAttachment = true
            } : null
        };
    }

    public static AppFinanceItemDto ToDto(QuotationEntry quo, Lead lead, HttpRequest request)
    {
        var relPath = quo.AttachmentPath ?? "";
        var hasAtt = !string.IsNullOrWhiteSpace(relPath);
        var fileUrl = hasAtt ? BuildAbsoluteUrl(request, relPath) : "";
        var dlUrl = hasAtt ? BuildAbsoluteUrl(request, $"/api/app/v1/quotations/{quo.Number}/download") : "";

        return new AppFinanceItemDto
        {
            Id = quo.Id,
            Number = quo.Number ?? "",
            Module = "Quotation",
            LeadId = lead.LeadId,
            LeadName = lead.LeadName,
            CompanyName = lead.CompanyName ?? "",
            LeadCustomer = !string.IsNullOrWhiteSpace(quo.LeadCustomer) ? quo.LeadCustomer : $"{lead.LeadName} ({lead.CompanyName})",
            Date = quo.Date ?? "",
            Notes = quo.Notes ?? "",
            Description = quo.Notes,
            ProjectRequirement = quo.ProjectRequirement,
            Status = quo.Status ?? "Draft",
            Subtotal = quo.Subtotal,
            Discount = quo.Discount,
            TaxRate = quo.TaxRate,
            Tax = quo.Tax,
            TaxName = quo.TaxName,
            TaxType = quo.TaxType,
            Total = quo.GrandTotal,
            ValidUntil = quo.ValidUntil,
            TermsConditions = quo.TermsConditions,
            AttachmentPath = quo.AttachmentPath,
            AttachmentFileName = quo.AttachmentFileName,
            AttachmentUrl = fileUrl,
            DownloadUrl = dlUrl,
            HasAttachment = hasAtt,
            Attachment = hasAtt ? new AppFinanceAttachmentDto
            {
                FileName = quo.AttachmentFileName ?? "quotation.pdf",
                RelativePath = relPath,
                FileUrl = fileUrl,
                DownloadUrl = dlUrl,
                HasAttachment = true
            } : null
        };
    }

    public static AppFinanceItemDto ToDto(ProposalEntry pro, Lead lead, HttpRequest request)
    {
        var relPath = pro.AttachmentPath ?? "";
        var hasAtt = !string.IsNullOrWhiteSpace(relPath);
        var fileUrl = hasAtt ? BuildAbsoluteUrl(request, relPath) : "";
        var dlUrl = hasAtt ? BuildAbsoluteUrl(request, $"/api/app/v1/proposals/{pro.Number}/download") : "";

        return new AppFinanceItemDto
        {
            Id = pro.Id,
            Number = pro.Number ?? "",
            Module = "Proposal",
            LeadId = lead.LeadId,
            LeadName = lead.LeadName,
            CompanyName = lead.CompanyName ?? "",
            LeadCustomer = !string.IsNullOrWhiteSpace(pro.LeadCustomer) ? pro.LeadCustomer : $"{lead.LeadName} ({lead.CompanyName})",
            Date = pro.Date ?? "",
            Title = pro.Title,
            Notes = pro.Notes ?? "",
            Description = !string.IsNullOrWhiteSpace(pro.Description) ? pro.Description : pro.Notes,
            ProjectRequirement = pro.ProjectRequirement,
            Status = pro.Status ?? "Draft",
            Subtotal = pro.Subtotal,
            Discount = pro.Discount,
            TaxRate = pro.TaxRate,
            Tax = pro.Tax,
            TaxName = pro.TaxName,
            TaxType = pro.TaxType,
            Total = pro.GrandTotal,
            TermsConditions = pro.TermsConditions,
            AttachmentPath = pro.AttachmentPath,
            AttachmentFileName = pro.AttachmentFileName,
            AttachmentUrl = fileUrl,
            DownloadUrl = dlUrl,
            HasAttachment = hasAtt,
            Attachment = hasAtt ? new AppFinanceAttachmentDto
            {
                FileName = pro.AttachmentFileName ?? "proposal.pdf",
                RelativePath = relPath,
                FileUrl = fileUrl,
                DownloadUrl = dlUrl,
                HasAttachment = true
            } : null
        };
    }
}

