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
    
    public int mapWidth = 10;
    public int mapHeight = 10;

    public MapEditorGridElement[,] map;

    public string mapName;

    public Tile selectedTilePrefab;

    public GameObject prefabShowingWhereYouArePuttingTile;

    public MineInMapEditor mineSelectionPrefab;

    public LumberInMapEditor lumberSelectionPrefab;

    public GameObject player1MarkerSelectionPrefab;
    public GameObject player2MarkerSelectionPrefab;

    public const string mapsFolderName = "Maps";

    public const string mapSizeFileKey = "MapSize";
    public const string tileKey = "Tile";

    public const string goldMineKey = "GoldMine";

    public const string lumberKey = "Lumber";

    public const string player1PositionKey = "Player1Position";
    public const string player2PositionKey = "Player2Position";

    public IntVector2 player1Position;
    public IntVector2 player2Position;

    public GameObject minePrefab;

    public GameObject lumberPrefab;

    public GameObject player1MarkerPrefab;
    public GameObject player2MarkerPrefab;

    public GameObject player1MarkerOnMapInstance;
    public GameObject player2MarkerOnMapInstance;

    public List<MineInMapEditor> mines = new List<MineInMapEditor>();

    public List<LumberInMapEditor> lumberList = new List<LumberInMapEditor>();

    public bool IsInMap(IntVector2 mapPosition)
    {
        return mapPosition.x >= 0 && mapPosition.x < mapWidth && mapPosition.y >= 0 && mapPosition.y < mapHeight;
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
        IntVector2 positionInMap = MapGridded.WorldToMapPosition(mousePositionInWorld);
        if (IsInEditingArea(mousePositionInWorld))
        {
            if (prefabShowingWhereYouArePuttingTile != null && IsInMap(positionInMap))
            {
                prefabShowingWhereYouArePuttingTile.transform.position = MapGridded.MapToWorldPosition(positionInMap);
                if (Input.GetMouseButton(0))
                {
                    SaveTileToMap(positionInMap);
                }
            }
            else if (mineSelectionPrefab != null)
            {
                mineSelectionPrefab.transform.position = MapGridded.MapToWorldPosition(positionInMap);
                if (Input.GetMouseButton(0))
                {
                    SaveMineToMap(positionInMap);
                }
            }
            else if (lumberSelectionPrefab != null)
            {
                lumberSelectionPrefab.transform.position = MapGridded.MapToWorldPosition(positionInMap);
                if (Input.GetMouseButton(0))
                {
                    SaveLumberToMap(positionInMap);
                }
            }
            else if (player1MarkerSelectionPrefab != null)
            {
                player1MarkerSelectionPrefab.transform.position = MapGridded.MapToWorldPosition(positionInMap);
                if (Input.GetMouseButton(0) && IsInMap(positionInMap) && map[positionInMap.y, positionInMap.x].tile.isWalkable && map[positionInMap.y, positionInMap.x].mine == null && map[positionInMap.y, positionInMap.x].lumber == null)
                {
                    player1Position = positionInMap;
                    if (player1MarkerOnMapInstance != null)
                    {
                        Destroy(player1MarkerOnMapInstance);
                    }
                    player1MarkerOnMapInstance = Instantiate(player1MarkerPrefab, MapGridded.MapToWorldPosition(positionInMap), Quaternion.identity);
                }
            }
            else if (player2MarkerSelectionPrefab != null)
            {
                player2MarkerSelectionPrefab.transform.position = MapGridded.MapToWorldPosition(positionInMap);
                if (Input.GetMouseButton(0) && IsInMap(positionInMap) && map[positionInMap.y, positionInMap.x].tile.isWalkable && map[positionInMap.y, positionInMap.x].mine == null && map[positionInMap.y, positionInMap.x].lumber == null)
                {
                    player2Position = positionInMap;
                    if (player2MarkerOnMapInstance != null)
                    {
                        Destroy(player2MarkerOnMapInstance);
                    }
                    player2MarkerOnMapInstance = Instantiate(player2MarkerPrefab, MapGridded.MapToWorldPosition(positionInMap), Quaternion.identity);
                }
            }
        }
    }

    public void RemovePlayer1Marker()
    {
        player1Position = null;
        if (player1MarkerOnMapInstance != null)
        {
            Destroy(player1MarkerOnMapInstance);
        }
    }

    public void RemovePlayer2Marker()
    {
        player2Position = null;
        if (player2MarkerOnMapInstance != null)
        {
            Destroy(player2MarkerOnMapInstance);
        }
    }

    public bool IsInEditingArea(Vector2 position)
    {
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Editing Area"));
        if (hitInfo.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CreateMap(string mapName, int mapWidth, int mapHeight)
    {
        this.mapName = mapName;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        InitializeMap();
    }

    public void InitializeMap()
    {
        map = new MapEditorGridElement[mapHeight, mapWidth];
        for (int row = 0; row < mapHeight; ++row)
        {
            for (int column = 0; column < mapWidth; ++column)
            {
                SaveTileToMap(new IntVector2(column, row), TileType.Grass);
            }
        }
    }

    public void SelectMine()
    {
        UnselectElementsToPut();
        mineSelectionPrefab = Instantiate(minePrefab).GetComponent<MineInMapEditor>();
    }

    public void SelectLumber()
    {
        UnselectElementsToPut();
        lumberSelectionPrefab = Instantiate(lumberPrefab).GetComponent<LumberInMapEditor>();
    }

    public void SelectPlayer1Position()
    {
        UnselectElementsToPut();
        player1MarkerSelectionPrefab = Instantiate(player1MarkerPrefab);
    }

    public void SelectPlayer2Position()
    {
        UnselectElementsToPut();
        player2MarkerSelectionPrefab = Instantiate(player2MarkerPrefab);
    }

    public void UnselectElementsToPut()
    {
        if (mineSelectionPrefab != null)
        {
            Destroy(mineSelectionPrefab.gameObject);
        }
        if (prefabShowingWhereYouArePuttingTile != null)
        {
            Destroy(prefabShowingWhereYouArePuttingTile.gameObject);
        }
        if (lumberSelectionPrefab != null)
        {
            Destroy(lumberSelectionPrefab.gameObject);
        }
        if (player1MarkerSelectionPrefab != null)
        {
            Destroy(player1MarkerSelectionPrefab.gameObject);
        }
        if (player2MarkerSelectionPrefab != null)
        {
            Destroy(player2MarkerSelectionPrefab.gameObject);
        }
    }

    public void SelectTile(TileType tileType)
    {
        UnselectElementsToPut();
        selectedTilePrefab = Tiles.Instance.tilesPrefabs.Find(tilePrefab => tilePrefab.tileType == tileType);
        prefabShowingWhereYouArePuttingTile = Instantiate(selectedTilePrefab.gameObject);
    }

    public void SaveTileToMap(IntVector2 positionInMap)
    {
        if (map[positionInMap.y, positionInMap.x] == null || map[positionInMap.y, positionInMap.x].tile.tileType != selectedTilePrefab.tileType)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            Tile tile = (Instantiate(selectedTilePrefab.gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
            if (map[positionInMap.y, positionInMap.x] != null && map[positionInMap.y, positionInMap.x].tile != null)
            {
                Destroy(map[positionInMap.y, positionInMap.x].tile.gameObject);
            }
            if (map[positionInMap.y, positionInMap.x] != null)
            {
                map[positionInMap.y, positionInMap.x].tile = tile;
            }
            else
            {
                map[positionInMap.y, positionInMap.x] = new MapEditorGridElement(positionInMap, tile, null);
            }
            if (!map[positionInMap.y, positionInMap.x].tile.isWalkable)
            {
                if (map[positionInMap.y, positionInMap.x].mine != null)
                {
                    mines.Remove(map[positionInMap.y, positionInMap.x].mine);
                    Destroy(map[positionInMap.y, positionInMap.x].mine.gameObject);
                }
                if (map[positionInMap.y, positionInMap.x].lumber != null)
                {
                    lumberList.Remove(map[positionInMap.y, positionInMap.x].lumber);
                    Destroy(map[positionInMap.y, positionInMap.x].lumber.gameObject);
                }
                if (player1Position != null && positionInMap.x == player1Position.x && positionInMap.y == player1Position.y)
                {
                    RemovePlayer1Marker();
                }
                if (player2Position != null && positionInMap.x == player2Position.x && positionInMap.y == player2Position.y)
                {
                    RemovePlayer2Marker();
                }
            }
        }
    }

    public void SaveMineToMap(IntVector2 positionInMap)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        if (minePrefab.GetComponent<MineInMapEditor>().CouldBeBuildInPlace(positionInMap))
        {
            MineInMapEditor mine = (Instantiate(minePrefab, postionToCreate, Quaternion.identity)).GetComponent<MineInMapEditor>();
            mine.SetPositionInMapGrid();
            mines.Add(mine);
            List<IntVector2> mapPositions = mine.GetPositionsOnMap();
            foreach (IntVector2 mapPosition in mapPositions)
            {
                if (player1Position != null && mapPosition.x == player1Position.x && mapPosition.y == player1Position.y)
                {
                    RemovePlayer1Marker();
                }
                if (player2Position != null && mapPosition.x == player2Position.x && mapPosition.y == player2Position.y)
                {
                    RemovePlayer2Marker();
                }
            }
        }
    }

    public void SaveLumberToMap(IntVector2 positionInMap)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        if (lumberPrefab.GetComponent<LumberInMapEditor>().CouldBeBuildInPlace(positionInMap))
        {
            LumberInMapEditor lumber = (Instantiate(lumberPrefab, postionToCreate, Quaternion.identity)).GetComponent<LumberInMapEditor>();
            lumber.SetPositionInMapGrid();
            lumberList.Add(lumber);
            if (player1Position != null && positionInMap.x == player1Position.x && positionInMap.y == player1Position.y)
            {
                RemovePlayer1Marker();
            }
            if (player2Position != null && positionInMap.x == player2Position.x && positionInMap.y == player2Position.y)
            {
                RemovePlayer2Marker();
            }
        }
    }

    public void SaveTileToMap(IntVector2 positionInMap, TileType tileType)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Tile tile = (Instantiate(Tiles.Instance.tilesPrefabs.Find(wantedTile => wantedTile.tileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        if (map[positionInMap.y, positionInMap.x] != null && map[positionInMap.y, positionInMap.x].tile != null)
        {
            Destroy(map[positionInMap.y, positionInMap.x].tile.gameObject);
        }
        if (map[positionInMap.y, positionInMap.x] != null)
        {
            map[positionInMap.y, positionInMap.x].tile = tile;
        }
        else
        {
            map[positionInMap.y, positionInMap.x] = new MapEditorGridElement(positionInMap, tile, null);
        }
        if (!map[positionInMap.y, positionInMap.x].tile.isWalkable)
        {
            if (map[positionInMap.y, positionInMap.x].mine != null)
            {
                mines.Remove(map[positionInMap.y, positionInMap.x].mine);
                Destroy(map[positionInMap.y, positionInMap.x].mine.gameObject);
            }
            if (map[positionInMap.y, positionInMap.x].lumber != null)
            {
                lumberList.Remove(map[positionInMap.y, positionInMap.x].lumber);
                Destroy(map[positionInMap.y, positionInMap.x].lumber.gameObject);
            }
            if (player1Position != null && positionInMap.x == player1Position.x && positionInMap.y == player1Position.y)
            {
                RemovePlayer1Marker();
            }
            if (player2Position != null && positionInMap.x == player2Position.x && positionInMap.y == player2Position.y)
            {
                RemovePlayer2Marker();
            }
        }
    }

    public void SaveLoadedMap()
    {
        SaveMap(Application.dataPath + "/" + mapsFolderName + "/" + mapName + ".map");
    }

    public void SaveMap(string filePath)
    {
        List<string> lines = new List<string>();
        lines.Add(mapSizeFileKey + " " + mapWidth + " " + mapHeight);
        lines.Add(player1PositionKey + " " + player1Position.x + " " + player1Position.y);
        lines.Add(player2PositionKey + " " + player2Position.x + " " + player2Position.y);
        for (int rowIndex = 0; rowIndex < mapHeight; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < mapWidth; ++columnIndex)
            {
                lines.Add(tileKey + " " + columnIndex + " " + rowIndex + " " + map[rowIndex, columnIndex].tile.tileType.ToString());
            }
        }
        foreach (MineInMapEditor mine in mines)
        {
            lines.Add(goldMineKey + " " + mine.placeOnMapGrid.x + " " + mine.placeOnMapGrid.y);
        }
        foreach (LumberInMapEditor lumber in lumberList)
        {
            lines.Add(lumberKey + " " + lumber.placeOnMapGrid.x + " " + lumber.placeOnMapGrid.y);
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
                    mapWidth = int.Parse(words[1]);
                    mapHeight = int.Parse(words[2]);
                    map = new MapEditorGridElement[mapHeight, mapWidth];
                    break;
                case player1PositionKey:
                    player1MarkerOnMapInstance = Instantiate(player1MarkerPrefab, MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2]))), Quaternion.identity);
                    player1Position = MapGridded.WorldToMapPosition(player1MarkerOnMapInstance.transform.position);
                    break;
                case player2PositionKey:
                    player2MarkerOnMapInstance = Instantiate(player2MarkerPrefab, MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2]))), Quaternion.identity);
                    player2Position = MapGridded.WorldToMapPosition(player2MarkerOnMapInstance.transform.position);
                    break;
                case tileKey:
                    SaveTileToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])), (TileType)System.Enum.Parse(typeof(TileType), words[3]));
                    break;
                case goldMineKey:
                    SaveMineToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
                case lumberKey:
                    SaveLumberToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
            }
        }
    }

    public void CleanMapEditor()
    {
        foreach (MapEditorGridElement mapGridElement in map)
        {
            Destroy(mapGridElement.tile);
        }
        foreach (MineInMapEditor mine in mines)
        {
            Destroy(mine.gameObject);
        }
        mines.Clear();
        foreach (LumberInMapEditor lumber in lumberList)
        {
            Destroy(lumber.gameObject);
        }
        lumberList.Clear();
        player1Position = null;
        player2Position = null;
        if (player1MarkerOnMapInstance != null)
        {
            Destroy(player1MarkerOnMapInstance.gameObject);
        }
        if (player2MarkerOnMapInstance != null)
        {
            Destroy(player2MarkerOnMapInstance.gameObject);
        }
    }
}