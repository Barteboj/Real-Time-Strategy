using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum BuildingType
{
    Castle,
    Barracks,
    Farm,
    ShootingRange
}

public class Building : NetworkBehaviour
{
    public string buildingName;
    public int maxHealth;
    [SyncVar(hook = "OnChangeActualHealth")]
    public int actualHealth;

    public int width;
    public int height;

    public IntVector2 placeOnMapGrid;

    public List<MapGridElement> lastUsedCanBuildGridElements = new List<MapGridElement>();

    public GameObject buildingViewGameObject;
    public GameObject buildField;

    public bool isInBuildingProcess = true;
    public bool isBuilded = false;
    public float buildCompletition = 0f;
    public float buildTime;

    public GameObject selectionIndicator;
    public BoxCollider2D selectionIndicatorCollider;

    public Sprite portrait;

    public int level = 1;

    public ActionButtonType[] buttonTypes;

    public Worker builder;

    public Unit trainedUnit;

    [SyncVar]
    public float actualTrainingTime;

    public bool isTraining = false;

    public int goldCost = 0;
    public int lumberCost = 0;
    
    public BuildingType buildingType;
    public PlayerType owner;

    public GameObject smallFlameObject;
    public GameObject bigFlameObject;

    public float averageDamageFactor = 0.6f;
    public float criticalDamageFactor = 0.3f;

    [SyncVar]
    private float actualBuildTime = 0f;

