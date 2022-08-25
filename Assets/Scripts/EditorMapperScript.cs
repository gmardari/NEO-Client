using EO.Map;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using EO;

public class EditorMapperScript : MonoBehaviour
{
    public int mapId;
    public string mapName;
    public string outputFolder;
    public bool doNotOverwrite;

    private Tilemap ground_layer;
    private Tilemap wall_down_layer;
    private Tilemap wall_right_layer;
    private Tilemap special_layer;

    public void Hello()
    {
        Debug.Log("Hello Editor!");
    }

    private void Init()
    {
        ground_layer = transform.Find("Ground").GetComponent<Tilemap>();
        wall_down_layer = transform.Find("WallsDown").GetComponent<Tilemap>();
        wall_right_layer = transform.Find("WallsRight").GetComponent<Tilemap>();
        special_layer = transform.Find("Special").GetComponent<Tilemap>();
    }

    private Vector2Int GetDimensions()
    {
        ground_layer.CompressBounds();
        return (Vector2Int) ground_layer.cellBounds.size;
    }

    Tile GetTile(MapLayer layer, Vector2Int pos)
    {
        switch (layer)
        {
            case MapLayer.GROUND:
                return (Tile) ground_layer.GetTile((Vector3Int)pos);
            case MapLayer.WALLS_DOWN:
                return (Tile) wall_down_layer.GetTile((Vector3Int)pos);
            case MapLayer.WALLS_RIGHT:
                return (Tile) wall_right_layer.GetTile((Vector3Int)pos);
            case MapLayer.SPECIAL:
                return (Tile) special_layer.GetTile((Vector3Int)pos);

        }


        return null;
    }

    //TODO:Save and load layer id's as ints, not strings
    public void SaveCurrentMap()
    {
        Init();

        string outDirPath = Application.streamingAssetsPath + @"\" + outputFolder;
        string filePath = outDirPath + @"\" +"map" + mapId.ToString("D3") + ".txt";

        Directory.CreateDirectory(outDirPath);

        if (doNotOverwrite && File.Exists(filePath))
        {
            Debug.LogWarning($"Map {filePath} already exists! Not saving to stop accidental saves.");
            return;
        }
        //TODO: find exceptions


        Vector2Int mapDimensions = GetDimensions();
        int xMin = ground_layer.cellBounds.xMin;
        int yMin = ground_layer.cellBounds.yMin;
        int xMax = ground_layer.cellBounds.xMax;
        int yMax = ground_layer.cellBounds.yMax;

        Debug.Log($"Saving map to {filePath}");
        MapContainer container = new MapContainer();
        container.eo_version = "0.2";
        container.mapId = mapId;
        container.mapName = mapName;

        container.width = mapDimensions.x;
        container.height = mapDimensions.y;
        container.minX = xMin;
        container.minY = yMin;
        
        container.groundLayer = new int[container.width * container.height];
        container.wallsDownLayer = new int[container.width * container.height];
        container.wallsRightLayer = new int[container.width * container.height];
        container.specialLayer = new MapSpecialLayerInfo(container.width * container.height);


        SaveGroundLayer(xMin, xMax, yMin, yMax, ref container);
        SaveWallsLayer(xMin, xMax, yMin, yMax, ref container);
        SaveSpecialLayer(xMin, xMax, yMin, yMax, ref container);


        //string json = JsonUtility.ToJson(container);
        string json = JsonConvert.SerializeObject(container);
        File.WriteAllText(filePath, json);


    }

    private void SaveGroundLayer(int xMin, int xMax, int yMin, int yMax, ref MapContainer container)
    {
        int i = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                int tileId = -1;
                Tile tile = GetTile(MapLayer.GROUND, new Vector2Int(x, y));

                if (tile != null)
                {
                    tileId = int.Parse(tile.name);
                }
                container.groundLayer[i] = tileId;
                i++;
            }
        }
    }

    private void SaveWallsLayer(int xMin, int xMax, int yMin, int yMax, ref MapContainer container)
    {
        int i = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                int tileId = -1;
                Tile tile = GetTile(MapLayer.WALLS_DOWN, new Vector2Int(x, y));

                if (tile != null)
                {
                    tileId = int.Parse(tile.name);
                }
                container.wallsDownLayer[i] = tileId;
                i++;
            }
        }

        i = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                int tileId = -1;
                Tile tile = GetTile(MapLayer.WALLS_RIGHT, new Vector2Int(x, y));

                if (tile != null)
                {
                    tileId = int.Parse(tile.name);
                }
                container.wallsRightLayer[i] = tileId;
                i++;
            }
        }

    }

    private void SaveSpecialLayer(int xMin, int xMax, int yMin, int yMax, ref MapContainer container)
    {
        int i = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                //TODO: Make it start at 0 
                int tileId = -1;
                
                Tile tile = GetTile(MapLayer.SPECIAL, new Vector2Int(x, y));

                if (tile != null)
                {
                   tileId = int.Parse(tile.name);

                    switch (tileId)
                    {
                        case (int)MapSpecialIndex.NPC_SPAWN:

                            if (container.specialLayer.npcSpawnList == null)
                                container.specialLayer.npcSpawnList = new List<MapNpcSpawnInfo>();

                            container.specialLayer.npcSpawnList.Add(new MapNpcSpawnInfo
                            { npcId = 1 , fidgetTimeMin = 1, fidgetTimeMax = 2, respawnTimeMin = 3, respawnTimeMax = 5});
                            break;

                        case (int)MapSpecialIndex.WARP:

                            if (container.specialLayer.warpList == null)
                                container.specialLayer.warpList = new List<MapWarpInfo>();

                            container.specialLayer.warpList.Add(new MapWarpInfo
                            { mapId = 2, x = 0, y = 0, direction = 2 });
                            break;
                    }
                    
                }

                

                container.specialLayer.tiles[i] = tileId;

                

                i++;
            }
        }
    }

}
