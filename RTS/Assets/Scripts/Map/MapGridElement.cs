using UnityEngine;
using System.Collections;

public class MapGridElement : MonoBehaviour
{
    public Unit unit;
    public Building building;
    public Tile tile;
    public PathNode pathNode;
    public bool isFogged;
    public bool isSighted;
}
