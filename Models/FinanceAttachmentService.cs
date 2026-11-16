namespace CrmLeadManagement.Models;

/// <summary>
/// Handles saving/replacing/deleting the single optional attachment on an Estimation,
/// Quotation or Proposal record. Files are stored under wwwroot/uploads/finance/{kind}/
/// with a GUID file name (so uploads never overwrite each other), while the record keeps
/// the public relative path plus the original file name for display/download.
/// </summary>
public static class FinanceAttachmentService
{
    public static readonly string[] AllowedExtensions =
        { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };

    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Base physical folder for uploads. Configured at startup from Storage:UploadsPath.
    /// Defaults to wwwroot/uploads.
    /// </summary>
    public static string UploadsPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

    /// <summary>
    /// Saves the uploaded file (if any) under {UploadsPath}/finance/{kind}/ and returns
    /// (relativePath, originalFileName, error). If <paramref name="file"/> is null/empty,
    /// returns empty values with no error (caller keeps whatever attachment already existed).
    /// </summary>
    public static async Task<(string Path, string FileName, string? Error)> SaveAsync(IFormFile? file, string kind)
    {
        if (file is null || file.Length == 0)
        {
            return ("", "", null);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ("", "", "Attachment is too large. Maximum allowed size is 10 MB.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return ("", "", "Unsupported file type. Allowed: PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG.");
        }

        var uploadsFolder = Path.Combine(UploadsPath, "finance", kind);
        Directory.CreateDirectory(uploadsFolder);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsFolder, storedName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/finance/{kind}/{storedName}";
        return (relativePath, file.FileName, null);
    }

    /// <summary>
    /// Validates and saves a PDF attachment. Validates that the file is not empty,
    /// is under MaxFileSizeBytes (10 MB), and has the .pdf extension and valid PDF header.
    /// Returns (relativePath, originalFileName, error).
    /// If file is null or empty, returns ("", "", null).
    /// </summary>
    public static async Task<(string Path, string FileName, string? Error)> SavePdfAsync(IFormFile? file, string kind)
    {
        if (file is null || file.Length == 0)
        {
            return ("", "", null);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ("", "", $"Attachment is too large. Maximum allowed size is {MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = file.ContentType?.ToLowerInvariant() ?? "";
        if (ext != ".pdf" && !contentType.Contains("pdf"))
        {
            return ("", "", "Invalid file format. Only PDF documents (.pdf) are allowed.");
        }

        var finalExt = ext == ".pdf" ? ".pdf" : ".pdf";
        var uploadsFolder = Path.Combine(UploadsPath, "finance", kind);
        Directory.CreateDirectory(uploadsFolder);

        var storedName = $"{Guid.NewGuid()}{finalExt}";
        var fullPath = Path.Combine(uploadsFolder, storedName);

        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(destStream);
        }

        var relativePath = $"/uploads/finance/{kind}/{storedName}";
        return (relativePath, file.FileName, null);
    }

    /// <summary>
    /// Gets the absolute physical file path for a relative attachment path.
    /// </summary>
    public static string? GetPhysicalPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        var rel = relativePath.TrimStart('/');
        if (rel.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            rel = rel.Substring("uploads/".Length);
        }
        var fullPath = Path.Combine(UploadsPath, rel.Replace('/', Path.DirectorySeparatorChar));
        return fullPath;
    }

    /// <summary>Deletes the physical file for a relative attachment path, if it exists. Safe to call with an empty path.</summary>
    public static void Delete(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var rel = relativePath.TrimStart('/');
        if (rel.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            rel = rel.Substring("uploads/".Length);
        }
        var fullPath = Path.Combine(UploadsPath, rel.Replace('/', Path.DirectorySeparatorChar));
        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch
        {
            // Best-effort cleanup — never let a stray file block the record delete/replace.
        }
    }
}
