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
    public IntVector2 positionInGrid;
    protected int indexOfFollowedPathNode;
    protected IntVector2 requestedTargetPositionInGrid;
    protected bool hasFinishedGoingToLastStep = false;
    protected Coroutine disableHitAnimationCoroutine;

    public string unitName;
    public int armor;
    public int minDamage;
    public int maxDamage;
    public int range;
    public int speed;
    public int maxHealth;

    [SyncVar(hook = "OnActualHealthChange")]
    public int actualHealth;

    [SyncVar(hook = "OnChangePositionInGridSyncVar")]
    public Vector2 positionInGridSyncVar;

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

    public float averageDamageFactor = 0.6f;
    public float criticalDamageFactor = 0.3f;

    private void Awake()
    {
        gameObject.GetComponentInChildren<MinimapElement>().image.color = MultiplayerController.Instance.playerColors[(int)owner];
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).activeUnits.Add(this);
        ++MultiplayerController.Instance.players.Find(item => item.playerType == owner).allUnitsAmount;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (isServer)
        {
            actualHealth = maxHealth;
            InitializePositionInGrid();
            MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodAmount += foodCost;
        }
    }

    public virtual void Update()
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
    }

    public virtual void GetHit(int damage, Warrior attacker)
    {
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            ++MultiplayerController.Instance.players.Find(item => item.playerType == attacker.owner).kills;
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
        if (MultiplayerController.Instance.localPlayer.selector.selectedUnits.Contains(this))
        {
            MultiplayerController.Instance.localPlayer.selector.Unselect(this);
        }
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodAmount -= foodCost;
        NetworkServer.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (MultiplayerController.Instance != null && SelectionInfoKeeper.Instance != null)
        {
            if (MultiplayerController.Instance.localPlayer.selector.selectedUnits.Contains(this))
            {
                MultiplayerController.Instance.localPlayer.selector.Unselect(this);
            }
            MultiplayerController.Instance.players.Find(item => item.playerType == owner).activeUnits.Remove(this);
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
                requestedTargetPositionInGrid = new IntVector2(targetPath[targetPath.Count - 1].x, targetPath[targetPath.Count - 1].y);
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
                nextNodeToFollow = MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x];
            }
            indexOfFollowedPathNode = 0;
            if (MapGridded.Instance.mapGrid[nextNodeToFollow.y, nextNodeToFollow.x].ChecklIfIsWalkableForUnit(this))
            {
                SetNewPositionOnMap(new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y));
                if (pathToFollow.Count > 0)
                {
                    requestedTargetPositionInGrid = new IntVector2(pathToFollow[pathToFollow.Count - 1].x, pathToFollow[pathToFollow.Count - 1].y);
                }
                else
                {
                    requestedTargetPositionInGrid = positionInGrid;
                }
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
                if (CheckIfCanGoTo(new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y)))
                {
                    positionInGridSyncVar = new Vector2(nextNodeToFollow.x, nextNodeToFollow.y);
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
        positionInGridSyncVar = new Vector2(newPosition.x, newPosition.y);
        if (positionInGrid != null)
        {
            ClearPositionInGrid();
        }
        positionInGrid = new IntVector2((int)newPosition.x, (int)newPosition.y);
        FillPositionInGrid();
        gameObject.transform.position = MapGridded.MapToWorldPosition(newPosition);
    }

    public void SetNewPositionOnMap(IntVector2 newPosition)
    {
        positionInGridSyncVar = new Vector2(newPosition.x, newPosition.y);
    }

    public void ClearPositionInGrid()
    {
        MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x].unit = null;
    }

    [ClientRpc]
    public void RpcClearPositionInGrid()
    {
        ClearPositionInGrid();
    }

    public void FillPositionInGrid()
    {
        MapGridded.Instance.mapGrid[positionInGrid.y, positionInGrid.x].unit = this;
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
