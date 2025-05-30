using Microsoft.AspNetCore.Mvc.Filters;
using PhotoSite.Models;
using System.IO;

namespace PhotoSite.Filters;

public class ExceptionLogFilter(AppSettings _appSettings) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        string filename = makeFilename();
        string filePath = Path.Combine(_appSettings.ErrorLogPath, filename);

        if (!Directory.Exists(_appSettings.ErrorLogPath))
            Directory.CreateDirectory(_appSettings.ErrorLogPath);

        using FileStream stream = File.Create(filePath);
        using StreamWriter writer = new(stream);

        writer.Write(context.Exception.ToString());
    }

    string makeFilename()
        => $"Error_{DateTime.Now:yyyy-MM-dd_hh:mm:ss.ffff}_{makeUniqueId()}.txt";

    string makeUniqueId()
    => $"{Guid.NewGuid()}".Split("-").First();
}