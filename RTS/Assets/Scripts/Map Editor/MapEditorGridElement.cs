using UnityEngine;
using System.Collections;

public class MapEditorGridElement
{
    public IntVector2 PositionInMapGrid { get; set; }
    public Tile Tile { get; set; }
    public MineInMapEditor Mine { get; set; }
    public LumberInMapEditor Lumber { get; set; }

    public MapEditorGridElement(IntVector2 positionInMapGrid, Tile tile, MineInMapEditor mine)
    {
        this.PositionInMapGrid = positionInMapGrid;
        this.Tile = tile;
        this.Mine = mine;
    }
}
