using EO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class ResourceLibrary : MonoBehaviour
{
    public static ResourceLibrary Singleton;

    public const int ARMOR_SPRITES_NUM = 22;
    public const int BOOT_SPRITES_NUM = 16;
    public const int WEAPON_SPRITES_NUM = 17;
    public const int HAIR_SPRITES_NUM = 4;
    public const int HAIR_COLOURS_NUM = 10;

    public SpriteAtlas maleArmorAtlas;
    public SpriteAtlas femaleArmorAtlas;
    [HideInInspector]
    public Tile[] groundLayerTiles;
    [HideInInspector]
    public Tile[] objLayerTiles;
    [HideInInspector]
    public Tile[] overlayLayerTiles;
    [HideInInspector]
    public Tile[] wallLayerTiles;
    /*
    [HideInInspector]
    public Sprite[] itemDropSprites;
    */
    //private Sprite[] _test;
    private Sprite[] itemSprites;
    private Sprite[][] bootsSprites_male;
    private Sprite[][] bootsSprites_female;
    private Sprite[][] armorSprites_male;
    private Sprite[][] armorSprites_female;
    private Sprite[][] weaponSprites_male;
    private Sprite[][] weaponSprites_female;
    private Sprite[] hairStyleMaleSprites;
    private Sprite[] hairStyleFemaleSprites;

    public Sprite[] charSprites;
    public Sprite[] charWalkSprites;
    public Sprite[] charMeleeAttackSprites;
    public Sprite[] charBowAttackSprites;
    public Sprite[] charSpellCastSprites;
    

    public Sprite[] genderUIImages;
    public Sprite[] hairStyleUIImages;
    public Sprite[] hairColourUIImages;
    public Sprite[] skinColourUIImages;


    [HideInInspector]
    public Sprite[] npcSprites;

    public Sprite[] headBar_dmgSprites;
    public uint[] goldGfxIds;
    public uint[] goldThreshold; 

    public Sound[] music;
    public Sound[] guitar;
    public Sound[] harp;
    public Sound[] sfx;

    
    void Awake()
    {
        Singleton = this;
        groundLayerTiles = Resources.LoadAll<Tile>("Tiles/Ground");
        objLayerTiles = Resources.LoadAll<Tile>("Tiles/Objects");
        overlayLayerTiles = Resources.LoadAll<Tile>("Tiles/Overlay");
        wallLayerTiles = Resources.LoadAll<Tile>("Tiles/Walls");
        //var allItemSprites = Resources.LoadAll<Sprite>("Sprites/gfx023");
       
        //bootsSprites = Resources.LoadAll<Sprite>("Sprites/gfx011");
        
        hairStyleMaleSprites = Resources.LoadAll<Sprite>("Sprites/gfx009");
        hairStyleFemaleSprites = Resources.LoadAll<Sprite>("Sprites/gfx010");
        npcSprites = Resources.LoadAll<Sprite>("Sprites/gfx021");

        Array.Sort(groundLayerTiles, CompareTiles);
        Array.Sort(wallLayerTiles, CompareTiles);
        //Array.Sort(allItemSprites, CompareSprites);
        //Array.Sort(bootsSprites, CompareSprites);
        Array.Sort(npcSprites, CompareSprites);

        /*_test = new Sprite[armor_atlas.spriteCount];
        armor_atlas.GetSprites(_test);*/
        LoadWeaponSprites();
        //LoadArmorSprites();
        LoadBootSprites();
        LoadItemSprites();

        LoadSounds(music);
        LoadSounds(guitar);
        LoadSounds(harp);
        LoadSounds(sfx);

        /*
        itemDropSprites = new Sprite[(int) Mathf.Ceil( ((float) allItemSprites.Length) / 2f)];
        itemSprites = new Sprite[allItemSprites.Length / 2];
        
        //fill up item drop sprites
        for(int a = 0; a < (allItemSprites.Length / 2); a++)
        {
            int i = a * 2;
            itemDropSprites[a] = allItemSprites[i];
            itemSprites[a] = allItemSprites[i + 1];
        }
        */

        /*
        for(int a = 0; a < (allItemSprites.Length / 2); a++)
        {
            int i = (a * 2) + 1;
           
        }
        */
    }

/*    public Sprite GetTestByName(string name)
    {
        return armor_atlas.GetSprite(name);
    }

    public Sprite GetTestSprite(int index)
    {
        return _test[index];
    }*/

    public void LoadWeaponSprites()
    {
        //Male
        {
            var wSprites = Resources.LoadAll<Sprite>("Sprites/gfx017");
            Array.Sort(wSprites, CompareSprites);

            int numWeapons = int.Parse(wSprites[wSprites.Length - 1].name) / 100;
            weaponSprites_male = new Sprite[numWeapons][];

            int w = 1;
            int len = 0;

            for (int i = 0; i < wSprites.Length; i++)
            {
                int j = int.Parse(wSprites[i].name) / 100;

                if (w != j)
                {
                    //Add all the sprites to the array
                    weaponSprites_male[w - 1] = new Sprite[len];

                    int l = 0;
                    for (int k = i - 1; k > (i - 1 - len); k--)
                    {
                        weaponSprites_male[w - 1][len - 1 - l] = wSprites[k];
                        l++;
                    }

                    w = j;
                    len = 0;


                }

                len++;
            }

            weaponSprites_male[w - 1] = new Sprite[len];

            int ii = wSprites.Length - 1;
            int ll = 0;
            for (int k = ii - 1; k > (ii - 1 - len); k--)
            {
                weaponSprites_male[w - 1][len - 1 - ll] = wSprites[k];
                ll++;
            }
        }

        //Female
        {
            var wSprites = Resources.LoadAll<Sprite>("Sprites/gfx018");
            Array.Sort(wSprites, CompareSprites);

            int numWeapons = int.Parse(wSprites[wSprites.Length - 1].name) / 100;
            weaponSprites_female = new Sprite[numWeapons][];

            int w = 1;
            int len = 0;

            for (int i = 0; i < wSprites.Length; i++)
            {
                int j = int.Parse(wSprites[i].name) / 100;

                if (w != j)
                {
                    //Add all the sprites to the array
                    weaponSprites_female[w - 1] = new Sprite[len];

                    int l = 0;
                    for (int k = i - 1; k > (i - 1 - len); k--)
                    {
                        weaponSprites_female[w - 1][len - 1 - l] = wSprites[k];
                        l++;
                    }

                    w = j;
                    len = 0;


                }

                len++;
            }

            weaponSprites_female[w - 1] = new Sprite[len];

            int ii = wSprites.Length - 1;
            int ll = 0;
            for (int k = ii - 1; k > (ii - 1 - len); k--)
            {
                weaponSprites_female[w - 1][len - 1 - ll] = wSprites[k];
                ll++;
            }
        }
    }

    public void LoadArmorSprites()
    {
        //Male
        {
            List<Sprite[]> arrays = new List<Sprite[]>();
            var sprites = Resources.LoadAll<Sprite>("Sprites/gfx013");
            Array.Sort(sprites, CompareSprites);

            Sprite[] arr;
            int len = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                if ((i + 1) < sprites.Length)
                {
                    int index1 = int.Parse(sprites[i].name);
                    int index2 = int.Parse(sprites[i + 1].name);

                    len++;

                    if (index2 - index1 > 1)
                    {
                        arr = new Sprite[len];
                        for (int k = 0; k < len; k++)
                        {
                            arr[k] = sprites[i - len + 1 + k];
                        }
                        arrays.Add(arr);
                        len = 0;
                    }
                }
            }

            int ii = sprites.Length - 1;
            arr = new Sprite[len];
            for (int k = 0; k < len; k++)
            {
                arr[k] = sprites[ii - len + 1 + k];
            }
            arrays.Add(arr);

            armorSprites_male = arrays.ToArray();
        }

        //Female
        {
            List<Sprite[]> arrays = new List<Sprite[]>();
            var sprites = Resources.LoadAll<Sprite>("Sprites/gfx014");
            Array.Sort(sprites, CompareSprites);

            Sprite[] arr;
            int len = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                if ((i + 1) < sprites.Length)
                {
                    int index1 = int.Parse(sprites[i].name);
                    int index2 = int.Parse(sprites[i + 1].name);

                    len++;

                    if (index2 - index1 > 1)
                    {
                        arr = new Sprite[len];
                        for (int k = 0; k < len; k++)
                        {
                            arr[k] = sprites[i - len + 1 + k];
                        }
                        arrays.Add(arr);
                        len = 0;
                    }
                }
            }

            int ii = sprites.Length - 1;
            arr = new Sprite[len];
            for (int k = 0; k < len; k++)
            {
                arr[k] = sprites[ii - len + 1 + k];
            }
            arrays.Add(arr);

            armorSprites_female = arrays.ToArray();
        }
    }

    public void LoadBootSprites()
    {
        //Male
        {
            List<Sprite[]> arrays = new List<Sprite[]>();
            var sprites = Resources.LoadAll<Sprite>("Sprites/gfx011");
            Array.Sort(sprites, CompareSprites);

            Sprite[] arr;
            int len = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                if ((i + 1) < sprites.Length)
                {
                    int index1 = int.Parse(sprites[i].name);
                    int index2 = int.Parse(sprites[i + 1].name);

                    len++;

                    if (index2 - index1 > 1)
                    {
                        arr = new Sprite[len];
                        for (int k = 0; k < len; k++)
                        {
                            arr[k] = sprites[i - len + 1 + k];
                        }
                        arrays.Add(arr);
                        len = 0;
                    }
                }
            }

            int ii = sprites.Length - 1;
            arr = new Sprite[len];
            for (int k = 0; k < len; k++)
            {
                arr[k] = sprites[ii - len + 1 + k];
            }
            arrays.Add(arr);

            bootsSprites_male = arrays.ToArray();
        }

        //Female
        {
            List<Sprite[]> arrays = new List<Sprite[]>();
            var sprites = Resources.LoadAll<Sprite>("Sprites/gfx012");
            Array.Sort(sprites, CompareSprites);

            Sprite[] arr;
            int len = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                if ((i + 1) < sprites.Length)
                {
                    int index1 = int.Parse(sprites[i].name);
                    int index2 = int.Parse(sprites[i + 1].name);

                    len++;

                    if (index2 - index1 > 1)
                    {
                        arr = new Sprite[len];
                        for (int k = 0; k < len; k++)
                        {
                            arr[k] = sprites[i - len + 1 + k];
                        }
                        arrays.Add(arr);
                        len = 0;
                    }
                }
            }

            int ii = sprites.Length - 1;
            arr = new Sprite[len];
            for (int k = 0; k < len; k++)
            {
                arr[k] = sprites[ii - len + 1 + k];
            }
            arrays.Add(arr);

            bootsSprites_female = arrays.ToArray();
        }
    }

    public void LoadSounds(Sound[] sounds)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;

            //s.source.outputAudioMixerGroup = mixerGroup;
        }
    }

    public void LoadItemSprites()
    {
        var sprites = Resources.LoadAll<Sprite>("Sprites/gfx023");
        Array.Sort(sprites, CompareSprites);

        int lastIndex = int.Parse(sprites[sprites.Length - 1].name);
        int startIndex = int.Parse(sprites[0].name);
        int len = lastIndex - startIndex + 1;
        
        itemSprites = new Sprite[len];

        int j = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if ((i + 1) < sprites.Length)
            {
                int index1 = int.Parse(sprites[i].name);
                int index2 = int.Parse(sprites[i + 1].name);
                itemSprites[j++] = sprites[i];

                if (index2 - index1 > 1)
                {
                    for(int k = 1; k < (index2 - index1); k++)
                    {
                        itemSprites[j++] = null;
                    }
                }
            }
            else
                itemSprites[j++] = sprites[i];
        }

        Debug.Log($"Loaded {len} item sprites");
    }

    public Sprite[] GetCharSprites(byte gender, byte race)
    {
        int startIndex = 4 * race + 2 * gender;

        if(charSprites.Length <= (startIndex + 1))
        {
            return null;
        }

        Sprite[] sprites = new Sprite[2];
        Array.Copy(charSprites, startIndex, sprites, 0, 2);

        return sprites;
    }

    //16 * 8
    public Sprite[] GetCharWalkSprites(ushort gender, ushort race)
    {
        int startIndex = 16 * race + 8 * gender;

        //Make sure we have 8 sprites to read
        if (charWalkSprites.Length <= (startIndex + 8))
        {
            return null;
        }

        Sprite[] sprites = new Sprite[8];
        Array.Copy(charWalkSprites, startIndex, sprites, 0, 8);

        return sprites;
    }

    //Sprite sheet is a 8 * 7 frame
    public Sprite[] GetCharMeleeAttackSprites(ushort gender, ushort race)
    {
        
        int startIndex = 8 * race + 4 * gender;

        if (charMeleeAttackSprites.Length <= (startIndex + 4))
        {
            return null;
        }

        Sprite[] sprites = new Sprite[4];
        Array.Copy(charMeleeAttackSprites, startIndex, sprites, 0, 4);

        return sprites;
    }

    public Sprite[] GetCharHairSprites(byte gender, byte hairStyle, byte hairColour)
    {
        int startIndex = (HAIR_SPRITES_NUM * HAIR_COLOURS_NUM) * hairStyle + (HAIR_SPRITES_NUM * hairColour);

        Sprite[] arr = (gender == 0) ? hairStyleFemaleSprites : hairStyleMaleSprites;

        //The last sprite index does not go over the bounds
        if(arr.Length <= (startIndex + HAIR_SPRITES_NUM))
        {
            return null;
        }

        Sprite[] sprites = new Sprite[HAIR_SPRITES_NUM];
        Array.Copy(arr, startIndex, sprites, 0, HAIR_SPRITES_NUM);

        return sprites;

    }
    /*
    public Sprite[] GetArmorSprites(uint gfxId)
    {
        int startIndex = (int) gfxId * ARMOR_SPRITES_NUM;

        if ((startIndex + ARMOR_SPRITES_NUM) > armorSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[ARMOR_SPRITES_NUM];
        Array.Copy(armorSprites, startIndex, sprites, 0, ARMOR_SPRITES_NUM);

        return sprites;
    }
    */

    //TODO: Implement gender swapping armors
    /*public Sprite[] GetArmorSprites(byte gender, uint gfxId)
    {
        switch(gender)
        {
            case 0:
                if ((gfxId - 1) >= armorSprites_female.Length)
                    return null;

                return armorSprites_female[gfxId - 1];
            case 1:
                if ((gfxId - 1) >= armorSprites_male.Length)
                    return null;

                return armorSprites_male[gfxId - 1];
        }
      
        return null;
    }*/
    //TODO: Implement all genders

    public Sprite[] GetArmorSprites(byte gender, uint gfxId)
    {
        SpriteAtlas spriteAtlas = (gender == 0) ? femaleArmorAtlas : maleArmorAtlas;

        Sprite[] arr = new Sprite[ARMOR_SPRITES_NUM];

        uint startingIndex = (gfxId - 1) * 50 + 101;

        for(int i = 0; i < ARMOR_SPRITES_NUM; i++)
        {
            arr[i] = spriteAtlas.GetSprite((startingIndex + i).ToString());
        }

        return arr;
    }

    /*
    public Sprite[] GetBootSprites(uint gfxId)
    {
        int startIndex = (int) gfxId * BOOT_SPRITES_NUM;

        if ((startIndex + BOOT_SPRITES_NUM) > bootsSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[BOOT_SPRITES_NUM];
        Array.Copy(bootsSprites, startIndex, sprites, 0, BOOT_SPRITES_NUM);

        return sprites;
    }
    */

    public Sprite[] GetBootSprites(byte gender, uint gfxId)
    {
        switch(gender)
        {
            case 0:
                if ((gfxId - 1) >= bootsSprites_female.Length)
                    return null;

                return bootsSprites_female[gfxId - 1];

            case 1:
                if ((gfxId - 1) >= bootsSprites_male.Length)
                    return null;

                return bootsSprites_male[gfxId - 1];
        }

        return null;
    }

    /*
    public Sprite[] GetWeaponSprites(uint gfxId)
    {
        int startIndex = (int)gfxId * WEAPON_SPRITES_NUM;

        if ((startIndex + WEAPON_SPRITES_NUM) > weaponSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[WEAPON_SPRITES_NUM];
        Array.Copy(weaponSprites, startIndex, sprites, 0, WEAPON_SPRITES_NUM);

        return sprites;
    }
    */

    public Sprite[] GetWeaponSprites(byte gender, uint gfxId)
    {
        switch(gender)
        {
            case 0:
                if ((gfxId - 1) >= weaponSprites_female.Length)
                    return null;

                return weaponSprites_female[gfxId - 1];
            case 1:
                if ((gfxId - 1) >= weaponSprites_male.Length)
                    return null;

                return weaponSprites_male[gfxId - 1];
        }

        return null;
    }
    
    public Sprite[] GetNpcIdleSprites(uint gfxId)
    {
        int startIndex = 16 * (int) gfxId;

        if((startIndex + 4) > npcSprites.Length)
        {
            return null;
        }

        Sprite[] sprites = new Sprite[4];
        Array.Copy(npcSprites, startIndex, sprites, 0, 4);

        return sprites;
    }

    public Sprite[] GetNpcWalkSprites(uint gfxId)
    {
        int startIndex = 16 * (int)gfxId + 4;

        if ((startIndex + 8) > npcSprites.Length)
        {
            return null;
        }

        Sprite[] sprites = new Sprite[8];
        Array.Copy(npcSprites, startIndex, sprites, 0, 8);

        return sprites;
    }

    public Sprite[] GetNpcAttackSprites(uint gfxId)
    {
        int startIndex = 16 * (int)gfxId + 12;

        if ((startIndex + 4) > npcSprites.Length)
        {
            return null;
        }

        Sprite[] sprites = new Sprite[4];
        Array.Copy(npcSprites, startIndex, sprites, 0, 4);

        return sprites;
    }

    public Sprite GetDropItemSprite(uint gfxId)
    {
        uint index = (gfxId - 1) * 2;

        if (index >= itemSprites.Length)
            return null;

        return itemSprites[index];
    }

    public Sprite GetInvItemSprite(uint gfxId)
    {
        uint index = (gfxId - 1) * 2 + 1;

        if (index >= itemSprites.Length)
            return null;

        return itemSprites[index];
    }

    public Sprite[] GetItemBodySprites(byte gender, uint itemId)
    {
        ItemDataEntry entry = DataFiles.Singleton.GetItemData(itemId);
        ItemType type = (ItemType)entry.itemType;

        switch(type)
        {
            case ItemType.WEAPON:
                return GetWeaponSprites(gender, entry.bodyGfx);
            case ItemType.ARMOR:
                return GetArmorSprites(gender, entry.bodyGfx);
            case ItemType.BOOTS:
                return GetBootSprites(gender, entry.bodyGfx);
        }

        return null;
    }

    public Sprite GetGoldDropSprite(uint amount)
    {
        for(int i = goldThreshold.Length - 1; i >= 0; i--)
        {
            if(amount >= goldThreshold[i])
            {
                return GetDropItemSprite(goldGfxIds[i]);
            }
        }

        return null;
    }

    private int CompareTiles(Tile x, Tile y)
    {
        if (x == null)

            if (y == null)
                return 0;
            else

                return -1;
        else
        {
            if (y == null)
                return 1;
            else
            {
                int xNameId = int.Parse(x.name);
                int yNameId = int.Parse(y.name);
                int retval = xNameId.CompareTo(yNameId);

                return retval;
            }
        }
    }

    private int CompareSprites(Sprite x, Sprite y)
    {
        if (x == null)

            if (y == null)
                return 0;
            else

                return -1;
        else
        {
            if (y == null)
                return 1;
            else
            {
                int xNameId = int.Parse(x.name);
                int yNameId = int.Parse(y.name);
                int retval = xNameId.CompareTo(yNameId);

                return retval;
            }
        }
    }
}
