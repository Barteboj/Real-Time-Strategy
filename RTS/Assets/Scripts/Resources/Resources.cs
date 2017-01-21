using UnityEngine;
using System.Collections;

public class Resources : MonoBehaviour
{
    [SerializeField]
    private GameObject minePrefab;
    public GameObject MinePrefab
    {
        get
        {
            return minePrefab;
        }
    }
    [SerializeField]
    private GameObject treePrefab;
    public GameObject TreePrefab
    {
        get
        {
            return treePrefab;
        }
    }

    private static Resources instance;

    public static Resources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Resources>();
            }
            return instance;
        }
    }

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
}
