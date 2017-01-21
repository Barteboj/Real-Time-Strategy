using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildings : MonoBehaviour
{
    [SerializeField]
    private List<Building> buildingsList;
    public List<Building> BuildingsList
    {
        get
        {
            return buildingsList;
        }
    }

    private static Buildings instance;

    public static Buildings Instance
    {
        get
        {
            if (instance == null)
            {
                Buildings newInstance = FindObjectOfType<Buildings>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not Buildings attached to scene and is tried to be obtained");
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
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public GameObject GetBuildingPrefab(BuildingType buildingType, PlayerType owner)
    {
        return buildingsList.Find(item => item.BuildingType == buildingType && item.Owner == owner).gameObject;
    }
}
