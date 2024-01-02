using Logging.Net;

namespace BlazorDrive.App.Helpers;

public class ConfigHelper
{
    public ConfigHelper()
    {
        
    }

    public Task Perform()
    {
        Logger.Info("Checking config file");
        var dir = PathBuilder.Dir("storage");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        var path = PathBuilder.File("storage", "config.json");

        if (File.Exists(path))
        {
            Logger.Info("Config file exists, continuing startup");
            return Task.CompletedTask;
        }

        FileStream fs = File.Create(path);
        fs.Close();
        return Task.CompletedTask;
    }
}