using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PacketManager
{
    private Dictionary<uint, PacketHandler> handlers;
    List<PacketHandler> rm_list = new List<PacketHandler>();

    private uint _nextHandlerId;

    public PacketManager()
    {
        handlers = new Dictionary<uint, PacketHandler>();
    }

    public void Update(float dt)
    {
        foreach (var handler in handlers.Values)
        {
            handler.Update(dt);

            if (handler.Expired)
            {
                rm_list.Add(handler);

                if (handler.TimedOut)
                    handler.OnTimeOut();
            }
                
        }

        if(rm_list.Count > 0)
        {
            foreach (var handler in rm_list)
            {
                Debug.Log($"Removing handler: {handler.type}");
                handlers.Remove(handler.handlerId);
            }
                

            rm_list.Clear();
        }
    }

    public PacketHandler Register(PacketType type, Action<PacketReader> onReceive)
    {
        var handler = new PacketHandler(type, this, _nextHandlerId++, onReceive);

        //PacketHandler handler = new PacketHandler(type, this, _nextHandlerId++);
        handlers[handler.handlerId] = handler;

        return handler;
    }

    public PacketHandler Register(PacketType type, float timeoutInterval, Action<PacketReader> onReceive, Action onTimeout)
    {
        PacketHandler handler = new PacketHandler(type, this, _nextHandlerId++, timeoutInterval, onReceive, onTimeout);
        handlers[handler.handlerId] = handler;

        return handler;
    }

    public void OnPacketRead(PacketReader packet)
    {
        Debug.Log($"PacketManager Read: packet type: {packet.packetType}");

        switch(packet.packetType)
        {
            case PacketType.HELLO_PACKET:
                PacketGreetHandler.HandleHelloPacket(packet);
                break;

            case PacketType.LOGIN_RESPONSE:
                PacketGreetHandler.HandleLoginResponse(packet);
                break;
            case PacketType.SET_CHARACTER_SLOT:
                PacketAccountHandler.HandleSetCharacterSlot(packet);
                break;
            case PacketType.SET_MAP:
                PacketMapHandler.HandleSetMap(packet);
                break;
            case PacketType.SET_CHARACTER_DEF:
                PacketPlayerHandler.HandleSetCharacterDef(packet);
                break;
            case PacketType.SET_NPC_DEF:
                PacketEntityHandler.HandleSetNpcDef(packet);
                break;
            case PacketType.SET_ITEM_DEF:
                PacketItemHandler.HandleSetItemDef(packet);
                break;
            case PacketType.SET_PLAYER_INV_ITEM:
                PacketItemHandler.HandleSetPlayerInvItem(packet);
                break;
            case PacketType.SET_PAPERDOLL_SLOT:
                PacketPlayerHandler.HandleSetPaperdollSlot(packet);
                break;
            case PacketType.ACK:
                PacketOtherHandler.HandleAck(packet);
                break;

        }

        foreach(var handler in handlers.Values)
        {
            if(handler.type.Equals(packet.packetType))
            {
                if(!handler.Expired)
                    handler.Handle(packet);
            }
        }
    }
}

