using System.Net;
using System.Net.Sockets;
using AppSorry.Data;
using AppSorry.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not found. Please set ConnectionStrings__DefaultConnection environment variable.");
}

// Railway private networking uses IPv6 which may be unreachable; force IPv4
connectionString = ForceIPv4Host(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IMusicService, MusicService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseStaticFiles();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

static string ForceIPv4Host(string connectionString)
{
    try
    {
        var csb = new NpgsqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(csb.Host) && !IPAddress.TryParse(csb.Host, out _))
        {
            var addresses = Dns.GetHostAddresses(csb.Host);
            var ipv4 = Array.Find(addresses, a => a.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4 != null)
                csb.Host = ipv4.ToString();
        }
        return csb.ConnectionString;
    }
    catch
    {
        return connectionString;
    }
}
