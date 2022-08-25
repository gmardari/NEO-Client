using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EO;

public class InvManager : MonoBehaviour
{
    public static InvManager Singleton;
    public Dictionary<uint, InventoryItem> invItems;
    public int[,] slots;
    public I_UIItem dragItem;

    public GameObject uiItemPrefab;
    [HideInInspector]
    public GameObject invContainer;

    public static Vector2Int SLOT_SIZE = new Vector2Int(14, 4);


    void Awake()
    {
        Singleton = this;
        invItems = new Dictionary<uint, InventoryItem>();
        slots = new int[SLOT_SIZE.x, SLOT_SIZE.y];

        for (int x = 0; x < SLOT_SIZE.x; x++)
        {
            for (int y = 0; y < SLOT_SIZE.y; y++)
            {
                slots[x, y] = -1;
            }
        }

        //invContainer = UIManager.Singleton.guiRoot.transform.Find("Inventory/Items").gameObject;
        //paperdollSlotsContainer = UIManager.Singleton.guiRoot.transform.Find("Paperdoll/Slots").gameObject;
    }


    public void OnSetItemDef(Vector2Int pos, uint itemId, uint quantity)
    {
        //var invContainer = UIManager.Singleton.guiRoot.transform.Find("Overlay/Inventory/Items").gameObject;

        if(invItems.TryGetValue(itemId, out InventoryItem item))
        {
            SetItemAmount(itemId, quantity);

            if(quantity > 0)
            {
                item.SetSlotPosition(pos);
            }
            
        }
        else
        {
            //Create UI object for inv item
            GameObject obj = Instantiate(uiItemPrefab);
            InvListener listener = obj.GetComponent<InvListener>();
            ItemDataEntry data = DataFiles.Singleton.itemDataFile.entries[(int)itemId];

            //Positioning automatically occurs in the constructor
            item = new InventoryItem(itemId, quantity, listener, pos, data); 
            obj.GetComponent<InvListener>().Init(item);
            listener.SetUIPosition();

            invItems.Add(itemId, item);
        }
    }

    public bool IsInBounds(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.y >= 0 && pos.x < SLOT_SIZE.x && pos.y < SLOT_SIZE.y)
            return true;

