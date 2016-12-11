using UnityEngine;
using System.Collections;

public class PathNode
{
    public int CostFunctionValue;
    public int CostFromStart;
    public int HeuristicCostToGoal;

    public MapGridElement parentPathNode;

    public bool isWalkable;
}
