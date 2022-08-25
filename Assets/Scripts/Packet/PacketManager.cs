using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PacketManager
{
    private Dictionary<int, PacketHandler> handlers;
    List<PacketHandler> rm_list = new List<PacketHandler>();

    private int _nextHandlerId;

    public PacketManager()
    {
        handlers = new Dictionary<int, PacketHandler>();
    }

    public void Update(float dt)
    {
        foreach (var handler in handlers.Values)
        {
            handler.Update(dt);

            if (handler.Expired)
                rm_list.Add(handler);
        }

        foreach (var handler in rm_list)
            handlers.Remove(handler.handlerId);

        rm_list.Clear();
    }

    public PacketHandler Register(Type type, Action<Packet> onReceive)
    {
        PacketHandler handler = new PacketHandler(type, this, _nextHandlerId++, onReceive);
        handlers[handler.handlerId] = handler;

        return handler;
    }

    public PacketHandler Register(Type type, Action<Packet> onReceive, Action onTimeout, float timeoutInterval)
    {
        PacketHandler handler = new PacketHandler(type, this, _nextHandlerId++, timeoutInterval, onReceive, onTimeout);
        handlers[handler.handlerId] = handler;

        return handler;
    }

    public void OnPacketRead(Packet packet)
    {
        foreach(var handler in handlers.Values)
        {
            if(handler.type.Equals(packet.GetType()))
            {
                if(!handler.Expired)
                    handler.Handle(packet);
            }
        }
    }
}

