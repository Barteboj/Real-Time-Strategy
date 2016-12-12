﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessagesController : MonoBehaviour
{
    public Text messageGUI;
    public float messageTime;

    private Coroutine showMessageCoroutine;

    private static MessagesController instance;

    public static MessagesController Instance
    {
        get
        {
            if (instance == null)
            {
                MessagesController foundInstance = FindObjectOfType<MessagesController>();
                if (foundInstance != null)
                {
                    instance = foundInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("No MessagesController on scene and is trying to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of MessagesController destroying excessive one");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void ShowMessage(string messageText)
    {
        messageGUI.text = messageText;
        messageGUI.enabled = true;
        if (showMessageCoroutine != null)
        {
            StopCoroutine(showMessageCoroutine);
        }
        showMessageCoroutine = StartCoroutine(HideMessageAfterTime());
    }

    public IEnumerator HideMessageAfterTime()
    {
        yield return new WaitForSeconds(messageTime);
        messageGUI.enabled = false;
    }
}