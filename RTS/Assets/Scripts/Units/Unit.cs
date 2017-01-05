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
    public int actualHealth;

    [SyncVar(hook = "OnChangePositionInGridSyncVar")]
    public Vector2 positionInGridSyncVar;

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
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            ShowActualInfo();
        }
    }

    public void ShowActualInfo()
    {
        SelectionInfoKeeper.Instance.Assign(this);
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
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        }
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
        if (MultiplayerController.Instance.localPlayer.selectController.selectedUnit == this)
        {
            MultiplayerController.Instance.localPlayer.selectController.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).foodAmount -= foodCost;
        NetworkServer.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        MultiplayerController.Instance.players.Find(item => item.playerType == owner).activeUnits.Remove(this);
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

    public void Select()
    {
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
