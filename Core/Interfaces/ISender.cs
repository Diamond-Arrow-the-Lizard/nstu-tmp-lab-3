using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface ISender
{
    void SendMessage(NetworkStream stream, string message);
}