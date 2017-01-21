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
        }
    }

    public List<MapGridElement> FindPath(IntVector2 startNodePosition, IntVector2 goalNodePosition)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        MapGridElement goalNode = MapGridded.Instance.MapGrid[goalNodePosition.Y, goalNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.X - goalNode.X) + Mathf.Abs(startNode.Y - goalNode.Y)) * 10;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (actualOpenNode == goalNode)
            {
                return ReconstructPath(startNode, goalNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.X - goalNode.X) + Mathf.Abs(nextNode.Y - goalNode.Y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForLumber(IntVector2 startNodePosition, out LumberInGame lumberToCut)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = 0;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Lumber != null && !item.Lumber.IsDepleted && !item.Lumber.IsBeingCut) != null)
            {
                lumberToCut = MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Lumber != null && !item.Lumber.IsDepleted && !item.Lumber.IsBeingCut).Lumber;
                return ReconstructPath(startNode, actualOpenNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = 0;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        lumberToCut = null;
        return null;
    }

    public List<MapGridElement> FindPathForMine(IntVector2 startNodePosition, Mine mine)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.X - MapGridded.WorldToMapPosition(mine.transform.position).X) + Mathf.Abs(startNode.Y - MapGridded.WorldToMapPosition(mine.transform.position).Y)) * 10;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Mine == mine) != null)
            {
                return ReconstructPath(startNode, actualOpenNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.X - MapGridded.WorldToMapPosition(mine.transform.position).X) + Mathf.Abs(nextNode.Y - MapGridded.WorldToMapPosition(mine.transform.position).Y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForUnit(IntVector2 startNodePosition, Unit unit)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.X - unit.PositionInGrid.X) + Mathf.Abs(startNode.Y - unit.PositionInGrid.Y)) * 10;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Unit == unit) != null)
            {
                return ReconstructPath(startNode, actualOpenNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.X - unit.PositionInGrid.X) + Mathf.Abs(nextNode.Y - unit.PositionInGrid.Y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForBuilding(IntVector2 startNodePosition, Building building)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.X - MapGridded.WorldToMapPosition(building.transform.position).X) + Mathf.Abs(startNode.Y - MapGridded.WorldToMapPosition(building.transform.position).Y)) * 10;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Building == building) != null)
            {
                return ReconstructPath(startNode, actualOpenNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.X - MapGridded.WorldToMapPosition(building.transform.position).X) + Mathf.Abs(nextNode.Y - MapGridded.WorldToMapPosition(building.transform.position).Y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForNearestCastle(IntVector2 startNodePosition, PlayerType castleOwner, out Building castle)
    {
        MapGridElement startNode = MapGridded.Instance.MapGrid[startNodePosition.Y, startNodePosition.X];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.PathNode.CostFromStart = 0;
        startNode.PathNode.HeuristicCostToGoal = 0;
        startNode.PathNode.CostFunctionValue = startNode.PathNode.CostFromStart + startNode.PathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Building != null && item.Building.BuildingType == BuildingType.Castle && item.Building.Owner == castleOwner) != null)
            {
                castle = MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.X, actualOpenNode.Y)).Find(item => item.Building != null && item.Building.BuildingType == BuildingType.Castle && item.Building.Owner == castleOwner).Building;
                return ReconstructPath(startNode, actualOpenNode);
            }
            openNodes.Remove(actualOpenNode);
            closedNodes.Add(actualOpenNode);
            foreach (MapGridElement nextNode in GetAdjacentNodesForPath(actualOpenNode, startNodePosition))
            {
                if (closedNodes.Contains(nextNode))
                {
                    continue;
                }
                int testedCostFromStart = actualOpenNode.PathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.PathNode.HeuristicCostToGoal = 0;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.PathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.PathNode.parentPathNode = actualOpenNode;
                    nextNode.PathNode.CostFromStart = testedCostFromStart;
                    nextNode.PathNode.CostFunctionValue = nextNode.PathNode.CostFromStart + nextNode.PathNode.HeuristicCostToGoal;
                }
            }
        }
        castle = null;
        return null;
    }

    public List<MapGridElement> ReconstructPath(MapGridElement start, MapGridElement goal)
    {
        List<MapGridElement> path = new List<MapGridElement>();
        MapGridElement actualNode = goal;
        while (actualNode.PathNode != start.PathNode)
        {
            path.Add(actualNode);
            actualNode = actualNode.PathNode.parentPathNode;
        }
        path.Reverse();
        return path;
    }

    public MapGridElement GetPathNodeWithMinimumFunctionValue(List<MapGridElement> pathNodes)
    {
        MapGridElement pathNodeWithMinimumF = pathNodes[0];
        foreach (MapGridElement actualPathNode in pathNodes)
        {
            if (actualPathNode.PathNode.CostFunctionValue < pathNodeWithMinimumF.PathNode.CostFunctionValue)
            {
                pathNodeWithMinimumF = actualPathNode;
            }
        }
        return pathNodeWithMinimumF;
    }

    public MapGridElement[] GetAdjacentNodesForPath(MapGridElement pathNode, IntVector2 startPathPosition)
    {
        List<MapGridElement> adjacentNodes = new List<MapGridElement>();
        if (pathNode.Y - 1 >= 0 && MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X]);
        }
        if (pathNode.Y + 1 < MapGridded.Instance.MapGrid.GetLength(0) && MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X]);
        }
        if (pathNode.X + 1 < MapGridded.Instance.MapGrid.GetLength(1) && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X + 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X + 1]);
        }
        if (pathNode.X - 1 >= 0 && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X - 1]);
        }
        if (pathNode.X - 1 >= 0 && pathNode.Y - 1 >= 0 && MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X - 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X - 1]);
        }
        if (pathNode.X + 1 < MapGridded.Instance.MapGrid.GetLength(1) && pathNode.Y - 1 >= 0 && MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X + 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y - 1, pathNode.X + 1]);
        }
        if (pathNode.X + 1 < MapGridded.Instance.MapGrid.GetLength(1) && pathNode.Y + 1 < MapGridded.Instance.MapGrid.GetLength(0) && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X + 1]);
        }
        if (pathNode.X - 1 >= 0 && pathNode.Y + 1 < MapGridded.Instance.MapGrid.GetLength(0) && MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X - 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.MapGrid[pathNode.Y, pathNode.X - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.MapGrid[pathNode.Y + 1, pathNode.X - 1]);
        }
        return adjacentNodes.ToArray();
    }
}
