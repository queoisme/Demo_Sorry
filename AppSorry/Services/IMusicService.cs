using AppSorry.DTOs;
using AppSorry.Models;

namespace AppSorry.Services;

public interface IMusicService
{
    Task<BackgroundMusic> UploadAudioAsync(IFormFile file, string title);
    Task<BackgroundMusic?> AddYouTubeMusicAsync(string youtubeUrl);
    Task<IEnumerable<BackgroundMusic>> GetAllMusicAsync();
    Task<BackgroundMusic?> GetMusicByIdAsync(int id);
    Task<bool> DeleteMusicAsync(int id);
}
