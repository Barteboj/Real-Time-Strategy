using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CostGUI : MonoBehaviour
{
    public GameObject goldcostGUI;
    public GameObject lumberCostGUI;
    public GameObject foodCostGUI;
    public Text goldCostText;
    public Text lumberCostText;
    public Text foodCostText;

    public GameObject GUI;

    private static CostGUI instance;

    public static CostGUI Instance
    {
        get
        {
            if (instance == null)
            {
                if (FindObjectOfType<CostGUI>())
                {
                    instance = FindObjectOfType<CostGUI>();
                    return instance;
                }
                else
                {
                    Debug.LogError("CostGUI instance not added to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public bool IsVisible
    {
        get
        {
            return GUI.activeInHierarchy;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of CostGUI destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void ShowCostGUI(int goldCostAmount, int lumberGoldAmount, int foodCostAmount)
    {
        HideCostGUI();

        goldCostText.text = goldCostAmount.ToString();
        lumberCostText.text = lumberGoldAmount.ToString();
        foodCostText.text = foodCostAmount.ToString();

        if (goldCostAmount > 0)
        {
            goldcostGUI.SetActive(true);
        }
        if (lumberGoldAmount > 0)
        {
            lumberCostGUI.SetActive(true);
        }
        if (foodCostAmount > 0)
        {
            foodCostGUI.SetActive(true);
        }
        GUI.SetActive(true);
    }

    public void HideCostGUI()
    {
        goldcostGUI.SetActive(false);
        lumberCostGUI.SetActive(false);
        foodCostGUI.SetActive(false);
        GUI.SetActive(false);
    }
}
