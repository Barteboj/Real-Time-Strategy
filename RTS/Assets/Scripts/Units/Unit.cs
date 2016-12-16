﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
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
    private int actualHealth;
    public int ActualHealth
    {
        get
        {
            return actualHealth;
        }
        set
        {
            actualHealth = value;
            if (SelectController.Instance.selectedUnit == this)
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

    void Awake()
    {
        actualHealth = maxHealth;
        InitializePositionInGrid();
        Players.Instance.LocalPlayer.FoodAmount += foodCost;
    }

    public virtual void Update()
    {
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
        if (SelectController.Instance.selectedUnit == this)
        {
            SelectController.Instance.Unselect();
            SelectionInfoKeeper.Instance.Hide();
            Unselect();
        }
        Players.Instance.LocalPlayer.FoodAmount -= foodCost;
        Destroy(gameObject);
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
            }
            else
            {
                isFollowingPath = false;
                RequestGoTo(new IntVector2(pathToFollow[pathToFollow.Count - 1].x, pathToFollow[pathToFollow.Count - 1].y));
            }
        }
    }

    public virtual void FollowPath()
    {
        gameObject.transform.position += (Vector3)(new Vector2(nextNodeToFollow.x, nextNodeToFollow.y) - (Vector2)gameObject.transform.position).normalized * speed * Time.deltaTime;
        if (((Vector2)gameObject.transform.position - new Vector2(nextNodeToFollow.x, nextNodeToFollow.y)).magnitude < 0.03f)
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
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.Assign(this);
        SelectionInfoKeeper.Instance.SetHealthBar((float)actualHealth / maxHealth);
        SelectionInfoKeeper.Instance.Show();
        ActionButtons.Instance.HideAllButtons();
        foreach (ActionButtonType buttonType in buttonTypes)
        {
            ActionButtons.Instance.buttons.Find(button => button.buttonType == buttonType).Show();
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
