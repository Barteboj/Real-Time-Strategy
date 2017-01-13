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

    public int mapSizeX = 10;
    public int mapSizeY = 10;

    public MapGridded map;

    public Vector2 player1StartingPosition;
    public Vector2 player2StartingPosition;

    public GameObject LoadingMapScreen;

    public string mapText = "";

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
        LoadMap(Application.dataPath + "/" + MapEditor.mapsFolderName + "/" + MultiplayerController.Instance.mapName + ".map");
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
                    map.mapGrid = new MapGridElement[mapSizeY, mapSizeX];
                    Minimap.Instance.SetMapSize(mapSizeX);
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
                        mapGrid[positionInMap.y, positionInMap.x] = new MapGridElement(Tiles.Instance.tilesPrefabs.Find(item => item.tileType == (TileType)System.Enum.Parse(typeof(TileType), words[3])));
                        break;
                    case MapEditor.goldMineKey:
                        positionInMap = new IntVector2(int.Parse(words[1]), int.Parse(words[2]));
                        postionToCreate = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        foreach (IntVector2 minePositionInMap in Resources.Instance.minePrefab.GetComponent<Mine>().GetMapPositions(positionInMap))
                        {
                            mapGrid[minePositionInMap.y, minePositionInMap.x].mine = Resources.Instance.minePrefab.GetComponent<Mine>();
                        }
                        break;
                    case MapEditor.lumberKey:
                        positionInMap = new IntVector2(int.Parse(words[1]), int.Parse(words[2]));
                        postionToCreate = MapGridded.MapToWorldPosition(new IntVector2(int.Parse(words[1]), int.Parse(words[2])));
                        mapGrid[positionInMap.y, positionInMap.x].lumber = Resources.Instance.treePrefab.GetComponent<LumberInGame>();
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
            if (mapSizeX <= 0 || mapSizeY <= 0 || mapSizeX > 50 || mapSizeY > 50 || player1StartingPosition == null || player2StartingPosition == null || player1StartingPosition == player2StartingPosition || player1StartingPosition.x < 0 || player1StartingPosition.x > mapSizeX - 1 || player1StartingPosition.y < 0 || player1StartingPosition.y > mapSizeY - 1 || player2StartingPosition.x < 0 || player2StartingPosition.x > mapSizeX - 1 || player2StartingPosition.y < 0 || player2StartingPosition.y > mapSizeY - 1 || !mapGrid[MapGridded.WorldToMapPosition(player1StartingPosition).y, MapGridded.WorldToMapPosition(player1StartingPosition).x].isWalkable || !mapGrid[MapGridded.WorldToMapPosition(player2StartingPosition).y, MapGridded.WorldToMapPosition(player2StartingPosition).x].isWalkable)
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
        Tile tile = (Instantiate(Tiles.Instance.tilesPrefabs.Find(wantedTile => wantedTile.tileType == tileType).gameObject, postionToCreate, Quaternion.identity)).GetComponent<Tile>();
        map.mapGrid[positionInMap.y, positionInMap.x] = new MapGridElement(positionInMap.x, positionInMap.y, tile, Instantiate(Tiles.Instance.canBuildIndicator, tile.transform.position, Quaternion.identity), Instantiate(Tiles.Instance.cannotBuildIndicator, tile.transform.position, Quaternion.identity));
    }

    public void SaveMineToMap(IntVector2 positionInMap)
    {
        if (isServer)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            NetworkServer.Spawn(Instantiate(Resources.Instance.minePrefab, postionToCreate, Quaternion.identity));
        }
    }

    public void SaveLumberToMap(IntVector2 positionInMap)
    {
        if (isServer)
        {
            Vector2 postionToCreate = MapGridded.MapToWorldPosition(positionInMap);
            NetworkServer.Spawn(Instantiate(Resources.Instance.treePrefab, postionToCreate, Quaternion.identity));
        }
    }
}
