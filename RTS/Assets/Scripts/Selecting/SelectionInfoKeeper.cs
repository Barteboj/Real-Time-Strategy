using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SelectionInfoKeeper : MonoBehaviour
{
    [SerializeField]
    private Text unitName;
    public Text UnitName
    {
        get
        {
            return unitName;
        }
    }

    [SerializeField]
    private GameObject buildCompletitionBar;
    [SerializeField]
    private Image buildCompletitionBarFill;

    [SerializeField]
    private GameObject trainingUnitGameObject;
    public GameObject TrainingUnitGameObject
    {
        get
        {
            return trainingUnitGameObject;
        }
    }
    [SerializeField]
    private Image trainingCompletitionBarFill;
    [SerializeField]
    private Image trainedUnitPortrait;
    public Image TrainedUnitPortrait
    {
        get
        {
            return trainedUnitPortrait;
        }
    }

    [SerializeField]
    private GameObject goldLeftInfoGameObject;
    public GameObject GoldLeftInfoGameObject
    {
        get
        {
            return goldLeftInfoGameObject;
        }
    }

    [SerializeField]
    private Text goldLeftAmountText;
    public Text GoldLeftAmountText
    {
        get
        {
            return goldLeftAmountText;
        }
    }

    [SerializeField]
    private List<Selection> selections;
    public List<Selection> Selections
    {
        get
        {
            return selections;
        }
    }

    private static SelectionInfoKeeper instance;

    public static SelectionInfoKeeper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SelectionInfoKeeper>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of SelectionInfoKeeper destroying excessive");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void ShowBuildCompletitionBar()
    {
        buildCompletitionBar.SetActive(true);
    }

    public void HideBuildCompletitionBar()
    {
        buildCompletitionBar.SetActive(false);
    }

    public void ShowTrainingInfo()
    {
        trainingUnitGameObject.SetActive(true);
    }

    public void HideTrainingInfo()
    {
        trainingUnitGameObject.SetActive(false);
    }

    public void SetCompletitionBar(float fillAmount)
    {
        buildCompletitionBarFill.fillAmount = fillAmount;
    }

    public void SetTrainingBar(float fillAmount)
    {
        trainingCompletitionBarFill.fillAmount = fillAmount;
    }
}
