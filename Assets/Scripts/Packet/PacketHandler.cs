using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PacketHandler
{
    public int handlerId;
    public Type type;
    private PacketManager manager;
    private Action<Packet> onReceive;
    private Action onTimeout;

    private float _lifetime;
    private float _timeoutInterval;
    private bool _handleOnce;
    private bool expired;

    public bool Expired { get { return expired || (_timeoutInterval > 0) ? _lifetime >= _timeoutInterval : false; } }

    //No timeout
    public PacketHandler(Type type, PacketManager manager, int handlerId, Action<Packet> onReceive)
    {
        this.type = type;
        this.manager = manager;
        this.handlerId = handlerId;
        this.onReceive = onReceive;
    }

    public PacketHandler(Type type, PacketManager manager, int handlerId, float timeoutInterval, Action<Packet> onReceive, Action onTimeout)
    {
        this.type = type;
        this.manager = manager;
        this.handlerId = handlerId;
        this.onReceive = onReceive;
        this.onTimeout = onTimeout;
        this._timeoutInterval = timeoutInterval;
        this._handleOnce = true;
    }

    public void Update(float dt)
    {
        _lifetime += dt;

        if (_lifetime >= _timeoutInterval)
        {
            if (onTimeout != null)
                onTimeout();

            expired = true;
        }
    }

    public void Handle(Packet packet)
    {
        if(onReceive != null)
            onReceive(packet);

        if(_handleOnce)
            expired = true;
    }


}

