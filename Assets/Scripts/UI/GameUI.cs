using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Singleton;
    public GameObject headBarPrefab;

    public Dictionary<ulong, HeadbarInstance> headBars;

   
    void Awake()
    {
        Singleton = this;
        headBars = new Dictionary<ulong, HeadbarInstance>();
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
