using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineInMapEditor : MonoBehaviour
{
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;

    private IntVector2 placeOnMapGrid;
    public IntVector2 PlaceOnMapGrid
    {
        get
        {
            return placeOnMapGrid;
        }
    }

    public void SetPositionInMapGrid()
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                MapEditor.Instance.Map[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].Mine = this;
            }
        }
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid)
    {
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (!MapEditor.Instance.IsInMap(new IntVector2(placeInGrid.X + column, placeInGrid.Y + row)) || (!MapEditor.Instance.Map[placeInGrid.Y + row, placeInGrid.X + column].Tile.IsWalkable || MapEditor.Instance.Map[placeInGrid.Y + row, placeInGrid.X + column].Mine != null || MapEditor.Instance.Map[placeInGrid.Y + row, placeInGrid.X + column].Lumber != null))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public List<IntVector2> GetPositionsOnMap()
    {
        List<IntVector2> positions = new List<IntVector2>();
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                positions.Add(new IntVector2(placeOnMapGrid.X + column, placeOnMapGrid.Y + row));
            }
        }
        return positions;
    }
}
