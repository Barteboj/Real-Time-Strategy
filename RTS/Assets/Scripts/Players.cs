using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Players : MonoBehaviour
{
    private static Players instance;

    public static Players Instance
    {
        get
        {
            if (instance == null)
            {
                Players foundPlayers = FindObjectOfType<Players>();
                if (foundPlayers != null)
                {
                    instance = foundPlayers;
                    return instance;
                }
                else
                {
                    Debug.LogError("No players instance on scene");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public List<Player> players = new List<Player>();

    public Player LocalPlayer
    {
        get
        {
            return players[0];
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of Players - destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
            players.Add(gameObject.AddComponent<Player>());
        }
    }
}
