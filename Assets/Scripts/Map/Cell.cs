using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EO;
public class Cell 
{
    //-1 is nothing (black)
    public int groundLayerId;
    public int specialLayerId;
    public LinkedList<IEntity> entities;

    public Cell()
    {
        groundLayerId = -1;
        specialLayerId = -1;
        this.entities = new LinkedList<IEntity>();
    }

    public Cell(int groundLayerId, int specialLayerId)
    {
        this.groundLayerId = groundLayerId;
        this.specialLayerId = specialLayerId;
        this.entities = new LinkedList<IEntity>();
    }

    public bool IsWall()
    {
        return (specialLayerId == (int) MapSpecialIndex.WALL || groundLayerId == -1);
    }

    public void AddEntity(IEntity entity)
    {
        entities.AddLast(entity);
    }

    public bool RemoveEntity(IEntity entity)
    {
        return entities.Remove(entity);
    }

    public bool RemoveEntityFromId(ulong entityId)
    {
        for(var node = entities.First; node != null; node = node.Next)
        {
            if(node.Value.EntityId == entityId)
            {
                entities.Remove(node);
                return true;
            }
        }

        return false;
    }

    public bool HasEntity(IEntity entity)
    {
        return entities.Contains(entity);
    }

    public IEntity GetEntityOfType<T>()
    {
        foreach (IEntity entity in entities)
        {
            if (entity.GetType().Equals(typeof(T)))
            {
                return entity;
            }
        }

        return null;
    }

    public ItemEntity PopItem()
    {
        foreach(ItemEntity item in entities)
        {
            return item;
        }

        return null;
    }
}
