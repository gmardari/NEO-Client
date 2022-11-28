using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameUI : MonoBehaviour
{
    public static GameUI Singleton;
    public static UIStats uiStats;
    public GameObject headBarPrefab;
    public GameObject leftButtons;
    public GameObject rightButtons;
    public GameObject display;

    public Dictionary<ulong, HeadbarInstance> headBars;
    public DisplayState state;

    public enum DisplayState
    {
        INVENTORY,
        ACTIVE_SKILLS,
        PASSIVE_SKILLS,
        CHAT,
        STATS,
        PLAYERS,
        PARTY,
        KEYBINDS,
        SETTINGS,
        HELP
    }

    void Awake()
    {
        Singleton = this;
        headBars = new Dictionary<ulong, HeadbarInstance>();
        uiStats = GetComponent<UIStats>();

        foreach(Transform t in leftButtons.transform)
        {
            t.GetComponent<Button>().onClick.AddListener(() => AudioManager.Singleton.Play(1));
        }

        foreach (Transform t in rightButtons.transform)
        {
            t.GetComponent<Button>().onClick.AddListener(() => AudioManager.Singleton.Play(1));
        }

        foreach (Transform r in display.transform)
        {
            foreach(Transform t in r)
            {
                Button btn = t.GetComponent<Button>();

                if (btn != null)
                    btn.onClick.AddListener(() => AudioManager.Singleton.Play(1));
            }
        }

        transform.Find("Paperdoll/OK").GetComponent<Button>().onClick.AddListener(() => AudioManager.Singleton.Play(2));
        SetDisplayState(state);
    }


    public void SetDisplayState(DisplayState state)
    {
        if(this.state == state)
            return;

        foreach(Transform t in display.transform)
        {
            t.gameObject.SetActive(false);
        }

        switch(state)
        {
            case DisplayState.INVENTORY:
                display.transform.Find("Inventory").gameObject.SetActive(true);
                break;

            case DisplayState.STATS:
                display.transform.Find("Stats").gameObject.SetActive(true);
                uiStats.OnEnterState();
                break;
        }

        this.state = state;
    }

    public void ShowHeadBar(GameObject entObj, ulong health, ulong maxHealth, long deltaHp)
    {
        ulong entityId = utils.GetEntityId(entObj);
        //Debug.Log("Delta Hp: " + deltaHp);

        if(headBars.TryGetValue(entityId, out HeadbarInstance headBarInstance))
        {
            headBarInstance.SetHealth(health, maxHealth, deltaHp);
        }
        else
        {
            GameObject headBar = GameObject.Instantiate(headBarPrefab, UIManager.Singleton.guiBranch.transform);
            headBar.name = "HeadBar";


            headBars.Add(entityId, new HeadbarInstance(headBar, entObj, health, maxHealth, deltaHp));
        }
      
    }

    public void OnStateChange(int index)
    {
        SetDisplayState((DisplayState) index);
    }

    // Update is called once per frame
    void Update()
    {
        ulong[] keysToRemove = new ulong[headBars.Count];
        int index = 0;

        foreach(var keyValPair in headBars)
        {
            var headBar = keyValPair.Value;

            if(headBar.entObj == null || headBar.IsExpired())
            {
                headBar.Destroy();
                keysToRemove[index++] = keyValPair.Key;
                continue;
            }

            headBar.timeElapsed += Time.deltaTime;
            headBar.UpdateUI();
        }

        for(int i = 0; i < index; i++)
        {
            headBars.Remove(keysToRemove[i]);
        }
    }
}
