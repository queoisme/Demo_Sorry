using AppSorry.DTOs;
using AppSorry.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppSorry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusicController(IMusicService musicService) : ControllerBase
{
    private readonly IMusicService _musicService = musicService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BackgroundMusicDto>>> GetAllMusic()
    {
        var musicList = await _musicService.GetAllMusicAsync();
        return Ok(musicList.Select(m => new BackgroundMusicDto
        {
            Id = m.Id,
            Title = m.Title,
            FileName = m.FileName,
            FileUrl = m.FileUrl,
            ContentType = m.ContentType,
            FileSize = m.FileSize,
            IsFromYoutube = m.IsFromYoutube,
            YoutubeUrl = m.YoutubeUrl,
            CreatedAt = m.CreatedAt
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BackgroundMusicDto>> GetMusic(int id)
    {
        var music = await _musicService.GetMusicByIdAsync(id);
        if (music == null)
            return NotFound();

        return Ok(new BackgroundMusicDto
        {
            Id = music.Id,
            Title = music.Title,
            FileName = music.FileName,
            FileUrl = music.FileUrl,
            ContentType = music.ContentType,
            FileSize = music.FileSize,
            IsFromYoutube = music.IsFromYoutube,
            YoutubeUrl = music.YoutubeUrl,
            CreatedAt = music.CreatedAt
        });
    }

    [HttpPost("upload-audio")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BackgroundMusicDto>> UploadAudio([FromForm] IFormFile file, [FromForm] string title)
    {
        try
        {
            var music = await _musicService.UploadAudioAsync(file, title);
            return CreatedAtAction(nameof(GetMusic), new { id = music.Id }, new BackgroundMusicDto
            {
                Id = music.Id,
                Title = music.Title,
                FileName = music.FileName,
                FileUrl = music.FileUrl,
                ContentType = music.ContentType,
                FileSize = music.FileSize,
                IsFromYoutube = music.IsFromYoutube,
                YoutubeUrl = music.YoutubeUrl,
                CreatedAt = music.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("youtube")]
    public async Task<ActionResult<BackgroundMusicDto>> AddYouTubeMusic([FromBody] YouTubeMusicDto dto)
    {
        try
        {
            var music = await _musicService.AddYouTubeMusicAsync(dto.YouTubeUrl);
            if (music == null)
                return BadRequest(new { message = "Failed to download YouTube music" });

            return CreatedAtAction(nameof(GetMusic), new { id = music.Id }, new BackgroundMusicDto
            {
                Id = music.Id,
                Title = music.Title,
                FileName = music.FileName,
                FileUrl = music.FileUrl,
                ContentType = music.ContentType,
                FileSize = music.FileSize,
                IsFromYoutube = music.IsFromYoutube,
                YoutubeUrl = music.YoutubeUrl,
                CreatedAt = music.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMusic(int id)
    {
        var result = await _musicService.DeleteMusicAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

public class BackgroundMusicDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsFromYoutube { get; set; }
    public string? YoutubeUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
