using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LumberInGame : NetworkBehaviour
{
    private bool isDepleted = false;
    public bool isBeingCut = false;

    public bool IsDepleted
    {
        get
        {
            return isDepleted;
        }
    }
    public Sprite depletedSprite;
    public SpriteRenderer spriteRenderer;
    public MinimapElement minimapElement;

    void Awake()
    {
        PlaceYourselfOnMapGrid();
    }

    public void Deplete()
    {
        isDepleted = true;
        spriteRenderer.sprite = depletedSprite;
        minimapElement.Hide();
    }

    [ClientRpc]
    public void RpcDeplete()
    {
        Deplete();
    }

    private void PlaceYourselfOnMapGrid()
    {
        IntVector2 positionOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        MapGridded.Instance.mapGrid[positionOnMapGrid.y, positionOnMapGrid.x].lumber = this;
    }
}
