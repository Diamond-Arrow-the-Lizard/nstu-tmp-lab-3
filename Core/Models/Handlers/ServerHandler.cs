namespace Lab3.Models.Handlers;

using System.Net.Sockets;
using System.Text;
using Lab3.Interfaces;

public class ServerHandler: ISender, IReceiver 
{
    public int Port { get; }

    public ServerHandler(int port)
    {
        Port = port;
    }

    public void SendMessage(NetworkStream stream, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    public string ReceiveMessage(NetworkStream stream)
    {
        byte[] buffer = new byte[8192];
        int bytes = stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytes);
    }
}