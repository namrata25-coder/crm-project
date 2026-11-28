using System.Security.Claims;
using CrmLeadManagement.Models;
using CrmLeadManagement.Security;
using CrmLeadManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmLeadManagement.Controllers;

[Authorize]
public class LeadController : Controller
{
    private readonly AppDbContext _db;

    public LeadController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Lead/List
    public IActionResult List()
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") || (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", "View"))
        {
            if (PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", "View"))
            {
                return RedirectToAction(nameof(Assigned));
            }
            TempData["ErrorMessage"] = "Access Denied: You do not have permission to view the Lead List.";
            return RedirectToAction("Projects", "Work");
        }

        ViewData["Title"] = "Lead List";
        ViewData["ActivePage"] = "LeadList";

        var roleMaster = currentUser != null ? _db.RoleMasters.FirstOrDefault(r => r.RoleName == currentUser.Role) : null;
        if (!isAdmin && roleMaster?.DataAccess == "OnlyAssigned" && currentUser != null)
        {
            var userLeads = LeadStore.Leads
                .Where(l => string.Equals(l.AssignedTo, currentUser.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(l.AssignedTo, currentUser.Email, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return View(userLeads);
        }

        return View(LeadStore.Leads);
    }

    // GET /Lead/Assigned
    public IActionResult Assigned()
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") || (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", "View"))
        {
            if (PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", "View"))
            {
                return RedirectToAction(nameof(List));
            }
            TempData["ErrorMessage"] = "Access Denied: You do not have permission to view Assigned Leads.";
            return RedirectToAction("Projects", "Work");
        }

        ViewData["Title"] = "Assigned Leads";
        ViewData["ActivePage"] = "AssignedList";

        var roleMaster = currentUser != null ? _db.RoleMasters.FirstOrDefault(r => r.RoleName == currentUser.Role) : null;
        if (!isAdmin && (roleMaster == null || roleMaster.DataAccess == "OnlyAssigned") && currentUser != null)
        {
            var userLeads = LeadStore.Leads
                .Where(l => string.Equals(l.AssignedTo, currentUser.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(l.AssignedTo, currentUser.Email, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return View(userLeads);
        }

        return View(LeadStore.Leads.Where(l => !string.IsNullOrWhiteSpace(l.AssignedTo)).ToList());
    }

    // GET /Lead/Details?id=LD-1001&tab=call
    // subtab is only relevant when tab=finance (quotation | proposal)
    public IActionResult Details(string id, string? tab, string? subtab)
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") || (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", "View") &&
            !PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", "View"))
        {
            TempData["ErrorMessage"] = "Access Denied: You do not have permission to view lead details.";
            return RedirectToAction(nameof(Assigned));
        }

        var lead = LeadStore.GetById(id);
        if (lead is null)
        {
            return RedirectToAction("List");
        }

        ViewData["Title"] = "Lead Details";
        ViewData["ActivePage"] = "LeadList";
        ViewData["ActiveTab"] = string.IsNullOrWhiteSpace(tab) ? "call" : tab;
        ViewData["ActiveFinanceSubTab"] = string.Equals(subtab, "proposal", StringComparison.OrdinalIgnoreCase)
            ? "proposal"
            : "quotation";
        return View(lead);
    }

    public IActionResult RequirementDocument(string id, int? reqId = null, string? category = null, bool print = false, string? mode = null)
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") ||
                       (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", "View") &&
            !PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", "View"))
        {
            return Forbid();
        }

        var lead = LeadStore.GetById(id);
        if (lead is null)
            return NotFound();

        ViewData["ReqId"] = reqId;
        ViewData["Category"] = category;
        ViewData["AutoPrint"] = print;
        ViewData["Mode"] = mode;
        ViewData["Title"] = !string.IsNullOrWhiteSpace(category)
            ? $"Requirement - Category {category} - {lead.LeadName}"
            : (reqId.HasValue && string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase))
                ? $"Requirement Form - {lead.LeadName}"
                : $"Requirement Document - {lead.LeadName}";
        return View(lead);
    }

    /// <summary>
    /// Handles both Add (empty LeadId) and Edit/Reassign (existing LeadId) from the
    /// shared lead modal. Shared by both the Lead List and Assigned List pages, which
    /// each pass their own URL back in returnUrl so the user lands back where they were.
    /// </summary>
    [HttpPost]
    public IActionResult Save(LeadInputModel Input, string? returnUrl)
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") || (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));
        var action = string.IsNullOrWhiteSpace(Input.LeadId) ? "Create" : "Edit";

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", action) &&
            !PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", action))
        {
            TempData["ErrorMessage"] = $"Access Denied: You do not have permission to {action.ToLower()} leads.";
            return RedirectToReturnUrl(returnUrl, "List");
        }

        if (string.IsNullOrWhiteSpace(Input.MobileNumber) || string.IsNullOrWhiteSpace(Input.Email))
        {
            TempData["ErrorMessage"] = "Mobile number and email are required.";
            return RedirectToReturnUrl(returnUrl, "List");
        }

        if (string.IsNullOrWhiteSpace(Input.LeadId))
        {
            LeadStore.AddLead(Input.ToLead());
        }
        else
        {
            LeadStore.UpdateLead(Input.ToLead());
        }

        return RedirectToReturnUrl(returnUrl, "List");
    }

    [HttpPost]
    public IActionResult Delete(string id, string? returnUrl)
    {
        var authEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var currentUser = _db.Users.FirstOrDefault(u => u.Email == authEmail);
        bool isAdmin = User.IsInRole("Admin") || (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (!isAdmin && !PermissionChecker.HasModulePermission(_db, currentUser, "Lead List", "Delete") &&
            !PermissionChecker.HasModulePermission(_db, currentUser, "Assigned Leads", "Delete"))
        {
            TempData["ErrorMessage"] = "Access Denied: You do not have permission to delete leads.";
            return RedirectToReturnUrl(returnUrl, "List");
        }

        LeadStore.DeleteLead(id);
        return RedirectToReturnUrl(returnUrl, "List");
    }

    /// <summary>Logs a new call against the lead and returns to the Call tab.</summary>
    [HttpPost]
    public IActionResult AddCall(string leadId, string callDate, string callTime, string? callType, string callStatus, string? duration, string? remarks, string? remark)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        var remarkText = !string.IsNullOrWhiteSpace(remark) ? remark : (remarks ?? "");

        if (!string.IsNullOrWhiteSpace(callDate) && !string.IsNullOrWhiteSpace(callTime) && !string.IsNullOrWhiteSpace(callStatus))
        {
            lead.CallHistory.Add(new CallHistoryEntry
            {
                CallDate = callDate,
                CallTime = callTime,
                CallType = string.IsNullOrWhiteSpace(callType) ? "Outgoing" : callType,
                CallStatus = callStatus,
                Duration = duration ?? "",
                Employee = lead.AssignedTo,
                Notes = remarkText
            });
            DbAccessor.Db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = leadId, tab = "call" });
    }

    /// <summary>Logs a new follow-up against the lead and returns to the Follow-up tab.</summary>
    [HttpPost]
    public IActionResult AddFollowUp(string leadId, string followUpDate, string followUpTime, string status, string? remarks, string? followUpType = "Call", string? nextFollowUpDate = null)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        if (!string.IsNullOrWhiteSpace(followUpDate) && !string.IsNullOrWhiteSpace(followUpTime) && !string.IsNullOrWhiteSpace(status))
        {
            lead.FollowUps.Add(new FollowUpEntry
            {
                FollowUpDate = followUpDate,
                FollowUpTime = followUpTime,
                FollowUpType = string.IsNullOrWhiteSpace(followUpType) ? "Call" : followUpType,
                AssignedEmployee = lead.AssignedTo,
                Status = status,
                Remarks = remarks ?? "",
                NextFollowUpDate = DateTime.TryParse(nextFollowUpDate, out var nextDate) ? nextDate : null
            });

            // Keep the lead's "next follow-up" quick-glance field in sync with the latest entry.
            lead.FollowUpDate = !string.IsNullOrWhiteSpace(nextFollowUpDate) ? nextFollowUpDate : followUpDate;
            DbAccessor.Db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = leadId, tab = "followup" });
    }

    /// <summary>Logs a new visit against the lead and returns to the Visit tab.</summary>
    [HttpPost]
    public IActionResult AddVisit(string leadId, string visitDate, string visitTime, string visitLocation, string? employee, string? outcome, string? conversation, string? remarks)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        if (!string.IsNullOrWhiteSpace(visitDate) && !string.IsNullOrWhiteSpace(visitTime) && !string.IsNullOrWhiteSpace(visitLocation))
        {
            lead.Visits.Add(new VisitEntry
            {
                VisitDate = visitDate,
                VisitTime = visitTime,
                VisitLocation = visitLocation,
                Employee = !string.IsNullOrWhiteSpace(employee) ? employee : (lead.AssignedTo ?? ""),
                Outcome = !string.IsNullOrWhiteSpace(outcome) ? outcome : "Interested",
                Conversation = conversation ?? "",
                Remarks = remarks ?? ""
            });
            DbAccessor.Db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = leadId, tab = "visit" });
    }

    /// <summary>Adds a new requirement (with optional image) against the lead and returns to the Requirement tab.</summary>
    [HttpPost]
    public async Task<IActionResult> AddRequirement(
        string leadId,
        string? title, string? description, string? priority, string? expectedDate, string? budget, string? status, IFormFile? image,
        List<string>? category, List<string>? items, List<string>? location,
        List<string>? h1, List<string>? h2, List<string>? h3,
        List<string>? w1, List<string>? w2, List<string>? w3,
        List<string>? types, List<string>? glass, List<string>? topology,
        List<string>? quantity, List<string>? areaCharge, List<string>? amount,
        string? requirementType = null, string? itemName = null, string? remarks = null,
        bool itemizedRequirement = false, int? reqId = null)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        var counts = new[] {
            category?.Count ?? 0,
            items?.Count ?? 0,
            location?.Count ?? 0,
            h1?.Count ?? 0,
            h2?.Count ?? 0,
            h3?.Count ?? 0,
            w1?.Count ?? 0,
            w2?.Count ?? 0,
            w3?.Count ?? 0,
            types?.Count ?? 0,
            glass?.Count ?? 0,
            topology?.Count ?? 0,
            quantity?.Count ?? 0,
            areaCharge?.Count ?? 0,
            amount?.Count ?? 0
        };
        var count = counts.Max();

        // 1. Single requirement form submission (standard)
        if (count == 0)
        {
            string imagePath = "";
            if (image is not null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (allowedExtensions.Contains(ext))
                {
                    var uploadsFolder = Path.Combine(FinanceAttachmentService.UploadsPath, "requirements");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = $"/uploads/requirements/{fileName}";
                }
            }

            var cleanType = requirementType?.Trim() ?? category?.FirstOrDefault()?.Trim() ?? "";
            var cleanTitle = !string.IsNullOrWhiteSpace(title) ? title.Trim() : (!string.IsNullOrWhiteSpace(itemName) ? itemName.Trim() : (!string.IsNullOrWhiteSpace(cleanType) ? cleanType : "Requirement"));
            var cleanItem = !string.IsNullOrWhiteSpace(itemName) ? itemName.Trim() : cleanTitle;
            var cleanQty = quantity?.FirstOrDefault()?.Trim() ?? "1";

            // If updating an existing requirement
            if (reqId.HasValue)
            {
                var existing = lead.Requirements.FirstOrDefault(r => r.Id == reqId.Value);
                if (existing is null)
                {
                    TempData["RequirementError"] = "The requirement you tried to edit could not be found.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }

                if (!string.IsNullOrWhiteSpace(cleanType)) existing.RequirementType = cleanType;
                if (!string.IsNullOrWhiteSpace(cleanType)) existing.Category = cleanType;
                existing.Title = cleanTitle;
                existing.ItemName = cleanItem;
                existing.Item = cleanItem;
                existing.Quantity = string.IsNullOrWhiteSpace(cleanQty) ? (existing.Quantity ?? "1") : cleanQty;
                existing.Priority = string.IsNullOrWhiteSpace(priority) ? (existing.Priority ?? "Medium") : priority.Trim();
                existing.Status = string.IsNullOrWhiteSpace(status) ? (existing.Status ?? "New") : status.Trim();
                existing.ExpectedDate = expectedDate?.Trim() ?? existing.ExpectedDate ?? "";
                existing.Budget = budget?.Trim() ?? existing.Budget ?? "";
                existing.Description = description?.Trim() ?? existing.Description ?? "";
                existing.Details = description?.Trim() ?? existing.Details ?? "";
                existing.Remarks = remarks?.Trim() ?? existing.Remarks ?? "";
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    existing.ImagePath = imagePath;
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
            }

            // New requirement
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(requirementType) && string.IsNullOrWhiteSpace(itemName))
            {
                TempData["RequirementError"] = "Please provide requirement type or title.";
                return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
            }

            lead.Requirements.Add(new RequirementEntry
            {
                Title = cleanTitle,
                ItemName = cleanItem,
                Item = cleanItem,
                RequirementType = cleanType,
                Category = cleanType,
                Quantity = string.IsNullOrWhiteSpace(cleanQty) ? "1" : cleanQty,
                Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority.Trim(),
                ExpectedDate = expectedDate?.Trim() ?? "",
                Budget = budget?.Trim() ?? "",
                Status = string.IsNullOrWhiteSpace(status) ? "New" : status.Trim(),
                ImagePath = imagePath,
                Description = description?.Trim() ?? "",
                Details = description?.Trim() ?? "",
                Remarks = remarks?.Trim() ?? ""
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
        }

        if (count > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            const long maxImageSizeBytes = 10 * 1024 * 1024;
            const long maxItemImageSizeBytes = 25 * 1024 * 1024;
            var cameraImages = new Dictionary<int, (string Extension, byte[] Bytes)>();
            var requirementToEdit = reqId.HasValue
                ? lead.Requirements.FirstOrDefault(requirement => requirement.Id == reqId.Value)
                : null;

            if (reqId.HasValue && requirementToEdit is null)
            {
                TempData["RequirementError"] = "The requirement you tried to edit could not be found.";
                return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
            }

            if (reqId.HasValue && count != 1)
            {
                TempData["RequirementError"] = "Edit one requirement item at a time.";
                return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
            }

            for (int i = 0; i < count; i++)
            {
                var rowCategory = category?.ElementAtOrDefault(i)?.Trim() ?? "";
                var rowItem = items?.ElementAtOrDefault(i)?.Trim() ?? "";
                var rowImages = GetRequirementItemImages(i);
                var imageData = Request.Form[$"requirementImageData_{i}"].FirstOrDefault()
                                ?? Request.Form[$"requirementImageData[{i}]"].FirstOrDefault()
                                ?? (Request.Form["requirementImageData"].Count > i ? Request.Form["requirementImageData"][i] : null);
                var hasAnyData = !string.IsNullOrWhiteSpace(rowCategory) ||
                                 !string.IsNullOrWhiteSpace(rowItem) ||
                                 !string.IsNullOrWhiteSpace(types?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(topology?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(glass?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(location?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(h1?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(h2?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(h3?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(w1?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(w2?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(w3?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(quantity?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(areaCharge?.ElementAtOrDefault(i)) ||
                                 !string.IsNullOrWhiteSpace(amount?.ElementAtOrDefault(i)) ||
                                 rowImages.Count > 0 || !string.IsNullOrWhiteSpace(imageData);
                if (!hasAnyData)
                    continue;

                if (string.IsNullOrWhiteSpace(rowCategory) || string.IsNullOrWhiteSpace(rowItem))
                {
                    TempData["RequirementError"] = "Enter a Category and Item for every non-empty item row.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }

                var cameraImageCount = string.IsNullOrWhiteSpace(imageData) ? 0 : 1;
                var existingImageCount = reqId.HasValue
                    ? GetRequirementImagePaths(requirementToEdit!).Count
                    : 0;
                if (rowImages.Count + cameraImageCount > 0 &&
                    existingImageCount + rowImages.Count + cameraImageCount > 10)
                {
                    TempData["RequirementError"] = "You can attach up to 10 photos to each item.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }

                if (rowImages.Any(file => !allowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase)))
                {
                    TempData["RequirementError"] = "Unsupported image type. Allowed formats are JPG, PNG, GIF and WEBP.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }

                if (rowImages.Any(file => file.Length > maxImageSizeBytes))
                {
                    TempData["RequirementError"] = "Each photo must be 10 MB or smaller.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }

                byte[] cameraImageBytes = Array.Empty<byte>();
                var cameraImageExtension = ".jpg";
                if (!string.IsNullOrWhiteSpace(imageData))
                {
                    if (!TryParseRequirementImage(imageData, out cameraImageExtension, out cameraImageBytes))
                    {
                        TempData["RequirementError"] = "The captured photo could not be read. Please capture it again.";
                        return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                    }

                    if (cameraImageBytes.Length > maxImageSizeBytes)
                    {
                        TempData["RequirementError"] = "Each photo must be 10 MB or smaller.";
                        return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                    }

                    cameraImages[i] = (cameraImageExtension, cameraImageBytes);
                }

                if (rowImages.Sum(file => file.Length) + cameraImageBytes.LongLength > maxItemImageSizeBytes)
                {
                    TempData["RequirementError"] = "The total photo size for each item must be 25 MB or smaller.";
                    return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
                }
            }

            var uploadsFolder = Path.Combine(FinanceAttachmentService.UploadsPath, "requirements");

            for (int i = 0; i < count; i++)
            {
                var cat = category?.ElementAtOrDefault(i)?.Trim() ?? "";
                var itm = items?.ElementAtOrDefault(i)?.Trim() ?? "";
                var loc = location?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _h1 = h1?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _h2 = h2?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _h3 = h3?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _w1 = w1?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _w2 = w2?.ElementAtOrDefault(i)?.Trim() ?? "";
                var _w3 = w3?.ElementAtOrDefault(i)?.Trim() ?? "";
                var qty = quantity?.ElementAtOrDefault(i)?.Trim() ?? "";
                var area = areaCharge?.ElementAtOrDefault(i)?.Trim() ?? "";
                var rowAmount = amount?.ElementAtOrDefault(i)?.Trim() ?? "";
                var typ = types?.ElementAtOrDefault(i)?.Trim() ?? "";
                var gls = glass?.ElementAtOrDefault(i)?.Trim() ?? "";
                var top = topology?.ElementAtOrDefault(i)?.Trim() ?? "";

                // Skip completely empty items
                if (string.IsNullOrWhiteSpace(cat) && string.IsNullOrWhiteSpace(itm) &&
                    string.IsNullOrWhiteSpace(loc) && string.IsNullOrWhiteSpace(_h1) &&
                    string.IsNullOrWhiteSpace(_h2) && string.IsNullOrWhiteSpace(_h3) &&
                    string.IsNullOrWhiteSpace(_w1) && string.IsNullOrWhiteSpace(_w2) &&
                    string.IsNullOrWhiteSpace(_w3) && string.IsNullOrWhiteSpace(typ) &&
                    string.IsNullOrWhiteSpace(gls) && string.IsNullOrWhiteSpace(top))
                {
                    continue;
                }

                var imagePaths = reqId.HasValue
                    ? GetRequirementImagePaths(requirementToEdit!)
                    : new List<string>();
                var rowImages = GetRequirementItemImages(i);

                foreach (var file in rowImages)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (allowedExtensions.Contains(ext))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imagePaths.Add($"/uploads/requirements/{fileName}");
                    }
                }

                if (cameraImages.TryGetValue(i, out var cameraImage))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}{cameraImage.Extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, cameraImage.Bytes);
                    imagePaths.Add($"/uploads/requirements/{fileName}");
                }

                // Build Item Title
                string itemTitle;
                if (!string.IsNullOrWhiteSpace(cat) && !string.IsNullOrWhiteSpace(itm))
                    itemTitle = $"{cat} — {itm}";
                else if (!string.IsNullOrWhiteSpace(itm))
                    itemTitle = itm;
                else if (!string.IsNullOrWhiteSpace(cat))
                    itemTitle = cat;
                else
                    itemTitle = $"Requirement Item #{i + 1}";

                // Build Description with location, dimensions, types, glass, topology
                var descParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(loc)) descParts.Add($"Location: {loc}");

                var hParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(_h1)) hParts.Add($"H1: {_h1}");
                if (!string.IsNullOrWhiteSpace(_h2)) hParts.Add($"H2: {_h2}");
                if (!string.IsNullOrWhiteSpace(_h3)) hParts.Add($"H3: {_h3}");
                if (hParts.Count > 0) descParts.Add($"Height: {string.Join(", ", hParts)}");

                var wParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(_w1)) wParts.Add($"W1: {_w1}");
                if (!string.IsNullOrWhiteSpace(_w2)) wParts.Add($"W2: {_w2}");
                if (!string.IsNullOrWhiteSpace(_w3)) wParts.Add($"W3: {_w3}");
                if (wParts.Count > 0) descParts.Add($"Width: {string.Join(", ", wParts)}");

                if (!string.IsNullOrWhiteSpace(typ)) descParts.Add($"Types: {typ}");
                if (!string.IsNullOrWhiteSpace(gls)) descParts.Add($"Glass: {gls}");
                if (!string.IsNullOrWhiteSpace(top)) descParts.Add($"Topology: {top}");

                var requirement = requirementToEdit ?? new RequirementEntry();
                requirement.Title = itemTitle;
                requirement.Description = string.Join(" | ", descParts);
                requirement.Priority = priority ?? "Medium";
                requirement.ExpectedDate = expectedDate ?? "";
                requirement.Budget = budget ?? "";
                requirement.Status = status ?? "New";
                requirement.ImagePath = imagePaths.FirstOrDefault() ?? "";
                requirement.ImagePathsJson = System.Text.Json.JsonSerializer.Serialize(imagePaths);
                requirement.ItemName = itm;
                requirement.RequirementType = cat;
                requirement.Category = cat;
                requirement.Item = itm;
                requirement.Location = loc;
                requirement.H1 = _h1;
                requirement.H2 = _h2;
                requirement.H3 = _h3;
                requirement.W1 = _w1;
                requirement.W2 = _w2;
                requirement.W3 = _w3;
                requirement.Quantity = string.IsNullOrWhiteSpace(qty) ? "1" : qty;
                requirement.AreaCharge = area;
                requirement.Amount = rowAmount;
                requirement.Types = typ;
                requirement.Topology = top;
                requirement.Glass = gls;
                requirement.Details = title ?? "";
                requirement.Remarks = description ?? "";

                if (requirementToEdit is null)
                    lead.Requirements.Add(requirement);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
        }

        // Backward compatibility for legacy single-form posts
        if (!string.IsNullOrWhiteSpace(title))
        {
            string imagePath = "";
            if (image is not null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (allowedExtensions.Contains(ext))
                {
                    var uploadsFolder = Path.Combine(FinanceAttachmentService.UploadsPath, "requirements");
                    Directory.CreateDirectory(uploadsFolder);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagePath = $"/uploads/requirements/{fileName}";
                }
            }

            lead.Requirements.Add(new RequirementEntry
            {
                Title = title,
                Description = description ?? "",
                Priority = priority ?? "Medium",
                ExpectedDate = expectedDate ?? "",
                Budget = budget ?? "",
                Status = status ?? "New",
                ImagePath = imagePath,
                Quantity = quantity?.FirstOrDefault()?.Trim() ?? "1",
                AreaCharge = areaCharge?.FirstOrDefault()?.Trim() ?? "",
                Amount = amount?.FirstOrDefault()?.Trim() ?? ""
            });
        }

        return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
    }

    private List<IFormFile> GetRequirementItemImages(int index)
    {
        var currentName = $"requirementImage_{index}";
        var legacyName = $"requirementImage[{index}]";
        return Request.Form.Files
            .Where(file => string.Equals(file.Name, currentName, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(file.Name, legacyName, StringComparison.OrdinalIgnoreCase))
            .Where(file => file.Length > 0)
            .ToList();
    }

    private static List<string> GetRequirementImagePaths(RequirementEntry requirement)
    {
        if (!string.IsNullOrWhiteSpace(requirement.ImagePathsJson))
        {
            try
            {
                var imagePaths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(requirement.ImagePathsJson);
                if (imagePaths is { Count: > 0 })
                    return imagePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();
            }
            catch (System.Text.Json.JsonException)
            {
            }
        }

        return string.IsNullOrWhiteSpace(requirement.ImagePath)
            ? new List<string>()
            : new List<string> { requirement.ImagePath };
    }

    private static bool TryParseRequirementImage(string data, out string extension, out byte[] bytes)
    {
        extension = ".jpg";
        var rawData = data;
        var commaIndex = rawData.IndexOf(',');
        if (commaIndex >= 0)
        {
            var header = rawData[..commaIndex];
            rawData = rawData[(commaIndex + 1)..];
            if (header.Contains("image/png", StringComparison.OrdinalIgnoreCase))
                extension = ".png";
            else if (header.Contains("image/webp", StringComparison.OrdinalIgnoreCase))
                extension = ".webp";
            else if (header.Contains("image/gif", StringComparison.OrdinalIgnoreCase))
                extension = ".gif";
        }

        try
        {
            bytes = Convert.FromBase64String(rawData);
            return bytes.Length > 0;
        }
        catch (FormatException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }

    /// <summary>Deletes a requirement entry from a lead.</summary>
    [HttpPost]
    public async Task<IActionResult> DeleteRequirement(string leadId, int reqId)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is not null)
        {
            var req = lead.Requirements.FirstOrDefault(r => r.Id == reqId);
            if (req is not null)
            {
                lead.Requirements.Remove(req);
                await _db.SaveChangesAsync();
            }
        }
        return RedirectToAction("Details", new { id = leadId, tab = "requirement" });
    }

    /// <summary>Assigns or re-assigns the lead to an employee and logs the change.</summary>
    [HttpPost]
    public IActionResult AssignLead(string leadId, string assignedEmployee, string assignmentDate, string? remarks)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        if (!string.IsNullOrWhiteSpace(assignedEmployee) && !string.IsNullOrWhiteSpace(assignmentDate))
        {
            lead.AssignmentHistory.Add(new AssignmentEntry
            {
                AssignedEmployee = assignedEmployee,
                AssignmentDate = assignmentDate,
                Remarks = remarks ?? ""
            });

            lead.AssignedTo = assignedEmployee;
            lead.AssignedDate = assignmentDate;
        }

        return RedirectToAction("Details", new { id = leadId, tab = "assign" });
    }

    // ===================== FINANCE: ESTIMATION =====================

    /// <summary>Creates a new Estimation, or updates an existing one when 'number' is supplied. Always linked to leadId.</summary>
    [HttpPost]
    public async Task<IActionResult> SaveEstimation(string leadId, string? number, string? date, string? projectRequirement,
        List<string>? itemName, List<string>? itemQty, List<string>? itemRate,
        string? discount, string? tax, string? validUntil, string? terms, string? notes, string? status, IFormFile? attachment,
        string? taxRate, string? taxName, string? taxType)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        var items = BuildLineItems(itemName, itemQty, itemRate);
        var subtotal = items.Sum(i => ParseDec(i.Amount));
        var discountVal = ParseDec(discount);
        var taxable = Math.Max(0, subtotal - discountVal);
        var rateVal = ParseDec(taxRate);
        var taxVal = rateVal > 0 ? (taxable * (rateVal / 100m)) : ParseDec(tax);
        var total = taxable + taxVal;

        var existing = string.IsNullOrWhiteSpace(number) ? null : lead.Estimations.FirstOrDefault(e => e.Number == number);
        var entry = existing ?? new EstimationEntry { Number = LeadStore.NextEstimationNumber() };

        var (attPath, attName, attError) = await FinanceAttachmentService.SaveAsync(attachment, "estimations");
        if (attError is not null)
        {
            TempData["EstimationError"] = attError;
            return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "estimation" });
        }
        if (!string.IsNullOrWhiteSpace(attPath))
        {
            // Replacing an existing attachment — remove the old file so uploads don't pile up.
            FinanceAttachmentService.Delete(entry.AttachmentPath);
            entry.AttachmentPath = attPath;
            entry.AttachmentFileName = attName;
        }

        entry.Date = !string.IsNullOrWhiteSpace(date) ? date : (!string.IsNullOrWhiteSpace(entry.Date) ? entry.Date : DateTime.UtcNow.ToString("yyyy-MM-dd"));
        entry.LeadCustomer = $"{lead.LeadName} ({lead.CompanyName})";
        if (projectRequirement != null) entry.ProjectRequirement = projectRequirement;
        if (items.Count > 0 || existing is null) entry.Items = items;
        if (subtotal > 0 || existing is null) entry.Subtotal = subtotal.ToString("0.##");
        if (discount != null) entry.Discount = discount;
        entry.Tax = taxVal.ToString("0.##");
        if (!string.IsNullOrWhiteSpace(taxRate)) entry.TaxRate = taxRate;
        if (!string.IsNullOrWhiteSpace(taxName)) entry.TaxName = taxName;
        if (!string.IsNullOrWhiteSpace(taxType)) entry.TaxType = taxType;
        if (total > 0 || existing is null) entry.Total = total.ToString("0.##");
        if (validUntil != null) entry.ValidUntil = validUntil;
        if (terms != null) entry.TermsConditions = terms;
        entry.Notes = notes ?? "";
        entry.Status = !string.IsNullOrWhiteSpace(status) ? status : (!string.IsNullOrWhiteSpace(entry.Status) ? entry.Status : "Draft");

        if (existing is null) lead.Estimations.Add(entry);

        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "estimation" });
    }

    [HttpPost]
    public IActionResult DeleteEstimation(string leadId, string number)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is not null)
        {
            var existing = lead.Estimations.FirstOrDefault(e => e.Number == number);
            if (existing is not null)
            {
                FinanceAttachmentService.Delete(existing.AttachmentPath);
                lead.Estimations.Remove(existing);
            }
        }
        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "estimation" });
    }

    // ===================== FINANCE: QUOTATION =====================

    /// <summary>Creates a new Quotation, or updates an existing one when 'number' is supplied. Always linked to leadId.</summary>
    [HttpPost]
    public async Task<IActionResult> SaveQuotation(string leadId, string? number, string? date, string? projectRequirement,
        List<string>? itemName, List<string>? itemQty, List<string>? itemRate,
        string? discount, string? tax, string? validUntil, string? terms, string? notes, string? status, IFormFile? attachment,
        string? taxRate, string? taxName, string? taxType)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        var items = BuildLineItems(itemName, itemQty, itemRate);
        var subtotal = items.Sum(i => ParseDec(i.Amount));
        var discountVal = ParseDec(discount);
        var taxable = Math.Max(0, subtotal - discountVal);
        var rateVal = ParseDec(taxRate);
        var taxVal = rateVal > 0 ? (taxable * (rateVal / 100m)) : ParseDec(tax);
        var grandTotal = taxable + taxVal;

        var existing = string.IsNullOrWhiteSpace(number) ? null : lead.Quotations.FirstOrDefault(q => q.Number == number);
        var entry = existing ?? new QuotationEntry { Number = LeadStore.NextQuotationNumber() };

        var (attPath, attName, attError) = await FinanceAttachmentService.SaveAsync(attachment, "quotations");
        if (attError is not null)
        {
            TempData["QuotationError"] = attError;
            return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "quotation" });
        }
        if (!string.IsNullOrWhiteSpace(attPath))
        {
            FinanceAttachmentService.Delete(entry.AttachmentPath);
            entry.AttachmentPath = attPath;
            entry.AttachmentFileName = attName;
        }

        entry.Date = !string.IsNullOrWhiteSpace(date) ? date : (!string.IsNullOrWhiteSpace(entry.Date) ? entry.Date : DateTime.UtcNow.ToString("yyyy-MM-dd"));
        entry.LeadCustomer = $"{lead.LeadName} ({lead.CompanyName})";
        if (projectRequirement != null) entry.ProjectRequirement = projectRequirement;
        if (items.Count > 0 || existing is null) entry.Items = items;
        if (subtotal > 0 || existing is null) entry.Subtotal = subtotal.ToString("0.##");
        if (discount != null) entry.Discount = discount;
        entry.Tax = taxVal.ToString("0.##");
        if (!string.IsNullOrWhiteSpace(taxRate)) entry.TaxRate = taxRate;
        if (!string.IsNullOrWhiteSpace(taxName)) entry.TaxName = taxName;
        if (!string.IsNullOrWhiteSpace(taxType)) entry.TaxType = taxType;
        if (grandTotal > 0 || existing is null) entry.GrandTotal = grandTotal.ToString("0.##");
        if (validUntil != null) entry.ValidUntil = validUntil;
        if (terms != null) entry.TermsConditions = terms;
        entry.Notes = notes ?? "";
        entry.Status = !string.IsNullOrWhiteSpace(status) ? status : (!string.IsNullOrWhiteSpace(entry.Status) ? entry.Status : "Draft");

        if (existing is null) lead.Quotations.Add(entry);

        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "quotation" });
    }

    [HttpPost]
    public IActionResult DeleteQuotation(string leadId, string number)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is not null)
        {
            var existing = lead.Quotations.FirstOrDefault(q => q.Number == number);
            if (existing is not null)
            {
                FinanceAttachmentService.Delete(existing.AttachmentPath);
                lead.Quotations.Remove(existing);
            }
        }
        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "quotation" });
    }

    // ===================== FINANCE: PROPOSAL =====================

    /// <summary>Creates a new Proposal, or updates an existing one when 'number' is supplied. Always linked to leadId.</summary>
    [HttpPost]
    public async Task<IActionResult> SaveProposal(string leadId, string? number, string? date, string? projectRequirement,
        string? title, string? scopeOfWork, string? description, string? deliverables, string? timeline,
        string? budget, string? terms, string? notes, string? status, IFormFile? attachment,
        string? taxRate, string? taxName, string? taxType)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is null) return RedirectToAction("List");

        var existing = string.IsNullOrWhiteSpace(number) ? null : lead.Proposals.FirstOrDefault(p => p.Number == number);
        var entry = existing ?? new ProposalEntry { Number = LeadStore.NextProposalNumber() };

        var (attPath, attName, attError) = await FinanceAttachmentService.SaveAsync(attachment, "proposals");
        if (attError is not null)
        {
            TempData["ProposalError"] = attError;
            return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "proposal" });
        }
        if (!string.IsNullOrWhiteSpace(attPath))
        {
            FinanceAttachmentService.Delete(entry.AttachmentPath);
            entry.AttachmentPath = attPath;
            entry.AttachmentFileName = attName;
        }

        entry.Date = !string.IsNullOrWhiteSpace(date) ? date : (!string.IsNullOrWhiteSpace(entry.Date) ? entry.Date : DateTime.UtcNow.ToString("yyyy-MM-dd"));
        entry.LeadCustomer = $"{lead.LeadName} ({lead.CompanyName})";
        if (projectRequirement != null) entry.ProjectRequirement = projectRequirement;
        entry.Title = !string.IsNullOrWhiteSpace(title) ? title : (!string.IsNullOrWhiteSpace(entry.Title) ? entry.Title : "Proposal");
        if (scopeOfWork != null) entry.ScopeOfWork = scopeOfWork;
        entry.Description = description ?? (notes ?? "");
        if (deliverables != null) entry.Deliverables = deliverables;
        if (timeline != null) entry.Timeline = timeline;
        if (budget != null) entry.Budget = budget;
        if (!string.IsNullOrWhiteSpace(taxRate)) entry.TaxRate = taxRate;
        if (!string.IsNullOrWhiteSpace(taxName)) entry.TaxName = taxName;
        if (!string.IsNullOrWhiteSpace(taxType)) entry.TaxType = taxType;
        if (terms != null) entry.TermsConditions = terms;
        entry.Notes = notes ?? (description ?? "");
        entry.Status = !string.IsNullOrWhiteSpace(status) ? status : (!string.IsNullOrWhiteSpace(entry.Status) ? entry.Status : "Draft");

        if (existing is null) lead.Proposals.Add(entry);

        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "proposal" });
    }

    [HttpPost]
    public IActionResult DeleteProposal(string leadId, string number)
    {
        var lead = LeadStore.GetById(leadId);
        if (lead is not null)
        {
            var existing = lead.Proposals.FirstOrDefault(p => p.Number == number);
            if (existing is not null)
            {
                FinanceAttachmentService.Delete(existing.AttachmentPath);
                lead.Proposals.Remove(existing);
            }
        }
        return RedirectToAction("Details", new { id = leadId, tab = "finance", subtab = "proposal" });
    }

    private static decimal ParseDec(string? s) => decimal.TryParse(s, out var v) ? v : 0;

    /// <summary>Builds the Items/Services line-item list from the parallel form arrays posted by the Finance forms.</summary>
    private static List<FinanceLineItem> BuildLineItems(List<string>? names, List<string>? qtys, List<string>? rates)
    {
        var items = new List<FinanceLineItem>();
        if (names is null) return items;

        for (int i = 0; i < names.Count; i++)
        {
            var name = names[i];
            if (string.IsNullOrWhiteSpace(name)) continue;

            var qty = (qtys is not null && i < qtys.Count) ? qtys[i] : "";
            var rate = (rates is not null && i < rates.Count) ? rates[i] : "";
            var amount = ParseDec(qty) * ParseDec(rate);

            items.Add(new FinanceLineItem { ItemName = name, Quantity = qty, Rate = rate, Amount = amount.ToString("0.##") });
        }

        return items;
    }

    private IActionResult RedirectToReturnUrl(string? returnUrl, string fallbackAction)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(fallbackAction);
    }
}
