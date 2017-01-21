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
    [SerializeField]
    private TileType tileType;
    [SerializeField]
    private Image tileRepresantation; 

    void Awake()
    {
        tileRepresantation.sprite = Tiles.Instance.TilesPrefabs.Find(tilePrefab => tilePrefab.TileType == tileType).Sprite;
    }

    public void SelectOnEditor()
    {
        MapEditor.Instance.SelectTile(tileType);
    }
}
