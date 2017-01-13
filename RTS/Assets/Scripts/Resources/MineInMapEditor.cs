using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineInMapEditor : MonoBehaviour
{
    public int width;
    public int height;

    public IntVector2 placeOnMapGrid;

    public void SetPositionInMapGrid()
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                MapEditor.Instance.map[placeOnMapGrid.y + row, placeOnMapGrid.x + column].mine = this;
            }
        }
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid)
    {
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (!MapEditor.Instance.IsInMap(new IntVector2(placeInGrid.x + column, placeInGrid.y + row)) || (!MapEditor.Instance.map[placeInGrid.y + row, placeInGrid.x + column].tile.isWalkable || MapEditor.Instance.map[placeInGrid.y + row, placeInGrid.x + column].mine != null || MapEditor.Instance.map[placeInGrid.y + row, placeInGrid.x + column].lumber != null))
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
                positions.Add(new IntVector2(placeOnMapGrid.x + column, placeOnMapGrid.y + row));
            }
        }
        return positions;
    }
}
