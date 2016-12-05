using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    private static MainMenuController instance;

    public static MainMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                MainMenuController newInstance = FindObjectOfType<MainMenuController>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not MainMenuController attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public GameObject mainMenuObject;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of MainMenuController on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void Show()
    {
        mainMenuObject.SetActive(true);
    }

    public void Hide()
    {
        mainMenuObject.SetActive(false);
    }
}
