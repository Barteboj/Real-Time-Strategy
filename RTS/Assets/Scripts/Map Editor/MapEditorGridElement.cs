using UnityEngine;
using System.Collections;

public class MapEditorGridElement
{
    public IntVector2 positionInMapGrid;
    public Tile tile;
    public MineInMapEditor mine;
    public LumberInMapEditor lumber;

    public MapEditorGridElement(IntVector2 positionInMapGrid, Tile tile, MineInMapEditor mine)
    {
        this.positionInMapGrid = positionInMapGrid;
        this.tile = tile;
        this.mine = mine;
    }
}
