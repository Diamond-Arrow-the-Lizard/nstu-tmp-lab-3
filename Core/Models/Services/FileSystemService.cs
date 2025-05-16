namespace Lab3.Models.Services;

using Lab3.Interfaces;
using System.IO;

public class FileSystemService : IFileSystemService
{
    public bool IsDirectory(string path) => Directory.Exists(path);
    public bool IsFile(string path) => File.Exists(path);
 
    public string[] ListDirectory(string path)
    {
        return Directory.GetFileSystemEntries(path)
            .Select(entry => {
                var name = Path.GetFileName(entry);
                var isDirectory = Directory.Exists(entry);
                // Ensure root is handled correctly
                if (path == entry) // Check if it's the root
                {
                    return isDirectory ? $"{name}/" : name;
                }
                return isDirectory ? $"{name}/" : name;
            })
            .ToArray();
    } 
    public string ReadFile(string path) => File.ReadAllText(path);
}