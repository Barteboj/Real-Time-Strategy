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
                instance = FindObjectOfType<MapEditor>();
            }
            return instance;
        }
    }
    
    private int mapWidth;
    private int mapHeight;

    public MapEditorGridElement[,] Map { get; set; }

    public string MapName { get; set; }

    private Tile selectedTilePrefab;
    private GameObject prefabShowingWhereYouArePuttingTile;
    private MineInMapEditor mineSelectionPrefab;
    private LumberInMapEditor lumberSelectionPrefab;
    private GameObject player1MarkerSelectionPrefab;
    private GameObject player2MarkerSelectionPrefab;

    public const string mapsFolderName = "Maps";
    public const string mapSizeFileKey = "MapSize";
    public const string tileKey = "Tile";
    public const string goldMineKey = "GoldMine";
    public const string lumberKey = "Lumber";
    public const string player1PositionKey = "Player1Position";
    public const string player2PositionKey = "Player2Position";

    private IntVector2 player1Position;
    private IntVector2 player2Position;

    [SerializeField]
    private GameObject minePrefab;
    [SerializeField]
    private GameObject lumberPrefab;
    [SerializeField]
    private GameObject player1MarkerPrefab;
    [SerializeField]
    private GameObject player2MarkerPrefab;

    private GameObject player1MarkerOnMapInstance;
    private GameObject player2MarkerOnMapInstance;

    private List<MineInMapEditor> mines = new List<MineInMapEditor>();
    private List<LumberInMapEditor> lumberList = new List<LumberInMapEditor>();

    public bool IsInMap(IntVector2 mapPosition)
    {
        return mapPosition.X >= 0 && mapPosition.X < mapWidth && mapPosition.Y >= 0 && mapPosition.Y < mapHeight;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
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
                if (Input.GetMouseButton(0) && IsInMap(positionInMap) && Map[positionInMap.Y, positionInMap.X].Tile.IsWalkable && Map[positionInMap.Y, positionInMap.X].Mine == null && Map[positionInMap.Y, positionInMap.X].Lumber == null)
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
                if (Input.GetMouseButton(0) && IsInMap(positionInMap) && Map[positionInMap.Y, positionInMap.X].Tile.IsWalkable && Map[positionInMap.Y, positionInMap.X].Mine == null && Map[positionInMap.Y, positionInMap.X].Lumber == null)
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
        this.MapName = mapName;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        InitializeMap();
    }

    public void InitializeMap()
    {
        Map = new MapEditorGridElement[mapHeight, mapWidth];
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
        selectedTilePrefab = Tiles.Instance.TilesPrefabs.Find(tilePrefab => tilePrefab.TileType == tileType);
        prefabShowingWhereYouArePuttingTile = Instantiate(selectedTilePrefab.gameObject);
    }

    public void SaveTileToMap(IntVector2 positionInMap)
    {
        if (Map[positionInMap.Y, positionInMap.X] == null || Map[positionInMap.Y, positionInMap.X].Tile.TileType != selectedTilePrefab.TileType)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            Tile tile = (Instantiate(selectedTilePrefab.gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
            if (Map[positionInMap.Y, positionInMap.X] != null && Map[positionInMap.Y, positionInMap.X].Tile != null)
            {
                Destroy(Map[positionInMap.Y, positionInMap.X].Tile.gameObject);
            }
            if (Map[positionInMap.Y, positionInMap.X] != null)
            {
                Map[positionInMap.Y, positionInMap.X].Tile = tile;
            }
            else
            {
                Map[positionInMap.Y, positionInMap.X] = new MapEditorGridElement(positionInMap, tile, null);
            }
            if (!Map[positionInMap.Y, positionInMap.X].Tile.IsWalkable)
            {
                if (Map[positionInMap.Y, positionInMap.X].Mine != null)
                {
                    mines.Remove(Map[positionInMap.Y, positionInMap.X].Mine);
                    Destroy(Map[positionInMap.Y, positionInMap.X].Mine.gameObject);
                }
                if (Map[positionInMap.Y, positionInMap.X].Lumber != null)
                {
                    lumberList.Remove(Map[positionInMap.Y, positionInMap.X].Lumber);
                    Destroy(Map[positionInMap.Y, positionInMap.X].Lumber.gameObject);
                }
                if (player1Position != null && positionInMap.X == player1Position.X && positionInMap.Y == player1Position.Y)
                {
                    RemovePlayer1Marker();
                }
                if (player2Position != null && positionInMap.X == player2Position.X && positionInMap.Y == player2Position.Y)
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
                if (player1Position != null && mapPosition.X == player1Position.X && mapPosition.Y == player1Position.Y)
                {
                    RemovePlayer1Marker();
                }
                if (player2Position != null && mapPosition.X == player2Position.X && mapPosition.Y == player2Position.Y)
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
            if (player1Position != null && positionInMap.X == player1Position.X && positionInMap.Y == player1Position.Y)
            {
                RemovePlayer1Marker();
            }
            if (player2Position != null && positionInMap.X == player2Position.X && positionInMap.Y == player2Position.Y)
            {
                RemovePlayer2Marker();
            }
        }
    }

    public void SaveTileToMap(IntVector2 positionInMap, TileType tileType)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Tile tile = (Instantiate(Tiles.Instance.TilesPrefabs.Find(wantedTile => wantedTile.TileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        if (Map[positionInMap.Y, positionInMap.X] != null && Map[positionInMap.Y, positionInMap.X].Tile != null)
        {
            Destroy(Map[positionInMap.Y, positionInMap.X].Tile.gameObject);
        }
        if (Map[positionInMap.Y, positionInMap.X] != null)
        {
            Map[positionInMap.Y, positionInMap.X].Tile = tile;
        }
        else
        {
            Map[positionInMap.Y, positionInMap.X] = new MapEditorGridElement(positionInMap, tile, null);
        }
        if (!Map[positionInMap.Y, positionInMap.X].Tile.IsWalkable)
        {
            if (Map[positionInMap.Y, positionInMap.X].Mine != null)
            {
                mines.Remove(Map[positionInMap.Y, positionInMap.X].Mine);
                Destroy(Map[positionInMap.Y, positionInMap.X].Mine.gameObject);
            }
            if (Map[positionInMap.Y, positionInMap.X].Lumber != null)
            {
                lumberList.Remove(Map[positionInMap.Y, positionInMap.X].Lumber);
                Destroy(Map[positionInMap.Y, positionInMap.X].Lumber.gameObject);
            }
            if (player1Position != null && positionInMap.X == player1Position.X && positionInMap.Y == player1Position.Y)
            {
                RemovePlayer1Marker();
            }
            if (player2Position != null && positionInMap.X == player2Position.X && positionInMap.Y == player2Position.Y)
            {
                RemovePlayer2Marker();
            }
        }
    }

    public void SaveLoadedMap()
    {
        SaveMap(Application.dataPath + "/" + mapsFolderName + "/" + MapName + ".map");
    }

    public void SaveMap(string filePath)
    {
        List<string> lines = new List<string>();
        lines.Add(mapSizeFileKey + " " + mapWidth + " " + mapHeight);
        if (player1Position != null)
        {
            lines.Add(player1PositionKey + " " + player1Position.X + " " + player1Position.Y);
        }
        if (player2Position != null)
        {
            lines.Add(player2PositionKey + " " + player2Position.X + " " + player2Position.Y);
        }
        for (int rowIndex = 0; rowIndex < mapHeight; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < mapWidth; ++columnIndex)
            {
                lines.Add(tileKey + " " + columnIndex + " " + rowIndex + " " + Map[rowIndex, columnIndex].Tile.TileType.ToString());
            }
        }
        foreach (MineInMapEditor mine in mines)
        {
            lines.Add(goldMineKey + " " + mine.PlaceOnMapGrid.X + " " + mine.PlaceOnMapGrid.Y);
        }
        foreach (LumberInMapEditor lumber in lumberList)
        {
            lines.Add(lumberKey + " " + lumber.PlaceOnMapGrid.X + " " + lumber.PlaceOnMapGrid.Y);
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
                    Map = new MapEditorGridElement[mapHeight, mapWidth];
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
        foreach (MapEditorGridElement mapGridElement in Map)
        {
            Destroy(mapGridElement.Tile);
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