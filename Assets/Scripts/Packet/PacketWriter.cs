using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public class PacketWriter
{
    public byte[] buffer;
    public int offset;
    public const int BufferSize = 512;

    public PacketWriter()
    {
        buffer = new byte[BufferSize];
        offset = 0;
    }

    public bool CanWrite(int size)
    {
        if ((BufferSize - offset) >= size)
            return true;

        return false;
    }

    public void ApplyLengthPadding()
    {
        offset += 4;
    }

    public void ApplyPadding(int byteLength)
    {
        offset += byteLength;
    }

    //Used when Length padding applied
    //The first integer for packet length is ignored, hence offset - 4
    public void WritePacketLength()
    {
        uint packetLength = (uint) (offset - 4);
        Array.Copy(BitConverter.GetBytes(packetLength), 0, buffer, 0, 4);
    }

    public void WriteByte(byte a)
    {
        if (CanWrite(1))
        {
            buffer[offset++] = a;
        }
    }

    public void WriteInt16(short a)
    {
        if (CanWrite(2))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 2);
            offset += 2;
        }
    }

    public void WriteInt32(int a)
    {
        if (CanWrite(4))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 4);
            offset += 4;
        }
    }

    public void WriteInt64(long a)
    {
        if (CanWrite(8))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 8);
            offset += 8;
        }
    }

    public void WriteUInt16(ushort a)
    {
        if (CanWrite(2))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 2);
            offset += 2;
        }
    }

    public void WriteUInt32(uint a)
    {
        if (CanWrite(4))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 4);
            offset += 4;
        }
    }

    public void WriteUInt64(ulong a)
    {
        if (CanWrite(8))
        {
            Array.Copy(BitConverter.GetBytes(a), 0, buffer, offset, 8);
            offset += 8;
        }
    }

    public void WriteString(string s)
    {
        byte[] stringData = Encoding.ASCII.GetBytes(s);
        int len = 4 + s.Length;

        if (CanWrite(len))
        {
            Array.Copy(BitConverter.GetBytes(stringData.Length), 0, buffer, offset, 4);
            Array.Copy(stringData, 0, buffer, offset + 4, stringData.Length);
            offset += len;
        }
    }

    public void WriteBoolean(bool b)
    {
        if(CanWrite(1))
        {
            buffer[offset++] = BitConverter.GetBytes(b)[0];
        }
       
    }

    //Serialize the packet fields through a binary serializer
    public bool WritePacket(Packet packet)
    {
        if (typeof(HelloPacket).IsInstanceOfType(packet))
        {
            HelloPacket helloPacket = (HelloPacket)packet;
            //Make space for size
            ApplyLengthPadding();

            WriteInt32((int) PacketType.HELLO_PACKET);
            WriteString(helloPacket.msg);

            WritePacketLength();

            return true;
        }
        else if(typeof(ReqEnterWorld).IsInstanceOfType(packet))
        {
            ReqEnterWorld cp = (ReqEnterWorld)packet;
            ApplyLengthPadding();

            WriteInt32((int) PacketType.REQUEST_ENTER_WORLD);
            WriteUInt32(cp.char_index);

            WritePacketLength();

            return true;
        }
        else if (typeof(RequestPlayerDir).IsInstanceOfType(packet))
        {
            RequestPlayerDir castedPacket = (RequestPlayerDir) packet;
            ApplyLengthPadding();

            WriteInt32((int) PacketType.REQUEST_PLAYER_DIR);
            WriteInt32(castedPacket.direction);
            WriteBoolean(castedPacket.walk);

            WritePacketLength();

            return true;
        }
        else if (packet is RequestPlayerAttack)
        {
            RequestPlayerAttack cp = (RequestPlayerAttack) packet;
            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_PLAYER_ATTACK);

            WritePacketLength();

            return true;
        }
        else if (packet is RequestItemPickup)
        {
            var cp = packet as RequestItemPickup;
            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ITEM_PICKUP);
            WriteUInt64(cp.entityId);

            WritePacketLength();

            return true;
        }
        else if (packet is RequestItemDrop)
        {
            var cp = packet as RequestItemDrop;
            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ITEM_DROP);
            WriteUInt64(cp.itemId);

            WritePacketLength();

            return true;
        }
        else if(packet is RequestItemMove)
        {
            var cp = packet as RequestItemMove;

            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ITEM_MOVE);
            WriteUInt32(cp.itemId);
            WriteUInt32(cp.x);
            WriteUInt32(cp.y);

            WritePacketLength();

            return true;
        }
        else if (packet is RequestItemEquip)
        {
            var cp = packet as RequestItemEquip;

            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ITEM_EQUIP);
            WriteUInt32(cp.itemId);
            WriteByte(cp.slotIndex);
            WriteBoolean(cp.equip);

            WritePacketLength();

            return true;
        }
        else if(packet is RequestItemConsume)
        {
            var cp = packet as RequestItemConsume;

            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ITEM_CONSUME);
            WriteUInt32(cp.itemId);
           
            WritePacketLength();

            return true;
        }
        else if (packet is RequestEntityInteract)
        {
            var cp = packet as RequestEntityInteract;

            ApplyLengthPadding();

            WriteInt32((int)PacketType.REQUEST_ENTITY_INTERACT);
            WriteUInt64(cp.entityId);
            WriteByte(cp.interactId);

            WritePacketLength();

            return true;
        }
        else if (typeof(SetNetworkTime).IsInstanceOfType(packet))
        {
            ApplyLengthPadding();
            WriteInt32((int)PacketType.SET_NET_TIME);
            ApplyPadding(8);
            WritePacketLength();

            return true;
        }
        else if(typeof(LoginAuth).IsInstanceOfType(packet))
        {
            LoginAuth cp = (LoginAuth) packet;

            ApplyLengthPadding();
            WriteInt32((int)PacketType.LOGIN_AUTH);
            WriteString(cp.username);
            WriteString(cp.password);
            WritePacketLength();

            return true;
        }
        else if (typeof(RequestResource).IsInstanceOfType(packet))
        {
            RequestResource cp = (RequestResource)packet;

            ApplyLengthPadding();
            WriteInt32((int)PacketType.REQUEST_RES);
            WriteByte(cp.resType);
            WritePacketLength();

            return true;
        }

        return false;
    }

    public void WriteJSONPacket(Packet packet)
    {
        ApplyLengthPadding();

        string json = JsonConvert.SerializeObject(packet);
        WriteInt32(packet.packetType);
        WriteString(json);
        WritePacketLength();
    }
}
