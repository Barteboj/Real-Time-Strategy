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

    public MapGridElement[,] mapGrid;

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
        return new Vector2(mapPosition.x, mapPosition.y);
    }

    public static IntVector2 WorldToMapPosition(Vector2 worldPosition)
    {
        return new IntVector2(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
    }

    public bool IsInMap(IntVector2 position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < mapGrid.GetLength(1) && position.y < mapGrid.GetLength(0);
    }

    public IntVector2 GetFirstFreePlaceAround(IntVector2 position, int width, int height)
    {
        for (int i = 0; i < height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x - 1, position.y + i);
            if (IsInMap(checkedPosition) && mapGrid[position.y + i, position.x - 1].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + i, position.y + height);
            if (IsInMap(checkedPosition) && mapGrid[position.y + height, position.x + i].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + width, position.y + i);
            if (IsInMap(checkedPosition) && mapGrid[position.y + i, position.x + width].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + i, position.y - 1);
            if (IsInMap(checkedPosition) && mapGrid[position.y - 1, position.x + i].isWalkable)
            {
                return checkedPosition;
            }
        }
        return GetFirstFreePlaceAround(new IntVector2(position.x - 1, position.y - 1), width + 2, height + 2);
    }

    public IntVector2 GetStrictFirstFreePlaceAround(IntVector2 position, int width, int height)
    {
        for (int i = 0; i < height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x - 1, position.y + i);
            if (IsInMap(checkedPosition) && mapGrid[position.y + i, position.x - 1].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + i, position.y + height);
            if (IsInMap(checkedPosition) && mapGrid[position.y + height, position.x + i].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < height; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + width, position.y + i);
            if (IsInMap(checkedPosition) && mapGrid[position.y + i, position.x + width].isWalkable)
            {
                return checkedPosition;
            }
        }
        for (int i = 0; i < width; ++i)
        {
            IntVector2 checkedPosition = new IntVector2(position.x + i, position.y - 1);
            if (IsInMap(checkedPosition) && mapGrid[position.y - 1, position.x + i].isWalkable)
            {
                return checkedPosition;
            }
        }
        return null;
    }

    public List<MapGridElement> GetGridElementsFromArea(IntVector2 centerPosition, int width, int height)
    {
        List<MapGridElement> mapGridElements = new List<MapGridElement>();
        for (int row = centerPosition.y - height; row <= centerPosition.y + height; ++row)
        {
            for (int column = centerPosition.x - width; column <= centerPosition.x + width; ++column)
            {
                if (IsInMap(new IntVector2(column, row)))
                {
                    mapGridElements.Add(mapGrid[row, column]);
                }
            }
        }
        return mapGridElements;
    }

    public List<MapGridElement> GetAdjacentGridElements(IntVector2 centerPosition)
    {
        List<MapGridElement> mapGridElements = new List<MapGridElement>();
        for (int row = centerPosition.y - 1; row <= centerPosition.y + 1; ++row)
        {
            for (int column = centerPosition.x - 1; column <= centerPosition.x + 1; ++column)
            {
                if (IsInMap(new IntVector2(column, row)))
                {
                    mapGridElements.Add(mapGrid[row, column]);
                }
            }
        }
        return mapGridElements;
    }
}
