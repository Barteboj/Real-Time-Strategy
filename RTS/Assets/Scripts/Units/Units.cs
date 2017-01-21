using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : MonoBehaviour
{
    [SerializeField]
    private List<Unit> unitsList;
    public List<Unit> UnitsList
    {
        get
        {
            return unitsList;
        }
    }

    private static Units instance;

    public static Units Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Units>();
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

    public GameObject GetUnitPrefab(UnitType unitType, PlayerType owner)
    {
        return unitsList.Find(item => item.UnitType == unitType && item.Owner == owner).gameObject;
    }
}
