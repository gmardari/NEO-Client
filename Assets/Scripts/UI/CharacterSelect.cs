using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterSelect : MonoBehaviour
{
    public GameObject newCharPrefab;

    
    void Awake()
    {
        
    }


    public void Init()
    {
        for (int i = 0; i < EOManager.cs_index; i++)
        {
            SetCharPanelEnabled(i, true);

        }

        //Hide login/logout buttons for non-initialized character slots
        for (int i = 2; i >= EOManager.cs_index; i--)
        {
            SetCharButtonsVisible(i, false);

        }

        AudioManager.Singleton.Play(4);
    }

    public void SetCharPanelEnabled(int index, bool enabled)
    {
        var panel = transform.Find("CharacterPanel" + (index + 1));
        var login = panel.Find("Login");
        var delete = panel.Find("Delete");

        if(enabled)
        {
            panel.Find("Name").GetComponent<Text>().text = EOManager.cs_defs[index].name;
            //Set character preview
            panel.Find("CharPrevContainer/CharacterPreview").GetComponent<CharacterPreview>().SetCharacterDef(EOManager.cs_defs[index]);
        }
        else
        {
            panel.Find("Name").GetComponent<Text>().text = "";
            //Set character preview
            panel.Find("CharPrevContainer/CharacterPreview").GetComponent<CharacterPreview>().SetCharacterDef(null);

            //login.GetComponent<Button>().onClick.RemoveAllListeners();
            //delete.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        SetCharButtonsVisible(index, enabled);
    }

    public void SetCharButtonsVisible(int index, bool visible)
    {
        var panel = transform.Find("CharacterPanel" + (index + 1));
        var login = panel.Find("Login");
        var delete = panel.Find("Delete");

        login.gameObject.SetActive(visible);
        delete.gameObject.SetActive(visible);
    }

    public void OnCharLoginClick(int b)
    {
        if(!EOManager.isEnteringWorld)
        {
            CS_CharacterDef def = EOManager.cs_defs[b];

            Debug.Log($"Entering world with char {def.name}");
            EOManager.Singleton.EnterWorld(b);
        }
    }

    public void OnCharDeleteClick(int b)
    {
        AudioManager.Singleton.Play(1);
    }

    public void OnCreateBtnClick()
    {
        var root = UIManager.Singleton.guiRoot;

        //Not open!
        if (!root.transform.Find(newCharPrefab.name))
        {
            GameObject newCharUi = Instantiate(newCharPrefab, root.transform);
        }

        AudioManager.Singleton.Play(1);
    }

    public void OnCharacterDefAdd(int index)
    {
        //int index = EOManager.cs_index - 1;
        SetCharPanelEnabled(index, true);
        //SetCharButtonsVisible(index, true);
    }

    public void OnPasswordBtnClick()
    {
        AudioManager.Singleton.Play(1);
    }

    public void OnExit()
    {
        AudioManager.Singleton.Play(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
