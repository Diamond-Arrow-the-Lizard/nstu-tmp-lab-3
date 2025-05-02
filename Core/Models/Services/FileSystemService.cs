namespace Lab3.Models.Services;

using Lab3.Interfaces;

public class FileSystemService: IFileSystemService
{
    public bool IsDirectory(string path) => Directory.Exists(path);
    public bool IsFile(string path) => File.Exists(path);
    public string[] ListDirectory(string path) =>
        Directory.GetFileSystemEntries(path);
    public string ReadFile(string path) =>
        File.ReadAllText(path);
}

