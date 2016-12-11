using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourcesGUI : MonoBehaviour
{
    private static ResourcesGUI instance;

    public static ResourcesGUI Instance
    {
        get
        {
            if (instance == null)
            {
                ResourcesGUI foundResourcesGUI = FindObjectOfType<ResourcesGUI>();
                if (foundResourcesGUI != null)
                {
                    instance = foundResourcesGUI;
                    return instance;
                }
                else
                {
                    Debug.LogError("No ResourcesGUI attached to scene and is being tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public Text goldText;
    public Text lumberText;
    public Text foodText;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of ResourcesGUI destroying escessive one");
        }
        else
        {
            instance = this;
        }
    }
}
