using System;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using EO.Inventory;

public class PacketReader
{
    public int packetType;
    public int packetLength;
    public Packet packet;
    public bool constructed;
    public PacketError error;

    public int messageSize;
    
    private byte[] buffer;
    private int readOffset;

    public PacketReader(byte[] buffer)
    {
        this.buffer = buffer;
        packetType = -1;
        packetLength = -1;
        messageSize = 4;
        error = PacketError.NONE;
    }

    public byte ReadByte()
    {
        return buffer[readOffset++];
    }

    public short ReadInt16()
    {
        short a = BitConverter.ToInt16(buffer, readOffset);
        readOffset += 2;

        return a;
    }

    public int ReadInt32()
    {
        int a = BitConverter.ToInt32(buffer, readOffset);
        readOffset += 4;

        return a;
    }

    public long ReadInt64()
    {
        long a = BitConverter.ToInt64(buffer, readOffset);
        readOffset += 8;

        return a;
    }

    public ushort ReadUInt16()
    {
        ushort a = BitConverter.ToUInt16(buffer, readOffset);
        readOffset += 2;

        return a;
    }

    public uint ReadUInt32()
    {
        uint a = BitConverter.ToUInt32(buffer, readOffset);
        readOffset += 4;

        return a;
    }

    public ulong ReadUInt64()
    {
        ulong a = BitConverter.ToUInt64(buffer, readOffset);
        readOffset += 8;

        return a;
    }

    public string ReadString()
    {
        if ((messageSize - readOffset) >= 4)
        {
            int stringLen = ReadInt32();
            try
            {
                string s = Encoding.ASCII.GetString(buffer, readOffset, stringLen);
                readOffset += stringLen;
                return s;
            }
            catch(Exception e)
            {
                Debug.LogError(e.StackTrace);
                return null;
            }
        }

        return null;
    }

    public bool ReadBoolean()
    {
        if ((messageSize - readOffset) >= 1)
        {
            return BitConverter.ToBoolean(buffer, readOffset++);
        }

        return false;
    }

    public void ReadPacketLength()
    {
        packetLength = ReadInt32();
        messageSize += packetLength;
    }

