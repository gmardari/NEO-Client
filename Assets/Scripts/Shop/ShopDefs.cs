using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ShopBuyItem
{
    public uint itemId;
    public uint price;
}

public struct ShopSellItem
{
    public uint itemId;
    public uint price;
}

public struct ShopCraftItem
{
    public uint itemId;
    public CraftIngridient[] ingridients;
}

public struct CraftIngridient
{
    public uint itemId;
    public uint price;
}

public enum ShopState
{
    MAIN,
    BUY,
    SELL,
    CRAFT
}

public class Shop
{
    public string name;
    public ShopState state;
    public ShopBuyItem[] buyItems;
    public ShopSellItem[] sellItems;
    public ShopCraftItem[] craftItems;

    public uint numBuyItems;
    public uint numSellItems;
    public uint numCraftItems;

    private uint _buyCount;             //Current num of items added to buyItems
    private uint _sellCount;            //Current num of items added to sellItems
    private uint _craftCount;           //Current num of items added to craftItems

    public Shop(string name, uint numBuy, uint numSell, uint numCraft)
    {
        this.name = name;
        this.numBuyItems = numBuy;
        this.numSellItems = numSell;
        this.numCraftItems = numCraft;
        this.state = ShopState.MAIN;

        if(numBuy > 0)
        {
            buyItems = new ShopBuyItem[numBuy];
        }

        if (numSell > 0)
        {
            sellItems = new ShopSellItem[numSell];
        }

        if (numCraft > 0)
        {
            craftItems = new ShopCraftItem[numCraft];
        }
    }

    public void AddBuyItem(uint itemId, uint price)
    {
        if(_buyCount >= buyItems.Length)
            return;

        buyItems[_buyCount++] = new ShopBuyItem
        {
            itemId = itemId,
            price = price
        };

    }

    public void AddSellItem(uint itemId, uint price)
    {
        if (_sellCount >= sellItems.Length)
            return;

        sellItems[_sellCount++] = new ShopSellItem
        {
            itemId = itemId,
            price = price
        };

    }

    public void AddCraftItem(uint itemId, CraftIngridient[] ingridients)
    {
        if (_craftCount >= craftItems.Length)
            return;

        craftItems[_craftCount++] = new ShopCraftItem
        {
            itemId = itemId,
            ingridients = ingridients
        };

    }
}
