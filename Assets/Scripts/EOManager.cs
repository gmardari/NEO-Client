using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using EO.Map;
using EO;
using UnityEngine.SceneManagement;

public class EOManager : MonoBehaviour
{
    public static EOManager Singleton;

    private static Socket connectingSocket;
    private static string host = "127.0.0.1";
    private static int port = 11000;
    private bool lastConnectionStatus;

    private byte[] receiveBuffer;
    private int receiveOffset;
    private int _messageSize;
    private int _packetLength;
    private const int BufferSize = 512;

    
    private static PacketReader reader;
    private static NetworkSession session;
    private const int IN_PACKETS_SIZE = 25;
  
    private Queue<Packet> incomingPackets;
    private Queue<PacketReader> _incoming;
    public static PacketManager packetManager;


    private int packetsRead;
    public static bool isWaitingCharacters;   //True when loading characters at character select window
    public static bool isEnteringWorld;       //True after logging in with character
    public static bool IsGameInit { get; set; }

    public static IPEndPoint remoteEndPoint => (IPEndPoint) connectingSocket.RemoteEndPoint;
    public static bool Connected => connectingSocket.Connected;

   
    public static EOMap eo_map;
    public static GameObject player;
    public static EOCharacter EO_Character => player.GetComponent<EOCharacter>();
    public static MapLoader mapLoader;
    public static CS_CharacterDef[] cs_defs;
    public static int cs_index;
    public static int freeStatPoints;
    public static int freeSkillPoints;

    public delegate void LoginSucceedEvent();
    public delegate void CSLoadedEvent();
    public delegate void AccountCreatedEvent(ACCOUNT_CREATE_RESP resp);
    public delegate void WorldEnteredEvent();
    public static LoginSucceedEvent OnLoginSucceed;
    public static CSLoadedEvent OnCSLoaded;
    public static WorldEnteredEvent OnWorldEnter;
    public static AccountCreatedEvent OnAccCreate;

    
    void Awake()
    {
        //GameObject mapObj = GameObject.Find("Map");
        //eo_map = mapObj.GetComponent<EOMap>();
        //mapLoader = mapObj.GetComponent<MapLoader>();
       
        eo_map = GetComponent<EOMap>();
        mapLoader = GetComponent<MapLoader>();

        cs_defs = new CS_CharacterDef[3];
        SceneManager.sceneUnloaded += OnSceneUnload;

        receiveBuffer = new byte[BufferSize];
        SetReceiveDefaults();
        //reader = new PacketReader(receiveBuffer);
        incomingPackets = new Queue<Packet>(IN_PACKETS_SIZE);
        _incoming = new Queue<PacketReader>(IN_PACKETS_SIZE);

        connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        session.state = NetworkSessionState.NO_CONNECTION;
        packetManager = new PacketManager();

        Singleton = this;

        //TODO: Remove
        freeStatPoints = 10;

        
    }

    private void SetReceiveDefaults()
    {
        _messageSize = 4;
        _packetLength = -1;
        receiveOffset = 0;
    }

