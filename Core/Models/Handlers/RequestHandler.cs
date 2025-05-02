namespace Lab3.Interfaces.Handlers;

using Lab3.Interfaces;

public class RequestHandler : IRequestHandler
{
    private readonly IFileSystemService _fileSystemService;

    public RequestHandler(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }
    public string HandleRequest(string path)
    {
        try
        {
            if (_fileSystemService.IsDirectory(path))
                return string.Join(Environment.NewLine, _fileSystemService.ListDirectory(path));
            if (_fileSystemService.IsFile(path))
                return _fileSystemService.ReadFile(path);
            return "Ошибка: пути не существует.";
        }
        catch (UnauthorizedAccessException)
        {
            return "У вас нет прав на просмотр этого пути.";
        }
    }

}