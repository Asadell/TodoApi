namespace TodoApi.Services;

public interface IFileService
{
    Task<(string fileName, string filePath, long fileSize)> SaveFileAsync(IFormFile file, string folder = "uploads");
    void DeleteFile(string filePath);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(string fileName, string filePath, long fileSize)> SaveFileAsync(IFormFile file, string folder = "uploads")
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return (uniqueFileName, filePath, file.Length);
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}