using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EO;
public enum CharacterAnimatorState
{
    IDLE,
    WALK,
    MELEE_ATTACK,
    RANGED_ATTACK,
    SPELL_CAST
}

public class CharacterAnimator : MonoBehaviour
{
    private Vector3 hair_off = new Vector3(0f, 0.266f, 1.5f);
    private Vector3 hair_off_mel_atk1 = new Vector3(-0.0790f, 0.203f, 1.5f);
    private Vector3 hair_off_mel_atk2 = new Vector3(-0.047f, 0.25f, 1.5f);
       
    private Vector3 armor_off = new Vector3(0f, -0.098f, 0f);
    private Vector3 armor_off_atk = new Vector3(0.032f, -0.098f, 0f);
    private Vector3 boot_off = new Vector3(0f, -0.078f, 1.0f);
    private Vector3 boot_off_walk1 = new Vector3(0.016f, -0.0308f, 1f);
    private Vector3 boot_off_walk2 = new Vector3(0f, -0.0308f, 1f); 
    private Vector3 boot_off_melee1 = new Vector3(0.032f, -0.078f, 1f);
    private Vector3 weapon_off = new Vector3(-0.192f, -0.088f, -0.5f);
    private Vector3 weapon_off_walk = new Vector3(-0.168f, -0.022f, -0.5f);
    private Vector3 weapon_off_melee = new Vector3(-0.076f, -0.174f, -0.7f);

    //private Vector3 weapon_off2 = new Vector3(0.180f, -0.088f, -0.5f);
    private uint direction;
    private CharacterAnimatorState state;
    private long nt_time;
    private CharacterDef def;

    private SpriteRenderer bodyRenderer;
    private SpriteRenderer hairRenderer;
    private SpriteRenderer armorRenderer;
    private SpriteRenderer bootsRenderer;
    private SpriteRenderer weaponRenderer;

    private Sprite[] bodySprites;
    private Sprite[] bodyWalkSprites;
    private Sprite[] bodyMeleeAttackSprites;
    private Sprite[] hairSprites;
    private Sprite[] armorSprites;
    private Sprite[] bootsSprites;
    private Sprite[] weaponSprites;

    public const float BASE_WALK_LENGTH = (2.0f / 3.0f);
    public const float BASE_ATTACK_LENGTH = 0.5f;

    public const int ARMOR_IDLE_OFFSET = 0;
    public const int ARMOR_WALK_OFFSET = 2;
    public const int ARMOR_ATTACK_OFFSET = 12;

    public const int BOOT_IDLE_OFFSET = 0;
    public const int BOOT_WALK_OFFSET = 2;
    public const int BOOT_ATTACK_OFFSET = 10;
    public const int WEAPON_IDLE_OFFSET = 0;
    public const int WEAPON_WALK_OFFSET = 2;
    public const int WEAPON_ATTACK_OFFSET = 12; 

    //TODO: Change to Awake()
    void Awake()
    {
        state = CharacterAnimatorState.IDLE;
        direction = 0;
        nt_time = 0;

        bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        hairRenderer = transform.Find("Hair").GetComponent<SpriteRenderer>();
        armorRenderer = transform.Find("Armor").GetComponent<SpriteRenderer>();
        bootsRenderer = transform.Find("Boots").GetComponent<SpriteRenderer>();
        weaponRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();

        /*
        def = new CharacterDef
        {
            gender = 1,
            hairStyle = 0,
            doll = new Paperdoll
            {
                armor = 2,
                boots = 1,
                weapon = 3
            }
        };
        ReloadSprites();
        */
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
        bodySprites = ResourceLibrary.Singleton.GetCharSprites(def.gender, def.race);
        bodyWalkSprites = ResourceLibrary.Singleton.GetCharWalkSprites(def.gender, def.race);
        bodyMeleeAttackSprites = ResourceLibrary.Singleton.GetCharMeleeAttackSprites(def.gender, def.race);
        hairSprites = ResourceLibrary.Singleton.GetCharHairSprites(def.gender, def.hairStyle, def.hairColour); 

        if (def.doll.armor > 0)
        {
            ItemDataEntry entry = DataFiles.Singleton.GetItemData((int)def.doll.armor - 1);
            armorSprites = ResourceLibrary.Singleton.GetArmorSprites(entry.bodyGfx);
        }
        else
            armorSprites = null;

        if (def.doll.boots > 0)
        {
            ItemDataEntry entry = DataFiles.Singleton.GetItemData((int)def.doll.boots - 1);
            bootsSprites = ResourceLibrary.Singleton.GetBootSprites(entry.bodyGfx);
        }
        else
            bootsSprites = null;

        if (def.doll.weapon > 0)
        {
            ItemDataEntry entry = DataFiles.Singleton.GetItemData((int)def.doll.weapon - 1);
            weaponSprites = ResourceLibrary.Singleton.GetWeaponSprites(entry.bodyGfx);

        }
        else
            weaponSprites = null;
    }

