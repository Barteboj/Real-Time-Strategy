using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum TileType
{
    Grass,
    Rocks
}

public class TileTypeEditorHolder : MonoBehaviour
{
    public TileType tileType;
    public MapEditor mapEditor;
    public Image tileRepresantation; 

    void Awake()
    {
        tileRepresantation.sprite = Tiles.Instance.tilesPrefabs.Find(tilePrefab => tilePrefab.tileType == tileType).sprite;
    }

    public void SelectOnEditor()
    {
        mapEditor.SelectTile(tileType);
    }
}
