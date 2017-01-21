using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MessagesController : NetworkBehaviour
{
    [SerializeField]
    private Text messageGUI;
    [SerializeField]
    private float messageTime;

    private Coroutine showMessageCoroutine;

    private static MessagesController instance;

    public static MessagesController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MessagesController>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    [ClientRpc]
    public void RpcShowMessage(string messageText, PlayerType targetPlayer)
    {
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == targetPlayer)
        {
            messageGUI.text = messageText;
            messageGUI.enabled = true;
            if (showMessageCoroutine != null)
            {
                StopCoroutine(showMessageCoroutine);
            }
            showMessageCoroutine = StartCoroutine(HideMessageAfterTime());
        }
    }

    public IEnumerator HideMessageAfterTime()
    {
        yield return new WaitForSeconds(messageTime);
        messageGUI.enabled = false;
    }
}
