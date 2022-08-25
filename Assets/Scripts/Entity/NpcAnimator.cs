using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcAnimatorState
{
    IDLE,
    WALK,
    ATTACK,
    DYING
}


public class NpcAnimator : MonoBehaviour
{
    private NpcAnimatorState state;
    private long nt_time;
    private uint direction;
    private NpcDef def;
    private bool init = default;
    private ResourceLibrary resLib;

    private SpriteRenderer bodyRenderer;

    private Sprite[] idleSprites;
    private Sprite[] walkSprites;
    private Sprite[] attackSprites;

    public const float BASE_WALK_LENGTH = (2.0f / 3.0f);
    public const float BASE_ATTACK_LENGTH = 0.5f;

    // Start is called before the first frame update
    void Awake()
    {
        state = NpcAnimatorState.IDLE;
        direction = 0;
        nt_time = 0;
        bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        resLib = GameObject.Find("Resources").GetComponent<ResourceLibrary>();

        //ReloadSprites();
    }

    public void SetNpcDef(NpcDef def)
    {
        this.def = def;
        ReloadSprites();

        init = true;
    }

    private void ReloadSprites()
    {
        idleSprites = resLib.GetNpcIdleSprites((uint) def.gfxId);
        walkSprites = resLib.GetNpcWalkSprites((uint) def.gfxId);
        attackSprites = resLib.GetNpcAttackSprites((uint) def.gfxId);
    }

    public void SetIdle(uint direction)
    {
        this.direction = direction;
        state = NpcAnimatorState.IDLE;
    }

    public void SetWalk(uint direction, long nt_time)
    {
        this.direction = direction;
        state = NpcAnimatorState.WALK;
        this.nt_time = nt_time;
    }

    public void SetAttack(uint direction, long nt_time)
    {
        this.direction = direction;

        state = NpcAnimatorState.ATTACK;
       

        this.nt_time = nt_time;
    }

    //TODO: Idle anims
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
            idle = 2;
        }
        else if (direction == 3)
        {
            flipX = false;
            idle = 2;
        }

        bodyRenderer.sprite = idleSprites[idle];
        bodyRenderer.flipX = flipX;

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

        bodyRenderer.sprite = walkSprites[4 * walk + frame];
        bodyRenderer.flipX = flipX;


    }

    private void DoAttackAnim()
    {
        bool flipX = false;
        int section = 0;

        if (direction == 0)
        {
            flipX = false;
            section = 0;
        }
        else if (direction == 1)
        {
            flipX = true;
            section = 0;
        }
        else if (direction == 2)
        {
            flipX = true;
            section = 1;
        }
        else if (direction == 3)
        {
            flipX = false;
            section = 1;
        }

        float deltaTime = ((float)(NetworkTime.GetNetworkTime() - nt_time)) / 1000f;
        
        deltaTime = deltaTime % BASE_ATTACK_LENGTH;
        float frameLength = BASE_ATTACK_LENGTH / 2;
        int frame = (int)(deltaTime / frameLength);

        bodyRenderer.sprite = attackSprites[2 * section + frame];
        bodyRenderer.flipX = flipX;


    }

    //TODO: Implement attack anim
    private void Evaluate()
    {
        switch (state)
        {
            case NpcAnimatorState.IDLE:

                DoIdleAnim();
                break;

            case NpcAnimatorState.WALK:

                DoWalkAnim();
                break;

            case NpcAnimatorState.ATTACK:

                DoAttackAnim();
                break;

            default:

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(init)
        {
            Evaluate();
        }
    }
}
