using UnityEngine;
using System.Collections;

public class Resources : MonoBehaviour
{
    public GameObject minePrefab;
    public GameObject treePrefab;

    private static Resources instance;

    public static Resources Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<Resources>())
                {
                    instance = FindObjectOfType<Resources>();
                    return instance;
                }
                else
                {
                    Debug.LogError("Resources instance not added to scene and is tried to be obtained");
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
            Debug.LogError("More than one instance of Resources destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