        return false;
    }

    public bool IsInBounds(Vector2Int pos, Vector2Int size)
    {
        if (pos.x >= 0 && pos.y >= 0 && (pos.x + size.x) <= SLOT_SIZE.x && (pos.y + size.y) <= SLOT_SIZE.y)
            return true;

        return false;
    }

    public bool ContainsItem(Vector2Int pos)
    {
        /*
        foreach(InventoryItem item in invItems.Values)
        {
            if(pos.x >=  item.pos.x && pos.y >= item.pos.y && pos.x < (item.pos.x + item.size.x) && pos.y < (item.pos.y + item.size.y))
            {
                return true;
            }
        }
        */

        if (slots[pos.x, pos.y] >= 0)
            return true;

        return false;
    }


    public bool ContainsItem(Vector2Int pos, Vector2Int size)
    {
        int posEndX = pos.x + size.x - 1;
        int posEndY = pos.y + size.y - 1;

        //Clamp the values
        posEndX = (posEndX >= SLOT_SIZE.x) ? (SLOT_SIZE.x - 1) : posEndX;
        posEndY = (posEndY >= SLOT_SIZE.y) ? (SLOT_SIZE.y - 1) : posEndY;

        for (int x = pos.x; x <= posEndX; x++)
        {
            for (int y = pos.y; y <= posEndY; y++)
            {
                if (slots[x, y] >= 0)
                    return true;
            }
        }

       

        return false;
    }

    //TODO: Implement
    public void SwapItems(int itemId1, int itemId2)
    {
        throw new System.Exception("Not implemented");
    }

    public void MoveItem(uint itemId, Vector2Int pos)
    {
        invItems[itemId].SetSlotPosition(pos);
    }

    //Assume we have that item
    public void SetItemAmount(uint itemId, uint quantity)
    {
        //Delete
        if(quantity == 0)
        {
            invItems[itemId].Quantity = quantity;

            invItems.Remove(itemId);
            
        }
        else
        {
            invItems[itemId].Quantity = quantity;
        }
    }

    public Vector2Int? GetFreeSpace(int itemId)
    {
        ItemDataEntry data = DataFiles.Singleton.itemDataFile.entries[itemId];
        
        for(int x = 0; x <= (SLOT_SIZE.x - data.sizeX); x++)
        {
            for(int y = 0; y <= (SLOT_SIZE.y - data.sizeY); y++)
            {
                if(!ContainsItem(new Vector2Int(x, y), new Vector2Int((int)data.sizeX, (int)data.sizeY)))
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return null;
    }

    public void RequestMove(InventoryItem item, Vector2Int pos)
    {
        RequestItemMove packet = new RequestItemMove(item.ItemId, (uint)pos.x, (uint)pos.y);
        EOManager.Singleton.SendPacket(packet);
    }

    public void RequestEquip(InventoryItem item, byte slotIndex)
    {
        RequestItemEquip p = new RequestItemEquip(item.ItemId, slotIndex, true);
        EOManager.Singleton.SendPacket(p);
    }

    public void RequestUnequip(PaperdollItem item, byte slotIndex)
    {
        RequestItemEquip p = new RequestItemEquip(item.ItemId, slotIndex, false);
        EOManager.Singleton.SendPacket(p);
    }

    public void OnItemDrag(I_UIItem newDragItem)
    {
        //End of drag
        //Reset old drag item
        if(newDragItem == null && dragItem != null)
        {
            //dragItem.uiObj.transform.SetParent(invContainer.transform);

            //Check if dragged to slot
            var mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var invPos = new Vector2(invContainer.transform.position.x, invContainer.transform.position.y);
            var delta = mousePos - invPos;
            //Invert the y axis
            delta.y = -delta.y;

            RectTransform r = invContainer.transform as RectTransform;

            if (delta.x >= 0 && delta.y >= 0 && delta.x <= r.rect.size.x && delta.y <= r.rect.size.y)
            {
                Vector2Int pos = new Vector2Int(((int) delta.x) / 25, ((int) delta.y) / 25);
                if(dragItem is InventoryItem dragInvItem)
                {
                    if (pos != dragInvItem.position && IsInBounds(pos, dragInvItem.size))
                    {
                        dragItem.UI.SetUIPosition();
                        RequestMove(dragInvItem, pos);
                    }
                    else
                        dragItem.UI.SetUIPosition();
                }
                else
                    dragItem.UI.SetUIPosition();

            }
            else
                dragItem.UI.SetUIPosition(); //Restore to originnal position

            //Debug.Log($"Inv container position: {invContainer.transform.position}");
            //Debug.Log("In rect: " + RectTransformUtility.PixelAdjustPoint);




        }

       

        if(newDragItem != null)
        {
            newDragItem.UI.transform.SetParent(UIManager.Singleton.guiBranch.transform);
        }

        dragItem = newDragItem;
    }

    public void OnItemDoubleClick(I_UIItem item)
    {
        
        if(item is InventoryItem invItem)
        {
            if (!ChestInvManager.Singleton.chest_opened)
            {
                if (InventoryItem.IsEquipment(invItem))
                {
                    //Equip
                    //RequestEquip(invItem);
                    int itemTypeId = ((int)invItem.Type);

                    //All item types between HAT and BOOTS have the same ID as the slotIndex
                    if (itemTypeId >= (int)ItemType.HAT && itemTypeId <= (int)ItemType.BOOTS)
                    {
                        int slotIndex = itemTypeId - ((int)ItemType.HAT);
                        RequestEquip(invItem, (byte)slotIndex);
                    }
                    else
                    {
                        //TODO: Finish
                        switch (invItem.Type)
                        {
                            case ItemType.RING:

                                break;
                            case ItemType.BRACELET:

                                break;
                            case ItemType.BRACER:

                                break;

                        }
                    }
                }
            }
            //Player inv item -> chest
            else
            {
                //TODO: Check for items that can't be traded/stored
                ReqChestItemGive packet = new ReqChestItemGive(invItem.ItemId, invItem.Quantity);
                EOManager.Singleton.SendPacket(packet);
            }
        }
        else if(item is PaperdollItem dollItem)
        {
            RequestUnequip(dollItem, (byte) dollItem.slot);
        }
        
    }

    private void Update()
    {
        //Move dragged item to cursor
        if(dragItem != null)
        {
            var rectTransform = dragItem.UI.transform as RectTransform;

            rectTransform.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y - Screen.height);
        }
    }
}
