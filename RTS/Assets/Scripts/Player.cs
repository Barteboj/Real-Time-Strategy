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

    private int lumberAmount = 0;
    public int LumberAmount
    {
        get
        {
            return lumberAmount;
        }
        set
        {
            lumberAmount = value;
            ResourcesGUI.Instance.lumberText.text = lumberAmount.ToString();
        }
    }

    private int foodMaxAmount = 0;
    public int FoodMaxAmount
    {
        get
        {
            return foodMaxAmount;
        }
        set
        {
            foodMaxAmount = value;
            ResourcesGUI.Instance.foodMaxText.text = foodMaxAmount.ToString();
        }
    }

    private int foodAmount = 0;
    public int FoodAmount
    {
        get
        {
            return foodAmount;
        }
        set
        {
            foodAmount = value;
            ResourcesGUI.Instance.foodText.text = foodAmount.ToString();
        }
    }

    public bool HasCastle
    {
        get
        {
            return castles != null && castles.Count > 0;
        }
    }

    public Color color;

    void Awake()
    {
        GoldAmount = 10000;
        FoodMaxAmount = 10000;
    }
}
