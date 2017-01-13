using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tiles : MonoBehaviour
{
    private static Tiles instance;

    public List<Tile> tilesPrefabs;
    public GameObject canBuildIndicator;
    public GameObject cannotBuildIndicator;

    public static Tiles Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<Tiles>())
                {
                    instance = FindObjectOfType<Tiles>();
                    return instance;
                }
                else
                {
                    Debug.LogError("Tiles instance not added to scene and is tried to be obtained");
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
            Debug.LogError("More than one instance of Tiles destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
