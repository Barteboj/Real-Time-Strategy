using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private TileType tileType;
    public TileType TileType
    {
        get
        {
            return tileType;
        }
    }

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite
    {
        get
        {
            return sprite;
        }
    }
    [SerializeField]
    private bool isWalkable = true;
    public bool IsWalkable
    {
        get
        {
            return isWalkable;
        }
    }
}
