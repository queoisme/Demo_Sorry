using AppSorry.DTOs;
using AppSorry.Models;

namespace AppSorry.Services;

public interface IMediaService
{
    Task<MediaItem> UploadImageAsync(IFormFile file, string? caption, string? emojis);
    Task<IEnumerable<MediaItem>> GetAllMediaAsync();
    Task<MediaItem?> GetMediaByIdAsync(int id);
    Task<bool> DeleteMediaAsync(int id);
}
