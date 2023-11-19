using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Data_Manager : MonoBehaviour
{
    [SerializeField][Range(0, 1)] float rainPercent = 0;
    [SerializeField][Range(0, 1)] float snowPercent = 0;
    [SerializeField] Vector3 mapAnimatronicPlacementSpot;
    [SerializeField] float worldFlyingSphereSize;
    [SerializeField] SaveFileData saveFileData;
    [SerializeField] MapData mapData;
    [SerializeField] bool retryMap;

    private void Awake()
    {
        string saveFilePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml";

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("You entered a map without a save file");
            SceneManager.LoadScene(0);
        }
        else
        {
            saveFileData = saveFileData.DeserializeFromXML(File.ReadAllText(saveFilePath));
        }

        if(saveFileData == null)
        {
            Debug.LogError("You entered a map with a corrupt save file");
            SceneManager.LoadScene(0);
        }

        string mapSavePath = Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml";
        if (!File.Exists(mapSavePath))
        {
            GenerateNewRandomMapSaveData();
        }
        else
        {
            mapData = mapData.DeserializeFromXML(File.ReadAllText(mapSavePath));
        }
        ApplySaveFileValuesToScene();
    }

    private void Update()
    {
        if(retryMap)
        {
            retryMap = false;
            GenerateNewRandomMapSaveData();
            ApplySaveFileValuesToScene();
        }
        saveFileData.timeElapsed += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/SaveFile.xml", saveFileData.SerializeToXML());
        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
    }

    public int GetSeed()
    {
        return saveFileData.worldSeed;
    }

    void ApplySaveFileValuesToScene()
    {
        Custom_Geometry[] customGeo = FindObjectsByType<Custom_Geometry>(FindObjectsSortMode.None);

        bool addedNewGeoData = false;
        for (int i = 0; i < customGeo.Length; i++)
        {
            int check = CheckMapDataForString(customGeo[i].GetKey());
            if (check == -1)
            {
                Color setColor = Color.white;
                switch (customGeo[i].GetMaterialColorType())
                {
                    case Custom_Geometry.MaterialColorType.primary:
                        setColor = mapData.startingColorPrimary;
                        break;
                    case Custom_Geometry.MaterialColorType.secondary:
                        setColor = mapData.startingColorSecondary;
                        break;
                    case Custom_Geometry.MaterialColorType.tertiary:
                        setColor = mapData.startingColorTertiary;
                        break;
                    case Custom_Geometry.MaterialColorType.light:
                        setColor = mapData.startingColorLight;
                        break;
                    case Custom_Geometry.MaterialColorType.dark:
                        setColor = mapData.startingColorDark;
                        break;
                    default:
                        break;
                }
                mapData.geometryData.Add(new CustomGeometryData()
                {
                    color = setColor,
                    key = customGeo[i].GetKey(),
                    material = customGeo[i].GetMaterial(),//This needs to be calculated randomly
                    grime = Random.Range(0.0f, 1.0f),//This needs to be calculated randomly
                });
                addedNewGeoData = true;
                check = mapData.geometryData.Count - 1;
            }
            customGeo[i].SetColor(mapData.geometryData[check].color);
            customGeo[i].SetMaterial(mapData.geometryData[check].material);
            customGeo[i].SetGrime(mapData.geometryData[check].grime);
        }
        for (int i = 0; i < mapData.animatronics.Count; i++)
        {
            GameObject animatronic = GameObject.Instantiate(new GameObject());
            animatronic.name = mapData.animatronics[i].objectHash.ToString();
            if (mapData.animatronics[i].yetToBePlaced)
            {
                animatronic.transform.position = mapAnimatronicPlacementSpot;
                mapData.animatronics[i].yetToBePlaced = false;
                addedNewGeoData = true;
            }
            else
            {
                animatronic.transform.position = mapData.animatronics[i].position;
                animatronic.transform.rotation = mapData.animatronics[i].rotation;
            }
            animatronic.AddComponent<Rigidbody>();
            Combo_Animatronic combo = animatronic.AddComponent<Combo_Animatronic>();
            combo.InsertDeadInterface(this.GetComponent<DEAD_Interface>());
            combo.ReassignFullSaveFile(mapData.animatronics[i]);
        }

        if(addedNewGeoData)
        {
            DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
        }
    }

    int CheckMapDataForString(string key)
    {
        for (int i = 0; i < mapData.geometryData.Count; i++)
        {
            if (mapData.geometryData[i].key == key)
            {
                return i;
            }
        }
        return -1;
    }

    void GenerateNewRandomMapSaveData()
    {
        mapData = new MapData();
        mapData.mapHistory = new List<MapHistory>();
        mapData.geometryData = new List<CustomGeometryData>();

        mapData.mapHistory.Add(new MapHistory(){ 
            change = PlayerPrefs.GetString("CreateWorldName"), 
            eventType = MapHistory.HistoricalEvent.locationNameChange, 
            time = saveFileData.timeFileStarted
        });
        Color[] palette = Name_Generator.GetRandomPalette(saveFileData.worldSeed);
        mapData.startingColorPrimary = palette[0];
        mapData.startingColorSecondary = palette[1];
        mapData.startingColorTertiary = palette[2];
        mapData.startingColorLight = palette[3];
        mapData.startingColorDark = palette[4];

        DEAD_Save_Load.WriteFile(Application.persistentDataPath + "/Saves/Save" + PlayerPrefs.GetInt("CurrentSaveFile") + "/MapData" + saveFileData.currentMap + ".xml", mapData.SerializeToXML());
    }

    public float GetCurrentRainValue()
    {
        return rainPercent;
    }

    public float GetCurrentSnowValue()
    {
        return snowPercent;
    }

    public float GetWorldFlyingSphereSize()
    {
        return worldFlyingSphereSize;
    }


}