    //size of buffer doesnt start from offset
    public bool ReadPacket()
    {
        //Read an integer for packet type
        if ((messageSize - readOffset) < 4)
        {
            error = PacketError.NO_PACKET_TYPE;
            return false;
        }
        else
        {
            int a = ReadInt32();

            //Successful
            if (a >= 0)
            {
                packetType = a;
            }
        }

        switch (packetType)
        {
            //Hello packet!
            case (int) PacketType.HELLO_PACKET:

                string s = ReadString();

                if (s != null)
                {
                    //Debug.Log("Got message: " + s);
                    packet = new HelloPacket(s);
                }
                else
                {
                    error = PacketError.INVALID_DATA;
                    return false;
                }
             //InitGamePacket not supported for client
                break;

            case (int) PacketType.SET_MAP:

                uint mapId = ReadUInt32();
                ulong characterId = ReadUInt64();

                packet = new SetMapPacket(mapId, characterId);
                break;

            case (int) PacketType.SET_ENTITY_DEF:

                ulong entityId = ReadUInt64();
                uint entityType = ReadUInt32();
                int x = ReadInt32();
                int y = ReadInt32();

                packet = new SetEntityDef(entityId, entityType, x, y);
                break;

            case (int) PacketType.SET_ENTITY_POS:

                entityId = ReadUInt64();
                x = ReadInt32();
                y = ReadInt32();

                packet = new SetEntityPos(entityId, x, y);
                break;

            case (int) PacketType.SET_ENTITY_DIR:

                entityId = ReadUInt64();
                uint direction = ReadUInt32();

                packet = new SetEntityDir(entityId, direction);
                break;

            case (int) PacketType.SET_ENTITY_WALK:

                entityId = ReadUInt64();
                int fromX = ReadInt32();
                int fromY = ReadInt32();
                direction = ReadUInt32();
                byte speed = ReadByte();
                long timeStarted = ReadInt64();

                packet = new SetEntityWalk(entityId, fromX, fromY, direction, speed, timeStarted);
                break;

            case (int) PacketType.SET_ENTITY_ATTACK:

                entityId = ReadUInt64();
                timeStarted = ReadInt64();

                packet = new SetEntityAttack(entityId,timeStarted);
                break;

            case (int)PacketType.SET_ENTITY_PROP:

                entityId = ReadUInt64();
                uint propType = ReadUInt32();
                object propValue = null;
                
                //Read in propValue based on propType
                switch(propType)
                {
                    case (uint)EntityProperty.HEALTH:
                        propValue = ReadUInt64();
                        break;

                    case (uint)EntityProperty.NAME:
                        propValue = ReadString();
                        break;

                }

                packet = new SetEntityProp(entityId, propType, propValue);
                break;
            case (int)PacketType.SET_ENTITY_HEALTH:
                {
                    entityId = ReadUInt64();
                    ulong health = ReadUInt64();
                    ulong maxHealth = ReadUInt64();
                    long deltaHp = ReadInt64();

                    packet = new SetEntityHealth(entityId, health, maxHealth, deltaHp);
                    break;
                }
            case (int) PacketType.REMOVE_ENTITY:

                entityId = ReadUInt64();

                packet = new RemoveEntity(entityId);
                break;
                
            case (int) PacketType.ACK:

                byte type = ReadByte();

                packet = new Ack(type);
                break;
                
            case (int) PacketType.SET_NET_TIME:

                long netTime = ReadInt64();

                packet = new SetNetworkTime(netTime);
                break;

            case (int) PacketType.LOGIN_RESPONSE:

                uint response = ReadUInt32();
                string responseMsg = ReadString();

                packet = new LoginResponse(response, responseMsg);
                break;
                
            case (int)PacketType.SET_CHARACTER_DEF:
                {
                    entityId = ReadUInt64();
                    x = ReadInt32();
                    y = ReadInt32();
                    direction = ReadUInt32();
                    ulong health = ReadUInt64();
                    ulong maxHealth = ReadUInt64();

                    string name = ReadString();
                    byte gender = ReadByte(); ;
                    byte race = ReadByte();
                    byte hairStyle = ReadByte();
                    uint armor = ReadUInt32();
                    uint hat = ReadUInt32();
                    uint boots = ReadUInt32();
                    uint back = ReadUInt32();
                    uint weapon = ReadUInt32();

                    CharacterDef def = new CharacterDef
                    {
                        name = name,
                        gender = gender,
                        race = race,
                        hairStyle = hairStyle,
                        doll = new Paperdoll
                        {
                            armor = armor,
                            hat = hat,
                            boots = boots,
                            back = back,
                            weapon = weapon
                        }

                    };

                    packet = new SetCharacterDef(entityId, x, y, direction, health, maxHealth, def);
                    break;
                }
            case (int)PacketType.SET_NPC_DEF:
                {
                    entityId = ReadUInt64();
                    x = ReadInt32();
                    y = ReadInt32();
                    direction = ReadUInt32();
                    ulong health = ReadUInt64();
                    ulong maxHealth = ReadUInt64();
                    uint npcId = ReadUInt32();

                    packet = new SetNpcDef(entityId, x, y, direction, health, maxHealth, npcId);
                    break;
                }
            case (int) PacketType.SET_ITEM_DEF:

                entityId = ReadUInt64();
                x = ReadInt32();
                y = ReadInt32();
                uint itemId = ReadUInt32();
                uint quantity = ReadUInt32();

                packet = new SetItemDef(entityId, x, y, itemId, quantity);

                break;

            case (int) PacketType.SET_PLAYER_INV_ITEM:

                itemId = ReadUInt32();
                quantity = ReadUInt32();
                uint slotX = ReadUInt32();
                uint slotY = ReadUInt32();

                packet = new SetPlayerInvItem(itemId, quantity, slotX, slotY);

                break;

            case (int)PacketType.SET_CHEST_INV_ITEM:
                {

                    break;
                }
            case (int)PacketType.SET_PAPERDOLL_SLOT:
                {
                    entityId = ReadUInt64();
                    byte slotIndex = ReadByte();
                    itemId = ReadUInt32();
                    bool equipped = ReadBoolean();

                    packet = new SetPaperdollSlot(entityId, slotIndex, itemId, equipped);

                    break;
                }

            case (int)PacketType.INIT_PLAYER_VALS:
                {
                    ulong health = ReadUInt64();
                    ulong maxHealth = ReadUInt64();
                    ulong mana = ReadUInt64();
                    ulong maxMana = ReadUInt64();
                    ulong energy = ReadUInt64();
                    ulong maxEnergy = ReadUInt64();

                    uint level = ReadUInt32();
                    ulong exp = ReadUInt64();
                    ulong expLevel = ReadUInt64();
                    ulong expTNL = ReadUInt64();

                    packet = new InitPlayerVals(health, maxHealth, mana, maxMana, energy, maxEnergy, level, exp, expLevel, expTNL);

                    break;
                }
                
            default:
                error = PacketError.INVALID_PACKET_TYPE;
                return false;
        }

        constructed = true;

        return true;
    }