    public void OnChangeActualHealth(int newValue)
    {
        actualHealth = newValue;
        if (isBuilded)
        {
            if ((float)actualHealth / maxHealth < criticalDamageFactor)
            {
                smallFlameObject.SetActive(false);
                bigFlameObject.SetActive(true);
            }
            else if ((float)actualHealth / maxHealth < averageDamageFactor)
            {
                smallFlameObject.SetActive(true);
                bigFlameObject.SetActive(false);
            }
            else
            {
                smallFlameObject.SetActive(false);
                bigFlameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isServer)
        {
            if (isInBuildingProcess)
            {
                UpdateBuildingProcess();
            }
            if (isTraining)
            {
                UpdateTrainingProcess();
            }
        }
        if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
        {
            ShowActualInfo();
        }
    }

    public void UpdateBuildingProcess()
    {
        actualBuildTime += Time.deltaTime;
        actualHealth = Mathf.RoundToInt(actualBuildTime / buildTime * maxHealth);
        if (actualBuildTime >= buildTime)
        {
            FinishBuild();
        }
    }

    public virtual void FinishBuild()
    {
        RpcFinishBuild();
        builder.FinishBuild();
    }

    [ClientRpc]
    void RpcFinishBuild()
    {
        buildingViewGameObject.SetActive(true);
        buildField.SetActive(false);
        isInBuildingProcess = false;
        isBuilded = true;
        actualHealth = maxHealth;
        if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
        {
            SelectionInfoKeeper.Instance.actualHealth.text = actualHealth.ToString();
            SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
            if (buttonTypes != null)
            {
                foreach (ActionButtonType buttonType in buttonTypes)
                {
                    ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
                }
            }
        }
        if (buildingType == BuildingType.Castle)
        {
            if (MultiplayerController.Instance.localPlayer.playerType == owner)
            {
                MultiplayerController.Instance.localPlayer.buildings.Add(this);
            }
        }
    }

    public void UpdateTrainingProcess()
    {
        actualTrainingTime += Time.deltaTime;
        if (actualTrainingTime >= trainedUnit.trainingTime)
        {
            FinishTraining();
        }
    }

    public void FinishTraining()
    {
        GameObject instantiatedUnit = Instantiate(trainedUnit.gameObject, MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(gameObject.transform.position), width, height)), Quaternion.identity);
        instantiatedUnit.GetComponent<Unit>().InitializePositionInGrid();
        NetworkServer.Spawn(instantiatedUnit);
        isTraining = false;
        RpcFinishTraining();
        actualTrainingTime = 0;
    }

    [ClientRpc]
    void RpcFinishTraining()
    {
        if (MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
            {
                SelectionInfoKeeper.Instance.HideTrainingInfo();

                foreach (ActionButtonType buttonType in buttonTypes)
                {
                    ActionButton button = ActionButtons.Instance.buttons.Find(item => item.buttonType == buttonType);
                    if (button.GetType() == typeof(TrainingButton))
                    {
                        button.Show();
                    }
                }
            }
            isTraining = false;
        }
    }

    public void ShowActualInfo()
    {
        SelectionInfoKeeper.Instance.actualHealth.text = actualHealth.ToString();
        SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        if ((float)actualHealth / maxHealth < criticalDamageFactor)
        {
            SelectionInfoKeeper.Instance.healthBar.color = Color.red;
        }
        else if ((float)actualHealth / maxHealth < averageDamageFactor)
        {
            SelectionInfoKeeper.Instance.healthBar.color = Color.yellow;
        }
        else
        {
            SelectionInfoKeeper.Instance.healthBar.color = Color.green;
        }
        if (MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            if (actualBuildTime < buildTime)
            {
                SelectionInfoKeeper.Instance.SetCompletitionBar(actualBuildTime / buildTime);
            }
            if (isTraining)
            {
                SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / trainedUnit.trainingTime);
            }
        }
    }

    public void Train(Unit unitToTrain)
    {
        trainedUnit = unitToTrain;
        actualTrainingTime = 0f;
        isTraining = true;
        RpcTrain(trainedUnit.unitType);
    }

    [ClientRpc]
    void RpcTrain(UnitType unitType)
    {
        if (owner == MultiplayerController.Instance.localPlayer.playerType)
        {
            isTraining = true;
            trainedUnit = Units.Instance.unitsList.Find(item => item.unitType == unitType && item.owner == owner);
            List<ActionButton> trainingButtons = ActionButtons.Instance.buttons.FindAll(button => button.GetType() == typeof(TrainingButton));
            foreach (ActionButton button in trainingButtons)
            {
                button.Hide();
            }
            SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / trainedUnit.trainingTime);
            SelectionInfoKeeper.Instance.trainedUnitPortrait.sprite = trainedUnit.portrait;
            SelectionInfoKeeper.Instance.ShowTrainingInfo();
        }
    }

    public void StartBuildProcess()
    {
        actualHealth = 0;
        RpcStartBuildProcess();
    }

    [ClientRpc]
    void RpcStartBuildProcess()
    {
        buildingViewGameObject.SetActive(false);
        buildField.SetActive(true);
        isInBuildingProcess = true;
        selectionIndicatorCollider.enabled = true;
    }

    public void Select()
    {
        if (MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
        else
        {
            selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.unitName.text = buildingName;
        SelectionInfoKeeper.Instance.unitLevel.text = level.ToString();
        if (actualBuildTime < buildTime)
        {
            SelectionInfoKeeper.Instance.SetCompletitionBar(actualBuildTime / buildTime);
            SelectionInfoKeeper.Instance.ShowBuildCompletitionBar();
            ActionButtons.Instance.HideAllButtons();
        }
        else
        {
            ActionButtons.Instance.HideAllButtons();
            if (MultiplayerController.Instance.localPlayer.playerType == owner)
            {
                foreach (ActionButtonType buttonType in buttonTypes)
                {
                    ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
                }
            }
        }
        if (isTraining && MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            List<ActionButton> trainingButtons = ActionButtons.Instance.buttons.FindAll(button => button.GetType() == typeof(TrainingButton));
            foreach (ActionButton button in trainingButtons)
            {
                button.Hide();
            }
            SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / trainedUnit.trainingTime);
            SelectionInfoKeeper.Instance.trainedUnitPortrait.sprite = trainedUnit.portrait;
            SelectionInfoKeeper.Instance.ShowTrainingInfo();
        }
        SelectionInfoKeeper.Instance.maxHealth.text = maxHealth.ToString();
        SelectionInfoKeeper.Instance.actualHealth.text = actualHealth.ToString();
        SelectionInfoKeeper.Instance.unitPortrait.sprite = portrait;
        SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        SelectionInfoKeeper.Instance.Show();
    }

    public void Unselect()
    {
        selectionIndicator.SetActive(false);
        SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
        SelectionInfoKeeper.Instance.HideTrainingInfo();
        SelectionInfoKeeper.Instance.Hide();
    }

    [ClientRpc]
    void RpcUnselect()
    {
        if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
        {
            Unselect();
        }
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid, Unit builder)
    {
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (!MapGridded.Instance.IsInMap(new IntVector2(placeInGrid.x + column, placeInGrid.y + row)) || (!MapGridded.Instance.mapGrid[placeInGrid.y + row, placeInGrid.x + column].isWalkable && MapGridded.Instance.mapGrid[placeInGrid.y + row, placeInGrid.x + column].unit != builder))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ShowBuildGrid()
    {
        HideBuildGrid();
        lastUsedCanBuildGridElements = new List<MapGridElement>();
        placeOnMapGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (MapGridded.Instance.IsInMap(new IntVector2(placeOnMapGrid.x + column, placeOnMapGrid.y + row)))
                {
                    if (!MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].isWalkable && (MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].unit != builder))
                    {
                        MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].ShowCannotBuildIndicator();
                        MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].HideCanBuildIndicator();
                    }
                    else
                    {
                        MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].ShowCanBuildIndicator();
                        MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].HideCannotBuildIndicator();
                    }
                    lastUsedCanBuildGridElements.Add(MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column]);
                }
            }
        }
    }

    public void HideBuildGrid()
    {
        foreach (MapGridElement lastUsedCanBuildGridElement in lastUsedCanBuildGridElements)
        {
            lastUsedCanBuildGridElement.HideCanBuildIndicator();
            lastUsedCanBuildGridElement.HideCannotBuildIndicator();
        }
    }

    public void PlaceOnMap()
    {
        RpcPlaceOnMap(gameObject.transform.position);
    }

    [ClientRpc]
    void RpcPlaceOnMap(Vector2 buildingPositioninWorld)
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(buildingPositioninWorld);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (MapGridded.Instance.IsInMap(new IntVector2(placeOnMapGrid.x + column, placeOnMapGrid.y + row)) || MapGridded.Instance.mapGrid[row, column].isWalkable)
                {
                    MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].building = this;
                }
            }
        }
    }

    public void VanishFromMap()
    {
        RpcVanishFromMap(gameObject.transform.position);
    }

    [ClientRpc]
    void RpcVanishFromMap(Vector2 buildingPositioninWorld)
    {
        placeOnMapGrid = MapGridded.WorldToMapPosition(buildingPositioninWorld);
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (MapGridded.Instance.IsInMap(new IntVector2(placeOnMapGrid.x + column, placeOnMapGrid.y + row)) || MapGridded.Instance.mapGrid[row, column].isWalkable)
                {
                    MapGridded.Instance.mapGrid[placeOnMapGrid.y + row, placeOnMapGrid.x + column].building = null;
                }
            }
        }
    }

    public bool CheckIfIsInBuildingArea(IntVector2 positionToCheck)
    {
        IntVector2 buildingPlaceOnMap = MapGridded.WorldToMapPosition(gameObject.transform.position);
        return positionToCheck.x >= buildingPlaceOnMap.x && positionToCheck.x <= buildingPlaceOnMap.x + width - 1 && positionToCheck.y >= buildingPlaceOnMap.y && positionToCheck.y <= buildingPlaceOnMap.y + height - 1;
    }

    public virtual void GetHit(int damage, Warrior attacker)
    {
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            attacker.StopAttack();
            DestroyYourself();
        }
    }

    public virtual void DestroyYourself()
    {
        RpcUnselect();
        NetworkServer.Destroy(gameObject);
    }
}
