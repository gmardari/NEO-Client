using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EO;

public class DataFiles : MonoBehaviour
{
    public static DataFiles Singleton;
    public static string dataFileDirPath = Application.streamingAssetsPath + "/data/";
    public NpcDataFile npcDataFile;
    public ItemDataFile itemDataFile;

    // Start is called before the first frame update
    void Awake()
    {
        Singleton = this;
        Directory.CreateDirectory(dataFileDirPath);
        LoadDataFiles();

        //SaveExample();
    }

    public void LoadDataFiles()
    {
        if(npcDataFile == null)
            npcDataFile = ReadNpcDataFile();

        if(itemDataFile == null)
            itemDataFile = ReadItemDataFile();
    }

    public void SaveExample()
    {
        NpcDataFile dataFile = new NpcDataFile();
        dataFile.EO_VERSION = "0.1";
        dataFile.Entries = new List<NpcDataEntry>();
        dataFile.Entries.Add(new NpcDataEntry { GfxId = 0, Name = "Sheep", NpcType = (int)NpcType.MOB_PASSIVE, MaxHealth = 10 });
        SaveDataFile(dataFile, true);

    }

    NpcDataFile ReadNpcDataFile()
    {
        string filePath = dataFileDirPath + "dat001.txt";
        NpcDataFile dataFile;

        if(File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            if(json != null && json.Length > 0)
            {
                dataFile = JsonConvert.DeserializeObject<NpcDataFile>(json);
                return dataFile;
            }
        }

        return null;
    }

    ItemDataFile ReadItemDataFile()
    {
        string filePath = dataFileDirPath + "dat002.txt";
        ItemDataFile dataFile;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            if (json != null && json.Length > 0)
            {
                dataFile = JsonConvert.DeserializeObject<ItemDataFile>(json);
                return dataFile;
            }
        }

        return null;
    }

    public ItemDataEntry GetItemData(uint itemId)
    {
        return itemDataFile.entries[(int)itemId];
    }

    public void SaveDataFile(NpcDataFile dataFile, bool overwrite)
    {
        string outputFilePath = dataFileDirPath + "dat001.txt";
        if(File.Exists(outputFilePath) && !overwrite)
        {
            Debug.LogWarning($"Failed to save Npc data file to {outputFilePath} because of existing data file.");

            return;
        }

        string json = JsonConvert.SerializeObject(dataFile);
        File.WriteAllText(outputFilePath, json);

        Debug.Log("Saved data file");
    }


}
