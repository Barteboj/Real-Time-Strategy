using UnityEngine;
using System.Collections;

public class MapGridElement
{
    public Unit unit;
    public Building building;
    public Tile tile;
    public Mine mine;
    public LumberInGame lumber;
    public PathNode pathNode = new PathNode();
    public int x;
    public int y;
    public GameObject canBuildIndicator;
    public GameObject cannotBuildIndicator;

    public bool isWalkable
    {
        get
        {
            return unit == null && building == null && mine == null && (lumber == null || lumber.IsDepleted) && tile.isWalkable;
        }
    }

    public bool CheckIfIsGoodForPath(IntVector2 pathStartPosition)
    {
        return (this.unit == null || ((Mathf.Abs(this.unit.positionInGrid.x - pathStartPosition.x) > 1 || Mathf.Abs(this.unit.positionInGrid.y - pathStartPosition.y) > 1) && this.unit.isMoving)) && building == null && mine == null && (lumber == null || lumber.IsDepleted) && tile.isWalkable;
    }

    public bool ChecklIfIsWalkableForUnit(Unit unit)
    {
        return (this.unit == null || this.unit == unit) && building == null && mine == null && (lumber == null || lumber.IsDepleted) && tile.isWalkable;
    }

    public void ShowCanBuildIndicator()
    {
        canBuildIndicator.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void ShowCannotBuildIndicator()
    {
        cannotBuildIndicator.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void HideCanBuildIndicator()
    {
        canBuildIndicator.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void HideCannotBuildIndicator()
    {
        cannotBuildIndicator.GetComponent<SpriteRenderer>().enabled = false;
    }

    public MapGridElement(int x, int y, Tile tile, GameObject canBuildIndicator, GameObject cannotBuildIndicator)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
        pathNode = new PathNode();
        this.canBuildIndicator = canBuildIndicator;
        this.cannotBuildIndicator = cannotBuildIndicator;
    }

    public MapGridElement(Tile tile)
    {
        this.tile = tile;
    }

    public int GetCostToGetHereFrom(MapGridElement pathNode)
    {
        if (pathNode == this)
        {
            return 0;
        }
        else
        {
            return this.x != pathNode.x && this.y != pathNode.y ? 14 : 10;
        }
    }
}
