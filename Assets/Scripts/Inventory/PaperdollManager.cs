using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EO;
using EO.Inventory;

public class PaperdollManager : MonoBehaviour
{
    public static PaperdollManager Singleton;

    public GameObject uiItemPrefab;

    [HideInInspector]
    public PaperdollItem[] items;
    [HideInInspector]
    public GameObject uiPaperdoll;
    [HideInInspector]
    public GameObject slotsContainer;
    [HideInInspector]
    public GameObject ui_properties;
    [HideInInspector]
    public bool isOpened;
    [HideInInspector]
    public GameObject owner;

    public const uint NUM_SLOTS = 15;

    
    void Awake()
    {
        Singleton = this;
        //slotsContainer = transform.Find("Slots").gameObject;
        items = new PaperdollItem[NUM_SLOTS];

    }

    public void ShowPaperdoll(GameObject playerObj)
    {
        var eochar = playerObj.GetComponent<EOCharacter>();
        var paperdoll = eochar.characterDef.doll;

        uiPaperdoll.SetActive(true);
        isOpened = true;
        owner = playerObj;

        Debug.Log("Showing paperdoll of player " + eochar.CharacterName);
        Debug.Log(paperdoll);

        if (paperdoll.armor > 0)
        {
            AddItem(PaperdollSlot.ARMOR, paperdoll.armor - 1);
        }
        if(paperdoll.weapon > 0)
        {
            AddItem(PaperdollSlot.WEAPON, paperdoll.weapon - 1);
        }
        if (paperdoll.boots > 0)
        {
            AddItem(PaperdollSlot.BOOTS, paperdoll.boots - 1);
        }

        var nameField = ui_properties.transform.Find("Name").gameObject;
        var nameText = nameField.GetComponent<Text>();
        nameText.text = eochar.characterDef.name;
    }

    public void HidePaperdoll()
    {
        uiPaperdoll.SetActive(false);
        isOpened = false;
        owner = null;

        for(int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                items[i].Destroy();
                items[i] = null;
            }
        }
    }

    public void OnPaperdollBtnClick()
    {
        if(isOpened)
        {
            HidePaperdoll();
        }
        else
        {
            ShowPaperdoll(EOManager.player);
        }
    }

    public void OnOkBtnClick()
    {
        HidePaperdoll();
    }

    public void AddItem(PaperdollSlot slot, uint itemId)
    {
        Transform parent = PaperdollManager.Singleton.slotsContainer.transform.GetChild((int)slot);
        //parent transform must be set for it to properly work
        GameObject obj = Instantiate(uiItemPrefab, parent);
        PaperdollItem item = new PaperdollItem(slot, itemId, obj);

        //Positioning automatically occurs in the constructor
        //item.UI.Init(item);
        //item.UI.SetUIPosition();

        int slotIndex = (int)slot;
        items[slotIndex] = item;
        Debug.Log($"Added item {item.Name} at slot {slot}");
    }

    public void RemoveItem(uint slotIndex)
    {
        var dollItem = items[slotIndex];

        if(dollItem != null)
            dollItem.Destroy();

        items[slotIndex] = null;
    }

    //Might not work? TODO: Check if works
    public void EditPaperdoll(GameObject playerObj, CharacterDef def, byte slotIndex, uint itemId)
    {
        if (slotIndex < 0 || slotIndex >= NUM_SLOTS)
        {
            Debug.LogWarning($"Can't edit paperdoll with invalid slotIndex: {slotIndex}");
            return;
        }
        //packet.itemId is shifted by +1 (so packet.itemId - 1 is the real item Id)
        //uint val = (packet.equipped) ? packet.itemId : 0;

        bool equipped = itemId != 0;
        PaperdollSlot slot = (PaperdollSlot) slotIndex;
        //Set paperdoll
        def.doll.Set(slot, itemId);

        //Make sure we are in game
        if (UIManager.Singleton.state != UIState.IN_GAME)
            return;
        
        //Set UI elements
        if(isOpened && owner == playerObj)
        {
            RemoveItem(slotIndex);

            if (equipped)
            {
                AddItem(slot, itemId - 1);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
