using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

public class ServerTmp : MonoBehaviour
{

    public int port = 13000;
    
    private bool mRunning;
    private IPAddress ipAddress;
    string msg = "";
    Thread mThread;
    TcpListener tcp_Listener = null;
    
    void Start()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
        foreach (IPAddress addr in localIPs)
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = addr;
                Debug.Log(ipAddress);
            }
        }
        mRunning = true;
        ThreadStart ts = new ThreadStart(SayHello);
        mThread = new Thread(ts);
        mThread.Start();
        Debug.Log("Thread done...");
    }
    
    public void stopListening()
    {
        mRunning = false;
    }
    
    void SayHello()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
           
            // TcpListener server = new TcpListener(port);
            tcp_Listener = new TcpListener(localAddr, port);
            tcp_Listener.Start();
            Debug.Log("Server Start");
            while (mRunning)
            {
                TcpClient client = tcp_Listener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                StreamReader reader = new StreamReader(ns);
                msg = reader.ReadLine();
                Debug.Log(msg);
                reader.Close();
                client.Close();
            }
        }
        catch (ThreadAbortException e)
        {
            Debug.Log(e);
        }
        finally
        {
            mRunning = false;
            tcp_Listener.Stop();
        }
    }
    void OnApplicationQuit()
    {
        // stop listening thread
        stopListening();
        // wait fpr listening thread to terminate (max. 500ms)
        mThread.Join(500);
    }

}