    public void Connect()
    {
        if (connectingSocket.Connected)
        {
            Debug.Log("Already connected");
            return;
        }
            

        try
        {
            connectingSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(host), port),
                new AsyncCallback(ConnectCallback), null);
        }
        catch (Exception e)
        {
            if (connectingSocket.Connected)
            {
                Disconnect();
            }

            Debug.LogWarning(e.ToString());
        }
    }

    public void Login(string username, string password)
    {
        if (!connectingSocket.Connected)
            return;

        if(session.state == NetworkSessionState.ACCEPTED)
        {
            PacketWriter writer = new PacketWriter(PacketType.LOGIN_AUTH);
            writer.WriteString(username)
                .WriteString(password)
                .Send();
            //SendPacket(new LoginAuth(username, password));
        }
    }


    public void CreateAccount(string username, string password, string realName, string loc, string email)
    {
        PacketWriter writer = new PacketWriter(PacketType.ACCOUNT_CREATE);
        writer.WriteString(username)
            .WriteString(password)
            .WriteString(realName)
            .WriteString(loc)
            .WriteString(email)
            .Send();
    }


    public void CreateCharacter(string name, byte gender, byte hairStyle, byte hairColour, byte skinColour)
    {
        PacketWriter writer = new PacketWriter(PacketType.CHARACTER_CREATE);
        writer.WriteString(name)
            .WriteByte(gender)
            .WriteByte(hairStyle)
            .WriteByte(hairColour)
            .WriteByte(skinColour)
            .Send();
    }

    public void EnterWorld(int char_index)
    {
        if(Connected && session.state == NetworkSessionState.AUTH)
        {
            //Sync game time

            //SendPacket(new SetNetworkTime(0));
            PacketWriter writer = new PacketWriter(PacketType.SET_NET_TIME);
            writer.WriteInt64(0)
                .Send();

            NetworkTime.timeSyncStart = NetworkTime.GetLocalTime();

            //Load world
            //SendPacket(new ReqEnterWorld((uint)char_index));
            writer = new PacketWriter(PacketType.REQUEST_ENTER_WORLD);
            writer.WriteUInt32((uint) char_index)
                .Send();

            isEnteringWorld = true;

            packetManager.Register(PacketType.ENTER_WORLD_RESPONSE, 2.0f, OnEnterWorldResponse,
                () =>
                {
                    UIManager.Singleton.ShowGameDialog("Connection failed", "No response from server");
                    isEnteringWorld = false;
                });
        }
    }

    /*public void PickupItem(Item item)
    {
        RequestItemPickup p = new RequestItemPickup(item.EntityId);

        SendPacket(p);
    }*/

    private void OnApplicationQuit()
    {
        if (connectingSocket.Connected)
        {
            Disconnect();
            PlayerPrefs.DeleteAll();
        }
    }

    private void OnSceneUnload(Scene current)
    {
        if (connectingSocket.Connected)
        {
            Disconnect();
        }
    }


    private void Disconnect()
    {
        connectingSocket.Shutdown(SocketShutdown.Both);
        connectingSocket.Close();

        UIManager.Singleton.ShowGameDialog("Disconnected", "Lost connection to game server");
        Debug.Log("Disconnected");
    }



    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            connectingSocket.EndConnect(ar);
            session.state = NetworkSessionState.PINGING;

            Debug.Log($"Client connected to {connectingSocket.RemoteEndPoint}");
            Debug.Log("Sending greeting...");
            //SendGreeting("Hi Server! I am client!");

            Receive();
            PacketWriter writer = new PacketWriter(PacketType.HELLO_PACKET);
            writer.WriteString("NEO_CLIENT version:0.1")
                .Send();
            //SendPacket(new HelloPacket("NEO_CLIENT version:0.1"));
            
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public void SendPacket(Packet packet)
    {
        if (!Connected)
            return;

        try
        {
            PacketWriter builder = new PacketWriter();
            /*
            if (!builder.WritePacket(packet))
                Debug.LogWarning($"Attempted to send a packet of type {packet.GetType().ToString()} that packet writer can't write (ignoring)");
            */
            builder.WriteJSONPacket(packet);
            //Debug.Log("Sending packet of buffer length " + builder.offset);

            connectingSocket.BeginSend(builder.buffer, 0, builder.offset, SocketFlags.None,
                new AsyncCallback(SendCallback), null);
        }
        catch(Exception e)
        {
            Disconnect();
            Debug.LogWarning(e);
        }
    }

    public void SendPacketNew(PacketWriter writer)
    {
        if (!Connected)
            return;

        try
        {
            /*
            if (!builder.WritePacket(packet))
                Debug.LogWarning($"Attempted to send a packet of type {packet.GetType().ToString()} that packet writer can't write (ignoring)");
            */
            //builder.WriteJSONPacket(packet);
            //Debug.Log("Sending packet of buffer length " + builder.offset);
            writer.WritePacketLength();
            connectingSocket.BeginSend(writer.buffer, 0, writer.offset, SocketFlags.None,
                new AsyncCallback(SendCallback), null);
        }
        catch (Exception e)
        {
            Disconnect();
            Debug.LogWarning(e);
        }
    }

    //TODO: add catch SocketException
    private void Receive()
    {
        try
        {
            /*
            connectingSocket.BeginReceive(receiveBuffer, 0, BufferSize, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), null);
            */

            connectingSocket.BeginReceive(receiveBuffer, receiveOffset, (_messageSize - receiveOffset),
                    SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            Disconnect();
            Debug.LogWarning(e);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int bytesRead = connectingSocket.EndReceive(ar);

            //TODO: Put at bottom
            
            if (bytesRead > 0)
            {
                if ((_messageSize - (receiveOffset + bytesRead)) == 0)
                {
                    //We haven't read in the length of the packet yet. Do so now
                    if (_packetLength == -1)
                    {
                        //reader.ReadPacketLength();
                        _packetLength = PacketReader.ReadPacketLength(receiveBuffer, 0);
                        _messageSize += _packetLength;
                        receiveOffset += bytesRead;
                        //Debug.Log($"Packet len: {_packetLength}, Message size: {_messageSize}");

                       
                        //Debug.Log($"Packet length of {reader.packetLength}");
                    }
                    else
                    {
                        //Debug.Log("We did it! We constructed a message");

                        /* if (reader.ReadJSONPacket())
                         {
                             //Debug.Log($"Constructed packet {reader.packet}");
                             packetsRead++;

                             //Debug.Log($"Read {packetsRead} packets from server");
                             incomingPackets.Enqueue(reader.packet);

                         }
                         else
                         {
                             Debug.Log($"Failed to construct packet, error:{reader.error}");
                         }*/
                        reader = new PacketReader(receiveBuffer, _packetLength);

                        PacketType packetType = reader.ReadPacketType();
                        if(packetType != PacketType.NONE)
                        {
                            packetsRead++;
                            _incoming.Enqueue(reader);
                            //Debug.Log($"Read {packetsRead} packets from server");
                            //incomingPackets.Enqueue(reader.packet);
                        }
                        else
                        {
                            Debug.LogWarning("Failed to read packet, error: " + reader.error.ToString());
                        }

                        //TODO: Finish packet reading
                        //TODO: Clear packet reader instead of always re-allocating? Not sure if more efficient
                        //reader = new PacketReader(receiveBuffer);
                        //receiveOffset = 0;

                        SetReceiveDefaults();
                    }

                }
                else
                {
                    receiveOffset += bytesRead;
                }

                Receive();
            }
            //Disconnected! TODO: Implement
            else
            {
                Disconnect();
                Debug.Log("Lost connection to server");
            }

           

        }
        catch (Exception e)
        {
            Disconnect();
            Debug.LogWarning(e);
        }

    }


    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            int bytesSent = connectingSocket.EndSend(ar);
           // Debug.Log($"Sent {bytesSent} bytes to server");
        }
        catch (Exception e)
        {
            Disconnect();
            Debug.LogWarning(e);
        }
    }

    private void SendGreeting(String data)
    {
        if (!connectingSocket.Connected)
            return;

        try
        {
            var packet = new List<byte>();
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            packet.AddRange(BitConverter.GetBytes(byteData.Length));
            packet.AddRange(byteData);
            byte[] bytePacket = packet.ToArray();

            connectingSocket.BeginSend(bytePacket, 0, bytePacket.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), "Sent greetings");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    private void OnEnterWorldResponse(PacketReader packet)
    {
        bool accepted = packet.ReadBoolean();

        if(accepted)
        {
            SetSessionState(NetworkSessionState.IN_GAME);
            UIManager.Singleton.SetState(UIState.IN_GAME);
        }
        else
        {
            UIManager.Singleton.ShowGameDialog("Rejected", "Server rejected you from joining world");
        }

        //isEnteringWorld = false;
    }

    void Update()
    {
        if (lastConnectionStatus && !Connected)
        {
            Debug.Log("Lost connection to server");
            UIManager.Singleton.ShowGameDialog("Lost connection", "Lost connection to server", 
                () => Debug.Log("Hehe xd"));
        }

        lastConnectionStatus = Connected;

        packetManager.Update(Time.deltaTime);

        if (Connected)
        {
            while (_incoming.Count > 0)
            {
                //Packet packet = incomingPackets.Dequeue();
                PacketReader pr = _incoming.Dequeue();
                //Debug.Log($"Reading packet of type: {packet.GetType()}");
                packetManager.OnPacketRead(pr);
                /*
                switch (session.state)
                {
                     case NetworkSessionState.PINGING:
                        if (packet is HelloPacket)
                        {
                            //Debug.Log("Initiating game...");
                            session.state = NetworkSessionState.ACCEPTED;

                           
                            NetworkTime.timeSyncStart = NetworkTime.GetLocalTime();
                            SendPacket(new SetNetworkTime(0));
                            SendPacket(new InitGamePacket());
                            
                        }
                        break;

                    case NetworkSessionState.ACCEPTED:

                        if (packet is LoginResponse)
                        {
                            var cp = (LoginResponse) packet;
                            
                            Debug.Log($"LOGIN RESPONSE: {cp.responseMsg}");

                            if (cp.response == 200)
                            {
                                if(OnLoginSucceed != null)
                                    OnLoginSucceed();

                                session.state = NetworkSessionState.AUTH;
                                isWaitingCharacters = true;
                                SendPacket(new RequestResource((byte)RESOURCE_TYPE.CS_CHARACTERS));
                            }
                            else
                            {
                                UIManager.Singleton.ShowGameDialog("Login", "Login: " + cp.responseMsg);
                            }

                        }
                        else if(packet is AccountCreateResponse)
                        {
                            var cp = packet as AccountCreateResponse;

                            ACCOUNT_CREATE_RESP resp = (ACCOUNT_CREATE_RESP)cp.response;

                            Debug.Log(resp.ToString());

                            if(OnAccCreate != null)
                                OnAccCreate(resp);
                        }
                        break;

                    case NetworkSessionState.AUTH:

                        if (isWaitingCharacters)
                        {
                            if (packet is SetCharacterDef)
                            {
                                SetCharacterDef cp = (SetCharacterDef) packet;
                                

                                if (cs_index < 3)
                                {
                                    cs_defs[cs_index++] = new CharacterDef(cp);
                                }
                                else
                                {
                                    Debug.LogWarning("[Character Select] Received too many character defintions than can be allowed.");
                                }
                            }
                            else if (packet is Ack)
                            {
                                var cp = (Ack) packet;

                                if(cp.resType == (byte) RESOURCE_TYPE.CS_CHARACTERS)
                                {
                                    isWaitingCharacters = false;

                                    if(OnCSLoaded != null)
                                        OnCSLoaded();
                                }
                            }
                        }
                        //When a character is created after logging in and loading all previous characters
                        else
                        {
                            if (packet is SetCharacterDef)
                            {
                                SetCharacterDef cp = (SetCharacterDef)packet;


                                if (cs_index < 3)
                                {
                                    cs_defs[cs_index++] = new CharacterDef(cp);

                                    if (UIManager.Singleton.state == UIState.CHARACTER_SELECT)
                                        UIManager.Singleton.guiBranch.GetComponent<CharacterSelect>().OnCharacterDefAdd(cs_defs[cs_index - 1]);
                                }
                                else
                                {
                                    Debug.LogWarning("[Character Select] Received too many character defintions than can be allowed.");
                                }
                            }
                        }

                        if (isEnteringWorld)
                        {
                            if (packet is EnterWorldResp)
                            {
                                var cp = packet as EnterWorldResp;

                                if (cp.accepted)
                                {
                                    session.state = NetworkSessionState.IN_GAME;
                                }
                            }
                        }

                        break;

                    case NetworkSessionState.IN_GAME:

                        if (packet is SetMapPacket)
                        {
                            var cp = packet as SetMapPacket;
                            Debug.Log($"Loading in new map. Character: {cp.myCharacterId}, Map Id: {cp.mapId}");
                            eo_map.SetMap(cp.mapId, cp.myCharacterId);
                        }
                        else if (packet is Ack)
                        {
                            Ack ackPacket = (Ack)packet;

                            //zero is 'map loaded'
                            if (ackPacket.resType == (byte)RESOURCE_TYPE.MAP)
                            {
                                eo_map.SetReady();

                                //First time entering world
                                if(isEnteringWorld)
                                {
                                    IsGameInit = true;
                                    isEnteringWorld = false;

                                   

                                    if(OnWorldEnter != null)
                                        OnWorldEnter();
                                    
                                    ChestInvManager.Singleton.SetShop("Shop", 3, 3, 3);
                                    ChestInvManager.Singleton.ShowShopDisplay();
                                  
                                }
                            }
                            else if (ackPacket.resType == (byte)RESOURCE_TYPE.CHEST_CONTENTS)
                            {
                                ChestInvManager.Singleton.NetDisplayChest();
                            }
                        }
                        else
                        {
                            eo_map.EnqueuePacket(packet);
                        }

                        break;
                }
                */


                /*
                if (packet is SetNetworkTime)
                {
                    SetNetworkTime cp = (SetNetworkTime) packet;
                    var latency = (NetworkTime.GetLocalTime() - NetworkTime.timeSyncStart) / 2;

                    NetworkTime.timeDelta = (cp.netTime + latency) - NetworkTime.timeSyncStart;
                    NetworkTime.synced = true;

                    Debug.Log($"Synced network time, latency: {latency}, timeDelta: {NetworkTime.timeDelta}," +
                        $" current net_time:{NetworkTime.GetNetworkTime()}");

                }
                */
            }
        }

    }

    public NetworkSessionState GetSessionState() { return session.state; }

    public void SetSessionState(NetworkSessionState newState)
    {
        if (session.state == newState)
            return;

        var oldState = session.state;
        session.state = newState;  
        
        if(newState == NetworkSessionState.PINGING)
        {

        }

    }

}
