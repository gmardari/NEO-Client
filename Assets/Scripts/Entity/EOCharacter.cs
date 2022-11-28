using UnityEngine;
using UnityEngine.Tilemaps;
using EO.Map;

//TODO: Change player Object ownership to server
namespace EO
{
    public class EOCharacter : MonoBehaviour
    {
        public CharacterDef characterDef;
        public EntityDef entityDef;
        public uint direction;
        public uint net_direction;
        public CharProperties props;
        private CharacterState cState;
        private CharacterState net_cState;
        private WalkAnim walkAnim = new WalkAnim();
        private WalkAnim net_walkAnim = new WalkAnim();
        private AttackAnim net_attackAnim = new AttackAnim();
        public bool isLocalPlayer;
        public string CharacterName => characterDef.name;
        
        //Accessors
        public Vector2Int net_position
        { get { return entityDef.net_position; } }
        public Vector2Int position
        { get { return entityDef.position; } set { entityDef.position = value; } }


        private CharacterAnimator animator;
        //private MoveInput moveInput;
        private request_intervals request_intervals;
        private PlayerInputManager inputManager;
        private float inputSyncInterval;
        //WalkAnim walkAnim;

        //Events
        public delegate void SpawnEvent();
        public delegate void MapChangedEvent(int mapId);
        SpawnEvent onSpawn;
        MapChangedEvent onMapChanged;


        void Awake()
        {
            //map = GameObject.Find("Map0").GetComponent<EOMap>();
            cState = CharacterState.IDLE;
            inputManager = new PlayerInputManager(0.25f);
            //moveInput = new MoveInput(-1);

            //mapGrid = map.GetComponent<Grid>();
            //tilemap = map.transform.GetChild(0).GetComponent<Tilemap>();
            entityDef = GetComponent<EntityDef>();
           
            animator = GetComponent<CharacterAnimator>();

            animator.SetIdle(direction);
        }

        public void Init(CharacterDef def, bool isLocalPlayer)
        {
            this.characterDef = def;
            this.isLocalPlayer = isLocalPlayer;
            animator.SetCharacterDef(this.characterDef);
        }
        /*
        void SetDirection(int direction)
        {
            orientation.direction = direction;
        }

        void SetPosition(Vector2Int pos)
        {
            var orient = orientation.Value;
            orientation.Value = new Orientation(orient.mapId, pos, orient.direction);
        }
        */

        public void NetSetDirection(uint direction)
        {
            this.net_direction = direction;
            this.direction = net_direction;
           // _animator.SetIdle(direction);
        }

        public void ClientSetDirection(uint direction)
        {
            this.direction = direction;
        }

        public void NetSetWalk(SetEntityWalk packet)
        {
            
            if((NetworkTime.GetNetworkTime() - packet.timeStarted) >= WalkAnim.timeStep)
            {
                Debug.Log("Ignoring SetEntityWalk packet because the animation has passed");
            }
            else
            {
                net_cState = CharacterState.WALK;
                cState = net_cState;

                net_walkAnim = new WalkAnim(new Vector2Int(packet.fromX, packet.fromY), (int) packet.direction, packet.timeStarted);
                walkAnim = net_walkAnim;
                //_animator.SetWalk((uint) walkAnim.direction, walkAnim.timeStarted);
            }
        }

        public void NetSetAttack(SetEntityAttack packet)
        {
            if ((NetworkTime.GetNetworkTime() - packet.timeStarted) >= AttackAnim.timeStep)
            {
                Debug.Log("Ignoring SetEntityAttack packet because the animation has passed");
            }
            else
            {
                net_cState = CharacterState.ATTACK;
                cState = net_cState;

                net_attackAnim = new AttackAnim(net_direction, packet.timeStarted);

                if(characterDef.doll.weapon == 0)
                    AudioManager.Singleton.PlayAtPoint(9, net_position);
                else
                    AudioManager.Singleton.PlayAtPoint(14, net_position);
            }
        }

/*        public void NetSetPaperdoll(SetPaperdollSlot packet)
        {
            PaperdollManager.Singleton.EditPaperdoll(this.gameObject, characterDef, packet);
            animator.OnCharacterDefChange();

        }*/

        public void NetSetPaperdoll(byte slotIndex, uint itemId)
        {
            PaperdollManager.Singleton.EditPaperdoll(this.gameObject, characterDef, slotIndex, itemId);
            animator.OnCharacterDefChange();
        }


        //Teleport to new map
        public void NetSetMap(uint mapId)
        {
            //Clear animations
            switch(cState)
            {
                case CharacterState.WALK:
                    {
                        walkAnim.Clear();
                        cState = CharacterState.IDLE;
                        break;
                    }
                case CharacterState.ATTACK:
                    {
                        net_attackAnim.Clear();
                        cState = CharacterState.IDLE;
                        break;
                    }
            }
        }

        public void NetSetHealth(SetEntityHealth packet)
        {
            props.health = packet.health;
            props.maxHealth = packet.maxHealth;

            GameUI.Singleton.ShowHeadBar(this.gameObject, props.health, props.maxHealth, packet.deltaHp);
        }

