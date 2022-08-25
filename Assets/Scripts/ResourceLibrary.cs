using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceLibrary : MonoBehaviour
{
    public static ResourceLibrary Singleton;

    public const int ARMOR_SPRITES_NUM = 22;
    public const int BOOT_SPRITES_NUM = 16;
    public const int WEAPON_SPRITES_NUM = 17;
    public const int HAIR_SPRITES_NUM = 4;
    public const int HAIR_COLOURS_NUM = 10;


    [HideInInspector]
    public Tile[] groundLayerTiles;
    [HideInInspector]
    public Tile[] objLayerTiles;
    [HideInInspector]
    public Tile[] overlayLayerTiles;
    [HideInInspector]
    public Tile[] wallLayerTiles;
    [HideInInspector]
    public Sprite[] itemDropSprites;
    [HideInInspector]
    public Sprite[] itemSprites;
    [HideInInspector]
    public Sprite[] bootsSprites;
    [HideInInspector]
    public Sprite[] armorSprites;
    [HideInInspector]
    public Sprite[] weaponSprites;
    [HideInInspector]
    public Sprite[] hairStyleMaleSprites;
    [HideInInspector]
    public Sprite[] hairStyleFemaleSprites;

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

    public AudioClip[] music;
    public AudioClip[] sfx;

    
    void Awake()
    {
        Singleton = this;
        groundLayerTiles = Resources.LoadAll<Tile>("Tiles/Ground");
        objLayerTiles = Resources.LoadAll<Tile>("Tiles/Objects");
        overlayLayerTiles = Resources.LoadAll<Tile>("Tiles/Overlay");
        wallLayerTiles = Resources.LoadAll<Tile>("Tiles/Walls");
        var allItemSprites = Resources.LoadAll<Sprite>("Sprites/gfx023");
        armorSprites = Resources.LoadAll<Sprite>("Sprites/gfx013");
        bootsSprites = Resources.LoadAll<Sprite>("Sprites/gfx011");
        weaponSprites = Resources.LoadAll<Sprite>("Sprites/gfx017");
        hairStyleMaleSprites = Resources.LoadAll<Sprite>("Sprites/gfx009");
        hairStyleFemaleSprites = Resources.LoadAll<Sprite>("Sprites/gfx010");
        npcSprites = Resources.LoadAll<Sprite>("Sprites/gfx021");

        Array.Sort(groundLayerTiles, CompareTiles);
        Array.Sort(wallLayerTiles, CompareTiles);
        Array.Sort(allItemSprites, CompareSprites);
        Array.Sort(armorSprites, CompareSprites);
        Array.Sort(bootsSprites, CompareSprites);
        Array.Sort(weaponSprites, CompareSprites);
        Array.Sort(npcSprites, CompareSprites);
        

        itemDropSprites = new Sprite[(int) Mathf.Ceil( ((float) allItemSprites.Length) / 2f)];
        itemSprites = new Sprite[allItemSprites.Length / 2];
        
        //fill up item drop sprites
        for(int a = 0; a < (allItemSprites.Length / 2); a++)
        {
            int i = a * 2;
            itemDropSprites[a] = allItemSprites[i];
            itemSprites[a] = allItemSprites[i + 1];
        }

        /*
        for(int a = 0; a < (allItemSprites.Length / 2); a++)
        {
            int i = (a * 2) + 1;
           
        }
        */
    }


    public Sprite[] GetCharSprites(ushort gender, ushort race)
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

    public Sprite[] GetArmorSprites(uint gfxId)
    {
        int startIndex = (int) gfxId * ARMOR_SPRITES_NUM;

        if ((startIndex + ARMOR_SPRITES_NUM) > armorSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[ARMOR_SPRITES_NUM];
        Array.Copy(armorSprites, startIndex, sprites, 0, ARMOR_SPRITES_NUM);

        return sprites;
    }

    public Sprite[] GetBootSprites(uint gfxId)
    {
        int startIndex = (int) gfxId * BOOT_SPRITES_NUM;

        if ((startIndex + BOOT_SPRITES_NUM) > bootsSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[BOOT_SPRITES_NUM];
        Array.Copy(bootsSprites, startIndex, sprites, 0, BOOT_SPRITES_NUM);

        return sprites;
    }

    public Sprite[] GetWeaponSprites(uint gfxId)
    {
        int startIndex = (int)gfxId * WEAPON_SPRITES_NUM;

        if ((startIndex + WEAPON_SPRITES_NUM) > weaponSprites.Length)
            return null;

        Sprite[] sprites = new Sprite[WEAPON_SPRITES_NUM];
        Array.Copy(weaponSprites, startIndex, sprites, 0, WEAPON_SPRITES_NUM);

        return sprites;
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
