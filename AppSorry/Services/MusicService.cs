using AppSorry.Data;
using AppSorry.Models;
using Microsoft.EntityFrameworkCore;

namespace AppSorry.Services;

public class MusicService(AppDbContext context) : IMusicService
{
    private readonly AppDbContext _context = context;
    private readonly string _uploadPath = Path.Combine("wwwroot", "uploads", "music");
    private static readonly string[] AllowedAudioTypes = { "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp3" };
    private const long MaxFileSize = 50 * 1024 * 1024;

    public async Task<BackgroundMusic> UploadAudioAsync(IFormFile file, string title)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty");
        
        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds {MaxFileSize / 1024 / 1024}MB limit");
        
        if (!AllowedAudioTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Invalid file type. Only MP3, WAV, and OGG are allowed");

        Directory.CreateDirectory(_uploadPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var music = new BackgroundMusic
        {
            Title = title,
            FileName = fileName,
            FileUrl = $"/uploads/music/{fileName}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            IsFromYoutube = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.BackgroundMusic.Add(music);
        await _context.SaveChangesAsync();

        return music;
    }

    public async Task<BackgroundMusic?> AddYouTubeMusicAsync(string youtubeUrl)
    {
        if (string.IsNullOrWhiteSpace(youtubeUrl))
            throw new ArgumentException("YouTube URL is required");

        if (!IsValidYouTubeUrl(youtubeUrl))
            throw new ArgumentException("Invalid YouTube URL format");

        var title = await GetYouTubeTitleAsync(youtubeUrl);

        var music = new BackgroundMusic
        {
            Title = title,
            FileName = string.Empty,
            FileUrl = youtubeUrl,
            ContentType = "audio/youtube",
            FileSize = 0,
            IsFromYoutube = true,
            YoutubeUrl = youtubeUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.BackgroundMusic.Add(music);
        await _context.SaveChangesAsync();

        return music;
    }

    private static async Task<string> GetYouTubeTitleAsync(string youtubeUrl)
    {
        try
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            var oembedUrl = $"https://www.youtube.com/oembed?url={Uri.EscapeDataString(youtubeUrl)}&format=json";
            var response = await http.GetFromJsonAsync<System.Text.Json.JsonElement>(oembedUrl);
            return response.GetProperty("title").GetString() ?? "YouTube Music";
        }
        catch
        {
            return "YouTube Music";
        }
    }

    public async Task<IEnumerable<BackgroundMusic>> GetAllMusicAsync()
    {
        return await _context.BackgroundMusic
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<BackgroundMusic?> GetMusicByIdAsync(int id)
    {
        return await _context.BackgroundMusic.FindAsync(id);
    }

    public async Task<bool> DeleteMusicAsync(int id)
    {
        var music = await _context.BackgroundMusic.FindAsync(id);
        if (music == null)
            return false;

        var filePath = Path.Combine(_uploadPath, music.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.BackgroundMusic.Remove(music);
        await _context.SaveChangesAsync();
        return true;
    }

    private static bool IsValidYouTubeUrl(string url)
    {
        return url.Contains("youtube.com/watch") || 
               url.Contains("youtu.be/") || 
               url.Contains("youtube.com/shorts/");
    }
}
