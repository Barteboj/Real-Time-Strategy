using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourcesGUI : MonoBehaviour
{
    private static ResourcesGUI instance;

    public static ResourcesGUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ResourcesGUI>();
            }
            return instance;
        }
    }

    [SerializeField]
    private Text goldText;
    public Text GoldText
    {
        get
        {
            return goldText;
        }
    }
    [SerializeField]
    private Text lumberText;
    public Text LumberText
    {
        get
        {
            return lumberText;
        }
    }
    [SerializeField]
    private Text foodText;
    public Text FoodText
    {
        get
        {
            return foodText;
        }
    }
    [SerializeField]
    private Text foodMaxText;
    public Text FoodMaxText
    {
        get
        {
            return foodMaxText;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of ResourcesGUI destroying escessive one");
        }
        else
        {
            instance = this;
        }
    }
}
