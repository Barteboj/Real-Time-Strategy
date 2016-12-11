using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public List<Building> castles = new List<Building>();

    private int goldAmount = 0;
    public int GoldAmount
    {
        get
        {
            return goldAmount;
        }
        set
        {
            goldAmount = value;
            ResourcesGUI.Instance.goldText.text = goldAmount.ToString();
        }
    }

    public bool HasCastle
    {
        get
        {
            return castles != null && castles.Count > 0;
        }
    }

    void Awake()
    {
        GoldAmount = 300;
    }
}
