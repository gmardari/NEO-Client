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

//TODO: Add game object reference
public interface I_UIItem
{
    public uint ItemId { get; }
    public ItemType ItemType { get; }
    public InvListener UI { get; }

    public void Destroy();
}

//An item in a player inventory.
public class PlayerItem : I_UIItem
{
    public Vector2Int position;
    public Vector2Int size;
    private uint quantity;

    public uint ItemId { get; private set; }
    public uint Quantity 
    { 
        get { return quantity; }
        set 
        {
            quantity = value;

            if (quantity == 0 && ItemType != ItemType.GOLD)
                Destroy();
        }
    }

    public ItemType ItemType => (ItemType) itemData.itemType;
    public string Name => itemData.name;
    public InvListener UI { get; private set; }
    public GameObject Obj => UI.gameObject;

    private ItemDataEntry itemData;

    public PlayerItem(uint itemId, uint quantity, GameObject obj, Vector2Int pos)
    {
        this.ItemId = itemId;
        this.Quantity = quantity;
        this.UI = obj.GetComponent<InvListener>();
        itemData = DataFiles.Singleton.GetItemData(itemId);

        this.size = new Vector2Int((int) itemData.sizeX, (int) itemData.sizeY);
        //this.type = (ItemType) data.itemType;
        InitPosition(pos);
        Init();
        //this.name = data.name;
    }

    public void Init()
    {
        UI.Init(this);
        UI.SetUIPosition();
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
                InvManager.Singleton.slots[x, y] = (int) ItemId;
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
                InvManager.Singleton.slots[x, y] = (int) ItemId;
            }
        }

        position = newPos;
        UI.SetUIPosition();
    }
    public void Destroy()
    {
        if(UI != null)
        {
            GameObject.Destroy(UI.gameObject);
            UI = null;
        }
        InvManager.Singleton.OnItemDestroy(ItemId);
    }

    public static bool IsEquipment(PlayerItem item)
    {
        uint itemTypeInt = (uint) item.ItemType;

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
    private ItemDataEntry itemData;
    
    

    public uint ItemId { get { return itemId; } }
    public string Name => itemData.name;
    public ItemType ItemType => (ItemType)itemData.itemType;

    public InvListener UI { get; private set; }

    public PaperdollItem(PaperdollSlot slot, uint itemId, GameObject obj)
    {
        (this.slot, this.itemId, this.UI) = (slot, itemId, obj.GetComponent<InvListener>());
        itemData = DataFiles.Singleton.GetItemData(itemId);

        Init();
    }

    public void Init()
    {
        UI.Init(this);
        UI.SetUIPosition();
    }

    public void Destroy()
    {
        if(UI != null)
        {
            GameObject.Destroy(UI.gameObject);
            UI = null;
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
    public ItemType ItemType { get { return type; } }
    public InvListener UI { get { return ui; } }

    public ChestItem(uint itemId, uint quantity, uint slotIndex, InvListener ui)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.slotIndex = slotIndex;
        this.ui = ui;

        var entry = DataFiles.Singleton.GetItemData(itemId);
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