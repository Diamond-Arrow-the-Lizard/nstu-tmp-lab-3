using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface IReceiver
{
    Task<string> ReceiveMessageAsync(NetworkStream stream);
}