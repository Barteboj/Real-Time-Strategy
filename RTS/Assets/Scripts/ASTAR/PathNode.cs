using UnityEngine;
using System.Collections;

public class PathNode
{
    public int CostFunctionValue;
    public int CostFromStart;
    public int HeuristicCostToGoal;

    public PathNode parentPathNode;

    public bool isWalkable;

    public int x;
    public int y;

    public PathNode(Tile tile, int x, int y)
    {
        isWalkable = tile.isWalkable;
        this.x = x;
        this.y = y;
    }

    public int GetCostToGetHereFrom(PathNode pathNode)
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