    public void SetIdle(uint direction)
    {
        this.direction = direction;
        state = CharacterAnimatorState.IDLE;
    }

    public void SetWalk(uint direction, long nt_time)
    {
        this.direction = direction;
        state = CharacterAnimatorState.WALK;
        this.nt_time = nt_time;
    }

    public void SetAttack(uint direction, bool melee, long nt_time)
    {
        this.direction = direction;

        if (melee)
            state = CharacterAnimatorState.MELEE_ATTACK;
        else
            state = CharacterAnimatorState.RANGED_ATTACK;
       
        this.nt_time = nt_time;
    }

    private void DoIdleAnim()
    {
        bool flipX = false;
        int idle = 0;

        if (direction == 0)
        {
            flipX = false;
            idle = 0;
        }
        else if (direction == 1)
        {
            flipX = true;
            idle = 0;
        }
        else if (direction == 2)
        {
            flipX = true;
            idle = 1;
        }
        else if (direction == 3)
        {
            flipX = false;
            idle = 1;
        }

        bodyRenderer.sprite = bodySprites[idle];
        bodyRenderer.flipX = flipX;

        hairRenderer.sprite = hairSprites[idle * 2 + 1];
        hairRenderer.flipX = flipX;


        //Equiotment:
        if (def.doll.armor > 0)
        {
            armorRenderer.sprite = armorSprites[ARMOR_IDLE_OFFSET + idle];
            armorRenderer.flipX = flipX;
        }
        else
            armorRenderer.sprite = null;

        if (def.doll.boots > 0)
        {
            bootsRenderer.sprite = bootsSprites[BOOT_IDLE_OFFSET + idle];
            bootsRenderer.flipX = flipX;
        }
        else
            bootsRenderer.sprite = null;

        if (def.doll.weapon > 0)
        {
            weaponRenderer.sprite = weaponSprites[WEAPON_IDLE_OFFSET + idle];
            weaponRenderer.flipX = flipX;
        }
        else
            weaponRenderer.sprite = null;


        int flipInt = flipX ? -1 : 1;

        Vector3 localPos = hairRenderer.transform.localPosition;

        hairRenderer.transform.localPosition = new Vector3(hair_off.x + flipInt * -0.016f, hair_off.y, hair_off.z);
        armorRenderer.transform.localPosition = armor_off;
        bootsRenderer.transform.localPosition = boot_off;
        weaponRenderer.transform.localPosition = new Vector3(weapon_off.x * flipInt, weapon_off.y, weapon_off.z);

    }

    private void DoWalkAnim()
    {
        bool flipX = false;
        int walk = 0;

        if (direction == 0)
        {
            flipX = false;
            walk = 0;
        }
        else if (direction == 1)
        {
            flipX = true;
            walk = 0;
        }
        else if (direction == 2)
        {
            flipX = true;
            walk = 1;
        }
        else if (direction == 3)
        {
            flipX = false;
            walk = 1;
        }

        float deltaTime = ((float)(NetworkTime.GetNetworkTime() - nt_time)) / 1000f;

        deltaTime = deltaTime % BASE_WALK_LENGTH;
        float frameLength = BASE_WALK_LENGTH / 4;
        int frame = (int)(deltaTime / frameLength);

        bodyRenderer.sprite = bodyWalkSprites[4 * walk + frame];
        bodyRenderer.flipX = flipX;

        hairRenderer.sprite = hairSprites[walk * 2 + 1];
        hairRenderer.flipX = flipX;

        if (def.doll.armor > 0)
        {
            armorRenderer.sprite = armorSprites[ARMOR_WALK_OFFSET + 4 * walk + frame];
            armorRenderer.flipX = flipX;
        }
        else
            armorRenderer.sprite = null;

        if (def.doll.boots > 0)
        {
            bootsRenderer.sprite = bootsSprites[BOOT_WALK_OFFSET + 4 * walk + frame];
            bootsRenderer.flipX = flipX;
        }
        else
            bootsRenderer.sprite = null;

        if (def.doll.weapon > 0)
        {
            weaponRenderer.sprite = weaponSprites[WEAPON_WALK_OFFSET + 4 * walk + frame];
            weaponRenderer.flipX = flipX;
        }
        else
            weaponRenderer.sprite = null;

        int flipInt = flipX ? -1 : 1;
        float xShift = flipInt * -0.015625f;

        Vector3 bootPos;

        if (walk == 0)
            bootPos = new Vector3(boot_off_walk1.x * flipInt, boot_off_walk1.y, boot_off_walk1.z);
        else
            bootPos = boot_off_walk2;

        //if (walk == 0)
        hairRenderer.transform.localPosition = new Vector3(hair_off.x + (walk * xShift), hair_off.y + 0.03125f, hair_off.z);
        armorRenderer.transform.localPosition = new Vector3(armor_off.x, armor_off.y + 0.03125f, armor_off.z);
        bootsRenderer.transform.localPosition = bootPos;
        weaponRenderer.transform.localPosition = new Vector3(weapon_off_walk.x * flipInt, weapon_off_walk.y, weapon_off_walk.z);
    }


