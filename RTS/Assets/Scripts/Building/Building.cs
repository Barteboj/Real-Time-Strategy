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
    public float buildTime;

    public GameObject selectionIndicator;
    public BoxCollider2D selectionIndicatorCollider;

    public Sprite portrait;

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
    public float actualBuildTime = 0f;

    public List<Unit> visiters = new List<Unit>();

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

    private void Awake()
    {
        gameObject.GetComponentInChildren<MinimapElement>().Hide();
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
    }

    public void UpdateBuildingProcess()
    {
        actualBuildTime += Time.deltaTime;
        actualHealth =  Mathf.RoundToInt(actualBuildTime / buildTime * maxHealth);
        if (actualHealth > maxHealth)
        {
            actualHealth = maxHealth;
        }
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
        if (MultiplayerController.Instance.localPlayer.selector.selectedBuilding == this && MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
            MultiplayerController.Instance.localPlayer.actionButtonsController.ShowButtons(this);
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
            if (MultiplayerController.Instance.localPlayer.selector.selectedBuilding == this)
            {
                SelectionInfoKeeper.Instance.HideTrainingInfo();
                MultiplayerController.Instance.localPlayer.actionButtonsController.ShowButtons(this);
            }
            isTraining = false;
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
        gameObject.GetComponentInChildren<MinimapElement>().image.color = MultiplayerController.Instance.playerColors[(int)owner];
        gameObject.GetComponentInChildren<MinimapElement>().Show();
        buildingViewGameObject.SetActive(false);
        buildField.SetActive(true);
        isInBuildingProcess = true;
        selectionIndicatorCollider.enabled = true;
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).activeBuildings.Add(this);
        ++MultiplayerController.Instance.players.Find(item => item.playerType == owner).allBuildingsAmount;
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

    public virtual void GetHit(int damage, Warrior attacker)
    {
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            ++MultiplayerController.Instance.players.Find(item => item.playerType == attacker.owner).razings;
            attacker.StopAttack();
            DestroyYourself();
        }
    }

    private void OnDestroy()
    {
        if (MultiplayerController.Instance != null)
        {
            if (actualBuildTime > 0f)
            {
                if (MultiplayerController.Instance.localPlayer.selector.selectedBuilding == this)
                {
                    MultiplayerController.Instance.localPlayer.selector.Unselect(this);
                }
                MultiplayerController.Instance.players.Find(item => item.playerType == owner).activeBuildings.Remove(this);
            }
        }
    }

    public virtual void DestroyYourself()
    {
        foreach (Unit visiter in visiters)
        {
            visiter.Die();
        }
        NetworkServer.Destroy(gameObject);
    }
}
