using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayersOnline : MonoBehaviour
{
    private static PlayersOnline instance;

    public static PlayersOnline Instance
    {
        get
        {
            if (instance == null)
            {
                PlayersOnline newInstance = FindObjectOfType<PlayersOnline>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not PlayersOnline attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public List<PlayerOnline> players = new List<PlayerOnline>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void AddPlayer(PlayerOnline player)
    {
        players.Add(player);
    }
}
