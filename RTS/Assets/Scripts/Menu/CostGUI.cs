using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CostGUI : MonoBehaviour
{
    [SerializeField]
    private GameObject goldcostGUI;
    [SerializeField]
    private GameObject lumberCostGUI;
    [SerializeField]
    private GameObject foodCostGUI;
    [SerializeField]
    private Text goldCostText;
    [SerializeField]
    private Text lumberCostText;
    [SerializeField]
    private Text foodCostText;
    [SerializeField]
    private GameObject GUI;

    private static CostGUI instance;

    public static CostGUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CostGUI>();
            }
            return instance;
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
