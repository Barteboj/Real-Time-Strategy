using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    public const string gameSceneName = "Game";
    public const string mapEditorSceneName = "Map Editor";
    public const string lobbySceneName = "Lobby";

    public void LoadGameScene()
    {
        SceneManager.LoadScene(lobbySceneName);
    }

    public void LoadMapEditorScene()
    {
        SceneManager.LoadScene(mapEditorSceneName);
    }

    public void QuitFromGame()
    {
        Application.Quit();
    }
}
