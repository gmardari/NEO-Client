using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;

namespace EO.Map
{
    public class EOMap : MonoBehaviour
    {
        public GameObject playerPrefab;
        public GameObject npcPrefab;
        public GameObject itemPrefab;
        public GameObject chestPrefab;
        public GameObject entitiesContainer;
        public GameObject itemsContainer;

        private GameObject render_mapObj;
        private Grid grid;
        private Tilemap[] layers;
        private Tilemap ground_layer;
        private Tilemap wall_down_layer;
        private Tilemap wall_right_layer;
        private Tilemap special_layer;
        private MapLoader mapLoader;

        private Dictionary<ulong, GameObject> entities;
        private const int PacketQueueSize = 25;
        private Queue<Packet> packetQueue;
        private ulong characterId;

        private uint mapId;
        private string mapName;
        private BoundsInt mapBounds;
        private Cell[,] cells;
        public Vector2Int? cursorPos;
        private bool isLoaded;
        private bool readyToLoad;
        private bool isLoadingProcess;
        private bool loadingError;

        private Rect dropWindowRect = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);
        private bool showDropWindow = false;
        private string dropWindowInputString = "1";
        private int dropWindowOutput = -1;

        public Grid Grid
        { get { return grid; } }
        public Tilemap TileMap
        { get { return GetComponent<Tilemap>(); } }
        public BoundsInt MapBounds
        { get { return mapBounds; } }
        public uint MapId
        { get { return mapId; } }
        public string MapName
        { get { return mapName;  }  }
        public bool IsLoaded
        { get { return isLoaded; } }

        private void Awake()
        {
           // Debug.Log("Map name: " + name);
            render_mapObj = GameObject.Find("RenderedMap");
            grid = render_mapObj.GetComponent<Grid>();

            layers = new Tilemap[render_mapObj.transform.childCount];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = render_mapObj.transform.GetChild(i).GetComponent<Tilemap>();
            }

            //tileMap = map.transform.GetChild(0).GetComponent<Tilemap>();
            ground_layer = render_mapObj.transform.Find("Ground").GetComponent<Tilemap>();
            wall_down_layer = render_mapObj.transform.Find("WallsDown").GetComponent<Tilemap>();
            wall_right_layer = render_mapObj.transform.Find("WallsRight").GetComponent<Tilemap>();
            special_layer = render_mapObj.transform.Find("Special").GetComponent<Tilemap>();

