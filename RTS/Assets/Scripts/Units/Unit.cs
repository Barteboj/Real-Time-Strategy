using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    Peasant,
    Warrior,
    Archer
}

public class Unit : NetworkBehaviour
{
    protected bool isFollowingPath = false;
    protected MapGridElement nextNodeToFollow;
    protected IntVector2 positionInGrid
    {
        get
        {
            return positionInGridField;
        }
        set
        {
            positionInGridField = value;
            if (isServer)
            {
                positionInGridSyncVar = new Vector2(positionInGridField.x, positionInGridField.y);
            }
        }
    }
    protected IntVector2 positionInGridField;
    protected int indexOfFollowedPathNode;
    protected IntVector2 requestedTargetPositionInGrid;
    protected bool hasFinishedGoingToLastStep = false;

    protected Coroutine disableHitAnimationCoroutine;

    public string unitName;
    public int level = 1;
    public int armor;
    public int minDamage;
    public int maxDamage;
    public int range;
    public int sight;
    public int speed;
    public int maxHealth;
    [SyncVar(hook = "OnActualHealthChange")]
    private int actualHealth;

    [SyncVar(hook = "OnChangePositionInGridSyncVar")]
    public Vector2 positionInGridSyncVar;

    public void OnChangePositionInGridSyncVar(Vector2 newPosition)
    {
        if (positionInGrid != null)
        {
            ClearPositionInGrid();
        }
        positionInGrid = new IntVector2((int)newPosition.x, (int)newPosition.y);
        FillPositionInGrid();
    }

