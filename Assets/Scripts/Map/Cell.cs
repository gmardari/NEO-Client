using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EO;
public class Cell 
{
    public Vector2Int Pos { get; private set; }

    //-1 is nothing (black)
    public int groundLayerId;
    public int specialLayerId;
    public LinkedList<IEntity> entities;
    private List<Item> items;

    public Cell(Vector2Int pos)
    {
        this.Pos = pos;
        groundLayerId = -1;
        specialLayerId = -1;
        this.entities = new LinkedList<IEntity>();
        this.items = new List<Item>();
    }

    public Cell(Vector2Int pos, int groundLayerId, int specialLayerId)
    {
        this.Pos = pos;
        this.groundLayerId = groundLayerId;
        this.specialLayerId = specialLayerId;
        this.entities = new LinkedList<IEntity>();
        this.items = new List<Item>();
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

    public Item AddItem(Item item)
    {
        items.Add(item);

        return item;
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public Item PopItem()
    {
        if (items.Count == 0)
            return null;

        return items[items.Count - 1];
    }
}
