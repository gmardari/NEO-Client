using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HeadbarInstance
{
    public GameObject headBar;
    public GameObject entObj;
    public float timeElapsed;
    public ulong health;
    public ulong maxHealth;
    public long deltaHp;

    private SpriteRenderer bodyRenderer;

    public const float lifetime = 1.5f;

    public HeadbarInstance(GameObject headBar, GameObject entObj, ulong health, ulong maxHealth, long deltaHp)
    {
        this.headBar = headBar;
        this.entObj = entObj;
        this.health = health;
        this.maxHealth = maxHealth;
        this.deltaHp = deltaHp;
        this.bodyRenderer = entObj.transform.Find("Body").GetComponent<SpriteRenderer>();

        SetDamageSprites();
        //UpdateUI();
    }

    public bool IsExpired()
    {
        if (timeElapsed >= lifetime)
            return true;

        return false;
    }

    public void SetHealth(ulong health, ulong maxHealth, long deltaHp)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.deltaHp = deltaHp;
        timeElapsed = 0;

        SetDamageSprites();
    }

    private void SetDamageSprites()
    {
        var hitObj = headBar.transform.Find("Hit").gameObject;

        //Clear previous damage sprites
        for(int i = 0; i < hitObj.transform.childCount; i++)
        {
            GameObject.Destroy(hitObj.transform.GetChild(i).gameObject);
        }
        
        if(deltaHp == 0)
        {
            CreateDamageSprite(hitObj, ResourceLibrary.Singleton.headBar_dmgSprites[10]); //MISS sprite
        }
        //HEAL
        //TODO: Implement
        else if(deltaHp > 0)
        {

        }
        //DAMAGE
        else if(deltaHp < 0)
        {
            string s = Math.Abs(deltaHp).ToString();
            char[] cArray = s.ToCharArray();

            foreach(char c in cArray)
            {
                int spriteIndex = c - '0';
                CreateDamageSprite(hitObj, ResourceLibrary.Singleton.headBar_dmgSprites[spriteIndex]);
            }
        }

       
    }

    private void CreateDamageSprite(GameObject hitObj, Sprite sprite)
    {
        GameObject obj = new GameObject("Damage", typeof(RectTransform), typeof(Image));
        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize();

        //obj.transform.parent = hitObj.transform;
        obj.transform.SetParent(hitObj.transform, false);
    }
    
    public void UpdateUI()
    {
        //Set health bar
        GameObject healthBar = headBar.transform.Find("Health/Bar").gameObject;
        float healthAlpha = (maxHealth > 0) ? (float)(((double) health) / ((double) maxHealth)) : 0;

        healthBar.GetComponent<Image>().fillAmount = healthAlpha;

        Vector3 worldPos = entObj.transform.position + new Vector3(0, bodyRenderer.bounds.size.y, 0);
        //Set position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        

        RectTransform rectTransform = headBar.transform as RectTransform;
        rectTransform.anchoredPosition = screenPos;
    }

    public void Destroy()
    {
        GameObject.Destroy(headBar);
    }
}

