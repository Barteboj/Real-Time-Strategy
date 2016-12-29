using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : MonoBehaviour
{
    public List<Unit> unitsList;

    private static Units instance;

    public static Units Instance
    {
        get
        {
            if (instance == null)
            {
                Units newInstance = FindObjectOfType<Units>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not Units attached to scene and is tried to be obtained");
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

    public GameObject GetUnitPrefabFromUnitType(UnitType unitType)
    {
        return unitsList.Find(item => item.unitType == unitType).gameObject;
    }
}
