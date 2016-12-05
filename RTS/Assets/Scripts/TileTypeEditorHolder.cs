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
        tileRepresantation.color = Tiles.Instance.tilesPrefabs.Find(tilePrefab => tilePrefab.tileType == tileType).GetComponent<SpriteRenderer>().color;
    }

    public void AssignOnEditor()
    {
        mapEditor.selectedTilePrefab = Tiles.Instance.tilesPrefabs.Find(tilePrefab => tilePrefab.tileType == tileType);
        if (mapEditor.prefabShowingWhereYouArePuttingTile != null)
        {
            Destroy(mapEditor.prefabShowingWhereYouArePuttingTile);
        }
        mapEditor.prefabShowingWhereYouArePuttingTile = (GameObject)Instantiate(mapEditor.selectedTilePrefab.gameObject);
    }
}
