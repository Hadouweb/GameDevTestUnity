using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;   

public class Server : MonoBehaviour {                                                                                                                                                                                                                                                                            
    Thread receiveThread; 
    UdpClient client; 
    public int port = 50000;  
    string strReceiveUDP="";
    string LocalIP = String.Empty;
    string hostname;

    public void Start()
    {
        Application.runInBackground = true;
        init();	
    }

    public void Update()
    {
        Debug.Log("Receive: " + UDPGetPacket());
    }
    
    // init
    private void init()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        hostname = "localhost";
        IPAddress[] ips = Dns.GetHostAddresses(hostname);
        if (ips.Length > 0)
        {
            LocalIP = ips[0].ToString();
        }
        Debug.Log(LocalIP);
    }

    void OnGUI()
    {
        Rect rectObj = new Rect(10, 10, 400, 200);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, hostname + " MY IP : " + LocalIP + " : " + strReceiveUDP, style);
    }

    private  void ReceiveData() 
    {
        client = new UdpClient(port);
        while (true) 
        {
            try 
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Broadcast, port);
                //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                strReceiveUDP = text;
                Debug.Log(strReceiveUDP);
            }
            catch (Exception err) 
            {
                print(err.ToString());
            }
        }
    }

    public string UDPGetPacket()
    {
        return strReceiveUDP;
    }

    void OnDisable()
    {
        if ( receiveThread!= null)	receiveThread.Abort();
        client.Close();
    }                                                                                                                                                                
}                                                                                                  