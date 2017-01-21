using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LumberInGame : NetworkBehaviour
{
    private bool isDepleted = false;
    public bool IsBeingCut { get; set; }

    public bool IsDepleted
    {
        get
        {
            return isDepleted;
        }
    }

    [SerializeField]
    private Sprite depletedSprite;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private MinimapElement minimapElement;

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
        MapGridded.Instance.MapGrid[positionOnMapGrid.Y, positionOnMapGrid.X].Lumber = this;
    }
}