        public void ClientSetWalk(int fromX, int fromY, int direction, long timeStarted)
        {
            if ((NetworkTime.GetNetworkTime() - timeStarted) > WalkAnim.timeStep)
            {
                //Ignoring
            }
            else
            {
                cState = CharacterState.WALK;
                walkAnim = new WalkAnim(new Vector2Int(fromX, fromY), direction, timeStarted);
                //_animator.SetWalk((uint) walkAnim.direction, walkAnim.timeStarted);
            }
        }


        void UpdateAnimation()
        {
            //ResetAnimatorParams();

            switch(cState)
            {
                case CharacterState.IDLE:
                    animator.SetIdle(direction);
                    //PlayIdleAnim();
                    break;

                case CharacterState.WALK:
                    //PlayWalkAnim();
                    animator.SetWalk((uint)walkAnim.direction, walkAnim.timeStarted);
                    break;

                case CharacterState.ATTACK:

                    animator.SetAttack((uint)net_attackAnim.direction, true, net_attackAnim.timeStarted);
                    break;

                default:
                   // PlayIdleAnim();
                    break;
            }
        }

        public void SetInputServerRpc(int direction, bool walking)
        {
            EOManager.Singleton.SendPacket(new RequestPlayerDir(direction, walking));
        }

        public void SetAttackInputServerRpc()
        {
            EOManager.Singleton.SendPacket(new RequestPlayerAttack());
        }

        public void ChangeDirectionServerRpc(int direction)
        {
            //Validity check
            if (direction < 0 || direction > 3)
                return;

            
            EOManager.Singleton.SendPacket(new RequestPlayerDir(direction, false));
        }

        public void MoveDirectionServerRpc(int direction)
        {
            if (direction < 0 || direction > 3)
                return;

            EOManager.Singleton.SendPacket(new RequestPlayerDir(direction, true));
        }


        // Update is called once per frame
        void Update()
        {
            if (isLocalPlayer && EOManager.eo_map.IsLoaded)
            {
                float inVert = Input.GetAxisRaw("Vertical");
                float inHor = Input.GetAxisRaw("Horizontal");
                bool attackKey = Input.GetKey(KeyCode.LeftControl);
                bool inputMoveHold = inputManager.Update(inHor, inVert, attackKey, Time.deltaTime);

                request_intervals.AddTime(Time.deltaTime);
                inputSyncInterval += Time.deltaTime;

                if (!ChestInvManager.Singleton.IsDisplayed)
                {
                    if (cState == CharacterState.IDLE)
                    {
                        if (inputManager.isAttackKey)
                        {

                        }
                        else if (inputMoveHold)
                        {
                            ClientSetWalk(net_position.x, net_position.y, inputManager.direction, NetworkTime.GetNetworkTime());
                        }
                        else if (inputManager.HasInput())
                        {
                            ClientSetDirection((uint)inputManager.direction);
                        }
                    }


                    if (inputSyncInterval >= 0.1f)
                    {
                        if (inputManager.HasInput())
                        {
                            bool walking = (cState == CharacterState.WALK);

                            SetInputServerRpc(inputManager.direction, walking);
                        }
                        //The frame after key was released
                        else if (inputManager.lastDirection >= 0)
                        {
                            bool walking = (cState == CharacterState.WALK);

                            SetInputServerRpc(inputManager.direction, walking);
                        }

                        if (inputManager.isAttackKey)
                        {
                            SetAttackInputServerRpc();
                        }

                        inputSyncInterval = 0.0f;
                    }
                }
            }
            UpdateAnimation();

            /*
            if (moveInput.duration > 0.0f)
                Debug.Log(moveInput.duration);
            */

            //Set position without any offsets
            //Vector3 cellWorldPos = mapGrid.CellToWorld(MapPosToVector3());
            //Vector3 cellWorldPos = EOManager.eo_map.PosToWorldSpace(Position);
            // Vector3 walkOffset = default;

            bool renderAtPos = true;

            //Offset the position in World Space for the walk animation
            if(walkAnim.valid)
            {
                float alpha = walkAnim.GetCappedAlpha();
                if(alpha < 1.0f)
                {
                    Vector3 cellWorldPos = EOManager.eo_map.CellToWorld(walkAnim.from);
                    Vector2Int posAfterWalk = EOMap.PositionAfterWalk(walkAnim.from, walkAnim.direction);

                    Vector3 toCellWorldPos = EOManager.eo_map.CellToWorld(posAfterWalk);
                    Vector3 deltaVec = toCellWorldPos - cellWorldPos;
                    Vector3 walkOffset = deltaVec * alpha;
                    //Debug.Log($"I'm walking! Alpha: {cappedAlpha}", this);
                    transform.position = cellWorldPos + walkOffset + new Vector3(0, 0.2f, 0);

                    renderAtPos = false;
                }
                else
                {
                    cState = CharacterState.IDLE;
                    walkAnim.Clear();
                }
                
            }
            else if(net_attackAnim.valid)
            {
                float alpha = net_attackAnim.GetCappedAlpha();

                if(alpha >= 1.0f)
                {
                    cState = CharacterState.IDLE;
                    net_attackAnim.Clear();
                }
            }
            
            if(renderAtPos)
            {
                Vector3 cellWorldPos = EOManager.eo_map.CellToWorld(net_position);
                //transform.position = cellWorldPos + new Vector3(0, 0.6f, 0);
                transform.position = cellWorldPos + new Vector3(0, 0.2f, 0);
            }


           

        }

        private void OnMouseDown()
        {
            Debug.Log("interact eo char");
        }
    }

}