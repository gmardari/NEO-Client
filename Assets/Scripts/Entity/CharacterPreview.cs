using EO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPreview : MonoBehaviour
{
    private CharacterDef def;

    public Image bodyImg;
    public Image hairImg;
    public Image armorImg;
    public Image bootsImg;
    public Image weaponImg;

    private void Awake()
    {
        ReloadSprites();
    }

    public void SetCharacterDef(CharacterDef def)
    {
        this.def = def;
        ReloadSprites();
    }

    public void OnCharacterDefChange()
    {
        ReloadSprites();
    }

    public void ReloadSprites()
    {
        if(def == null)
        {
            SetImagesVisible(false);
            return;
        }

       

        Sprite bodySprite = ResourceLibrary.Singleton.GetCharSprites(def.gender, def.race)[0];
        Sprite hairSprite = ResourceLibrary.Singleton.GetCharHairSprites(def.gender, def.hairStyle, def.hairColour)[1]; 
        Sprite armorSprite = null;
        Sprite bootSprite = null;
        Sprite weaponSprite = null; 

        if (def.doll.armor > 0)    
            armorSprite = ResourceLibrary.Singleton.GetItemBodySprites(def.gender, def.doll.armor - 1)[0];
        

        if (def.doll.boots > 0)
            bootSprite = ResourceLibrary.Singleton.GetItemBodySprites(def.gender, def.doll.boots - 1)[0];


        if (def.doll.weapon > 0)
            weaponSprite = ResourceLibrary.Singleton.GetItemBodySprites(def.gender, def.doll.weapon - 1)[0];

        bodyImg.sprite = bodySprite;
        hairImg.sprite = hairSprite;
        armorImg.sprite = armorSprite;
        bootsImg.sprite = bootSprite;
        weaponImg.sprite = weaponSprite;

        SetImagesVisible(true);
    }

    private void SetImagesVisible(bool visible)
    {
        foreach (Transform t in transform)
        {
            Image img = t.GetComponent<Image>();

            //Stay invisible if no sprite
            if (visible && img.sprite == null)
            {
                continue;
            }
          
            img.enabled = visible;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
