using UnityEngine;
using System.Collections;

public class PathNode
{
    public int CostFunctionValue { get; set; }
    public int CostFromStart { get; set; }
    public int HeuristicCostToGoal { get; set; }
    public MapGridElement parentPathNode { get; set; }
}
