using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EO;

public class ChestInvManager : MonoBehaviour
{
    public static ChestInvManager Singleton;

    public GameObject entryPrefab;
    public GameObject buyEntryPrefab;
    public GameObject sellEntryPrefab;
    public GameObject craftEntryPrefab;
    public GameObject shopItemEntryPrefab;

    [HideInInspector]
    public GameObject invDisplay;
    [HideInInspector]
    public List<ChestItem> items;

    [HideInInspector] 
    public ulong chestOwnerId;
    [HideInInspector]
    public bool chest_opened;
    [HideInInspector]
    public Shop shop;
    [HideInInspector]
    public bool shop_opened;

    public bool IsDisplayed { get { return invDisplay.activeInHierarchy; } }

    void Awake()
    {
        Singleton = this;
        items = new List<ChestItem>();
    }

    public void RegisterEvents()
    {
        var footer = invDisplay.transform.Find("Footer");

        footer.Find("Close").GetComponent<Button>().onClick.AddListener(OnCloseBtnClick);
        footer.Find("Back").GetComponent<Button>().onClick.AddListener(OnBackBtnClick);
    }

    public void SetShop(string title, uint numBuy, uint numSell, uint numCraft)
    {
        shop = new Shop(title, numBuy, numSell, numCraft);
        shop.AddBuyItem(0, 100);
        shop.AddBuyItem(1, 350);
        shop.AddBuyItem(2, 5050);

        shop.AddSellItem(0, 20);
        shop.AddSellItem(1, 40);
        shop.AddSellItem(2, 80);
    }

    public void ShowShopDisplay()
    {
        if (shop == null)
            return;
    
        //Initialize the UI
        ClearContents();

        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");
        var titleObj = invDisplay.transform.Find("Title").gameObject;
        titleObj.GetComponent<Text>().text = shop.name;

        uint numBuy = shop.numBuyItems;
        uint numSell = shop.numSellItems;
        uint numCraft = shop.numCraftItems;
        
        if(numBuy > 0)
        {
            var buyEntry = Instantiate(buyEntryPrefab, content);
            buyEntry.transform.Find("Desc").GetComponent<Text>().text = $"{numBuy} items to buy";
        }

        if (numSell > 0)
        {
            var sellEntry = Instantiate(sellEntryPrefab, content);
            sellEntry.transform.Find("Desc").GetComponent<Text>().text = $"{numSell} items to buy";
        }

        if (numCraft > 0)
        {
            var craftEntry = Instantiate(craftEntryPrefab, content);
            craftEntry.transform.Find("Desc").GetComponent<Text>().text = $"{numCraft} items to craft";
        }


        if (!invDisplay.activeInHierarchy)
            invDisplay.SetActive(true);

        SetButtons(false, true);
        shop_opened = true;
    }

    public void SetButtons(bool isBack, bool isClose)
    {
        var footer = invDisplay.transform.Find("Footer");
        var back = footer.Find("Back");
        var close = footer.Find("Close");

        back.gameObject.SetActive(isBack);
        close.gameObject.SetActive(isClose);
    }

    public void ClearContents()
    {
        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");

        foreach(Transform item in content)
        {
            Destroy(item.gameObject);
        }
    }

    public void ShowShopBuy()
    {
        if (shop == null)
            return;

        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");
        ClearContents();

        if(shop.numBuyItems > 0)
        {
            for(int i = 0; i < shop.numBuyItems; i++)
            {
                ShopBuyItem item = shop.buyItems[i];
                var data = DataFiles.Singleton.GetItemData(i);
                GameObject entry = Instantiate(shopItemEntryPrefab, content);
                entry.transform.Find("Name").GetComponent<Text>().text = data.name;
                entry.transform.Find("Price").GetComponent<Text>().text = $"Price: {item.price}";
                var fgImage = entry.transform.Find("FG").GetComponent<Image>();

                fgImage.sprite = ResourceLibrary.Singleton.itemDropSprites[data.displayGfx];
                fgImage.SetNativeSize();
            }
        }

        SetButtons(true, true);
    }

    public void ShowShopSell()
    {
        if (shop == null)
            return;

        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");
        ClearContents();

        if (shop.numSellItems > 0)
        {
            for (int i = 0; i < shop.numSellItems; i++)
            {
                ShopSellItem item = shop.sellItems[i];
                var data = DataFiles.Singleton.GetItemData(i);
                GameObject entry = Instantiate(shopItemEntryPrefab, content);
                entry.transform.Find("Name").GetComponent<Text>().text = data.name;
                entry.transform.Find("Price").GetComponent<Text>().text = $"Price: {item.price}";
                var fgImage = entry.transform.Find("FG").GetComponent<Image>();

                fgImage.sprite = ResourceLibrary.Singleton.itemDropSprites[data.displayGfx];
                fgImage.SetNativeSize();
            }
        }
        SetButtons(true, true);
    }

