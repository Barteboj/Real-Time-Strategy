using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    private static LobbyMenu instance;

    public static LobbyMenu Instance
    {
        get
        {
            if (instance == null)
            {
                LobbyMenu foundInstance = FindObjectOfType<LobbyMenu>();
                if (foundInstance != null)
                {
                    instance = foundInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("No LobbyMenu instance on scene and is being tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public Text player1StatusText;
    public Text player2StatusText;
    public Button playButton;
    public Text waitingForHostText;
    public Slider startingGoldSlider;
    public Slider startingLumberSlider;
}
