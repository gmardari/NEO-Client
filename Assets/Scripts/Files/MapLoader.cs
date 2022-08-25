using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using Newtonsoft.Json;

namespace EO.Map
{
    public class MapLoader : MonoBehaviour
    {
        public bool doNotOverwrite = false;
        private GameObject mapObj;
        private Tilemap tileMap;
        string outputFileName = "map001";
        string inputFileName = "map001";
        string outputMapId = "0";
        string outputMapName = "";
        string testLogFilePath = Application.streamingAssetsPath + "/Test_Log/";
        string mapFolderPath = Application.streamingAssetsPath + "/maps/";
        

        // Start is called before the first frame update
        void Awake()
        {
            mapObj = GameObject.Find("RenderedMap");
            tileMap = mapObj.GetComponentInChildren<Tilemap>();

            Directory.CreateDirectory(testLogFilePath);
            Directory.CreateDirectory(mapFolderPath);

        }



        public MapContainer[] LoadMaps()
        {
            string[] mapFiles = Directory.GetFiles(mapFolderPath);

            if (mapFiles.Length == 0)
                return null;

            MapContainer[] containers = new MapContainer[mapFiles.Length];
            int k = 0;

            for (int i = 0; i < mapFiles.Length; i++)
            {
                string filePath = mapFiles[i];

                string[] split = filePath.Split('.');
                if (split.Length > 1)
                {

                    if (split[split.Length - 1] == "txt")
                    {
                        //remove .txt ending
                        string noFileEnding = filePath.Remove(filePath.Length - 1 - 3, 4);
                        Debug.Log(filePath);

                        MapContainer container = ReadMapFromFile(noFileEnding, true);

                        if (container != null)
                        {
                            containers[k++] = container;
                            /*
                            GameObject map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
                            map.GetComponent<NetworkObject>().Spawn(true);
                            EOMap eoMap = map.GetComponent<EOMap>();
                            eoMap.LoadMap(container, (k == 0));


                            map.name = "Map" + container.mapId.ToString();
                            */
                            Debug.Log($"Successfully loaded in map: {noFileEnding}");
                        }
                    }

                }

            }

            if(k > 0)
            {
                System.Array.Resize(ref containers, k);
                return containers;
            }

            return null;
        }

        private void OnGUI()
        {
            
            if(!EOManager.Connected)
            {
                /*
                GUILayout.BeginArea(new Rect(350, 10, 300, 600));
                StartWriteFields();
                GUILayout.EndArea();
                */
            }
           

                /*
                GUILayout.BeginArea(new Rect(700, 10, 300, 600));
                StartReadFields();
                GUILayout.EndArea();
                */
            

           
        }

        Vector2Int GetDimensions()
        {
            tileMap.CompressBounds();
            return (Vector2Int) tileMap.cellBounds.size;
        }

        Tile GetTile(Vector2Int pos)
        {
            return (Tile)tileMap.GetTile((Vector3Int)pos);
        }

        /*
        void StartWriteFields()
        {
            GUILayout.Label("Save map to file with name:");
            outputFileName = GUILayout.TextField(outputFileName);

            GUILayout.Label("ID:");
            outputMapId = GUILayout.TextField(outputMapId);

            GUILayout.Label("Map name:");
            outputMapName = GUILayout.TextField(outputMapName);

            if (GUILayout.Button("Save to file"))
            {
                int mapId;
                if(outputFileName.Length > 0)
                {
                    if(outputMapId.Length > 0 && int.TryParse(outputMapId, out mapId) && mapId >= 0)
                    {
                        Debug.Log("Saving map to file path: ");
                        SaveCurrentMap(outputFileName, mapId, outputMapName);
                    }
                    else
                    {
                        Debug.LogWarning("Map ID must be a positive integer");
                    }    
                    
                }
               
            }
        }

        void StartReadFields()
        {
            GUILayout.Label("Read map from file:");
            inputFileName = GUILayout.TextField(inputFileName);
            if (GUILayout.Button("Read from file"))
            {
                if (inputFileName != null && inputFileName.Length > 0)
                {
                    Debug.Log($"Reading map {inputFileName}:");
                    MapContainer container = ReadMapFromFile(inputFileName, false);
                    if(container != null)
                    {
                        //mapObj.LoadMap(container, true);
                        Debug.Log("Not implemented!");
                    }
                }

            }
        }
        */


        /*
        void SaveCurrentMap(string fileName, int mapId, string mapName)
        {
            string filePath = mapFolderPath + fileName + ".txt";

            if(doNotOverwrite && File.Exists(filePath))
            {
                Debug.LogWarning($"Map {filePath} already exists! Not saving to stop accidental saves.");
                return;
            }
            //TODO: find exceptions


            Vector2Int mapDimensions = GetDimensions();
            int xMin = tileMap.cellBounds.xMin;
            int yMin = tileMap.cellBounds.yMin;
            int xMax = tileMap.cellBounds.xMax;
            int yMax = tileMap.cellBounds.yMax;

            Debug.Log($"Saving map to {filePath}");
            MapContainer container = new MapContainer();
            container.eo_version = "0.1";
            container.mapId = mapId;
            container.mapName = mapName;

            container.width = mapDimensions.x;
            container.height = mapDimensions.y;
            container.minX = xMin;
            container.minY = yMin;
            container.groundLayer = new string[container.width * container.height];


            int i = 0;
            for(int x = xMin; x < xMax; x++)
            {
                for(int y = yMin; y < yMax; y++)
                {
                    string tileId = "0";
                    Tile tile = GetTile(new Vector2Int(x, y));

                    if(tile != null)
                    {
                        tileId = tile.name;
                    }
                    container.groundLayer[i] = tileId;
                    i++;
                }
            }

            string json = JsonUtility.ToJson(container);
            File.WriteAllText(filePath, json);
            

        }
        */

        public MapContainer ReadMapFromFile(string fileName, bool isFullPath)
        {
            string filePath;

            if (isFullPath)
                filePath = fileName + ".txt";
            else
                filePath = mapFolderPath + fileName + ".txt";

            
            
            if(File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                MapContainer container = JsonConvert.DeserializeObject<MapContainer>(text);
                Debug.Log($"Successfully loaded map {container.mapName}");

                return container;
            }
            else
            {
                //Debug.LogWarning($"Couldn't find map file {filePath}", this);
            }

            return null;
        }
    }
}