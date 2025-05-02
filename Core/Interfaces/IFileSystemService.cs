namespace Lab3.Interfaces;

public interface IFileSystemService
{
    bool IsDirectory(string path);
    bool IsFile(string path);
    string[] ListDirectory(string path);
    string ReadFile(string path);
}