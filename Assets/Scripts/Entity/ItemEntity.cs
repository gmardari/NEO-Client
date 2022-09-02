using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EO
{
    public class ItemEntity : IEntity
    {
        private GameObject obj;
        private EntityType type;
        private ulong entityId;

        public uint itemId;
        public uint quantity;
        public string name;
        public ItemType itemType;

        public GameObject Obj { get => obj; set => obj = value; }
        public EntityType Type { get => type; set => type = value; }
        public ulong EntityId { get => entityId; set => entityId = value; }

        public ItemEntity(ulong entityId, uint itemId, uint quantity, GameObject obj)
        {
            this.entityId = entityId;
            this.obj = obj;
            this.type = EntityType.ITEM;
            this.itemId = itemId;
            this.quantity = quantity;

            ItemDataEntry entry = DataFiles.Singleton.itemDataFile.entries[(int)itemId];
            this.name = entry.name;
            this.itemType = (ItemType)entry.itemType;

            //Setup sprite
            if(itemType == ItemType.GOLD)
            {
                obj.GetComponent<SpriteRenderer>().sprite = ResourceLibrary.Singleton.GetGoldDropSprite(quantity);
            }
            else
            {
                obj.GetComponent<SpriteRenderer>().sprite = ResourceLibrary.Singleton.GetDropItemSprite(entry.displayGfx);
            }
        }

        public EntityDef GetDef()
        {
            return obj.GetComponent<EntityDef>();
        }
    }
}
