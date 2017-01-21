using UnityEngine;
using System.Collections;

public class MapGridElement
{
    public Unit Unit { get; set; }
    public Building Building { get; set; }
    public Tile Tile { get; set; }
    public Mine Mine { get; set; }
    public LumberInGame Lumber { get; set; }
    public PathNode PathNode { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public GameObject CanBuildIndicator { get; set; }
    public GameObject CannotBuildIndicator { get; set; }

    public bool IsWalkable
    {
        get
        {
            return Unit == null && Building == null && Mine == null && (Lumber == null || Lumber.IsDepleted) && Tile.IsWalkable;
        }
    }

    public bool CheckIfIsGoodForPath(IntVector2 pathStartPosition)
    {
        return (this.Unit == null || ((Mathf.Abs(this.Unit.PositionInGrid.X - pathStartPosition.X) > 1 || Mathf.Abs(this.Unit.PositionInGrid.Y - pathStartPosition.Y) > 1) && this.Unit.IsMoving)) && Building == null && Mine == null && (Lumber == null || Lumber.IsDepleted) && Tile.IsWalkable;
    }

    public bool ChecklIfIsWalkableForUnit(Unit unit)
    {
        return (this.Unit == null || this.Unit == unit) && Building == null && Mine == null && (Lumber == null || Lumber.IsDepleted) && Tile.IsWalkable;
    }

    public void ShowCanBuildIndicator()
    {
        CanBuildIndicator.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void ShowCannotBuildIndicator()
    {
        CannotBuildIndicator.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void HideCanBuildIndicator()
    {
        CanBuildIndicator.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void HideCannotBuildIndicator()
    {
        CannotBuildIndicator.GetComponent<SpriteRenderer>().enabled = false;
    }

    public MapGridElement(int x, int y, Tile tile, GameObject canBuildIndicator, GameObject cannotBuildIndicator)
    {
        this.X = x;
        this.Y = y;
        this.Tile = tile;
        PathNode = new PathNode();
        this.CanBuildIndicator = canBuildIndicator;
        this.CannotBuildIndicator = cannotBuildIndicator;
    }

    public MapGridElement(Tile tile)
    {
        this.Tile = tile;
    }

    public int GetCostToGetHereFrom(MapGridElement pathNode)
    {
        if (pathNode == this)
        {
            return 0;
        }
        else
        {
            return this.X != pathNode.X && this.Y != pathNode.Y ? 14 : 10;
        }
    }
}
