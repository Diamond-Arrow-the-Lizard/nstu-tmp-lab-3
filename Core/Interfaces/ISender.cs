using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface ISender
{
    Task SendMessageAsync(NetworkStream stream, string message);
}