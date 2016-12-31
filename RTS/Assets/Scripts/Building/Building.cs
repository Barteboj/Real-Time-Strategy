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

    public float actualTrainingTime;

    public bool isTraining = false;

    public int goldCost = 0;
    public int lumberCost = 0;
    
    public BuildingType buildingType;
    public PlayerType owner;

    private float actualbuildTime = 0f;

    void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (isInBuildingProcess)
        {
            actualbuildTime += Time.deltaTime;
            if (actualbuildTime >= buildTime)
            {
                FinishBuild();
            }
            else
            {
                actualHealth = Mathf.RoundToInt(actualbuildTime / buildTime * maxHealth);
                if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
                {
                    SelectionInfoKeeper.Instance.actualHealth.text = actualHealth.ToString();
                    SelectionInfoKeeper.Instance.SetCompletitionBar(actualbuildTime / buildTime);
                    SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
                }
            }
        }
        if (isTraining)
        {
            actualTrainingTime += Time.deltaTime;
            if (actualTrainingTime >= trainedUnit.trainingTime)
            {
                FinishTraining();
            }
            else
            {
                if (MultiplayerController.Instance.localPlayer.selectController.selectedBuilding == this)
                {
                    SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / trainedUnit.trainingTime);
                }
            }
        }
    }

    public void Train(Unit unitToTrain)
    {
        trainedUnit = unitToTrain;
        actualTrainingTime = 0f;
        isTraining = true;

        List<ActionButton> trainingButtons = ActionButtons.Instance.buttons.FindAll(button => button.GetType() == typeof(TrainingButton));
        foreach (ActionButton button in trainingButtons)
        {
            button.Hide();
        }
        SelectionInfoKeeper.Instance.SetTrainingBar(actualTrainingTime / trainedUnit.trainingTime);
        SelectionInfoKeeper.Instance.trainedUnitPortrait.sprite = trainedUnit.portrait;
        SelectionInfoKeeper.Instance.ShowTrainingInfo();
    }

    public void FinishTraining()
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
        GameObject instantiatedUnit = (GameObject)Instantiate(trainedUnit.gameObject, MapGridded.MapToWorldPosition(MapGridded.Instance.GetFirstFreePlaceAround(MapGridded.WorldToMapPosition(gameObject.transform.position), width, height)), Quaternion.identity);
        instantiatedUnit.GetComponent<Unit>().InitializePositionInGrid();
        NetworkServer.Spawn(instantiatedUnit);
        isTraining = false;
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

    public void StartBuildProcess()
    {
        /*buildingViewGameObject.SetActive(false);
        buildField.SetActive(true);
        isInBuildingProcess = true;
        actualHealth = 0;
        selectionIndicatorCollider.enabled = true;*/
        RpcStartBuildProcess();
    }

    [ClientRpc]
    void RpcStartBuildProcess()
    {
        buildingViewGameObject.SetActive(false);
        buildField.SetActive(true);
        isInBuildingProcess = true;
        actualHealth = 0;
        selectionIndicatorCollider.enabled = true;
    }

    public void Select()
    {
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.unitName.text = buildingName;
        SelectionInfoKeeper.Instance.unitLevel.text = level.ToString();
        if (actualbuildTime < buildTime)
        {
            SelectionInfoKeeper.Instance.SetCompletitionBar(actualbuildTime / buildTime);
            SelectionInfoKeeper.Instance.ShowBuildCompletitionBar();
            ActionButtons.Instance.HideAllButtons();
        }
        else
        {
            ActionButtons.Instance.HideAllButtons();
            if (buttonTypes != null)
            {
                foreach (ActionButtonType buttonType in buttonTypes)
                {
                    ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
                }
            }
        }
        if (isTraining)
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
        MultiplayerController.Instance.localPlayer.selectController.selectedUnit = null;
        MultiplayerController.Instance.localPlayer.selectController.selectedBuilding = this;
        SelectionInfoKeeper.Instance.Show();
    }

    public void Unselect()
    {
        selectionIndicator.SetActive(false);
        SelectionInfoKeeper.Instance.HideBuildCompletitionBar();
        SelectionInfoKeeper.Instance.HideTrainingInfo();
        SelectionInfoKeeper.Instance.Hide();
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

    public virtual void DestroyYourself()
    {

    }
}
