using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private Image healthBar;
    [SerializeField]
    private Text actualHealthText;
    [SerializeField]
    private Text maxHealthText;
    [SerializeField]
    private Unit selectedUnit;
    public Unit SelectedUnit
    {
        get
        {
            return selectedUnit;
        }
    }
    [SerializeField]
    private Building selectedBuilding;
    public Building SelectedBuilding
    {
        get
        {
            return selectedBuilding;
        }
    }
    [SerializeField]
    private Mine selectedMine;
    public Mine SelectedMine
    {
        get
        {
            return selectedMine;
        }
    }
    [SerializeField]
    private GameObject healthInfoGameObject;

    private void Update()
    {
        if (selectedUnit != null)
        {
            UpdateUnitSelectionView();
        }
        else if (selectedBuilding != null)
        {
            UpdateBuildingSelectionView();
        }
        else if (selectedMine != null)
        {
            UpdateMineSelectionView();
        }
    }

    public bool IsSomethingSelected
    {
        get
        {
            return selectedBuilding != null || selectedUnit != null || selectedMine != null;
        }
    }

    public void UpdateUnitSelectionView()
    {
        portrait.sprite = selectedUnit.Portrait;
        actualHealthText.text = selectedUnit.ActualHealth.ToString();
        maxHealthText.text = selectedUnit.MaxHealth.ToString();
        healthBar.fillAmount = (float)selectedUnit.ActualHealth / selectedUnit.MaxHealth;
        if (healthBar.fillAmount <= selectedUnit.CriticalDamageFactor)
        {
            healthBar.color = Color.red;
        }
        else if (healthBar.fillAmount <= selectedUnit.AverageDamageFactor)
        {
            healthBar.color = Color.yellow;
        }
        else
        {
            healthBar.color = Color.green;
        }
    }

    public void UpdateBuildingSelectionView()
    {
        portrait.sprite = selectedBuilding.Portrait;
        maxHealthText.text = selectedBuilding.MaxHealth.ToString();
        actualHealthText.text = selectedBuilding.ActualHealth.ToString();
        healthBar.fillAmount = (float)selectedBuilding.ActualHealth / selectedBuilding.MaxHealth;
        if ((float)selectedBuilding.ActualHealth / selectedBuilding.MaxHealth < selectedBuilding.CriticalDamageFactor)
        {
            healthBar.color = Color.red;
        }
        else if ((float)selectedBuilding.ActualHealth / selectedBuilding.MaxHealth < selectedBuilding.AverageDamageFactor)
        {
            healthBar.color = Color.yellow;
        }
        else
        {
            healthBar.color = Color.green;
        }
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == selectedBuilding.Owner)
        {
            if (selectedBuilding.ActualBuildTime < selectedBuilding.BuildTime)
            {
                SelectionInfoKeeper.Instance.SetCompletitionBar(selectedBuilding.ActualBuildTime / selectedBuilding.BuildTime);
            }
            if (selectedBuilding.IsTraining)
            {
                SelectionInfoKeeper.Instance.SetTrainingBar(selectedBuilding.ActualTrainingTime / selectedBuilding.TrainedUnit.TrainingTime);
            }
        }
    }

    public void UpdateMineSelectionView()
    {
        portrait.sprite = selectedMine.Portrait;
    }

    public void ShowSelection()
    {
        gameObject.SetActive(true);
    }

    public void HideSelection()
    {
        gameObject.SetActive(false);
    }

    public void Select(Unit unit)
    {
        healthInfoGameObject.SetActive(true);
        selectedUnit = unit;
        UpdateUnitSelectionView();
        ShowSelection();
    }

    public void Select(Building building)
    {
        healthInfoGameObject.SetActive(true);
        selectedBuilding = building;
        UpdateBuildingSelectionView();
        ShowSelection();
    }

    public void Select(Mine mine)
    {
        healthInfoGameObject.SetActive(true);
        selectedMine = mine;
        UpdateMineSelectionView();
        ShowSelection();
    }

    public void Unselect()
    {
        selectedUnit = null;
        selectedBuilding = null;
        selectedMine = null;
        HideSelection();
    }
}