    public bool ReadJSONPacket()
    {
        if ((messageSize - readOffset) < 4)
        {
            error = PacketError.NO_PACKET_TYPE;
            return false;
        }
        else
        {
            int a = ReadInt32();

            //Successful
            if (a >= 0)
            {
                packetType = a;
            }
            else
                return false;
        }

        string json = ReadString();

        switch (packetType)
        {
            case (int)PacketType.HELLO_PACKET:

                packet = JsonConvert.DeserializeObject<HelloPacket>(json);
                break;

            case (int)PacketType.ACCOUNT_CREATE_RESPONSE:

                packet = JsonConvert.DeserializeObject<AccountCreateResponse>(json);
                break;

            case (int)PacketType.CHARACTER_CREATE_RESPONSE:

                packet = JsonConvert.DeserializeObject<CharacterCreateResponse>(json);
                break;

            case (int)PacketType.ENTER_WORLD_RESPONSE:
                packet = JsonConvert.DeserializeObject<EnterWorldResp>(json);
                break;

            case (int)PacketType.SET_MAP:

                packet = JsonConvert.DeserializeObject<SetMapPacket>(json);
                break;

            case (int)PacketType.SET_ENTITY_DEF:

                packet = JsonConvert.DeserializeObject<SetEntityDef>(json);
                break;

            case (int)PacketType.SET_ENTITY_POS:

                packet = JsonConvert.DeserializeObject<SetEntityPos>(json);
                break;

            case (int)PacketType.SET_ENTITY_DIR:

                packet = JsonConvert.DeserializeObject<SetEntityDir>(json);
                break;

            case (int)PacketType.SET_ENTITY_WALK:

                packet = JsonConvert.DeserializeObject<SetEntityWalk>(json);
                break;

            case (int)PacketType.SET_ENTITY_ATTACK:

                packet = JsonConvert.DeserializeObject<SetEntityAttack>(json);
                break;

            case (int)PacketType.SET_ENTITY_PROP:

                packet = JsonConvert.DeserializeObject<SetEntityProp>(json);
                break;
            case (int)PacketType.SET_ENTITY_HEALTH:

                packet = JsonConvert.DeserializeObject<SetEntityHealth>(json);
                break;
            case (int)PacketType.REMOVE_ENTITY:

                packet = JsonConvert.DeserializeObject<RemoveEntity>(json);
                break;

            case (int)PacketType.ACK:

                packet = JsonConvert.DeserializeObject<Ack>(json);
                break;

            case (int)PacketType.SET_NET_TIME:

                packet = JsonConvert.DeserializeObject<SetNetworkTime>(json);
                break;

            case (int)PacketType.LOGIN_RESPONSE:

                packet = JsonConvert.DeserializeObject<LoginResponse>(json);
                break;

            case (int)PacketType.SET_CHARACTER_DEF:
                
                packet = JsonConvert.DeserializeObject<SetCharacterDef>(json);
                break;
                
            case (int)PacketType.SET_NPC_DEF:
                packet = JsonConvert.DeserializeObject<SetNpcDef>(json);
                break;
            case (int)PacketType.SET_ITEM_DEF:

                packet = JsonConvert.DeserializeObject<SetItemDef>(json);
                break;

            case (int)PacketType.SET_PLAYER_INV_ITEM:

                packet = JsonConvert.DeserializeObject<SetPlayerInvItem>(json);
                break;

            case (int)PacketType.SET_CHEST_INV_ITEM:
                packet = JsonConvert.DeserializeObject<SetChestInvItem>(json);
                break;
            case (int)PacketType.SET_PAPERDOLL_SLOT:
                packet = JsonConvert.DeserializeObject<SetPaperdollSlot>(json);
                break;

            case (int)PacketType.INIT_PLAYER_VALS:
                packet = JsonConvert.DeserializeObject<InitPlayerVals>(json);
                break;

            case (int)PacketType.CHEST_OPEN:
                packet = JsonConvert.DeserializeObject<ChestOpen>(json);
                break;

            default:
                error = PacketError.INVALID_PACKET_TYPE;
                return false;
        }

        return true;

    }
}
