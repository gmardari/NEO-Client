using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EntityDef : MonoBehaviour
{

    public Vector2Int net_position = default;
    public Vector2Int position = default;
    public uint mapId;
    public EntityType entityType;
    public ulong entityId;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(ulong _entityId, EntityType _entityType, uint _mapId, Vector2Int _position)
    {
        (entityId, entityType, mapId, net_position) = (_entityId, _entityType, _mapId, _position);
        position = net_position;
    }

    public void Init(uint _mapId, SetEntityDef defPacket)
    {
        (entityId, entityType, mapId, net_position) = (defPacket.entityId, (EntityType) defPacket.entityType, _mapId, new Vector2Int(defPacket.posX, defPacket.posY));
        position = net_position;
    }

    public void NetSetPosition(Vector2Int _position)
    {
        net_position = _position;
        position = net_position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
