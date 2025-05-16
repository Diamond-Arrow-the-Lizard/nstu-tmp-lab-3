using System;

namespace ClientUI.Models;

public class FileSystemItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    public string Icon => IsDirectory ? "📁" : "📄";
 
    // Normalize path separators
    public string DisplayPath => Path?.Replace('\\', '/') ?? throw new ArgumentNullException(nameof(Path));
}