using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour
{
    private static Map instance;

    public static Map Instance
    {
        get
        {
            if (instance == null)
            {
                Map newInstance = FindObjectOfType<Map>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not Map attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public MapRow[] mapTilesInRows;

    public Tile[,] mapTiles;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of Map on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
            mapTiles = new Tile[mapTilesInRows.Length, mapTilesInRows[0].tiles.Length];
            for (int row = 0; row < mapTiles.GetLength(0); ++row)
            {
                for (int column = 0; column < mapTiles.GetLength(1); ++column)
                {
                    mapTiles[row, column] = mapTilesInRows[row].tiles[column];
                }
            }
        }
    }

    /*public PathNode GetNodeFromPosition(Vector2 position)
    {
        return 
    }*/
}
