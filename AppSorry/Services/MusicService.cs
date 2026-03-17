using AppSorry.Data;
using AppSorry.DTOs;
using AppSorry.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace AppSorry.Services;

public class MusicService(
    AppDbContext context,
    IWebHostEnvironment environment) : IMusicService
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

        Directory.CreateDirectory(_uploadPath);

        var youtube = new YoutubeClient();
        var video = await youtube.Videos.GetAsync(youtubeUrl);
        
        var fileName = $"{Guid.NewGuid()}.mp3";
        var filePath = Path.Combine(_uploadPath, fileName);

        await youtube.Videos.DownloadAsync(
            youtubeUrl,
            new ConversionRequestBuilder(filePath).Build()
        );

        var fileInfo = new FileInfo(filePath);
        var music = new BackgroundMusic
        {
            Title = video.Title,
            FileName = fileName,
            FileUrl = $"/uploads/music/{fileName}",
            ContentType = "audio/mpeg",
            FileSize = fileInfo.Length,
            IsFromYoutube = true,
            YoutubeUrl = youtubeUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.BackgroundMusic.Add(music);
        await _context.SaveChangesAsync();

        return music;
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
