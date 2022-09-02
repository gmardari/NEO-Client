using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EO;
using EO.Inventory;

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

public interface I_UIItem
{
    public uint ItemId { get; }
    public ItemType Type { get; }
    public InvListener UI { get; }

    public void Destroy();
}

public class InventoryItem : I_UIItem
{
    private uint itemId;
    private ItemType type;
    private InvListener ui;

    private uint quantity;
    public Vector2Int position;
    public Vector2Int size;

    public string name;

    public uint ItemId { get { return itemId; } }
    public uint Quantity { get { return quantity; } 
        set
        {
            quantity = value;

            if (quantity == 0 && type != ItemType.GOLD)
                Destroy();
        }
    }
    public ItemType Type { get { return type; } }
    public InvListener UI { get { return ui; } }

    public InventoryItem(uint itemId, uint quantity, InvListener ui, Vector2Int pos, ItemDataEntry data)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.ui = ui;
        this.size = new Vector2Int((int)data.sizeX, (int)data.sizeY);
        this.type = (ItemType) data.itemType;
        InitPosition(pos);

        this.name = data.name;
    }

    /*
    //Sets UI Position to current position
    public void SetUIPosition()
    {
        RectTransform rectTransform = uiObj.transform as RectTransform;
        rectTransform.anchoredPosition = new Vector2(position.x * 26f, -position.y * 27f);
    }
    */

    private void InitPosition(Vector2Int pos)
    {
        int posEndX = pos.x + size.x - 1;
        int posEndY = pos.y + size.y - 1;

        
        for (int x = pos.x; x <= posEndX; x++)
        {
            for (int y = pos.y; y <= posEndY; y++)
            {
                InvManager.Singleton.slots[x, y] = (int) itemId;
            }
        }

        position = pos;
    }

    public void SetSlotPosition(Vector2Int newPos)
    {
        //Rempve from old position
        int posEndX = position.x + size.x - 1;
        int posEndY = position.y + size.y - 1;


        for (int x = position.x; x <= posEndX; x++)
        {
            for (int y = position.y; y <= posEndY; y++)
            {
                InvManager.Singleton.slots[x, y] = -1;
            }
        }


        posEndX = newPos.x + size.x - 1;
        posEndY = newPos.y + size.y - 1;


        for (int x = newPos.x; x <= posEndX; x++)
        {
            for (int y = newPos.y; y <= posEndY; y++)
            {
                InvManager.Singleton.slots[x, y] = (int) itemId;
            }
        }

        position = newPos;
        ui.SetUIPosition();
    }
    public void Destroy()
    {
        if(ui != null)
        {
            GameObject.Destroy(UI.gameObject);
            ui = null;
        }
    }

    public static bool IsEquipment(InventoryItem item)
    {
        uint itemTypeInt = (uint) item.type;

        if (itemTypeInt >= 3 && itemTypeInt <= 14)
            return true;

        return false;
    }


}


public class PaperdollItem : I_UIItem
{
    public PaperdollSlot slot;
    private uint itemId;
    private ItemType type;
    private InvListener ui;
    
    public string name;

    public uint ItemId { get { return itemId; } }

    public ItemType Type { get { return type; } }

    public InvListener UI { get { return ui; } }

    public PaperdollItem(PaperdollSlot slot, uint itemId, InvListener ui, ItemDataEntry data)
    {
        (this.slot, this.itemId, this.ui, this.name) = (slot, itemId, ui, data.name);
        this.type = (ItemType) data.itemType;
    }

    public void Destroy()
    {
        if(this.ui != null)
        {
            GameObject.Destroy(UI.gameObject);
            ui = null;
        }
           
    }
}

public class ChestItem : I_UIItem
{
    private uint itemId;
    private uint quantity;
    private uint slotIndex;
    private ItemType type;
    private InvListener ui;
   
    public string name;

    public uint ItemId { get { return itemId; } }
    public uint Quantity { get { return quantity; } 
        set 
        {   
            quantity = value; 
            if(value == 0)
                Destroy(); 
        } }

    public uint SlotIndex { get { return slotIndex; } set { slotIndex = value; } }
    public ItemType Type { get { return type; } }
    public InvListener UI { get { return ui; } }

    public ChestItem(uint itemId, uint quantity, uint slotIndex, InvListener ui)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.slotIndex = slotIndex;
        this.ui = ui;

        var entry = DataFiles.Singleton.GetItemData((int)itemId);
        this.type = (ItemType) entry.itemType;
        this.name = entry.name;
    }

    public void Destroy()
    {
        if(this.ui != null)
        {
            GameObject.Destroy(ui.gameObject);
            ui = null;
        }
      
    }
}

public enum UIItemType
{
    PLAYER_INV,
    PAPERDOLL,
    CHEST_INV
}