            mapLoader = GetComponent<MapLoader>();
            entities = new Dictionary<ulong, GameObject>();
            packetQueue = new Queue<Packet>(PacketQueueSize);
            //TODO: Is mapbounds xMax and yMax proper?
        }

        //Called when start loading in a new map
        public void SetMap(uint _mapId, ulong _characterId)
        {
            (mapId, characterId) = (_mapId, _characterId);

            if (isLoaded)
                UnloadMap();

            SetupMap();

            if(EOManager.player != null)
            {
                EOManager.EO_Character.NetSetMap(mapId);
            }

            //isLoadingProcess = true; //Start loading entities and characters
            //readyToLoad = true;


        }

        //Called after SetMap and all updates are sent to client from server
        public void SetReady()
        {
            if (isLoadingProcess)
                readyToLoad = true;
        }


        private void Update()
        {
            if(!isLoaded)
            {
                if(readyToLoad)
                    SetupMap();
            }
            else
            {
                //Operations on packets
                while(packetQueue.Count > 0)
                {
                    HandlePackets();
                }

            }
        }


        private bool SetupMap()
        {
            MapContainer container = mapLoader.ReadMapFromFile("map" + mapId.ToString("D3"), false);

            if (container != null)
            {
                if(LoadMap(container, true))
                {
                    Debug.Log($"Loaded map {mapName}, id:{mapId}");

                    //This Monobehaviour would be disabled if failed to previously load map
                    if (!this.enabled)
                        this.enabled = true;

                    return true;
                }
                else
                {
                    Debug.LogWarning($"Failed to load in map {mapName}, id:{mapId}");
                }

            }
            else
            {
                Debug.LogWarning($"Error loading map id:{mapId}");
                this.enabled = false;
            }

            return false;
        }

        public bool LoadMap(MapContainer container, bool draw)
        {
            int i = 0;

            mapId = (uint)container.mapId;
            mapName = container.mapName;
            mapBounds = new BoundsInt(container.minX, container.minY, 0, container.width, container.height, 0);
            Debug.Log($"Map Bounds: ({mapBounds.xMin},{mapBounds.yMin}) to ({mapBounds.xMax},{mapBounds.yMax})");
            cells = new Cell[container.width, container.height];
            /*
            if (draw)
            {
                ground_layer.ClearAllTiles();
                wall_down_layer.ClearAllTiles();
                wall_right_layer.ClearAllTiles();
                special_layer.ClearAllTiles();
            }
            */
            try
            {
                //ClearEntities(true);
                //LoadGroundLayer(in container, draw);
                //LoadWallsLayer(in container, draw);
                LoadLayers(container);

                //AddItemEntity(100, 0, new Vector2Int(0, 0));

                isLoaded = true;
                readyToLoad = false;
                isLoadingProcess = false;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                readyToLoad = false;
                loadingError = true;
                isLoadingProcess = false;
            }



            return false;
        }

        public void UnloadMap()
        {
            isLoaded = false;

            ClearTiles();
            ClearEntities(true);
        }

        public void ClearTiles()
        {
            foreach(var tilemap in layers)
            {
                tilemap.ClearAllTiles();
            }
        }

        private void LoadLayers(MapContainer container)
        {
            int i = 0;
            for (int x = mapBounds.xMin; x < mapBounds.xMax; x++)
            {
                for (int y = mapBounds.yMin; y < mapBounds.yMax; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    int groundTileId = container.groundLayer[i];
                    int objTileId = container.objectsLayer[i];
                    int overlayTileId = container.overlayLayer[i];
                    int wallsDownTileId = container.wallsDownLayer[i];
                    int wallsRightTileId = container.wallsRightLayer[i];
                    int specTilespec = container.specialLayer.tiles[i];
                    

                    int xShifted = x - mapBounds.xMin;
                    int yShifted = y - mapBounds.yMin;


                    cells[xShifted, yShifted] = new Cell(pos, groundTileId, specTilespec);
                    //cells[xShifted, yShifted].groundLayerId = groundTileId;

                    //Add walls
                    if (groundTileId >= 0)
                    {
                        ground_layer.SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.groundLayerTiles[groundTileId]);
                    }
                    else
                    {
                        //cells[i].type = CellType.WALL;
                    }

                    if (objTileId >= 0)
                        GetLayer(MapLayer.OBJECTS).SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.objLayerTiles[objTileId]);

                    if(overlayTileId >= 0)
                        GetLayer(MapLayer.OVERLAY).SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.overlayLayerTiles[overlayTileId]);

                    if (wallsDownTileId >= 0)
                        GetLayer(MapLayer.WALLS_DOWN).SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.wallLayerTiles[wallsDownTileId]);

                    if (wallsRightTileId >= 0)
                        GetLayer(MapLayer.WALLS_RIGHT).SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.wallLayerTiles[wallsRightTileId]);

                    i++;
                }
            }

            
            for(int j = 0; j < layers.Length; j++)
            {
                layers[j].CompressBounds();
            }
            
        }

        /*private void LoadGroundLayer(in MapContainer container, bool draw)
        {
            int i = 0;
            for (int x = mapBounds.xMin; x < mapBounds.xMax; x++)
            {
                for (int y = mapBounds.yMin; y < mapBounds.yMax; y++)
                {
                    int tileId = container.groundLayer[i];

                    int xShifted = x - mapBounds.xMin;
                    int yShifted = y - mapBounds.yMin;


                    cells[xShifted, yShifted] = new Cell();
                    cells[xShifted, yShifted].groundLayerId = tileId;

                    //Add walls
                    if (tileId >= 0)
                    {
                        //TODO: Move to another loop
                        if (draw)
                            ground_layer.SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.groundLayerTiles[tileId]);

                        //cells[i].type = CellType.NONE;
                    }
                    else
                    {
                        //cells[i].type = CellType.WALL;
                    }


                    i++;
                }
            }

            if (draw)
            {
                ground_layer.CompressBounds();
            }
        }*/

        /*private void LoadWallsLayer(in MapContainer container, bool draw)
        {
            int i = 0;
            for (int x = mapBounds.xMin; x < mapBounds.xMax; x++)
            {
                for (int y = mapBounds.yMin; y < mapBounds.yMax; y++)
                {
                    int tileIdDown = container.wallsDownLayer[i];
                    int tileIdRight = container.wallsRightLayer[i];

                    if (tileIdDown >= 0)
                    {
                        //TODO: Move to another loop
                        if (draw)
                            wall_down_layer.SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.wallLayerTiles[tileIdDown]);

                    }

                    if (tileIdRight >= 0)
                        if (draw)
                            wall_right_layer.SetTile(new Vector3Int(x, y, 0), ResourceLibrary.Singleton.wallLayerTiles[tileIdRight]);

                    i++;
                }
            }


            if (draw)
            {
                wall_down_layer.CompressBounds();
                wall_right_layer.CompressBounds();
            }
        }*/

        private void HandlePackets()
        {
            var packet = packetQueue.Dequeue();

            if (packet is SetEntityDef)
            {
                
                var cp = (SetEntityDef)packet;

                //TODO: Implement chest entity
                if (cp.entityType == (uint)EntityType.CHEST)
                {
                    Debug.Log($"Got chest entity at x: {cp.posX}, y: {cp.posY}");
                    var chest = Instantiate(chestPrefab, entitiesContainer.transform);
                    chest.GetComponent<EntityDef>().Init(cp.entityId, EntityType.CHEST, this.mapId, new Vector2Int(cp.posX, cp.posY));
                    entities[cp.entityId] = chest;
                }
                
            }
            else if(packet is SetCharacterDef)
            {
                var cp = packet as SetCharacterDef;

                //If the entity exists, remove and update it!
                //TODO: Consider updating the character rather than deleting and renewing
               // AddCharacter(cp);

            }
            else if (packet is SetNpcDef)
            {
                var cp = packet as SetNpcDef;

                //AddNpc(cp);
            }
            else if (packet is SetItemDef)
            {
                var cp = packet as SetItemDef;

                //AddItemEntity(cp.entityId, cp.itemId, cp.quantity, new Vector2Int(cp.x, cp.y));
            }
            else if (packet is SetPlayerInvItem)
            {
                var cp = packet as SetPlayerInvItem;

                Debug.Log($"SetPlayerInvItem item id: {cp.itemId}, pos: ({cp.x}, {cp.y} quant:{cp.quantity})");
                //InvManager.Singleton.OnSetItemDef(new Vector2Int((int) cp.x, (int) cp.y), cp.itemId, cp.quantity);
            }
            else if (packet is SetChestInvItem)
            {
                var cp = packet as SetChestInvItem;
                Debug.Log("Chest item");

                ChestInvManager.Singleton.NetSetItem(cp);
            }
            else if(packet is SetPaperdollSlot)
            {
               /* var cp = packet as SetPaperdollSlot;
                var entity = entities[cp.entityId];

                if(entity != null)
                {
                    var eo_char = entity.GetComponent<EOCharacter>();

                    if (eo_char != null)
                        eo_char.NetSetPaperdoll(cp);
                }*/
               
            }
            else if (packet is SetEntityDir)
            {
                var cp = (SetEntityDir)packet;
                GameObject entityObj;

                if (entities.TryGetValue(cp.entityId, out entityObj))
                {
                    var entityDef = entityObj.GetComponent<EntityDef>();

                    //Is character entity
                    if (entityDef != null)
                    {
                        switch (entityDef.entityType)
                        {
                            case EntityType.PLAYER:

                                var character = entityObj.GetComponent<EOCharacter>();
                                character.NetSetDirection(cp.direction);
                                break;

                            case EntityType.NPC:
                                var npc = entityObj.GetComponent<EONpc>();
                                npc.NetSetDirection(cp.direction);
                                break;
                        }

                    }
                }
            }
            else if (packet is SetEntityPos)
            {
                var cp = (SetEntityPos)packet;
                GameObject entityObj;

                if (entities.TryGetValue(cp.entityId, out entityObj))
                {
                    var entityDef = entityObj.GetComponent<EntityDef>();

                    if (entityDef != null)
                    {
                        entityDef.NetSetPosition(new Vector2Int(cp.posX, cp.posY));
                    }
                }
            }
            else if (packet is SetEntityWalk)
            {
                var cp = (SetEntityWalk)packet;
                GameObject entityObj;

                //Debug.Log("Got entity walk!");

                if (entities.TryGetValue(cp.entityId, out entityObj))
                {
                    var entityDef = entityObj.GetComponent<EntityDef>();

                    if (entityDef != null)
                    {
                        if (entityDef.entityType == EntityType.PLAYER)
                        {
                            var eo_character = entityObj.GetComponent<EOCharacter>();
                            eo_character.NetSetDirection(cp.direction);
                            eo_character.NetSetWalk(cp);
                        }
                        else if (entityDef.entityType == EntityType.NPC)
                        {
                            var eo_npc = entityObj.GetComponent<EONpc>();
                            eo_npc.NetSetDirection(cp.direction);
                            eo_npc.NetSetWalk(cp);
                        }
                    }
                }
            }
            else if (packet is SetEntityAttack)
            {
                var cp = (SetEntityAttack) packet;
                GameObject entityObj;
               

                if (entities.TryGetValue(cp.entityId, out entityObj))
                {
                    var entityDef = entityObj.GetComponent<EntityDef>();

                    if (entityDef != null)
                    {
                        if (entityDef.entityType == EntityType.PLAYER)
                        {
                            var eo_character = entityObj.GetComponent<EOCharacter>();
                            eo_character.NetSetAttack(cp);
                        }
                        else if (entityDef.entityType == EntityType.NPC)
                        {
                            var eo_npc = entityObj.GetComponent<EONpc>();
                            eo_npc.NetSetAttack(cp);
                        }
                    }
                }
            }
            else if(packet is SetEntityProp)
            {
                var cp = (SetEntityProp) packet;
                EntityProperty propType = (EntityProperty)cp.propType;
                

                Debug.Log($"Got packet for entity id {cp.entityId} of type {propType} of value {cp.propValue}");

                if (entities.TryGetValue(cp.entityId, out GameObject entObj))
                {
                    switch (propType)
                    {
                        case EntityProperty.HEALTH:
                            {
                                EntityDef def = entObj.GetComponent<EntityDef>();

                                switch(def.entityType)
                                {
                                    case EntityType.PLAYER:
                                        {
                                            EOCharacter eo_char = entObj.GetComponent<EOCharacter>();
                                            //eo_char.NetSetHealth((ulong)cp.propValue);
                                            //eo_char.props.health = (ulong) cp.propValue;
                                            break;
                                        }
                                    case EntityType.NPC:
                                        {
                                            EONpc eo_npc = entObj.GetComponent<EONpc>();
                                            //eo_npc.NetSetHealth((ulong)cp.propValue);
                                            break;
                                        }
                                }
                               

                                
                                break;
                            }
                    }
                }
                else
                    Debug.LogWarning("[SET ENTITY PROP] Entity not found");
               
            }
            else if (packet is SetEntityHealth)
            {
                var cp = (SetEntityHealth) packet;
                GameObject entityObj;


                if (entities.TryGetValue(cp.entityId, out entityObj))
                {
                    var entityDef = entityObj.GetComponent<EntityDef>();

                    if (entityDef != null)
                    {
                        if (entityDef.entityType == EntityType.PLAYER)
                        {
                            var eo_character = entityObj.GetComponent<EOCharacter>();
                            eo_character.NetSetHealth(cp);
                        }
                        else if (entityDef.entityType == EntityType.NPC)
                        {
                            var eo_npc = entityObj.GetComponent<EONpc>();
                            eo_npc.NetSetHealth(cp);
                        }
                    }
                }
            }
            else if (packet is RemoveEntity)
            {
                var cp = (RemoveEntity) packet;

                RemoveEntity(cp);
            }
            else if(packet is InitPlayerVals)
            {
                var cp = packet as InitPlayerVals;

                if(EOManager.EO_Character != null)
                {
                    Debug.Log("Init vals");
                    EOManager.EO_Character.props = new CharProperties(cp);
                }
            }
            else if (packet is ChestOpen)
            {
                var cp = packet as ChestOpen;

                ChestInvManager.Singleton.NetOpenChest(cp.entityId);
            }
        }

        public void AddNpc(ulong entityId, Vector2Int pos, uint direction, uint npcId, ulong health, ulong maxHealth)
        {
            GameObject npcObj = Instantiate(npcPrefab, entitiesContainer.transform);
            EntityDef def = npcObj.GetComponent<EntityDef>();

            def.Init(entityId, EntityType.NPC, mapId, pos);
            npcObj.GetComponent<EONpc>().Init((int) npcId);

            entities[entityId] = npcObj;
        }

        public void AddCharacter(ulong entityId, Vector2Int pos, uint direction, ulong health, ulong maxHealth, CharacterDef def)
        {
            if (entities.ContainsKey(entityId))
            {
                RemoveEntity(entityId);
            }

            GameObject playerObj = Instantiate(playerPrefab, entitiesContainer.transform);

            //playerObj.GetComponent<EntityDef>().Init(castedPacket.entityId, (EntityType) castedPacket.entityType, mapId, new Vector2Int(castedPacket.posX, castedPacket.posY));
            playerObj.GetComponent<EntityDef>().Init(entityId, EntityType.PLAYER, mapId, pos);
            entities[entityId] = playerObj;

            bool isLocalPlayer = (entityId == characterId);
            EOCharacter eo_char = playerObj.GetComponent<EOCharacter>();
            eo_char.Init(def, isLocalPlayer);

            //Set Local Player
            if (isLocalPlayer)
                EOManager.player = playerObj;

            Debug.Log($"Set character def id {entityId}");
        }

        public GameObject GetEntityObj(ulong entityId)
        {
            if(entities.TryGetValue(entityId, out GameObject obj))
            {
                return obj;
            }

            throw new KeyNotFoundException($"Entity id {entityId} doesn't exist.");
        }

        public EOCharacter GetCharacter(ulong entityId)
        {
            if (entities.TryGetValue(entityId, out GameObject obj))
            {
                var eo_char = obj.GetComponent<EOCharacter>();

                if (eo_char == null)
                    throw new Exception($"Entity id {entityId} isn't a character.");

                return eo_char;
            }
            else
                throw new KeyNotFoundException($"Character id {entityId} doesn't exist.");
        }

        public void RemoveEntity(RemoveEntity packet)
        {
            RemoveEntity(packet.entityId);
        }

        public void RemoveEntity(ulong entityId)
        {
            if(entities.Remove(entityId, out GameObject entityObj))
            {
                Vector2Int net_pos = utils.GetEntityNetPos(entityObj);
                GetCell(net_pos).RemoveEntityFromId(entityId);
                Destroy(entityObj);
            }
        }

        public void EnqueuePacket(Packet packet)
        {
            packetQueue.Enqueue(packet);
        }

        public void ClearEntities(bool keepLocalPlayer)
        {
            var hasLocalPlayer = (EOManager.player != null);

            foreach(var key in entities.Keys)
            {
                var ent = entities[key];

                if (keepLocalPlayer && hasLocalPlayer && GameObject.ReferenceEquals(ent, EOManager.player))
                {
                    Debug.Log("Skipping " + key);
                        continue;
                }
                Debug.Log("Removing entity with key: " + key);
                GameObject.Destroy(ent);
            }

            entities.Clear();

            if(keepLocalPlayer && hasLocalPlayer)
            {
                ulong id = EOManager.EO_Character.entityDef.entityId;
                entities.Add(id, EOManager.player);
            }
               
            
        }


        public void AddItem(uint itemId, uint quantity, byte layer, Vector2Int pos)
        {
            Cell cell = GetCell(pos);

            //Position gameObject in world space
            Vector3 worldSpace = CellToWorld(pos);
            worldSpace.y += 0.25f; //16 pixels up, tile.height / 2

            GameObject obj = Instantiate(itemPrefab, worldSpace, Quaternion.identity, itemsContainer.transform);
            obj.GetComponent<SpriteRenderer>().sortingOrder = 1 + layer; //Add 1 so it's on top of ground layer
            
            //Setup item object
            Item item = new Item(itemId, quantity, layer, obj);
            //item.GetDef().Init(entityId, EntityType.ITEM, mapId, cellPos);
            
            //Add to arrays
            cell.AddItem(item);
            //entities[entityId] = obj;
        }

        public Cell GetCell(Vector2Int position)
        {
            if (OutOfMapBounds(position))
                return null;

            int xShifted = position.x - mapBounds.xMin;
            int yShifted = position.y - mapBounds.yMin;

            return cells[xShifted, yShifted];
        }

        public Vector2Int GetDimensions()
        {
            return (Vector2Int) mapBounds.size;
        }

        public static Vector2Int PositionAfterWalk(Vector2Int current, int direction)
        {
            if (direction == 0)
                return current + new Vector2Int(-1, 0);
            if (direction == 1)
                return current + new Vector2Int(0, -1);
            if (direction == 2)
                return current + new Vector2Int(1, 0);
            if (direction == 3)
                return current + new Vector2Int(0, 1);

            return current;
        }


        public Vector3 CellToWorld(Vector2Int mapPos)
        {
            return grid.CellToWorld((Vector3Int)mapPos);
        }

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            Vector3Int cellPos3 = ground_layer.WorldToCell(worldPos);
            return new Vector2Int(cellPos3.x, cellPos3.y);
        }

        public Tile GetTile(Vector2Int mapPos)
        {
            return (Tile) ground_layer.GetTile((Vector3Int)mapPos);
           
        }

        public Tilemap GetLayer(MapLayer layer)
        {
            return layers[(int)layer];
        }

        public bool OutOfMapBounds(Vector2Int pos)
        {
            if (pos.x < mapBounds.xMin || pos.x >= mapBounds.xMax || pos.y < mapBounds.yMin || pos.y >= mapBounds.yMax)
                return true;

            return false;
        }
        public bool CanMoveToPos(Vector2Int pos)
        {
            if (OutOfMapBounds(pos))
                return false;

            //Debug.Log(WorldToCell(pos));

            Cell cell = GetCell(pos);

            if (!cell.IsWall())
                return true;

            /*
            Tile tile = GetTile(pos);

            if (tile != null)
            {
                return true;
            }

            return false;
            */


            return false;

        }

        private void DoModalWindow(int windowId)
        {
            //Drop window
            if (windowId == 0)
            {
                GUI.Label(new Rect(10, 10, 180, 20), "Enter number items to drop");
                dropWindowInputString = GUI.TextField(new Rect(10, 30, 100, 20), dropWindowInputString);
                if (GUI.Button(new Rect(10, 50, 30, 20), "OK"))
                {

                }

                if (GUI.Button(new Rect(50, 50, 50, 20), "Cancel"))
                {

                }

            }

        }

        //TODO: Clip item hints
        private void OnGUI()
        {
            if (showDropWindow)
            {
                dropWindowRect = GUI.ModalWindow(0, dropWindowRect, DoModalWindow, "Drop Items");
            }


            //Display item hints on items with the cursor on top of them
            if (cursorPos is Vector2Int cursorPosVal)
            {
                Item item = GetCell(cursorPosVal).PopItem();

                if (item != null)
                {
                    var worldPos = CellToWorld(cursorPosVal);
                    var screenPos = Camera.main.WorldToScreenPoint(worldPos + new Vector3(0, 0.5f, 0f));
                    //Debug.Log($"World pos: {worldPos}, Screen pos: {screenPos}");
                    int textRectX = 100;
                    int textRectY = 30;
                    Vector2 imGuiPos = utils.ScreenToIMGUISpace(screenPos.x - (textRectX / 2), screenPos.y + textRectY);

                    GUI.Label(new Rect(imGuiPos.x, imGuiPos.y, textRectX, 30), item.Name);
                }
            }
        }




        public override string ToString()
        {
            return $"Map id: {mapId}, width: {mapBounds.size.x}, height: {mapBounds.size.y}, origin-x: {mapBounds.xMin}, origin-y: {mapBounds.yMin}";
        }


    }
}