namespace AppSorry.Models;

public class BackgroundMusic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsFromYoutube { get; set; }
    public string? YoutubeUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
