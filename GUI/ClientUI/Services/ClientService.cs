using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab3.Interfaces;
using Lab3.Models.Handlers;

namespace ClientUI.Services;

public class ClientService
{
    private readonly ClientHandler _handler;

    public ClientService(IFileSystemService fileSystemService, ClientHandler handler)
    {
        _handler = handler;
    }

    public Task<IEnumerable<string>> GetDrivesAsync()
    {
        /* TODO Connect + Receive */
        throw new NotImplementedException("Getting drives is WIP");
    }

    public Task<string> SendPathAsync(string path)
    {
        /* TODO Send + Receive */ 
        throw new NotImplementedException("Sending paths is WIP");
    }

    public void Disconnect()
    {
        /* TODO Close */
        throw new NotImplementedException("Disconnect is WIP");
    }
}