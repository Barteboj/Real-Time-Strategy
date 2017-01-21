using UnityEngine;
using System.Collections;

public class LumberInMapEditor : MonoBehaviour
{
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
        MapEditor.Instance.Map[placeOnMapGrid.Y, placeOnMapGrid.X].Lumber = this;
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid)
    {
        if (!MapEditor.Instance.IsInMap(new IntVector2(placeInGrid.X, placeInGrid.Y)) || (!MapEditor.Instance.Map[placeInGrid.Y, placeInGrid.X].Tile.IsWalkable || MapEditor.Instance.Map[placeInGrid.Y, placeInGrid.X].Mine != null) || MapEditor.Instance.Map[placeInGrid.Y, placeInGrid.X].Lumber != null)
        {
            return false;
        }
        return true;
    }
}
