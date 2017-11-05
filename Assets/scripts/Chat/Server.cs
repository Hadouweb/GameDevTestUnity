using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;
    private TcpListener server;
    private bool serverStarted;
    
    public int port = 6321;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Server has been started on port " + port    );
        }
        catch (Exception e)
        {
            Debug.Log("socket error: " + e.Message);
            throw;
        }
    }

    private void Update()
    {
        if (!serverStarted)
            return;
        foreach (ServerClient client in clients)
        {
            if (!IsConnected(client.tcp))
            {
                client.tcp.Close();
                disconnectList.Add(client);
                continue;
            }
            else
            {
                NetworkStream s = client.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }
    }

    private void OnIncomingData(ServerClient client, string data)
    {
        Broadcast(data, clients);
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message + " to client " + c.clientName);
                throw;
            }
        }
    }
    
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
        
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return c.Client.Receive(new byte[1], SocketFlags.Peek) != 0;
                }
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    } 
    
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();
        
        Broadcast(clients[clients.Count- 1].clientName + " has connected", clients);
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}
