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

            //int b = i;
            login.GetComponent<Button>().onClick.AddListener(() => OnCharLoginClick(index));
            delete.GetComponent<Button>().onClick.AddListener(() => OnCharDeleteClick(index));
        }
        else
        {
            panel.Find("Name").GetComponent<Text>().text = "";
            //Set character preview
            panel.Find("CharPrevContainer/CharacterPreview").GetComponent<CharacterPreview>().SetCharacterDef(null);

            
            login.GetComponent<Button>().onClick.RemoveAllListeners();
            delete.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void SetCharButtonsVisible(int index, bool visible)
    {
        var panel = transform.Find("CharacterPanel" + (index + 1));
        var login = panel.Find("Login");
        var delete = panel.Find("Delete");

        login.gameObject.SetActive(visible);
        delete.gameObject.SetActive(visible);
    }

    private void OnCharLoginClick(int b)
    {
        UIManager.Singleton.OnCharacterLoginClick(b);
    }

    private void OnCharDeleteClick(int b)
    {

    }

    public void OnCreateBtnClick()
    {
        var root = UIManager.Singleton.guiRoot;

        //Not open!
        if (!root.transform.Find(newCharPrefab.name))
        {
            GameObject newCharUi = Instantiate(newCharPrefab, root.transform);
        }
    }

    public void OnCharacterDefAdd(CharacterDef def)
    {
        int index = EOManager.cs_index - 1;
        SetCharPanelEnabled(index, true);
        SetCharButtonsVisible(index, true);
    }

    public void OnPasswordBtnClick()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
