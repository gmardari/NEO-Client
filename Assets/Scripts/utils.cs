using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class utils 
{
    public static Vector2 ScreenToIMGUISpace(Vector2 pos)
    {
        return ScreenToIMGUISpace(pos.x, pos.y);
    }

    public static Vector2 ScreenToIMGUISpace(float x, float y)
    {
        return new Vector2(x, Screen.height - y);
    }

    public static ulong GetEntityId(GameObject entObj)
    {
        EntityDef def = entObj.GetComponent<EntityDef>();

        if (def == null)
            throw new Exception("The given game object is not an entity");

        return def.entityId;
    }

    public static Vector2Int GetEntityNetPos(GameObject entObj)
    {
        EntityDef def = entObj.GetComponent<EntityDef>();

        if (def == null)
            throw new Exception("The given game object is not an entity");

        return def.net_position;
    }
}

public static class NetworkTime
{
    public static long timeSyncStart = default;
    public static long timeDelta = -1;
    public static bool synced;

    public static long GetLocalTime()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    public static long GetNetworkTime()
    {
        if (!synced)
            return -1;

        return GetLocalTime() + timeDelta;
    }

}

public struct request_intervals
{
    public float dirInterval;
    public float walkInterval;

    public void AddTime(float delta)
    {
        dirInterval += delta;
        walkInterval += delta;
    }
}

public class MoveInput
{
    public int direction;
    public float duration;
    private const float bias = 0.25f;
    public bool valid;
    //public float lastInputSwitch

    public MoveInput(int direction)
    {
        this.direction = direction;
        this.duration = 0.0f;
        this.valid = IsValid();
    }


    public bool IsValid()
    {
        if (direction >= 0 && direction <= 3)
            return true;

        return false;
    }

    public int AxisToDirection(float hor, float vert)
    {
        int dir = -1;

        if (vert > 0.0f)
            dir = 2;
        else if (vert < 0.0f)
            dir = 0;
        else if (hor > 0.0f)
            dir = 1;
        else if (hor < 0.0f)
            dir = 3;


        return dir;
    }

    public void Set(int direction, bool resetDuration)
    {
        this.direction = direction;
        if (resetDuration)
            duration = 0.0f;

        this.valid = IsValid();
    }

    public bool Update(float hor, float vert, float deltaTime)
    {
        int newInput = AxisToDirection(hor, vert);

        if (valid && direction == newInput)
        {
            this.duration += deltaTime;
        }
        else
        {
            Set(newInput, false);
        }

        if (duration >= bias)
            return true;

        return false;
    }

    public void Clear()
    {
        this.direction = -1;
        this.duration = 0.0f;
        this.valid = IsValid();
    }

    public void ResetTimer()
    {
        duration = 0.0f;
    }
}

public class PlayerInputManager
{
    //How long it takes to go from IDLE to WALKING by holding down an input
    public float holdInterval;
    public float holdTimer;

    public int lastDirection;
    public int direction;

    public bool isAttackKey;

    public PlayerInputManager(float holdInterval)
    {
        this.lastDirection = -1;
        this.direction = -1;
        this.holdInterval = holdInterval;
    }

    public bool Update(float hor, float vert, bool attackKey, float deltaTime)
    {
        lastDirection = direction;
        direction = AxisToDirection(hor, vert);
        isAttackKey = attackKey;

        if (direction >= 0 && (direction == lastDirection))
        {
            holdTimer += deltaTime;
        }
        else
            holdTimer = 0.0f;

        return holdTimer >= holdInterval;
    }

    public int AxisToDirection(float hor, float vert)
    {
        int dir = -1;

        if (vert > 0.0f)
            dir = 2;
        else if (vert < 0.0f)
            dir = 0;
        else if (hor > 0.0f)
            dir = 1;
        else if (hor < 0.0f)
            dir = 3;


        return dir;
    }

    public bool HasInput()
    {
        return direction >= 0;
    }

    public void Reset()
    {
        
    }
}