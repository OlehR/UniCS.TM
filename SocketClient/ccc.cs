using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using LitJson;
using System.Diagnostics;

public class NetworkClient : MonoBehaviour
{
    public enum PeerType
    {
        Connecting,
        Connected,
        Idle,
        Error
    }
    PeerType peerType = PeerType.Idle;

    class NetworkMessage
    {
        public string id;
    }

    public class NetworkRawMessage
    {
        public const int headerSize = 16;
        public byte[] rawHeader = new byte[16];
        public byte[] decodedHeader = null;
        public int hLen;
        public int contentLen;
        public byte[] gheader = null;
        public byte[] content = null;
        public byte flag1;  // 'Z' 'H'
        public byte flag2;
    }
    List<JsonData> backQueue = new List<JsonData>();
    List<JsonData> frontQueue = new List<JsonData>();

    public string hostIP = "111.10.24.20";
    public int hostPort = 12807;

    Socket client = null;

    float lastSendTime = 0;
    bool login = false;
    object lockThis = new object();

    public void Initialize()
    {
        lastSendTime = Time.time;
        try
        {
            ConnectToServer();
        }
        catch (Exception e)
        {
            peerType = PeerType.Error;
            Debug.Log(e.Message);
        }
    }

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !login)
        {
            login = true;
            Debug.Log("Login");
            JsonData data = new JsonData();
            data["guid"] = "a1e733ce0878470ea65587c9cc28ba77";
            byte[] header = new byte[0];
            byte[] content = Encoding.UTF8.GetBytes(data.ToJson());
            SendData((byte)'H', (byte)'R', header, content);
        }
        if (lastSendTime + 10 < Time.time)
        {
            HeartCallback(null);
        }

        lock (lockThis)
        {
            List<JsonData> temp = backQueue;
            backQueue = frontQueue;
            frontQueue = temp;
        }
        foreach (JsonData jsonData in frontQueue)
        {
            //process jsondata 
            Debug.Log(string.Format("Message content: {0}", JsonMapper.ToJson(jsonData)));
        }
        frontQueue.Clear();
    }

    void OnApplicationQuit()
    {
        if (peerType == PeerType.Connected)
        {
            CloseClient();
        }
    }

    void ConnectToServer()
    {
        if (client == null) client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        client.BeginConnect(hostIP, hostPort, new AsyncCallback(ConnectCallback), client);
        peerType = PeerType.Connecting;
    }

    void ConnectCallback(IAsyncResult ar)
    {
        Debug.Log("Connected to server, start recieve data");

        RecieveHeader();//start recieve header
    }

    void RecieveHeader()
    {
        try
        {
            NetworkRawMessage msg = new NetworkRawMessage();
            client.BeginReceive(msg.rawHeader, 0, 16, SocketFlags.None, new AsyncCallback(RecieveHeaderCallback), msg);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void RecieveHeaderCallback(IAsyncResult ar)
    {
        try
        {
            int readLen = client.EndReceive(ar);
            if (readLen > 0)
            {
                NetworkRawMessage msg = (NetworkRawMessage)ar.AsyncState;
                msg.decodedHeader = GameClient.DataTransfer.analyzeHeader(msg.rawHeader);//decode header

                if (msg.decodedHeader != null)
                {

                    msg.flag1 = msg.decodedHeader[2];
                    msg.flag2 = msg.decodedHeader[3];
                    msg.hLen = BitConverter.ToInt32(msg.decodedHeader, 4);
                    msg.contentLen = BitConverter.ToInt32(msg.decodedHeader, 8);
                    msg.gheader = new byte[msg.hLen];
                    msg.content = new byte[msg.contentLen];
                    if (msg.hLen > 0)
                    {// if data header size is zero, skip it, read data directly
                        client.BeginReceive(msg.gheader, 0, msg.hLen, SocketFlags.None, new AsyncCallback(RecieveDataHeaderCallback), msg);
                    }
                    else if (msg.contentLen > 0)
                    {
                        client.BeginReceive(msg.content, 0, msg.contentLen, SocketFlags.None, new AsyncCallback(RecieveContentCallback), msg);
                    }
                    else if (msg.flag1 == (byte)'Z')
                    {  // heart package, no data, recieve next package
                        Debug.Log("server heart beat!");
                        RecieveHeader();
                    }
                    else
                    {
                        Debug.Log(string.Format("Null content with header length: {0}, flag:{1},{2}", readLen, msg.flag1, msg.flag2));
                    }
                }
                else
                {
                    OnNetworkError("invalid header, close....");
                }
            }
            else
            {
                OnNetworkError("failed read header, server may disconnect this client");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void RecieveDataHeaderCallback(IAsyncResult ar)
    {
        try
        {
            int readLen = client.EndReceive(ar);
            NetworkRawMessage msg = (NetworkRawMessage)ar.AsyncState;

            if (readLen > 0)
            {
                client.BeginReceive(msg.content, 0, msg.contentLen, SocketFlags.None, new AsyncCallback(RecieveContentCallback), msg);
            }
            else
            {
                OnNetworkError("failed read GHeader data");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void RecieveContentCallback(IAsyncResult ar)
    {
        try
        {
            int readLen = client.EndReceive(ar);
            NetworkRawMessage msg = (NetworkRawMessage)ar.AsyncState;

            if (readLen > 0)
            {
                LitJson.JsonData data = LitJson.JsonMapper.ToObject(Encoding.UTF8.GetString(msg.content));
                PushMessage(msg, data);
                RecieveHeader();//start async recieve next header 
            }
            else
            {
                OnNetworkError("failed read content data");
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    void RecieveCallback(IAsyncResult ar)
    {
        int bytesRead = client.EndReceive(ar);
    }

    void HeartCallback(object state)
    {
        Debug.Log("heart beat!");
        byte[] heart = GameClient.DataTransfer.createHeader((byte)'Z', (byte)'T', 0, 0);
        SendInternal(heart);
    }
    void SendInternal(byte[] data)
    {
        lastSendTime = Time.time;
        client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), client);
    }

    void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Send data success.");
    }

    void OnNetworkError(string s = "")
    {
        Debug.Log(s);
    }

    void PushMessage(NetworkRawMessage msg, JsonData jsonData)
    {

        lock (lockThis)
        {
            backQueue.Add(jsonData);
        }
    }

    void SendData(byte svn, byte msgType, byte[] header, byte[] content)
    {
        byte[] pheader = GameClient.DataTransfer.createHeader(svn, msgType, header.Length, content.Length);//encode header
        byte[] sendBytes = new byte[pheader.Length + header.Length + content.Length];
        pheader.CopyTo(sendBytes, 0);
        header.CopyTo(sendBytes, pheader.Length);
        content.CopyTo(sendBytes, pheader.Length + header.Length);
        SendInternal(sendBytes);
    }

    void CloseClient()
    {
        client.Close();
        client = null;
        peerType = PeerType.Idle;
    }
}