using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EO;
using EO.Map;


public class EONpc : MonoBehaviour
{
    public EntityDef def;
    public NpcDef npcDef;
    public NpcAnimator animator;
    public uint net_direction;
    private WalkAnim net_walkAnim;
    private AttackAnim net_attackAnim;

    public NpcState net_state;
    private bool init = default;

    //Accessors
    public Vector2Int net_position
    { get { return def.net_position; } }
   

    void Awake()
    {
        def = GetComponent<EntityDef>();
        animator = GetComponent<NpcAnimator>();
        net_state = NpcState.IDLE;
    }

    public void Init(int npcId)
    {

        if(npcId < DataFiles.Singleton.npcDataFile.Entries.Count)
        {
            NpcDataEntry entry = DataFiles.Singleton.npcDataFile.Entries[npcId];
            npcDef.name = entry.Name;
            npcDef.npcId = npcId;
            npcDef.npcType = entry.NpcType;
            npcDef.maxHealth = entry.MaxHealth;
            npcDef.gfxId = entry.GfxId;

            //init animator!
            animator.SetNpcDef(npcDef);

            init = true;
        }
        
    }

    public void NetSetDirection(uint _direction)
    {
        net_direction = _direction;
        // _animator.SetIdle(direction);
    }

    public void NetSetWalk(SetEntityWalk packet)
    {

        if ((NetworkTime.GetNetworkTime() - packet.timeStarted) > WalkAnim.timeStep)
        {
            Debug.Log("Ignoring SetEntityWalk packet because the animation has passed");
        }
        else
        {
            net_state = NpcState.WALK;
            //cState = net_cState;

            net_walkAnim = new WalkAnim(new Vector2Int(packet.fromX, packet.fromY), (int)packet.direction, packet.timeStarted);
            //net_walkAnim = net_walkAnim;
            //_animator.SetWalk((uint) walkAnim.direction, walkAnim.timeStarted);
        }
    }

    public void NetSetAttack(SetEntityAttack packet)
    {
        if ((NetworkTime.GetNetworkTime() - packet.timeStarted) >= AttackAnim.timeStep)
        {
            Debug.Log("Ignoring SetEntityAttack packet because the animation has passed");
        }
        else
        {
            net_state = NpcState.ATTACK;

            net_attackAnim = new AttackAnim(net_direction, packet.timeStarted);
        }
    }

    //TODO: Implement npc properties
    public void NetSetHealth(SetEntityHealth packet)
    {
        GameUI.Singleton.ShowHeadBar(this.gameObject, packet.health, packet.maxHealth, packet.deltaHp);
    }

    public void NetSetMap(uint mapId)
    {
        //Clear animations
        switch (net_state)
        {
            case NpcState.WALK:
                {
                    net_walkAnim.Clear();
                    net_state = NpcState.IDLE;
                    break;
                }
            case NpcState.ATTACK:
                {
                    net_attackAnim.Clear();
                    net_state = NpcState.IDLE;
                    break;
                }
        }
    }

    void UpdateAnimation()
    {
        //ResetAnimatorParams();

        switch (net_state)
        {
            case NpcState.IDLE:
                animator.SetIdle(net_direction);
                break;

            case NpcState.WALK:
                animator.SetWalk((uint) net_walkAnim.direction, net_walkAnim.timeStarted);
                break;

            case NpcState.ATTACK:
                animator.SetAttack((uint)net_attackAnim.direction, net_attackAnim.timeStarted);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();

        bool renderAtPos = true;

        if (net_walkAnim.valid)
        {
            float alpha = net_walkAnim.GetCappedAlpha();
            if (alpha < 1.0f)
            {
                Vector3 cellWorldPos = EOManager.eo_map.CellToWorld(net_walkAnim.from);
                Vector2Int posAfterWalk = EOMap.PositionAfterWalk(net_walkAnim.from, net_walkAnim.direction);

                Vector3 toCellWorldPos = EOManager.eo_map.CellToWorld(posAfterWalk);
                Vector3 deltaVec = toCellWorldPos - cellWorldPos;
                Vector3 walkOffset = deltaVec * alpha;
                //Debug.Log($"I'm walking! Alpha: {cappedAlpha}", this);
                transform.position = cellWorldPos + walkOffset + new Vector3(0, 0.2f, 0);

                renderAtPos = false;
            }
            else
            {
                net_state = NpcState.IDLE;
                net_walkAnim.Clear();
            }

        }
        else if (net_attackAnim.valid)
        {
            float alpha = net_attackAnim.GetCappedAlpha();

            if (alpha >= 1.0f)
            {
                net_state = NpcState.IDLE;
                net_attackAnim.Clear();
            }
        }

        if (renderAtPos)
        {
            Vector3 cellWorldPos = EOManager.eo_map.CellToWorld(net_position);
            //transform.position = cellWorldPos + new Vector3(0, 0.6f, 0);
            transform.position = cellWorldPos + new Vector3(0, 0.2f, 0);
        }
    }
}