[System.Serializable]
public struct DateTimeFloat
{
    public UDateTime time;
    public float value;
}

[System.Serializable, XmlRoot("Save File Data")]
public class SaveFileData
{
    public string firstName;
    public string lastName;
    public int money;
    public int worldSeed;
    public int currentMap;
    public UDateTime timeFileStarted;
    public float timeElapsed;
    public string SerializeToXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveFileData));
        using (System.IO.StringWriter writer = new System.IO.StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }

    public SaveFileData DeserializeFromXML(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveFileData));
        using (System.IO.StringReader reader = new System.IO.StringReader(xml))
        {
            return (SaveFileData)serializer.Deserialize(reader);
        }
    }

    public static string GetMap(int id)
    {
        switch (id)
        {
            case 0:
                return "Fast Food Place";
            case 1:
                return "Drive Thru";
            default:
                return "";
        }
    }
}

[System.Serializable]
public struct MapData
{
    public List<MapHistory> mapHistory;
    public List<CustomGeometryData> geometryData;
    public List<Combo_Animatronic_SaveFile> animatronics;

    public Color startingColorPrimary;
    public Color startingColorSecondary;
    public Color startingColorTertiary;
    public Color startingColorLight;
    public Color startingColorDark;

    public string SerializeToXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(MapData));
        using (System.IO.StringWriter writer = new System.IO.StringWriter())
        {
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }

    public MapData DeserializeFromXML(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(MapData));
        using (System.IO.StringReader reader = new System.IO.StringReader(xml))
        {
            return (MapData)serializer.Deserialize(reader);
        }
    }
}

[System.Serializable]
public struct MapHistory
{
    public string change;
    public UDateTime time;
    public HistoricalEvent eventType;

    public enum HistoricalEvent
    {
        locationNameChange,
        locationOwnerChange,
        buildingChange,
        closure,
        reopening,
    }
}

[System.Serializable]
public struct CustomGeometryData
{
    public string key;
    public int material;
    public Color color;
    public float grime;
}