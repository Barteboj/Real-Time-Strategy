using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapEditorMainMenuController : MonoBehaviour
{
    private static MapEditorMainMenuController instance;

    public GameObject mapCreationWindow;
    public GameObject mapLoadingWindow;
    public GameObject gridWithMapsGameObject;

    public static MapEditorMainMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                MapEditorMainMenuController newInstance = FindObjectOfType<MapEditorMainMenuController>();
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

    public void HideAdditionalWindows()
    {
        mapCreationWindow.SetActive(false);
        mapLoadingWindow.SetActive(false);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
