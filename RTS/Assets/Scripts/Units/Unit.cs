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
    protected IntVector2 positionInGrid;
    public IntVector2 PositionInGrid
    {
        get
        {
            return positionInGrid;
        }
    }
    protected int indexOfFollowedPathNode;
    protected IntVector2 requestedTargetPositionInGrid;
    protected bool hasFinishedGoingToLastStep = false;
    protected Coroutine disableHitAnimationCoroutine;
    [SerializeField]
    private string unitName;
    public string UnitName
    {
        get
        {
            return unitName;
        }
    }
    [SerializeField]
    private int armor;
    [SerializeField]
    private int minDamage;
    [SerializeField]
    private int maxDamage;
    [SerializeField]
    protected int range;
    [SerializeField]
    private int speed;
    [SerializeField]
    private int maxHealth;
    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }
    }
    [SyncVar(hook = "OnActualHealthChange")]
    protected int actualHealth;
    public int ActualHealth
    {
        get
        {
            return actualHealth;
        }
    }
    [SyncVar(hook = "OnChangePositionInGridSyncVar")]
    protected Vector2 positionInGridSyncVar;
    private List<MapGridElement> followedPath;
    [SerializeField]
    protected SpriteRenderer spriteRenderer;
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
    private GameObject selectionIndicator;
    public GameObject SelectionIndicator
    {
        get
        {
            return selectionIndicator;
        }
    }
    [SerializeField]
    protected GameObject selectionCollider;
    [SerializeField]
    protected ActionButtonType[] buttonTypes;
    public ActionButtonType[] ButtonTypes
    {
        get
        {
            return buttonTypes;
        }
    }
    [SerializeField]
    private float trainingTime;
    public float TrainingTime
    {
        get
        {
            return trainingTime;
        }
    }
    [SerializeField]
    private int goldCost;
    public int GoldCost
    {
        get
        {
            return goldCost;
        }
    }
    [SerializeField]
    private int lumberCost;
    public int LumberCost
    {
        get
        {
            return lumberCost;
        }
    }
    [SerializeField]
    private int foodCost;
    public int FoodCost
    {
        get
        {
            return foodCost;
        }
    }
    [SerializeField]
    private UnitType unitType;
    public UnitType UnitType
    {
        get
        {
            return unitType;
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
    private Vector2 goalPosition;
    protected bool isMoving = false;
    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
    }
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

    private void Awake()
    {
        gameObject.GetComponentInChildren<MinimapElement>().Image.color = MultiplayerController.Instance.PlayerColors[(int)owner];
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveUnits.Add(this);
        ++MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).AllUnitsAmount;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (isServer)
        {
            actualHealth = maxHealth;
            InitializePositionInGrid();
            MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).FoodAmount += foodCost;
        }
    }

    protected virtual void Update()
    {
        if (isMoving)
        {
            Vector2 previousPosition = gameObject.transform.position;
            gameObject.transform.position += ((Vector3)((goalPosition) - (Vector2)gameObject.transform.position)).normalized * speed * Time.deltaTime;
            if (((Vector2)gameObject.transform.position - goalPosition).magnitude < 0.03f || (((goalPosition.x < gameObject.transform.position.x && goalPosition.x > previousPosition.x) || (goalPosition.x > gameObject.transform.position.x && goalPosition.x < previousPosition.x)) || ((goalPosition.y < gameObject.transform.position.y && goalPosition.y > previousPosition.y) || (goalPosition.y > gameObject.transform.position.y && goalPosition.y < previousPosition.y))))
            {
                gameObject.transform.position = goalPosition;
                isMoving = false;
            }
        }
        if (isServer)
        {
            hasFinishedGoingToLastStep = false;
            if (isFollowingPath)
            {
                FollowPath();
            }
        }
    }

    void OnChangePositionInGridSyncVar(Vector2 newPosition)
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
    }

    public virtual void GetHit(int damage, Warrior attacker)
    {
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            ++MultiplayerController.Instance.Players.Find(item => item.PlayerType == attacker.owner).Kills;
            attacker.StopAttack();
            Die();
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
        if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
        {
            MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
        }
        MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).FoodAmount -= foodCost;
        NetworkServer.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (MultiplayerController.Instance != null && SelectionInfoKeeper.Instance != null)
        {
            if (MultiplayerController.Instance.LocalPlayer.Selector.SelectedUnits.Contains(this))
            {
                MultiplayerController.Instance.LocalPlayer.Selector.Unselect(this);
            }
            MultiplayerController.Instance.Players.Find(item => item.PlayerType == owner).ActiveUnits.Remove(this);
        }
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

    public virtual void RequestGoTo(List<MapGridElement> targetPath)
    {
        if (isFollowingPath)
        {
            if (targetPath.Count > 0)
            {
                requestedTargetPositionInGrid = new IntVector2(targetPath[targetPath.Count - 1].X, targetPath[targetPath.Count - 1].Y);
            }
            else
            {
                requestedTargetPositionInGrid = positionInGrid;
            }
        }
        else
        {
            StartFollowingPath(targetPath);
        }
    }

    public virtual void StartFollowingPath(List<MapGridElement> pathToFollow)
    {
        hasFinishedGoingToLastStep = true;
        if (pathToFollow != null)
        {
            followedPath = pathToFollow;
            if (pathToFollow.Count > 0)
            {
                nextNodeToFollow = pathToFollow[0];
            }
            else
            {
                nextNodeToFollow = MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X];
            }
            indexOfFollowedPathNode = 0;
            if (MapGridded.Instance.MapGrid[nextNodeToFollow.Y, nextNodeToFollow.X].ChecklIfIsWalkableForUnit(this))
            {
                SetNewPositionOnMap(new IntVector2(nextNodeToFollow.X, nextNodeToFollow.Y));
                if (pathToFollow.Count > 0)
                {
                    requestedTargetPositionInGrid = new IntVector2(pathToFollow[pathToFollow.Count - 1].X, pathToFollow[pathToFollow.Count - 1].Y);
                }
                else
                {
                    requestedTargetPositionInGrid = positionInGrid;
                }
                isFollowingPath = true;
                RpcMoveFromTo(gameObject.transform.position, new Vector2(nextNodeToFollow.X, nextNodeToFollow.Y));
            }
            else
            {
                isFollowingPath = false;
                RequestGoTo(new IntVector2(pathToFollow[pathToFollow.Count - 1].X, pathToFollow[pathToFollow.Count - 1].Y));
            }
        }
    }

    [ClientRpc]
    public void RpcMoveFromTo(Vector2 startingPosition, Vector2 goalPosition)
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
            if (followedPath.Count == 0 || nextNodeToFollow == followedPath[followedPath.Count - 1])
            {
                isFollowingPath = false;
            }
            else
            {
                ++indexOfFollowedPathNode;
                nextNodeToFollow = followedPath[indexOfFollowedPathNode];
                if (CheckIfCanGoTo(new IntVector2(nextNodeToFollow.X, nextNodeToFollow.Y)))
                {
                    positionInGridSyncVar = new Vector2(nextNodeToFollow.X, nextNodeToFollow.Y);
                    RpcMoveFromTo(gameObject.transform.position, new Vector2(nextNodeToFollow.X, nextNodeToFollow.Y));
                }
                else
                {
                    isFollowingPath = false;
                    RequestGoTo(new IntVector2(followedPath[followedPath.Count - 1].X, followedPath[followedPath.Count - 1].Y));
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
        positionInGridSyncVar = new Vector2(newPosition.X, newPosition.Y);
        if (positionInGrid != null)
        {
            ClearPositionInGrid();
        }
        positionInGrid = new IntVector2((int)newPosition.X, (int)newPosition.Y);
        FillPositionInGrid();
        gameObject.transform.position = MapGridded.MapToWorldPosition(newPosition);
    }

    public void SetNewPositionOnMap(IntVector2 newPosition)
    {
        positionInGridSyncVar = new Vector2(newPosition.X, newPosition.Y);
    }

    public void ClearPositionInGrid()
    {
        MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X].Unit = null;
    }

    [ClientRpc]
    public void RpcClearPositionInGrid()
    {
        ClearPositionInGrid();
    }

    public void FillPositionInGrid()
    {
        MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X].Unit = this;
    }

    public void InitializePositionInGrid()
    {
        positionInGridSyncVar = gameObject.transform.position;
    }

    [ClientRpc]
    public void RpcHideYourself()
    {
        HideYourself();
    }

    public virtual void HideYourself()
    {
        spriteRenderer.enabled = false;
        selectionCollider.SetActive(false);
        selectionIndicator.SetActive(false);
        enabled = false;
        gameObject.GetComponent<MinimapElement>().Hide();
    }

    [ClientRpc]
    public void RpcShowYourself()
    {
        ShowYourself();
    }

    public virtual void ShowYourself()
    {
        spriteRenderer.enabled = true;
        selectionCollider.SetActive(true);
        enabled = true;
        gameObject.GetComponent<MinimapElement>().Show();
    }

    [ClientRpc]
    public void RpcSetNewPosition(Vector2 position)
    {
        gameObject.transform.position = position;
    }

    public bool CheckIfCanGoTo(IntVector2 targetPosition)
    {
        if (Mathf.Abs(positionInGrid.X - targetPosition.X) > 1 || Mathf.Abs(positionInGrid.Y - targetPosition.Y) > 1)
        {
            return false;
        }
        else
        {
            if (targetPosition.X != positionInGrid.X && targetPosition.Y != positionInGrid.Y)
            {
                if (targetPosition.X > positionInGrid.X && targetPosition.Y > positionInGrid.Y)
                {
                    return MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X + 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y + 1, positionInGrid.X + 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y + 1, positionInGrid.X].IsWalkable;
                }
                else if (targetPosition.X > positionInGrid.X && targetPosition.Y < positionInGrid.Y)
                {
                    return MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X + 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y - 1, positionInGrid.X + 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y - 1, positionInGrid.X].IsWalkable;
                }
                else if (targetPosition.X < positionInGrid.X && targetPosition.Y < positionInGrid.Y)
                {
                    return MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X - 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y - 1, positionInGrid.X - 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y - 1, positionInGrid.X].IsWalkable;
                }
                else
                {
                    return MapGridded.Instance.MapGrid[positionInGrid.Y, positionInGrid.X - 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y + 1, positionInGrid.X - 1].IsWalkable && MapGridded.Instance.MapGrid[positionInGrid.Y + 1, positionInGrid.X].IsWalkable;
                }
            }
            else
            {
                return MapGridded.Instance.MapGrid[targetPosition.Y, targetPosition.X].IsWalkable;
            }
        }
    }
}
