namespace Lab3.Models.Services;

using Lab3.Interfaces;

public class DiskService : IDiskService
{
    public string GetDrivesString() =>
        string.Join(";", DriveInfo.GetDrives().Select(d => d.Name + "/"));
    
}