using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    public Image portrait;
    public Image healthBar;
    public Text actualHealthText;
    public Text maxHealthText;
    public Unit selectedUnit;
    public Building selectedBuilding;
    public Mine selectedMine;
    public GameObject healthInfoGameObject;

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
        portrait.sprite = selectedUnit.portrait;
        actualHealthText.text = selectedUnit.actualHealth.ToString();
        maxHealthText.text = selectedUnit.maxHealth.ToString();
        healthBar.fillAmount = (float)selectedUnit.actualHealth / selectedUnit.maxHealth;
        if (healthBar.fillAmount <= selectedUnit.criticalDamageFactor)
        {
            healthBar.color = Color.red;
        }
        else if (healthBar.fillAmount <= selectedUnit.averageDamageFactor)
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
        portrait.sprite = selectedBuilding.portrait;
        maxHealthText.text = selectedBuilding.maxHealth.ToString();
        actualHealthText.text = selectedBuilding.actualHealth.ToString();
        healthBar.fillAmount = (float)selectedBuilding.actualHealth / selectedBuilding.maxHealth;
        if ((float)selectedBuilding.actualHealth / selectedBuilding.maxHealth < selectedBuilding.criticalDamageFactor)
        {
            healthBar.color = Color.red;
        }
        else if ((float)selectedBuilding.actualHealth / selectedBuilding.maxHealth < selectedBuilding.averageDamageFactor)
        {
            healthBar.color = Color.yellow;
        }
        else
        {
            healthBar.color = Color.green;
        }
        if (MultiplayerController.Instance.localPlayer.playerType == selectedBuilding.owner)
        {
            if (selectedBuilding.actualBuildTime < selectedBuilding.buildTime)
            {
                SelectionInfoKeeper.Instance.SetCompletitionBar(selectedBuilding.actualBuildTime / selectedBuilding.buildTime);
            }
            if (selectedBuilding.isTraining)
            {
                SelectionInfoKeeper.Instance.SetTrainingBar(selectedBuilding.actualTrainingTime / selectedBuilding.trainedUnit.trainingTime);
            }
        }
    }

    public void UpdateMineSelectionView()
    {
        portrait.sprite = selectedMine.portrait;
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
