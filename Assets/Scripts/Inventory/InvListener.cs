using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EO;

public class InvListener : MonoBehaviour
{
    public I_UIItem item;
    public bool isPaperdoll;
    public UIItemType type;

    private RectTransform rectTransform;
    

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = transform as RectTransform;
      
    }

    public void Init(I_UIItem item)
    {
        this.item = item;
        //this.isPaperdoll = item is PaperdollItem;

        if (item is InventoryItem)
        {
            type = UIItemType.PLAYER_INV;
        }
        else if (item is PaperdollItem)
        {
            type = UIItemType.PAPERDOLL;
        }
        else if (item is ChestItem)
        {
            type = UIItemType.CHEST_INV;
        }

        if (type == UIItemType.PLAYER_INV || type == UIItemType.PAPERDOLL)
        {
            ItemDataEntry data = DataFiles.Singleton.GetItemData((int)item.ItemId);

            uint gfxId = data.displayGfx;
            Image image = GetComponent<Image>();
            image.sprite = ResourceLibrary.Singleton.itemSprites[gfxId];

            //Resize image 
            //Vector3 spriteSize = image.sprite.bounds.size * 64f;
            Vector2 imageSize = new Vector2(data.sizeX * 25f, data.sizeY * 25f);

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageSize.y);
        }
        else if(type == UIItemType.CHEST_INV)
        {
            UpdateUI();
        }
    }

    public void SetUIPosition()
    {
        if (type == UIItemType.PLAYER_INV)
        {
            var invItem = item as InventoryItem;

            rectTransform.SetParent(InvManager.Singleton.invContainer.transform);
            rectTransform.anchoredPosition = new Vector2(invItem.position.x * 26f, -invItem.position.y * 27f);
        }
        else if (type == UIItemType.PAPERDOLL)
        {
            var slots = PaperdollManager.Singleton.slotsContainer;
            var paperdollItem = item as PaperdollItem;

            rectTransform.SetParent(slots.transform.GetChild((int)paperdollItem.slot));
            rectTransform.anchoredPosition = new Vector2(0, 0);
           
        }
       
    }
    
    public void UpdateUI()
    {
        if(type == UIItemType.CHEST_INV)
        {
            ItemDataEntry data = DataFiles.Singleton.GetItemData((int)item.ItemId);

            uint gfxId = data.displayGfx;
            ChestItem chestItem = (ChestItem)item;

            transform.Find("Name").GetComponent<Text>().text = chestItem.name;
            transform.Find("Quantity").GetComponent<Text>().text = "x" + chestItem.Quantity.ToString();

            Image image = transform.Find("FG").GetComponent<Image>();
            image.sprite = ResourceLibrary.Singleton.itemDropSprites[gfxId];
            image.SetNativeSize();
        }
    }

    public void OnPointerEnter(BaseEventData data)
    {

    }

    public void OnPointerClick(BaseEventData data)
    {
        if(data is PointerEventData pointerData)
        {
            if(!pointerData.dragging)
            {
                if(pointerData.button == PointerEventData.InputButton.Left)
                {
                    if (pointerData.clickCount == 2)
                    {
                        if (type == UIItemType.PLAYER_INV || type == UIItemType.PAPERDOLL)
                            InvManager.Singleton.OnItemDoubleClick(item);
                    }

                    if (type == UIItemType.CHEST_INV)
                        ChestInvManager.Singleton.OnItemClick((ChestItem)item);
                }
            }
        }

        
    }

    public void OnBeginDrag(BaseEventData data)
    {
        //Debug.Log("DRAG!");
        if(type == UIItemType.PLAYER_INV || type == UIItemType.PAPERDOLL)
            InvManager.Singleton.OnItemDrag(item);
    }

    public void OnEndDrag(BaseEventData data)
    {
        //Debug.Log("NO DRAG!");
        if (type == UIItemType.PLAYER_INV || type == UIItemType.PAPERDOLL)
            InvManager.Singleton.OnItemDrag(null);
    }

}
