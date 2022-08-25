using System;
using System.Collections.Generic;
using UnityEngine;
using EO;
using EO.Inventory;

public struct IntRange
{
    public int min;
    public int max;

    public IntRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

}

public struct LongRange
{
    public long min;
    public long max;

    public LongRange(long min, long max)
    {
        this.min = min;
        this.max = max;
    }
}

public interface IEntity
{
    public ulong EntityId { get; set; }
    public GameObject Obj { get; set; }
    public EntityType Type { get; set; }

    public EntityDef GetDef();
}

public class ItemEntity : IEntity
{
    private GameObject obj;
    private EntityType type;
    private ulong entityId;

    public int itemId;
    public string name;
    public ItemType itemType;

    public GameObject Obj { get => obj; set => obj = value; }
    public EntityType Type { get => type; set => type = value; }
    public ulong EntityId { get => entityId; set => entityId = value; }    

    public ItemEntity(ulong entityId, int itemId, GameObject obj)
    {
        this.entityId = entityId;
        this.obj = obj;
        this.type = EntityType.ITEM;
        this.itemId = itemId;

        ItemDataEntry entry = DataFiles.Singleton.itemDataFile.entries[itemId];
        this.name = entry.name;
        this.itemType = (ItemType) entry.itemType;
    }

    public EntityDef GetDef()
    {
        return obj.GetComponent<EntityDef>();
    }
}

public enum ItemType : uint
{
    GOLD,
    STATIC,
    POTION,
    HAT,
    ARMOR,
    WEAPON,
    NECKLACE,
    BACK,
    GLOVES,
    BELT,
    CHARM,
    BOOTS,
    RING,
    BRACELET,
    BRACER
}

public enum EntityType : uint
{
    PLAYER,
    NPC,
    ITEM,
    WARP,
    CHEST,
    NPC_SPAWNER,
    ITEM_SPAWNER
}

public enum NpcType
{
    NONE,
    MOB_PASSIVE,
    MOB_AGGRESSIVE
}


public enum EntityProperty : uint
{
    HEALTH, //ulong
    MANA,   //ulong
    ENERGY, //ulong
    NAME,   //variable string
    CLASS   //variable string
}


public class CharacterDef
{
    public string name;
    public byte gender;
    public byte race;
    public byte skinColour;
    public byte hairStyle;
    public byte hairColour;
    public Paperdoll doll;

    public CharacterDef() { }

    public CharacterDef(SetCharacterDef p)
    {
        (name, gender, race, skinColour, hairStyle, hairColour) = (p.def.name, p.def.gender, p.def.race, 
            p.def.skinColour, p.def.hairStyle, p.def.hairColour);
        doll = p.def.doll;
        /*
        doll = new Paperdoll
        {
            hat = p.def.hat,
            armor = p.def.armor,
            boots = p.def.shoes,
            back = p.def.back,
            weapon = p.def.weapon
        };
        */
    }

}

public enum CharacterState
{
    IDLE,
    WALK,
    ATTACK,
    SPELLCAST,
    DYING
}

public struct NpcDef
{
    public string name;
    public int npcId;
    public long maxHealth;
    public int npcType;
    public int gfxId;

    public NpcDef(int _npcId, NpcDataEntry entry)
    {
        name = entry.Name;
        npcId = _npcId;
        maxHealth = entry.MaxHealth;
        npcType = entry.NpcType;
        gfxId = entry.GfxId;
    }
}

public enum NpcState
{
    IDLE,
    WALK,
    ATTACK,
    DYING
}


public struct Orientation
{
    public int mapId;
    public Vector2Int pos;
    public int direction;

    public Orientation(int mapId, Vector2Int pos, int direction)
    {
        this.mapId = mapId;
        this.pos = pos;
        this.direction = direction;
    }
}

public struct WalkAnim
{
    public Vector2Int from;
    public int direction;
    public bool valid;
    //The time needed for one full walk animation
    public const long timeStep = 750;
    public long timeStarted;

    public WalkAnim(Vector2Int from, int direction, long timeStarted)
    {
        this.from = from;
        this.direction = direction;
        this.valid = true;
        //TODO: Change
        //this.timeStarted = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        this.timeStarted = timeStarted;
    }

    public float GetAlpha()
    {
        //TODO: Fix
        //float timeDelta = NetworkManager.Singleton.ServerTime.TimeAsFloat - this.timeStarted;
        float timeDelta = NetworkTime.GetNetworkTime() - timeStarted;
        return timeDelta / timeStep;
    }

    public float GetCappedAlpha()
    {
        return Mathf.Min(GetAlpha(), 1.0f);
    }

    public void Clear()
    {
        valid = false;
    }

    public override string ToString()
    {
        return $"{from}, {direction}, {timeStarted}, {valid}";
    }
}

public struct AttackAnim
{
    public uint direction;
    public bool valid;
    public long timeStarted;

    public const long timeStep = 500;

    public AttackAnim(uint direction, long timeStarted)
    {
        this.direction = direction;
        this.valid = true;
        this.timeStarted = timeStarted;
    }

    public float GetAlpha()
    {
        long timeDelta = NetworkTime.GetNetworkTime() - timeStarted;
        return (timeDelta / timeStep);
    }

    public float GetCappedAlpha()
    {
        return Math.Min(GetAlpha(), 1.0f);
    }

    public bool TimeExpired()
    {
        if ((NetworkTime.GetNetworkTime() - timeStarted) >= timeStep)
            return true;

        return false;
    }

    public void Clear()
    {
        valid = false;
    }

    public override string ToString()
    {
        return $"Dir: {direction}, Time: {timeStarted}, Valid: {valid}";

    }
}

public enum NetworkSessionState
{
    NO_CONNECTION,
    PINGING,
    ACCEPTED,
    AUTH,
    IN_GAME
}

public struct NetworkSession
{
    public string username;
    public string char_name;
    public uint char_id;

    public NetworkSessionState state;
}

public enum UIState
{
    MAIN_MENU,
    LOGIN,
    CHARACTER_SELECT,
    ACCOUNT_CREATION,
    CREDITS,
    IN_GAME
}



public enum CellType
{
    NONE,
    WALL
}

public enum ACCOUNT_CREATE_RESP : byte
{
    SUCCESS,
    USER_INVALID,
    USER_TAKEN,
    PASS_INVALID,
    REALNAME_INVALID,
    LOCATION_INVALID,
    EMAIL_INVALID,
    SERVER_ERROR
}

public enum CHARACTER_CREATE_RESP : byte
{
    SUCCESS,
    NAME_TAKEN,
    CHARS_LIMIT_REACHED,
    GENDER_INVALID,
    HAIRSTYLE_INVALID,
    HAIRCOLOUR_INVALID,
    SKINCOLOUR_INVALID,
    SERVER_ERROR
}

