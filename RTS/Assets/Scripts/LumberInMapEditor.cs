using UnityEngine;
using System.Collections;

public class LumberInMapEditor : MonoBehaviour
{
    public IntVector2 placeOnMapGrid;

    public void SetPositionInMapGrid()
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        MapEditor.Instance.map[placeOnMapGrid.y, placeOnMapGrid.x].lumber = this;
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid)
    {
        if (!MapEditor.Instance.IsInMap(new IntVector2(placeInGrid.x, placeInGrid.y)) || (!MapEditor.Instance.map[placeInGrid.y, placeInGrid.x].tile.isWalkable || MapEditor.Instance.map[placeInGrid.y, placeInGrid.x].mine != null) || MapEditor.Instance.map[placeInGrid.y, placeInGrid.x].lumber != null)
        {
            return false;
        }
        return true;
    }
}
