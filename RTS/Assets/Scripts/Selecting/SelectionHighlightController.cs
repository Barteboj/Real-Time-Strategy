using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SelectionHighlightController : MonoBehaviour
{
    private bool isWaitingToTurnOff = false;

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (MultiplayerController.Instance.localPlayer.commander.isServer)
        {
            Unit unitToSelect = collision.collider.transform.parent.GetComponent<Unit>();
            Building buildingToSelect = collision.collider.transform.parent.GetComponent<Building>();
            Mine mineToSelect = collision.collider.transform.parent.GetComponent<Mine>();
            if (unitToSelect != null)
            {
                MultiplayerController.Instance.localPlayer.commander.SelectUnit(unitToSelect);
            }
            else if (buildingToSelect != null)
            {
                MultiplayerController.Instance.localPlayer.commander.SelectBuilding(buildingToSelect);
            }
            else if (mineToSelect != null)
            {
                MultiplayerController.Instance.localPlayer.commander.SelectMine(mineToSelect);
            }
        }
    }*/

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (MultiplayerController.Instance.localPlayer.commander.isServer)
        {
            Unit unitToUnselect = collision.collider.transform.parent.GetComponent<Unit>();
            Building buildingToUnselect = collision.collider.transform.parent.GetComponent<Building>();
            Mine mineToUnselect = collision.collider.transform.parent.GetComponent<Mine>();
            if (unitToUnselect != null)
            {
                //MultiplayerController.Instance.localPlayer.selectController.UnselectUnit(unitToUnselect);
            }
            else if (buildingToUnselect != null)
            {
                //MultiplayerController.Instance.localPlayer.selectController.UnselectBuilding(buildingToUnselect);
            }
            else if (mineToUnselect != null)
            {
                //MultiplayerController.Instance.localPlayer.selectController.UnselectMine(mineToUnselect);
            }
        }
    }

    private void OnEnable()
    {
        if (gameObject.GetComponentInParent<NetworkIdentity>().isLocalPlayer)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
        isWaitingToTurnOff = true;
    }

    private void Update()
    {

        if (!isWaitingToTurnOff)
        {
            gameObject.SetActive(false);
        }
        else
        {
            isWaitingToTurnOff = true;
        }
    }
}
