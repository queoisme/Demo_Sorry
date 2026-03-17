using AppSorry.Data;
using AppSorry.Models;
using Microsoft.EntityFrameworkCore;

namespace AppSorry.Services;

public class MediaService(AppDbContext context) : IMediaService
{
    private readonly AppDbContext _context = context;
    private readonly string _uploadPath = Path.Combine("wwwroot", "uploads", "images");
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 10 * 1024 * 1024;

    public async Task<MediaItem> UploadImageAsync(IFormFile file, string? caption, string? emojis)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty");
        
        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds {MaxFileSize / 1024 / 1024}MB limit");
        
        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed");

        Directory.CreateDirectory(_uploadPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var mediaItem = new MediaItem
        {
            FileName = fileName,
            FileUrl = $"/uploads/images/{fileName}",
            Caption = caption,
            Emojis = emojis,
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _context.MediaItems.Add(mediaItem);
        await _context.SaveChangesAsync();

        return mediaItem;
    }

    public async Task<IEnumerable<MediaItem>> GetAllMediaAsync()
    {
        return await _context.MediaItems
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<MediaItem?> GetMediaByIdAsync(int id)
    {
        return await _context.MediaItems.FindAsync(id);
    }

    public async Task<bool> DeleteMediaAsync(int id)
    {
        var mediaItem = await _context.MediaItems.FindAsync(id);
        if (mediaItem == null)
            return false;

        var filePath = Path.Combine(_uploadPath, mediaItem.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.MediaItems.Remove(mediaItem);
        await _context.SaveChangesAsync();
        return true;
    }
}
