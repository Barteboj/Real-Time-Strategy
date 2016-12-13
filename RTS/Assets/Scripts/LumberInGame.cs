using UnityEngine;
using System.Collections;

public class LumberInGame : MonoBehaviour
{
    private bool isDepleted = false;

    public bool IsDepleted
    {
        get
        {
            return isDepleted;
        }
    }
    public Sprite depletedSprite;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        PlaceYourselfOnMapGrid();
    }

    public void Deplete()
    {
        isDepleted = true;
        spriteRenderer.sprite = depletedSprite;
    }

    private void PlaceYourselfOnMapGrid()
    {
        IntVector2 positionOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        MapGridded.Instance.mapGrid[positionOnMapGrid.y, positionOnMapGrid.x].lumber = this;
    }
}
