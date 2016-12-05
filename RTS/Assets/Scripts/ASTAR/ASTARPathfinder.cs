using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ASTARPathfinder : MonoBehaviour
{
    private static ASTARPathfinder instance;

    public static ASTARPathfinder Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<ASTARPathfinder>())
                {
                    instance = FindObjectOfType<ASTARPathfinder>();
                    return instance;
                }
                else
                {
                    Debug.LogError("ASTARPathfinder instance not added to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public PathNode[,] pathNodes;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of ASTARPathfinder destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
            Tile[,] map = Map.Instance.mapTiles;
            pathNodes = new PathNode[map.GetLength(0), map.GetLength(1)];
            for (int row = 0; row < map.GetLength(0); ++row)
            {
                for (int column = 0; column < map.GetLength(1); ++column)
                {
                    pathNodes[row, column] = new PathNode(map[row, column], column, row);
                }
            }
        }
    }

    public List<PathNode> FindPath(IntVector2 startNodePosition, IntVector2 goalNodePosition)
    {
        return FindPath(pathNodes[startNodePosition.y, startNodePosition.x], pathNodes[goalNodePosition.y, goalNodePosition.x]);
    }

    public List<PathNode> FindPath(PathNode startNode, PathNode goalNode)
    {
        List<PathNode> closedNodes = new List<PathNode>();
        List<PathNode> openNodes = new List<PathNode>();
        openNodes.Add(startNode);
        startNode.CostFromStart = 0;
        startNode.HeuristicCostToGoal = (Mathf.Abs(startNode.x - goalNode.x) + Mathf.Abs(startNode.y - goalNode.y)) * 10;
        startNode.CostFunctionValue = startNode.CostFromStart + startNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            PathNode actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (actualOpenNode == goalNode)
            {
                return ReconstructPath(startNode, goalNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (PathNode nextNode in GetAdjacentNodes(actualOpenNode))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.x - goalNode.x) + Mathf.Abs(nextNode.y - goalNode.y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    //Map.Instance.mapTiles[nextNode.y, nextNode.x].GetComponent<SpriteRenderer>().color = Color.blue;
                    nextNode.parentPathNode = actualOpenNode;
                    nextNode.CostFromStart = testedCostFromStart;
                    nextNode.CostFunctionValue = nextNode.CostFromStart + nextNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<PathNode> ReconstructPath(PathNode start, PathNode goal)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode actualNode = goal;
        while (actualNode != start)
        {
            path.Add(actualNode);
            actualNode = actualNode.parentPathNode;
        }
        /*foreach (PathNode node in path)
        {
            Map.Instance.mapTiles[node.y, node.x].GetComponent<SpriteRenderer>().color = Color.red;
        }*/
        path.Reverse();
        return path;
    }

    public PathNode GetPathNodeWithMinimumFunctionValue(List<PathNode> pathNodes)
    {
        PathNode pathNodeWithMinimumF = pathNodes[0];
        foreach (PathNode actualPathNode in pathNodes)
        {
            if (actualPathNode.CostFunctionValue < pathNodeWithMinimumF.CostFunctionValue)
            {
                pathNodeWithMinimumF = actualPathNode;
            }
        }
        return pathNodeWithMinimumF;
    }

    public PathNode[] GetAdjacentNodes(PathNode pathNode)
    {
        List<PathNode> adjacentNodes = new List<PathNode>();
        if (pathNode.y - 1 >= 0 && pathNodes[pathNode.y - 1, pathNode.x].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y - 1, pathNode.x]);
        }
        if (pathNode.y + 1 < pathNodes.GetLength(0) && pathNodes[pathNode.y + 1, pathNode.x].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y + 1, pathNode.x]);
        }
        if (pathNode.x + 1 < pathNodes.GetLength(1) && pathNodes[pathNode.y, pathNode.x + 1].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y, pathNode.x + 1]);
        }
        if (pathNode.x - 1 >= 0 && pathNodes[pathNode.y, pathNode.x - 1].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y, pathNode.x - 1]);
        }
        if (pathNode.x - 1 >= 0 && pathNode.y - 1 >= 0 && pathNodes[pathNode.y - 1, pathNode.x].isWalkable && pathNodes[pathNode.y - 1, pathNode.x - 1].isWalkable && pathNodes[pathNode.y, pathNode.x - 1].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y - 1, pathNode.x - 1]);
        }
        if (pathNode.x + 1 < pathNodes.GetLength(1) && pathNode.y - 1 >= 0 && pathNodes[pathNode.y - 1, pathNode.x].isWalkable && pathNodes[pathNode.y - 1, pathNode.x + 1].isWalkable && pathNodes[pathNode.y, pathNode.x + 1].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y - 1, pathNode.x + 1]);
        }
        if (pathNode.x + 1 < pathNodes.GetLength(1) && pathNode.y + 1 < pathNodes.GetLength(0) && pathNodes[pathNode.y, pathNode.x + 1].isWalkable && pathNodes[pathNode.y + 1, pathNode.x + 1].isWalkable && pathNodes[pathNode.y + 1, pathNode.x].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y + 1, pathNode.x + 1]);
        }
        if (pathNode.x - 1 >= 0 && pathNode.y + 1 < pathNodes.GetLength(0) && pathNodes[pathNode.y + 1, pathNode.x].isWalkable && pathNodes[pathNode.y + 1, pathNode.x - 1].isWalkable && pathNodes[pathNode.y, pathNode.x - 1].isWalkable)
        {
            adjacentNodes.Add(pathNodes[pathNode.y + 1, pathNode.x - 1]);
        }
        return adjacentNodes.ToArray();
    }
}