    void OnActualHealthChange(int newValue)
    {
        actualHealth = newValue;
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        }
    }

    public int ActualHealth
    {
        get
        {
            return actualHealth;
        }
        set
        {
            actualHealth = value;
            if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
            {
                SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
            }
        }
    }
    public int ownerPlayerID;
    public List<MapGridElement> followedPath;
    public SpriteRenderer spriteRenderer;
    public Sprite portrait;
    public GameObject selectionIndicator;
    public GameObject selectionCollider;
    public ActionButtonType[] buttonTypes;
    public float trainingTime;

    public int goldCost;
    public int lumberCost;
    public int foodCost;

    public UnitType unitType;
    public PlayerType owner;

    public Vector2 goalPosition;
    public bool isMoving = false;

    void Awake()
    {
        
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!isServer)
        {
            return;
        }
        actualHealth = maxHealth;
        InitializePositionInGrid();
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodAmount += foodCost;
    }

    public virtual void Update()
    {
        if (isMoving)
        {
            gameObject.transform.position += ((Vector3)((goalPosition) - (Vector2)gameObject.transform.position)).normalized * speed * Time.deltaTime;
            if (((Vector2)gameObject.transform.position - goalPosition).magnitude < 0.03f)
            {
                gameObject.transform.position = goalPosition;
                isMoving = false;
            }
        }
        if (!isServer)
        {
            return;
        }
        hasFinishedGoingToLastStep = false;
        if (isFollowingPath)
        {
            FollowPath();
        }
    }

    public virtual void GetHit(int damage, Warrior attacker)
    {
        ActualHealth -= damage;
        if (actualHealth <= 0)
        {
            Die();
            attacker.StopAttack();
        }
        else
        {
            ShowHitAnimation();
        }
    }

    public virtual void ShowHitAnimation()
    {
        spriteRenderer.color = Color.red;
        if (disableHitAnimationCoroutine != null)
        {
            StopCoroutine(disableHitAnimationCoroutine);
        }
        disableHitAnimationCoroutine = StartCoroutine(DisableHitAnimation());
    }

    public IEnumerator DisableHitAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = Color.white;
    }

    public virtual void Die()
    {
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            MultiplayerController.Instance.localPlayer.selectController.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        MultiplayerController.Instance.localPlayer.foodAmount -= foodCost;
        MultiplayerController.Instance.localPlayer.UpdateResourcesGUI();
        NetworkServer.Destroy(gameObject);
    }

    public virtual void RequestGoTo(IntVector2 targetPositionInGrid)
    {
        if (isFollowingPath)
        {
            requestedTargetPositionInGrid = targetPositionInGrid;
        }
        else
        {
            StartFollowingPath(ASTARPathfinder.Instance.FindPath(positionInGrid, targetPositionInGrid));
        }
    }

    public virtual void StartFollowingPath(List<MapGridElement> pathToFollow)
    {
        hasFinishedGoingToLastStep = true;
        if (pathToFollow != null && pathToFollow.Count > 0)
        {
            followedPath = pathToFollow;
            nextNodeToFollow = pathToFollow[0];
            indexOfFollowedPathNode = 0;
            if (MapGridded.Instance.mapGrid[nextNodeToFollow.y, nextNodeToFollow.x].isWalkable)
            {
                SetNewPositionOnMap(new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y));
                requestedTargetPositionInGrid = new IntVector2(pathToFollow[pathToFollow.Count - 1].x, pathToFollow[pathToFollow.Count - 1].y);
                isFollowingPath = true;
                RpcMoveFromTo(gameObject.transform.position, new Vector2(nextNodeToFollow.x, nextNodeToFollow.y));
            }
            else
            {
                isFollowingPath = false;
                RequestGoTo(new IntVector2(pathToFollow[pathToFollow.Count - 1].x, pathToFollow[pathToFollow.Count - 1].y));
            }
        }
    }

    [ClientRpc]
    void RpcMoveFromTo(Vector2 startingPosition, Vector2 goalPosition)
    {
        gameObject.transform.position = startingPosition;
        this.goalPosition = goalPosition;
        isMoving = true;
    }

    public virtual void FollowPath()
    {
        if (((Vector2)gameObject.transform.position - goalPosition).magnitude < 0.03f)
        {
            hasFinishedGoingToLastStep = true;
            if (requestedTargetPositionInGrid != null)
            {
                isFollowingPath = false;
                SetNewPositionOnMapSettingWorldPosition(MapGridded.WorldToMapPosition(gameObject.transform.position));
                StartFollowingPath(ASTARPathfinder.Instance.FindPath(positionInGrid, requestedTargetPositionInGrid));
                requestedTargetPositionInGrid = null;
                return;
            }
            if (nextNodeToFollow == followedPath[followedPath.Count - 1])
            {
                isFollowingPath = false;
            }
            else
            {
                ++indexOfFollowedPathNode;
                nextNodeToFollow = followedPath[indexOfFollowedPathNode];
                if (CheckIfCanGoTo(new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y)))
                {
                    ClearPositionInGrid();
                    positionInGrid = new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y);
                    FillPositionInGrid();
                    RpcMoveFromTo(gameObject.transform.position, new Vector2(nextNodeToFollow.x, nextNodeToFollow.y));
                }
                else
                {
                    isFollowingPath = false;
                    RequestGoTo(new IntVector2(followedPath[followedPath.Count - 1].x, followedPath[followedPath.Count - 1].y));
                }
            }
        }
    }

    public void StopFollowingPath()
    {
        RequestGoTo(positionInGrid);
    }

    public void SetNewPositionOnMapSettingWorldPosition(IntVector2 newPosition)
    {
        ClearPositionInGrid();
        positionInGrid = newPosition;
        gameObject.transform.position = MapGridded.MapToWorldPosition(newPosition);
        FillPositionInGrid();
    }

    public void SetNewPositionOnMap(IntVector2 newPosition)
    {
        ClearPositionInGrid();
        positionInGrid = newPosition;
        FillPositionInGrid();
    }

    public void ClearPositionInGrid()
    {
        RpcClearPositionInGrid();
    }

    [ClientRpc]
    void RpcClearPositionInGrid()
    {
        MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x].unit = null;
    }

    public void FillPositionInGrid()
    {
        MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x].unit = this;
    }

    public void InitializePositionInGrid()
    {
        if (positionInGrid != null)
        {
            ClearPositionInGrid();
        }
        positionInGrid = MapGridded.WorldToMapPosition(gameObject.transform.position);
        FillPositionInGrid();
    }

    public void Select()
    {
        Debug.LogError("In unit select");
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.Assign(this);
        SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        SelectionInfoKeeper.Instance.Show();
        ActionButtons.Instance.HideAllButtons();
        if (MultiplayerController.Instance.localPlayer.playerType == owner)
        {
            foreach (ActionButtonType buttonType in buttonTypes)
            {
                ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
            }
            selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
        else
        {
            selectionIndicator.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }

    public void Unselect()
    {
        selectionIndicator.SetActive(false);
        SelectionInfoKeeper.Instance.Hide();
    }

    public bool CheckIfCanGoTo(IntVector2 targetPosition)
    {
        if (Mathf.Abs(positionInGrid.x - targetPosition.x) > 1 || Mathf.Abs(positionInGrid.y - targetPosition.y) > 1)
        {
            return false;
        }
        else
        {
            if (targetPosition.x != positionInGrid.x && targetPosition.y != positionInGrid.y)
            {
                if (targetPosition.x > positionInGrid.x && targetPosition.y > positionInGrid.y)
                {
                    return MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x + 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y + 1, positionInGrid.x + 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y + 1, positionInGrid.x].isWalkable;
                }
                else if (targetPosition.x > positionInGrid.x && targetPosition.y < positionInGrid.y)
                {
                    return MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x + 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y - 1, positionInGrid.x + 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y - 1, positionInGrid.x].isWalkable;
                }
                else if (targetPosition.x < positionInGrid.x && targetPosition.y < positionInGrid.y)
                {
                    return MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x - 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y - 1, positionInGrid.x - 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y - 1, positionInGrid.x].isWalkable;
                }
                else
                {
                    return MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x - 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y + 1, positionInGrid.x - 1].isWalkable && MapGridded.Instance.mapGrid[positionInGrid.y + 1, positionInGrid.x].isWalkable;
                }
            }
            else
            {
                return MapGridded.Instance.mapGrid[targetPosition.y, targetPosition.x].isWalkable;
            }
        }
    }
}
