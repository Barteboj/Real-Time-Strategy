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
        actualHealthText.text = selectedBuilding.actualHealth.ToString();
        maxHealthText.text = selectedBuilding.maxHealth.ToString();
        healthBar.fillAmount = (float)selectedBuilding.actualHealth / selectedBuilding.maxHealth;
        if (healthBar.fillAmount <= selectedBuilding.criticalDamageFactor)
        {
            healthBar.color = Color.red;
        }
        else if (healthBar.fillAmount <= selectedBuilding.averageDamageFactor)
        {
            healthBar.color = Color.yellow;
        }
        else
        {
            healthBar.color = Color.green;
        }
    }

    public void ShowSelection()
    {
        gameObject.SetActive(true);
    }

    public void HideSelection()
    {
        gameObject.SetActive(false);
    }

    public void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        UpdateUnitSelectionView();
        ShowSelection();
    }

    public void Unselect()
    {
        selectedUnit = null;
        selectedBuilding = null;
        HideSelection();
    }
}
