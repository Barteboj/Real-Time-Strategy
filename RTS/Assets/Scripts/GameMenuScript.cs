using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameMenuScript : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        NetworkManager.Shutdown();
        SceneManager.LoadScene("Main Menu");
    }
}
