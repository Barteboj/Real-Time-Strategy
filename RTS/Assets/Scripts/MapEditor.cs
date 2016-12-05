using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapEditor : MonoBehaviour
{
    private static MapEditor instance;

    public static MapEditor Instance
    {
        get
        {
            if (instance == null)
            {
                MapEditor newInstance = FindObjectOfType<MapEditor>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not MapEditor attached to scene and is tried to be obtained");
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

    public Tile[,] map;

    public string mapName;

    public Tile selectedTilePrefab;

    public GameObject prefabShowingWhereYouArePuttingTile;

    public const string mapsFolderName = "Maps";

    public const string mapSizeFileKey = "MapSize";
    public const string tileKey = "Tile";

    public bool IsInMap(IntVector2 mapPosition)
    {
        return mapPosition.x >= 0 && mapPosition.x < mapSizeX && mapPosition.y >= 0 && mapPosition.y < mapSizeY;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of Map Editor on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Update()
    {
        Vector2 mousePositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        IntVector2 positionInMap = WorldPositionToMapPosition(mousePositionInWorld);
        if (prefabShowingWhereYouArePuttingTile != null && IsInMap(positionInMap))
        {
            prefabShowingWhereYouArePuttingTile.transform.position = new Vector2(Mathf.Round(mousePositionInWorld.x - 0.5f), Mathf.Round(mousePositionInWorld.y - 0.5f));
            if (Input.GetMouseButton(0))
            {
                SaveTileToMap(positionInMap);
            }
        }
    }

    public void SaveTileToMap(IntVector2 positionInMap)
    {
        if (map[positionInMap.x, positionInMap.y] == null || map[positionInMap.x, positionInMap.y].tileType != selectedTilePrefab.tileType)
        {
            Vector2 postionToCreate = MapToWorldPosition(positionInMap);
            Tile tile = ((GameObject)Instantiate(selectedTilePrefab.gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
            if (map[positionInMap.x, positionInMap.y] != null)
            {
                Destroy(map[positionInMap.x, positionInMap.y].gameObject);
            }
            map[positionInMap.x, positionInMap.y] = tile;
        }
    }

    public void SaveTileToMap(IntVector2 positionInMap, TileType tileType)
    {
        Vector2 postionToCreate = MapToWorldPosition(positionInMap);
        Tile tile = ((GameObject)Instantiate(Tiles.Instance.tilesPrefabs.Find(wantedTile => wantedTile.tileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        if (map[positionInMap.x, positionInMap.y] != null)
        {
            Destroy(map[positionInMap.x, positionInMap.y].gameObject);
        }
        map[positionInMap.x, positionInMap.y] = tile;
    }

    public IntVector2 WorldPositionToMapPosition(Vector2 worldPosition)
    {
        return new IntVector2(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y));
    }

    public Vector2 MapToWorldPosition(IntVector2 mapPosition)
    {
        return new Vector2(mapPosition.x, mapPosition.y);
    }

    public void SaveLoadedMap()
    {
        SaveMap(Application.dataPath + "/" + mapsFolderName + "/" + mapName + ".map");
    }

    public void SaveMap(string filePath)
    {
        Debug.Log("Saving file " + filePath);
        List<string> lines = new List<string>();
        lines.Add(mapSizeFileKey + " " + mapSizeX + " " + mapSizeY);
        for (int rowIndex = 0; rowIndex < mapSizeX; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < mapSizeY; ++columnIndex)
            {
                lines.Add(tileKey + " " + rowIndex + " " + columnIndex + " " + map[rowIndex, columnIndex].tileType.ToString());
            }
        }
        File.WriteAllText(filePath, "");
        StreamWriter writer = new StreamWriter(filePath);
        foreach (string line in lines)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }

    public void LoadMap(string filePath)
    {
        List<string> lines = new List<string>(new StreamReader(filePath).ReadToEnd().Split('\n'));
        foreach (string line in lines)
        {
            string[] words = line.Split(' ');
            switch (words[0])
            {
                case mapSizeFileKey:
                    mapSizeX = int.Parse(words[1]);
                    mapSizeY = int.Parse(words[2]);
                    map = new Tile[mapSizeX, mapSizeY];
                    break;
                case tileKey:
                    SaveTileToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])), (TileType)System.Enum.Parse(typeof(TileType), words[3]));
                    break;
            }
        }
    }
}