    public void ShowShopCraft()
    {
        if (shop == null)
            return;

        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");
        ClearContents();

        if (shop.numCraftItems > 0)
        {
            for (int i = 0; i < shop.numCraftItems; i++)
            {
                ShopCraftItem item = shop.craftItems[i];
                var data = DataFiles.Singleton.GetItemData(i);
                GameObject entry = Instantiate(shopItemEntryPrefab, content);
                entry.transform.Find("Name").GetComponent<Text>().text = data.name;
                entry.transform.Find("Price").GetComponent<Text>().text = $"Ingridients: {item.ingridients.Length}";
                var fgImage = entry.transform.Find("FG").GetComponent<Image>();

                fgImage.sprite = ResourceLibrary.Singleton.itemDropSprites[data.displayGfx];
                fgImage.SetNativeSize();
            }
        }
        SetButtons(true, true);
    }

    public void OnEntryClick(uint slotIndex)
    {
        Debug.Log($"Clicked {slotIndex}");

        switch(shop.state)
        {
            case ShopState.MAIN:

                switch (slotIndex)
                {
                    case 0:
                        shop.state = ShopState.BUY;
                        ShowShopBuy();
                        break;
                    case 1:
                        shop.state = ShopState.SELL;
                        ShowShopSell();
                        break;

                    case 2:
                        shop.state = ShopState.CRAFT;
                        ShowShopCraft();
                        break;
                }

                break;

            case ShopState.BUY:

                Debug.Log($"Buying item {shop.buyItems[slotIndex].itemId}");
                break;

            case ShopState.SELL:

                Debug.Log($"Selling item {shop.sellItems[slotIndex].itemId}");
                break;

            case ShopState.CRAFT:

                Debug.Log($"Crafting item {shop.craftItems[slotIndex].itemId}");
                break;
        }
        
    }

    public void NetOpenChest(ulong entityId)
    {
        if(chest_opened)
        {
            //Do something
            ResetChest();
        }

        chestOwnerId = entityId;
        chest_opened = true;


    }

    public void NetDisplayChest()
    {
        DisplayChest();
    }

    public void OnItemClick(ChestItem item)
    {
        ReqChestItemTake packet = new ReqChestItemTake(item.SlotIndex, 1);
        EOManager.Singleton.SendPacket(packet);
    }

    public void NetSetItem(SetChestInvItem packet)
    {
        if (packet.quantity > 0)
        {
            if(packet.slotIndex >= items.Count)
                AddItem(packet.itemId, packet.quantity, packet.slotIndex);
            else
            {
                ChestItem item = items[(int)packet.slotIndex];

                if(item.ItemId == packet.itemId)
                {
                    item.Quantity = packet.quantity;
                    item.UI.UpdateUI();
                }
                //Set to a different item. Reset
                else
                {

                }
            }
            
        }
        else
            RemoveItem(packet.slotIndex);
    }

    public void AddItem(uint itemId, uint quantity, uint slotIndex)
    {
        Transform content = invDisplay.transform.Find("Scroll View/Viewport/Content");
        
        GameObject ui_entry = Instantiate(entryPrefab, content);
        InvListener listener = ui_entry.GetComponent<InvListener>();
        ChestItem item = new ChestItem(itemId, quantity, slotIndex, listener);
        listener.Init(item);
  
       
        //ui_entry.transform.Find("FG").GetComponent<Image>().sprite = ResourceLibrary.Singleton.itemDropSprites[gfxId];
        //ui_entry.transform.Find("FG").GetComponent<Image>().SetNativeSize();

        items.Add(item);

    }

    public void RemoveItem(uint slotIndex)
    {
        Debug.Log("Remove item at " + slotIndex);
        items[(int)slotIndex].Quantity = 0;

        items.RemoveAt((int)slotIndex);

        //Shift new indices!
        for (uint i = 0; i < items.Count; i++)
        {
            items[(int)i].SlotIndex = i;
        }
    }


    public void DisplayChest()
    {
        invDisplay.SetActive(true);
    }

    public void ResetChest()
    {
        invDisplay.SetActive(false);
        //Clear items

        foreach(ChestItem item in items)
        {
            item.Destroy();
        }
        items.Clear();
        chest_opened = false;

    }

    public void CloseChest()
    {
        ResetChest();
       

        //Send over the network
        ReqChestClose packet = new ReqChestClose();
        EOManager.Singleton.SendPacket(packet);
    }

    public void CloseShop()
    {
        ClearContents();
        invDisplay.SetActive(false);

        shop = null;
        shop_opened = false;
    }

    public void OnCloseBtnClick()
    {
        if(chest_opened)
            CloseChest();

        if (shop_opened)
            CloseShop();
    }

    public void OnBackBtnClick()
    {
        if(shop_opened && shop != null)
        {
            if(shop.state != ShopState.MAIN)
            {
                shop.state = ShopState.MAIN;
                ShowShopDisplay();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
