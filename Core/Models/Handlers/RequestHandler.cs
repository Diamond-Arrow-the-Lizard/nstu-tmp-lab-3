namespace Lab3.Interfaces.Handlers;

using Lab3.Interfaces;

public class RequestHandler : IRequestHandler
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IDiskService _diskService; 

    public RequestHandler(IFileSystemService fileSystemService, IDiskService diskService) 
    {
        _fileSystemService = fileSystemService;
        _diskService = diskService; 
    }

    public async Task<string> HandleRequestAsync(string request) 
    {
        try
        {
            await Task.Delay(0);
            if (request == "LIST_DRIVES")
            {
                return _diskService.GetDrivesString();
            }
            else if (request.StartsWith("LIST "))
            {
                string path = request.Substring(5); // Extract path
                return string.Join(Environment.NewLine, _fileSystemService.ListDirectory(path));
            }
            else if (request.StartsWith("GET "))
            {
                string filePath = request.Substring(4); // Extract file path
                return _fileSystemService.ReadFile(filePath);
            }
            else
            {
                return "Unknown Request";
            }
        }
        catch (UnauthorizedAccessException)
        {
            return "Permission denied";
        }
        catch (DirectoryNotFoundException)
        {
            return "Directory not found";
        }
        catch (FileNotFoundException)
        {
            return "File not found";
        }
    }
}