    private void DoAttackAnim(bool melee)
    {
        bool flipX = false;
        int attack = 0;

        if (direction == 0)
        {
            flipX = false;
            attack = 0;
        }
        else if (direction == 1)
        {
            flipX = true;
            attack = 0;
        }
        else if (direction == 2)
        {
            flipX = true;
            attack = 1;
        }
        else if (direction == 3)
        {
            flipX = false;
            attack = 1;
        }

        float deltaTime = ((float)(NetworkTime.GetNetworkTime() - nt_time)) / 1000f;

        deltaTime = deltaTime % BASE_ATTACK_LENGTH;
        float frameLength = BASE_ATTACK_LENGTH / 2;
        int frame = (int)(deltaTime / frameLength);

        bodyRenderer.sprite = bodyMeleeAttackSprites[2 * attack + frame];
        bodyRenderer.flipX = flipX;

        hairRenderer.sprite = hairSprites[attack * 2 + 1];
        hairRenderer.flipX = flipX;

        if (def.doll.armor > 0)
        {
            armorRenderer.sprite = armorSprites[ARMOR_ATTACK_OFFSET + 2 * attack + frame];
            armorRenderer.flipX = flipX;
        }
        else
            armorRenderer.sprite = null;

        if (def.doll.boots > 0)
        {
            if(frame == 0)
                bootsRenderer.sprite = bootsSprites[BOOT_IDLE_OFFSET + attack];
            else
                bootsRenderer.sprite = bootsSprites[BOOT_ATTACK_OFFSET + attack];

            bootsRenderer.flipX = flipX;
        }
        else
            bootsRenderer.sprite = null;

        if (def.doll.weapon > 0)
        {
            weaponRenderer.sprite = weaponSprites[WEAPON_ATTACK_OFFSET + 2 * attack + frame];
            weaponRenderer.flipX = flipX;
        }
        else
            weaponRenderer.sprite = null;


        int flipInt = flipX ? -1 : 1;

        Vector3 hairPos;
        Vector3 armorPos;
        Vector3 bootsPos;
        Vector3 weaponPos;

        if (frame == 0)
        {
            hairPos = new Vector3(hair_off.x + (flipInt * 0.016f), hair_off.y, hair_off.z);
            armorPos = new Vector3(armor_off_atk.x * flipInt, armor_off_atk.y, armor_off_atk.z);
            bootsPos = new Vector3(boot_off_melee1.x * flipInt, boot_off_melee1.y, boot_off_melee1.z);
            weaponPos = new Vector3(weapon_off_melee.x * flipInt, weapon_off_melee.y, weapon_off_melee.z);
        }
        else
        {
            if (attack == 0)
            {
                hairPos = new Vector3(hair_off_mel_atk1.x * flipInt, hair_off_mel_atk1.y, hair_off_mel_atk1.z);
                weaponPos = new Vector3(weapon_off_melee.x * flipInt, weapon_off_melee.y, -weapon_off_melee.z);
            }
            else
            {
                hairPos = new Vector3(hair_off_mel_atk2.x * flipInt, hair_off_mel_atk2.y, hair_off_mel_atk2.z);
                weaponPos = new Vector3(weapon_off_melee.x * flipInt, weapon_off_melee.y, weapon_off_melee.z);
            }
               
            armorPos = new Vector3(-armor_off_atk.x * flipInt, armor_off_atk.y, armor_off_atk.z);

            bootsPos = new Vector3(-boot_off_melee1.x * flipInt, boot_off_melee1.y, boot_off_melee1.z);
        }

       
           

        hairRenderer.transform.localPosition = hairPos;
        armorRenderer.transform.localPosition = armorPos;
        bootsRenderer.transform.localPosition = bootsPos;
        weaponRenderer.transform.localPosition = weaponPos;

        //Debug.Log($"Hair transform: {hairRenderer.transform.localPosition}");
    }

    private void Evaluate()
    {
        switch (state)
        {
            case CharacterAnimatorState.IDLE:

                DoIdleAnim();
                break;

            case CharacterAnimatorState.WALK:

                DoWalkAnim();
                break;

            case CharacterAnimatorState.MELEE_ATTACK:

                DoAttackAnim(true);
                break;

            case CharacterAnimatorState.RANGED_ATTACK:

                DoAttackAnim(false);
                break; 

            default:

                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(def != null)
            Evaluate();
    }
}
