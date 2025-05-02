using System.Net.Sockets;

namespace Lab3.Interfaces;

public interface IReceiver
{
    string ReceiveMessage(NetworkStream stream);
}