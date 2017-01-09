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
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        MapGridElement goalNode = MapGridded.Instance.mapGrid[goalNodePosition.y, goalNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.x - goalNode.x) + Mathf.Abs(startNode.y - goalNode.y)) * 10;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.x - goalNode.x) + Mathf.Abs(nextNode.y - goalNode.y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForLumber(IntVector2 startNodePosition, out LumberInGame lumberToCut)
    {
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = 0;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.lumber != null && !item.lumber.IsDepleted && !item.lumber.isBeingCut) != null)
            {
                lumberToCut = MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.lumber != null && !item.lumber.IsDepleted && !item.lumber.isBeingCut).lumber;
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = 0;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
                }
            }
        }
        lumberToCut = null;
        return null;
    }

    public List<MapGridElement> FindPathForMine(IntVector2 startNodePosition, Mine mine)
    {
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.x - MapGridded.WorldToMapPosition(mine.transform.position).x) + Mathf.Abs(startNode.y - MapGridded.WorldToMapPosition(mine.transform.position).y)) * 10;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.mine == mine) != null)
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.x - MapGridded.WorldToMapPosition(mine.transform.position).x) + Mathf.Abs(nextNode.y - MapGridded.WorldToMapPosition(mine.transform.position).y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForUnit(IntVector2 startNodePosition, Unit unit)
    {
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.x - unit.positionInGrid.x) + Mathf.Abs(startNode.y - unit.positionInGrid.y)) * 10;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.unit == unit) != null)
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.x - unit.positionInGrid.x) + Mathf.Abs(nextNode.y - unit.positionInGrid.y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForBuilding(IntVector2 startNodePosition, Building building)
    {
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(startNode.x - MapGridded.WorldToMapPosition(building.transform.position).x) + Mathf.Abs(startNode.y - MapGridded.WorldToMapPosition(building.transform.position).y)) * 10;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.building == building) != null)
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = (Mathf.Abs(nextNode.x - MapGridded.WorldToMapPosition(building.transform.position).x) + Mathf.Abs(nextNode.y - MapGridded.WorldToMapPosition(building.transform.position).y)) * 10;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
                }
            }
        }
        return null;
    }

    public List<MapGridElement> FindPathForNearestCastle(IntVector2 startNodePosition, PlayerType castleOwner, out Building castle)
    {
        MapGridElement startNode = MapGridded.Instance.mapGrid[startNodePosition.y, startNodePosition.x];
        List<MapGridElement> closedNodes = new List<MapGridElement>();
        List<MapGridElement> openNodes = new List<MapGridElement>();
        openNodes.Add(startNode);
        startNode.pathNode.CostFromStart = 0;
        startNode.pathNode.HeuristicCostToGoal = 0;
        startNode.pathNode.CostFunctionValue = startNode.pathNode.CostFromStart + startNode.pathNode.HeuristicCostToGoal;
        while (openNodes.Count > 0)
        {
            MapGridElement actualOpenNode = GetPathNodeWithMinimumFunctionValue(openNodes);
            if (MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.building != null && item.building.buildingType == BuildingType.Castle && item.building.owner == castleOwner) != null)
            {
                castle = MapGridded.Instance.GetAdjacentGridElements(new IntVector2(actualOpenNode.x, actualOpenNode.y)).Find(item => item.building != null && item.building.buildingType == BuildingType.Castle && item.building.owner == castleOwner).building;
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
                int testedCostFromStart = actualOpenNode.pathNode.CostFromStart + nextNode.GetCostToGetHereFrom(actualOpenNode);
                bool isTestedCostFromStartIsBetter = false;
                if (!openNodes.Contains(nextNode))
                {
                    openNodes.Add(nextNode);
                    nextNode.pathNode.HeuristicCostToGoal = 0;
                    isTestedCostFromStartIsBetter = true;
                }
                else if (testedCostFromStart < nextNode.pathNode.CostFromStart)
                {
                    isTestedCostFromStartIsBetter = true;
                }
                if (isTestedCostFromStartIsBetter)
                {
                    nextNode.pathNode.parentPathNode = actualOpenNode;
                    nextNode.pathNode.CostFromStart = testedCostFromStart;
                    nextNode.pathNode.CostFunctionValue = nextNode.pathNode.CostFromStart + nextNode.pathNode.HeuristicCostToGoal;
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
        while (actualNode.pathNode != start.pathNode)
        {
            path.Add(actualNode);
            actualNode = actualNode.pathNode.parentPathNode;
        }
        path.Reverse();
        return path;
    }

    public MapGridElement GetPathNodeWithMinimumFunctionValue(List<MapGridElement> pathNodes)
    {
        MapGridElement pathNodeWithMinimumF = pathNodes[0];
        foreach (MapGridElement actualPathNode in pathNodes)
        {
            if (actualPathNode.pathNode.CostFunctionValue < pathNodeWithMinimumF.pathNode.CostFunctionValue)
            {
                pathNodeWithMinimumF = actualPathNode;
            }
        }
        return pathNodeWithMinimumF;
    }

    public MapGridElement[] GetAdjacentNodesForPath(MapGridElement pathNode, IntVector2 startPathPosition)
    {
        List<MapGridElement> adjacentNodes = new List<MapGridElement>();
        if (pathNode.y - 1 >= 0 && MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x]);
        }
        if (pathNode.y + 1 < MapGridded.Instance.mapGrid.GetLength(0) && MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x]);
        }
        if (pathNode.x + 1 < MapGridded.Instance.mapGrid.GetLength(1) && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x + 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y, pathNode.x + 1]);
        }
        if (pathNode.x - 1 >= 0 && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y, pathNode.x - 1]);
        }
        if (pathNode.x - 1 >= 0 && pathNode.y - 1 >= 0 && MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x - 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x - 1]);
        }
        if (pathNode.x + 1 < MapGridded.Instance.mapGrid.GetLength(1) && pathNode.y - 1 >= 0 && MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x + 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y - 1, pathNode.x + 1]);
        }
        if (pathNode.x + 1 < MapGridded.Instance.mapGrid.GetLength(1) && pathNode.y + 1 < MapGridded.Instance.mapGrid.GetLength(0) && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x + 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x + 1]);
        }
        if (pathNode.x - 1 >= 0 && pathNode.y + 1 < MapGridded.Instance.mapGrid.GetLength(0) && MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x - 1].CheckIfIsGoodForPath(startPathPosition) && MapGridded.Instance.mapGrid[pathNode.y, pathNode.x - 1].CheckIfIsGoodForPath(startPathPosition))
        {
            adjacentNodes.Add(MapGridded.Instance.mapGrid[pathNode.y + 1, pathNode.x - 1]);
        }
        return adjacentNodes.ToArray();
    }

    public List<MapGridElement> FindNearestEntrancePath(IntVector2 positionOfEnterer, IntVector2 positionOfBuilding, int width, int height)
    {
        List<List<MapGridElement>> paths = new List<List<MapGridElement>>();
        for (int row = 0; row <= height; ++row)
        {
            IntVector2 position = new IntVector2(positionOfBuilding.x - 1, positionOfBuilding.y + row);
            if (MapGridded.Instance.IsInMap(position))
            {
                List<MapGridElement> foundPath = FindPath(positionOfEnterer, position);
                if (foundPath != null)
                {
                    paths.Add(foundPath);
                }
            }
        }
        for (int column = 0; column <= width; ++column)
        {
            IntVector2 position = new IntVector2(positionOfBuilding.x + column, positionOfBuilding.y + height);
            if (MapGridded.Instance.IsInMap(position))
            {
                List<MapGridElement> foundPath = FindPath(positionOfEnterer, position);
                if (foundPath != null)
                {
                    paths.Add(foundPath);
                }
            }
        }
        for (int row = height - 1; row >= -1; --row)
        {
            IntVector2 position = new IntVector2(positionOfBuilding.x + width, positionOfBuilding.y + row);
            if (MapGridded.Instance.IsInMap(position))
            {
                List<MapGridElement> foundPath = FindPath(positionOfEnterer, position);
                if (foundPath != null)
                {
                    paths.Add(foundPath);
                }
            }
        }
        for (int column = width - 1; column >= -1; --column)
        {
            IntVector2 position = new IntVector2(positionOfBuilding.x + column, positionOfBuilding.y - 1);
            if (MapGridded.Instance.IsInMap(position))
            {
                List<MapGridElement> foundPath = FindPath(positionOfEnterer, position);
                if (foundPath != null)
                {
                    paths.Add(foundPath);
                }
            }
        }
        if (paths.Count > 0)
        {
            List<MapGridElement> shortestPath = new List<MapGridElement>();
            int minDistance = int.MaxValue;
            foreach (List<MapGridElement> actualPath in paths)
            {
                int distance = 0;
                for (int i = 1; i < actualPath.Count; ++i)
                {
                    distance += actualPath[i].GetCostToGetHereFrom(actualPath[i - 1]);
                }
                if (distance < minDistance)
                {
                    shortestPath = actualPath;
                    minDistance = distance;
                }
            }
            return shortestPath;
        }
        else
        {
            return null;
        }
    }
}
