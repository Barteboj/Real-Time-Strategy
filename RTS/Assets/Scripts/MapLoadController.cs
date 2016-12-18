using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MapLoadController : MonoBehaviour
{
    private static MapLoadController instance;

    public static MapLoadController Instance
    {
        get
        {
            if (instance == null)
            {
                MapLoadController newInstance = FindObjectOfType<MapLoadController>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not MapLoadController attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public int mapSizeX = 10;
    public int mapSizeY = 10;

    public MapGridded map;

    public string mapNameToLoad;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of MapLoadController on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        LoadMap(Application.dataPath + "/" + MapEditor.mapsFolderName + "/" + mapNameToLoad + ".map");
    }

    public void LoadMap(string filePath)
    {
        List<string> lines = new List<string>(new StreamReader(filePath).ReadToEnd().Split('\n'));
        foreach (string line in lines)
        {
            string[] words = line.Split(' ');
            switch (words[0])
            {
                case MapEditor.mapSizeFileKey:
                    mapSizeX = int.Parse(words[1]);
                    mapSizeY = int.Parse(words[2]);
                    map.mapGrid = new MapGridElement[mapSizeY, mapSizeX];
                    break;
                case MapEditor.tileKey:
                    SaveToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])), (TileType)System.Enum.Parse(typeof(TileType), words[3]));
                    break;
                case MapEditor.goldMineKey:
                    SaveMineToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
                case MapEditor.lumberKey:
                    SaveLumberToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
            }
        }
    }

    public void SaveToMap(IntVector2 positionInMap, TileType tileType)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Tile tile = ((GameObject)Instantiate(Tiles.Instance.tilesPrefabs.Find(wantedTile => wantedTile.tileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        map.mapGrid[positionInMap.y, positionInMap.x] = new MapGridElement(positionInMap.x, positionInMap.y, tile, (GameObject)Instantiate(Tiles.Instance.canBuildIndicator, tile.transform.position, Quaternion.identity), (GameObject)Instantiate(Tiles.Instance.cannotBuildIndicator, tile.transform.position, Quaternion.identity));
    }

    public void SaveMineToMap(IntVector2 positionInMap)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Instantiate(Resources.Instance.minePrefab, postionToCreate, Quaternion.identity);
    }

    public void SaveLumberToMap(IntVector2 positionInMap)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Instantiate(Resources.Instance.treePrefab, postionToCreate, Quaternion.identity);
    }
}
