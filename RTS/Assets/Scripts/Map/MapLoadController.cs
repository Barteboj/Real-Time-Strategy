using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapLoadController : NetworkBehaviour
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
    
    private int mapSizeX = 10;
    private int mapSizeY = 10;

    private Vector2 player1StartingPosition;
    public Vector2 Player1StartingPosition
    {
        get
        {
            return player1StartingPosition;
        }
    }
    private Vector2 player2StartingPosition;
    public Vector2 Player2StartingPosition
    {
        get
        {
            return player2StartingPosition;
        }
    }

    [SerializeField]
    private GameObject LoadingMapScreen;

    private string mapText = "";

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
    }

    public void LoadChosenMap()
    {
        LoadMap(Application.dataPath + "/" + MapEditor.mapsFolderName + "/" + MultiplayerController.Instance.MapName + ".map");
        LoadingMapScreen.SetActive(false);
    }

    [ClientRpc]
    void RpcLoadMap()
    {
        List<string> lines = new List<string>(mapText.Split('\n'));
        foreach (string line in lines)
        {
            string[] words = line.Split(' ');
            switch (words[0])
            {
                case MapEditor.mapSizeFileKey:
                    mapSizeX = int.Parse(words[1]);
                    mapSizeY = int.Parse(words[2]);
                    MapGridded.Instance.MapGrid = new MapGridElement[mapSizeY, mapSizeX];
                    Minimap.Instance.MapSize = mapSizeX;
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
                case MapEditor.player1PositionKey:
                    player1StartingPosition = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
                case MapEditor.player2PositionKey:
                    player2StartingPosition = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                    break;
            }
        }
        LoadingMapScreen.SetActive(false);
        FindObjectOfType<GameCameraController>().enabled = true;
    }

    [ClientRpc]
    void RpcAddToMapText(string textToAdd)
    {
        mapText += textToAdd;
    }

    public void LoadMap(string filePath)
    {
        string wholeMapText = new StreamReader(filePath).ReadToEnd();
        int actualIndexInMapText = 0;
        while (actualIndexInMapText < wholeMapText.Length)
        {
            int numberOfCharsToReadInPacket = Mathf.Min(5000, wholeMapText.Length - actualIndexInMapText);
            string partOfMapText = wholeMapText.Substring(actualIndexInMapText, numberOfCharsToReadInPacket);
            RpcAddToMapText(partOfMapText);
            actualIndexInMapText += numberOfCharsToReadInPacket;
        }
        RpcLoadMap();
    }

    public static bool CheckMap(string mapName)
    {
        try
        {
            string wholeMapText = new StreamReader(Application.dataPath + "/" + MapEditor.mapsFolderName + "/" + mapName + ".map").ReadToEnd();
            List<string> lines = new List<string>(wholeMapText.Split('\n'));
            int mapSizeX = 0;
            int mapSizeY = 0;
            Vector2 player1StartingPosition = -Vector2.one;
            Vector2 player2StartingPosition = -Vector2.one;
            MapGridElement[,] mapGrid = null;
            IntVector2 positionInMap;
            Vector2 postionToCreate;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                switch (words[0])
                {
                    case MapEditor.mapSizeFileKey:
                        mapSizeX = int.Parse(words[1]);
                        mapSizeY = int.Parse(words[2]);
                        mapGrid = new MapGridElement[mapSizeY, mapSizeX];
                        break;
                    case MapEditor.tileKey:
                        positionInMap = new IntVector2(int.Parse(words[1]), int.Parse(words[2]));
                        postionToCreate = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        mapGrid[positionInMap.Y, positionInMap.X] = new MapGridElement(Tiles.Instance.TilesPrefabs.Find(item => item.TileType == (TileType)System.Enum.Parse(typeof(TileType), words[3])));
                        break;
                    case MapEditor.goldMineKey:
                        positionInMap = new IntVector2(int.Parse(words[1]), int.Parse(words[2]));
                        postionToCreate = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        foreach (IntVector2 minePositionInMap in Resources.Instance.MinePrefab.GetComponent<Mine>().GetMapPositions(positionInMap))
                        {
                            mapGrid[minePositionInMap.Y, minePositionInMap.X].Mine = Resources.Instance.MinePrefab.GetComponent<Mine>();
                        }
                        break;
                    case MapEditor.lumberKey:
                        positionInMap = new IntVector2(int.Parse(words[1]), int.Parse(words[2]));
                        postionToCreate = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        mapGrid[positionInMap.Y, positionInMap.X].Lumber = Resources.Instance.TreePrefab.GetComponent<LumberInGame>();
                        break;
                    case MapEditor.player1PositionKey:
                        player1StartingPosition = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        break;
                    case MapEditor.player2PositionKey:
                        player2StartingPosition = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        break;
                }
            }
            if (mapGrid == null)
            {
                return false;
            }
            foreach(MapGridElement mapGridElement in mapGrid)
            {
                if (mapGridElement == null)
                {
                    return false;
                }
            }
            if (mapSizeX <= 0 || mapSizeY <= 0 || mapSizeX > 50 || mapSizeY > 50 || player1StartingPosition == null || player2StartingPosition == null || player1StartingPosition == player2StartingPosition || player1StartingPosition.x < 0 || player1StartingPosition.x > mapSizeX - 1 || player1StartingPosition.y < 0 || player1StartingPosition.y > mapSizeY - 1 || player2StartingPosition.x < 0 || player2StartingPosition.x > mapSizeX - 1 || player2StartingPosition.y < 0 || player2StartingPosition.y > mapSizeY - 1 || !mapGrid[MapGridded.WorldToMapPosition(player1StartingPosition).Y, MapGridded.WorldToMapPosition(player1StartingPosition).X].IsWalkable || !mapGrid[MapGridded.WorldToMapPosition(player2StartingPosition).Y, MapGridded.WorldToMapPosition(player2StartingPosition).X].IsWalkable)
            {
                return false;
            }
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    public void SaveToMap(IntVector2 positionInMap, TileType tileType)
    {
        Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
        Tile tile = (Instantiate(Tiles.Instance.TilesPrefabs.Find(wantedTile => wantedTile.TileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        MapGridded.Instance.MapGrid[positionInMap.Y, positionInMap.X] = new MapGridElement(positionInMap.X, positionInMap.Y, tile, Instantiate(Tiles.Instance.CanBuildIndicator, tile.transform.position, Quaternion.identity), Instantiate(Tiles.Instance.CannotBuildIndicator, tile.transform.position, Quaternion.identity));
    }

    public void SaveMineToMap(IntVector2 positionInMap)
    {
        if (isServer)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            NetworkServer.Spawn(Instantiate(Resources.Instance.MinePrefab, postionToCreate, Quaternion.identity));
        }
    }

    public void SaveLumberToMap(IntVector2 positionInMap)
    {
        if (isServer)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            NetworkServer.Spawn(Instantiate(Resources.Instance.TreePrefab, postionToCreate, Quaternion.identity));
        }
    }
}
