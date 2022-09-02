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
    private int port = 11000;

    private byte[] receiveBuffer;
    private int receiveOffset;
    private const int BufferSize = 512;
    
    private PacketReader reader;
    public NetworkSession session;
    private const int IN_PACKETS_SIZE = 25;
    private Queue<Packet> incomingPackets;
    public PacketManager packetManager;

    private int packetsRead;
    private bool isGameInit;
    private bool isWaitingCharacters;   //True when loading characters at character select window
    private bool isEnteringWorld;       //True after logging in with character
    public bool IsGameInit { get { return isGameInit; } }

    public IPEndPoint remoteEndPoint
    { get { return (IPEndPoint)connectingSocket.RemoteEndPoint; } }
    public static bool Connected
    { get { return connectingSocket.Connected; } }
    public static EOCharacter EO_Character 
    { get { return player.GetComponent<EOCharacter>(); } }
   
    public static EOMap eo_map;
    public static GameObject player;
    public static MapLoader mapLoader;
    public static CharacterDef[] cs_defs;
    public static int cs_index;

    public delegate void LoginSucceed();
    public delegate void CSLoaded();
    public delegate void AccountCreated(ACCOUNT_CREATE_RESP resp);
    public delegate void WorldEntered();
    public LoginSucceed OnLoginSucceed;
    public CSLoaded OnCSLoaded;
    public WorldEntered OnWorldEnter;
    public AccountCreated OnAccCreate;

    
    void Awake()
    {
        //GameObject mapObj = GameObject.Find("Map");
        //eo_map = mapObj.GetComponent<EOMap>();
        //mapLoader = mapObj.GetComponent<MapLoader>();
        eo_map = GetComponent<EOMap>();
        mapLoader = GetComponent<MapLoader>();

        cs_defs = new CharacterDef[3];
        SceneManager.sceneUnloaded += OnSceneUnload;

        receiveBuffer = new byte[BufferSize];
        reader = new PacketReader(receiveBuffer);
        incomingPackets = new Queue<Packet>(IN_PACKETS_SIZE);
        connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        session.state = NetworkSessionState.NO_CONNECTION;
        packetManager = new PacketManager();

        Singleton = this;
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
            connectingSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port),
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
            SendPacket(new LoginAuth(username, password));
        }
    }


    public void CreateAccount(string username, string password, string realName, string loc, string email)
    {
        AccountCreate packet = new AccountCreate(username, password, realName, loc, email);
        SendPacket(packet);
    }


    public void CreateCharacter(string name, byte gender, byte hairStyle, byte hairColour, byte skinColour)
    {
        CharacterCreate packet = new CharacterCreate(name, gender, hairStyle, hairColour, skinColour);
        SendPacket(packet);
    }

    public void EnterWorld(int char_index)
    {
        if(Connected && session.state == NetworkSessionState.AUTH)
        {
            //Sync game time
            
            SendPacket(new SetNetworkTime(0));
            NetworkTime.timeSyncStart = NetworkTime.GetLocalTime();

            //Load world
            SendPacket(new ReqEnterWorld((uint)char_index));
            isEnteringWorld = true;
        }
    }

    public void PickupItem(ItemEntity item)
    {
        RequestItemPickup p = new RequestItemPickup(item.EntityId);

        SendPacket(p);
    }

    private void OnApplicationQuit()
    {
        if (connectingSocket.Connected)
        {
            Disconnect();
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
            SendPacket(new HelloPacket("NEO_CLIENT version:0.1"));
            
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
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

    //TODO: add catch SocketException
    private void Receive()
    {
        try
        {
            /*
            connectingSocket.BeginReceive(receiveBuffer, 0, BufferSize, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), null);
            */

            connectingSocket.BeginReceive(receiveBuffer, receiveOffset, (reader.messageSize - receiveOffset),
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
                if ((reader.messageSize - (receiveOffset + bytesRead)) == 0)
                {
                    //We haven't read in the length of the packet yet. Do so now
                    if (reader.packetLength == -1)
                    {
                        reader.ReadPacketLength();
                        receiveOffset += bytesRead;

                        //Debug.Log($"Packet length of {reader.packetLength}");
                    }
                    else
                    {
                        //Debug.Log("We did it! We constructed a message");

                        if (reader.ReadJSONPacket())
                        {
                            //Debug.Log($"Constructed packet {reader.packet}");
                            packetsRead++;

                            //Debug.Log($"Read {packetsRead} packets from server");
                            incomingPackets.Enqueue(reader.packet);

                        }
                        else
                        {
                            Debug.Log($"Failed to construct packet, error:{reader.error}");
                        }
                        //TODO: Finish packet reading
                        //TODO: Clear packet reader instead of always re-allocating? Not sure if more efficient
                        reader = new PacketReader(receiveBuffer);
                        receiveOffset = 0;
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
            Debug.LogWarning(e.ToString());
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
            Debug.LogWarning(e.ToString());
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

    void Update()
    {
        if(Connected)
        {
            packetManager.Update(Time.deltaTime);

            while (incomingPackets.Count > 0)
            {
                Packet packet = incomingPackets.Dequeue();
                //Debug.Log($"Reading packet of type: {packet.GetType()}");
                packetManager.OnPacketRead(packet);

                switch (session.state)
                {
                    case NetworkSessionState.PINGING:
                        if (packet is HelloPacket)
                        {
                            //Debug.Log("Initiating game...");
                            session.state = NetworkSessionState.ACCEPTED;

                            /*
                            NetworkTime.timeSyncStart = NetworkTime.GetLocalTime();
                            SendPacket(new SetNetworkTime(0));
                            SendPacket(new InitGamePacket());
                            */
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
                                    isGameInit = true;
                                    isEnteringWorld = false;

                                   

                                    if(OnWorldEnter != null)
                                        OnWorldEnter();
                                    /*
                                    ChestInvManager.Singleton.SetShop("Shop", 3, 3, 3);
                                    ChestInvManager.Singleton.ShowShopDisplay();
                                    */
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

                if (packet is SetNetworkTime)
                {
                    SetNetworkTime cp = (SetNetworkTime) packet;
                    var latency = (NetworkTime.GetLocalTime() - NetworkTime.timeSyncStart) / 2;

                    NetworkTime.timeDelta = (cp.netTime + latency) - NetworkTime.timeSyncStart;
                    NetworkTime.synced = true;

                    Debug.Log($"Synced network time, latency: {latency}, timeDelta: {NetworkTime.timeDelta}," +
                        $" current net_time:{NetworkTime.GetNetworkTime()}");

                }
            }
        }

    }



}
