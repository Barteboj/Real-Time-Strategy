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
    [SerializeField]
    private string buildingName;
    public string BuildingName
    {
        get
        {
            return buildingName;
        }
    }
    [SerializeField]
    private int maxHealth;
    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    [SerializeField]
    [SyncVar(hook = "OnChangeActualHealth")]
    private int actualHealth;
    public int ActualHealth
    {
        get
        {
            return actualHealth;
        }
    }
    [SerializeField]
    private int width;
    public int Width
    {
        get
        {
            return width;
        }
    }
    [SerializeField]
    private int height;
    public int Height
    {
        get
        {
            return height;
        }
    }
    private IntVector2 placeOnMapGrid;
    private List<MapGridElement> lastUsedCanBuildGridElements = new List<MapGridElement>();
    [SerializeField]
    private GameObject buildingViewGameObject;
    [SerializeField]
    private GameObject buildField;
    private bool isInBuildingProcess = true;
    private bool isBuilded = false;
    public bool IsBuilded
    {
        get
        {
            return isBuilded;
        }
    }
    [SerializeField]
    private float buildTime;
    public float BuildTime
    {
        get
        {
            return buildTime;
        }
    }
    [SerializeField]
    private GameObject selectionIndicator;
    public GameObject SelectionIndicator
    {
        get
        {
            return selectionIndicator;
        }
    }
    [SerializeField]
    private BoxCollider2D selectionIndicatorCollider;
    [SerializeField]
    private Sprite portrait;
    public Sprite Portrait
    {
        get
        {
            return portrait;
        }
    }
    [SerializeField]
    private ActionButtonType[] buttonTypes;
    public ActionButtonType[] ButtonTypes
    {
        get
        {
            return buttonTypes;
        }
    }
    public Worker Builder { get; set; }
    public Unit TrainedUnit { get; set; }
    [SyncVar]
    private float actualTrainingTime;
    public float ActualTrainingTime
    {
        get
        {
            return actualTrainingTime;
        }
    }
    public bool IsTraining { get; set; }
    [SerializeField]
    private int goldCost = 0;
    public int GoldCost
    {
        get
        {
            return goldCost;
        }
    }
    [SerializeField]
    private int lumberCost = 0;
    public int LumberCost
    {
        get
        {
            return lumberCost;
        }
    }
    [SerializeField]
    private BuildingType buildingType;
    public BuildingType BuildingType
    {
        get
        {
            return buildingType;
        }
    }
    [SerializeField]
    protected PlayerType owner;
    public PlayerType Owner
    {
        get
        {
            return owner;
        }
    }
    [SerializeField]
    private GameObject smallFlameObject;
    [SerializeField]
    private GameObject bigFlameObject;
    [SerializeField]
    private float averageDamageFactor = 0.6f;
    public float AverageDamageFactor
    {
        get
        {
            return averageDamageFactor;
        }
    }
    [SerializeField]
    private float criticalDamageFactor = 0.3f;
    public float CriticalDamageFactor
    {
        get
        {
            return criticalDamageFactor;
        }
    }
    [SyncVar]
    private float actualBuildTime = 0f;
    public float ActualBuildTime
    {
        get
        {
            return actualBuildTime;
        }
    }
    private List<Unit> visiters = new List<Unit>();
    public List<Unit> Visiters
    {
        get
        {
            return visiters;
        }
    }

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
            if (IsTraining)
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
        Builder.FinishBuild();
    }

    [ClientRpc]
    void RpcFinishBuild()
    {
        buildingViewGameObject.SetActive(true);
        buildField.SetActive(false);
        isInBuildingProcess = false;
        isBuilded = true;
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedBuilding == this && MultiplayerController.Instance.LocalPlayer.PlayerType == owner)
        {
            SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
            MultiplayerController.Instance.LocalPlayer.ActionButtonsController.ShowButtons(this);
        }
    }

    public void UpdateTrainingProcess()
    {
        actualTrainingTime += Time.deltaTime;
        if (actualTrainingTime >= TrainedUnit.TrainingTime)
        {
            FinishTraining();
        }
    }

    public void FinishTraining()
    {
        GameObject instantiatedUnit = Instantiate(TrainedUnit.gameObject, MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(gameObject.transform.position), width, height)), Quaternion.identity);
        instantiatedUnit.GetComponent<Unit>().InitializePositionInGrid();
        NetworkServer.Spawn(instantiatedUnit);
        IsTraining = false;
        RpcFinishTraining();
        actualTrainingTime = 0;
    }

    [ClientRpc]
    void RpcFinishTraining()
    {
        if (MultiplayerController.Instance.LocalPlayer.PlayerType == owner)
        {
            if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedBuilding == this)
            {
                SelectionInfoKeeper.Instance.HideTrainingInfo();
                MultiplayerController.Instance.LocalPlayer.ActionButtonsController.ShowButtons(this);
            }
            IsTraining = false;
        }
    }

    public void Train(Unit unitToTrain)
    {
        TrainedUnit = unitToTrain;
        actualTrainingTime = 0f;
        IsTraining = true;
        RpcTrain(TrainedUnit.UnitType);
    }

    [ClientRpc]
    void RpcTrain(UnitType unitType)
    {
        if (owner == MultiplayerController.Instance.LocalPlayer.PlayerType)
        {
            IsTraining = true;
            TrainedUnit = Units.Instance.UnitsList.Find(item => item.UnitType == unitType && item.Owner == owner);
            List<ActionButton> trainingButtons = ActionButtons.Instance.buttons.FindAll(button => button.GetType() == typeof(TrainingButton));
            foreach (ActionButton button in trainingButtons)
            {
                button.Hide();
            }
            SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / TrainedUnit.TrainingTime);
            SelectionInfoKeeper.Instance.TrainedUnitPortrait.sprite = TrainedUnit.Portrait;
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
        gameObject.GetComponentInChildren<MinimapElement>().Image.color = MultiplayerController.Instance.PlayerColors[(int)owner];
        gameObject.GetComponentInChildren<MinimapElement>().Show();
        buildingViewGameObject.SetActive(false);
        buildField.SetActive(true);
        isInBuildingProcess = true;
        selectionIndicatorCollider.enabled = true;
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveBuildings.Add(this);
        ++MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).AllBuildingsAmount;
    }

    public bool CouldBeBuildInPlace(IntVector2 placeInGrid, Unit builder)
    {
        for (int row = 0; row < height; ++row)
        {
            for (int column = 0; column < width; ++column)
            {
                if (!MapGridded.Instance.IsInMap(new IntVector2(placeInGrid.X + column, placeInGrid.Y + row)) || (!MapGridded.Instance.MapGrid[placeInGrid.Y + row, placeInGrid.X + column].IsWalkable && MapGridded.Instance.MapGrid[placeInGrid.Y + row, placeInGrid.X + column].Unit != builder))
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
                if (MapGridded.Instance.IsInMap(new IntVector2(placeOnMapGrid.X + column, placeOnMapGrid.Y + row)))
                {
                    if (!MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].IsWalkable && (MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].Unit != Builder))
                    {
                        MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].ShowCannotBuildIndicator();
                        MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].HideCanBuildIndicator();
                    }
                    else
                    {
                        MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].ShowCanBuildIndicator();
                        MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].HideCannotBuildIndicator();
                    }
                    lastUsedCanBuildGridElements.Add(MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column]);
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
                if (MapGridded.Instance.IsInMap(new IntVector2(placeOnMapGrid.X + column, placeOnMapGrid.Y + row)) || MapGridded.Instance.MapGrid[row, column].IsWalkable)
                {
                    MapGridded.Instance.MapGrid[placeOnMapGrid.Y + row, placeOnMapGrid.X + column].Building = this;
                }
            }
        }
    }

    public virtual void GetHit(int damage, Warrior attacker)
    {
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            ++MultiplayerController.Instance.Players.Find(item => item.PlayerType == attacker.Owner).Razings;
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
                if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedBuilding == this)
                {
                    MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
                }
                MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveBuildings.Remove(this);
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
