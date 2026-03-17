using Microsoft.EntityFrameworkCore;
using AppSorry.Models;

namespace AppSorry.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MediaItem> MediaItems { get; set; } = null!;
    public DbSet<BackgroundMusic> BackgroundMusic { get; set; } = null!;
}
