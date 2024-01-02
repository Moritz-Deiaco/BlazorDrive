using BlazorDrive.App.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BlazorDrive.App.Database;

public class DatabaseContext : DbContext
{
    private readonly ConfigService ConfigService;

    /*public DbSet<User>? Users { get; set; }*/


    public DatabaseContext(ConfigService configService)
    {
        ConfigService = configService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;
        var config = ConfigService.Get().Database;
        

        var connectionString = $"host={config.Host};" +
                               $"port={config.Port};" +
                               $"database={config.Database};" +
                               $"uid={config.Username};" +
                               $"pwd={config.Password}";

        ServerVersion version;
        try
        {
            version = ServerVersion.AutoDetect(connectionString);
        }
        catch (Exception e)
        {
            version = ServerVersion.Parse("5.7.37-mysql");
        }


        optionsBuilder.UseMySql(
            connectionString,
            version,
            builder => builder.EnableRetryOnFailure(5)
        );
    }
}
