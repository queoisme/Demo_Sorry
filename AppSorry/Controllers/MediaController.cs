using AppSorry.DTOs;
using AppSorry.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppSorry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    private readonly IMediaService _mediaService = mediaService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MediaItemDto>>> GetAllMedia()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var mediaItems = await _mediaService.GetAllMediaAsync();
        return Ok(mediaItems.Select(m => new MediaItemDto
        {
            Id = m.Id,
            FileName = m.FileName,
            FileUrl = $"{baseUrl}{m.FileUrl}",
            Caption = m.Caption,
            Emojis = m.Emojis,
            ContentType = m.ContentType,
            FileSize = m.FileSize,
            CreatedAt = m.CreatedAt
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MediaItemDto>> GetMedia(int id)
    {
        var mediaItem = await _mediaService.GetMediaByIdAsync(id);
        if (mediaItem == null)
            return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new MediaItemDto
        {
            Id = mediaItem.Id,
            FileName = mediaItem.FileName,
            FileUrl = $"{baseUrl}{mediaItem.FileUrl}",
            Caption = mediaItem.Caption,
            Emojis = mediaItem.Emojis,
            ContentType = mediaItem.ContentType,
            FileSize = mediaItem.FileSize,
            CreatedAt = mediaItem.CreatedAt
        });
    }

    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MediaItemDto>> UploadImage([FromForm] UploadImageRequest request)
    {
        try
        {
            var mediaItem = await _mediaService.UploadImageAsync(request.File, request.Caption, request.Emojis);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return CreatedAtAction(nameof(GetMedia), new { id = mediaItem.Id }, new MediaItemDto
            {
                Id = mediaItem.Id,
                FileName = mediaItem.FileName,
                FileUrl = $"{baseUrl}{mediaItem.FileUrl}",
                Caption = mediaItem.Caption,
                Emojis = mediaItem.Emojis,
                ContentType = mediaItem.ContentType,
                FileSize = mediaItem.FileSize,
                CreatedAt = mediaItem.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var result = await _mediaService.DeleteMediaAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

public class UploadImageRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Caption { get; set; }
    public string? Emojis { get; set; }
}

public class MediaItemDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? Emojis { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
