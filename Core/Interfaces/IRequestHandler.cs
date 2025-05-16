namespace Lab3.Interfaces;

public interface IRequestHandler
{
    Task<string> HandleRequestAsync(string path);
}