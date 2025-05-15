using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface ISender
{
    Task SendMessage(NetworkStream stream, string message);
}