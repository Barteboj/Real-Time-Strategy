using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class MapEditorMainMenuController : MonoBehaviour
{
    private static MapEditorMainMenuController instance;

    [SerializeField]
    private GameObject mapCreationWindow;
    [SerializeField]
    private GameObject mapLoadingWindow;

    public static MapEditorMainMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapEditorMainMenuController>();
            }
            return instance;
        }
    }

    [SerializeField]
    private GameObject mainMenuObject;

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
        NetworkManager.Shutdown();
        SceneManager.LoadScene(0);
    }
}
