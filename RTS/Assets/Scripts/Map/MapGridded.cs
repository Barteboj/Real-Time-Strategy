using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGridded : MonoBehaviour
{
    private static MapGridded instance;

    public static MapGridded Instance
    {
        get
        {
            if (instance == null)
            {
                MapGridded newInstance = FindObjectOfType<MapGridded>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not MapGridded attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public MapGridElement[,] MapGrid { get; set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of MapGridded on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public static Vector2 MapToWorldPosition(IntVector2 mapPosition)
    {
        return new Vector2(mapPosition.X, mapPosition.Y);
    }

    public static IntVector2 WorldToMapPosition(Vector2 worldPosition)
    {
        return new IntVector2(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
    }

    public bool IsInMap(IntVector2 position)
    {
        return position.X >= 0 && position.Y >= 0 && position.X < MapGrid.GetLength(1) && position.Y < MapGrid.GetLength(0);
    }

    public IntVector2 GetFirstFreePlaceAround(IntVector2 position, int width, int height)
    {
        for (int i = 0; i <= height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.X - 1, position.Y + i);
            if (IsInMap(checkedPosition) && MapGrid[checkedPosition.Y, checkedPosition.X].IsWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i <= width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.X + i, position.Y + height);
            if (IsInMap(checkedPosition) && MapGrid[checkedPosition.Y, checkedPosition.X].IsWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i <= height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.X + width, position.Y + height - 1 - i);
            if (IsInMap(checkedPosition) && MapGrid[checkedPosition.Y, checkedPosition.X].IsWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i <= width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.X + width - 1 - i, position.Y - 1);
            if (IsInMap(checkedPosition) && MapGrid[checkedPosition.Y, checkedPosition.X].IsWalkable)
            {
                return checkedPosition;
            }
        }
        return GetFirstFreePlaceAround(new IntVector2(position.X - 1, position.Y - 1), width + 2, height + 2);
    }

    public List<MapGridElement> GetAdjacentGridElements(IntVector2 centerPosition)
    {
        List<MapGridElement> mapGridElements = new List<MapGridElement>();
        for (int row = centerPosition.Y - 1; row <= centerPosition.Y + 1; ++row)
        {
            for (int column = centerPosition.X - 1; column <= centerPosition.X + 1; ++column)
            {
                if (IsInMap(new IntVector2(column, row)))
                {
                    mapGridElements.Add(MapGrid[row, column]);
                }
            }
        }
        return mapGridElements;
    }
}
