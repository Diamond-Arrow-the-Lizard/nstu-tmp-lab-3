using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface IReceiver
{
    Task<string> ReceiveMessage(NetworkStream stream);
}