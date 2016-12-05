using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    private bool isMoving = false;
    private bool isAttacking = false;
    private bool isIdling = false;
    private bool isDying = false;
    private bool isFollowingPath = false;
    private PathNode nextNodeToFollow;
    private IntVector2 positionInGrid;
    private int indexOfFollowedPathNode;
    private IntVector2 requestedTargetPositionInGrid;

    public string unitName;
    public int level = 1;
    public int armor;
    public int minDamage;
    public int maxDamage;
    public int range;
    public int sight;
    public int speed;
    public int maxHealth;
    public int actualHealth;
    public int ownerPlayerID;
    public List<PathNode> followedPath;

    public Sprite portrait;

    public GameObject selectionIndicator;

    public ActionButton actionButton;

    void Awake()
    {
        actualHealth = maxHealth;
        positionInGrid = new IntVector2(Mathf.RoundToInt(gameObject.transform.position.x), Mathf.RoundToInt(gameObject.transform.position.y));
        //Select();
        //Map.Instance.mapTiles[positionInGrid.y, positionInGrid.x].GetComponent<SpriteRenderer>().color = Color.black;
    }

    void Update()
    {
        if (isMoving)
        {
            Move();
        }
        if (isAttacking)
        {
            Attack();
        }
        if (isIdling)
        {
            Idle();
        }
        if (isDying)
        {
            Die();
        }
        if (isFollowingPath)
        {
            FollowPath();
        }
    }

    public virtual void Move()
    {

    }

    public virtual void Attack()
    {

    }

    public virtual void Idle()
    {

    }

    public virtual void Die()
    {

    }

    public void BuildBase()
    {

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

    public virtual void FollowPath()
    {
        gameObject.transform.position += (Vector3)(new Vector2(nextNodeToFollow.x, nextNodeToFollow.y) - (Vector2)gameObject.transform.position).normalized * speed * Time.deltaTime;
        if (((Vector2)gameObject.transform.position - new Vector2(nextNodeToFollow.x, nextNodeToFollow.y)).magnitude < 0.03f)
        {
            if (requestedTargetPositionInGrid != null)
            {
                positionInGrid = new IntVector2(Mathf.RoundToInt(gameObject.transform.position.x), Mathf.RoundToInt(gameObject.transform.position.y));
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
                //Map.Instance.mapTiles[positionInGrid.y, positionInGrid.x].GetComponent<SpriteRenderer>().color = Color.white;
                positionInGrid = new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y);
                //Map.Instance.mapTiles[positionInGrid.y, positionInGrid.x].GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
    }

    public virtual void StartFollowingPath(List<PathNode> pathToFollow)
    {
        if (pathToFollow != null)
        {
            followedPath = pathToFollow;
            nextNodeToFollow = pathToFollow[0];
            indexOfFollowedPathNode = 0;
            //Map.Instance.mapTiles[positionInGrid.y, positionInGrid.x].GetComponent<SpriteRenderer>().color = Color.white;
            positionInGrid = new IntVector2(nextNodeToFollow.x, nextNodeToFollow.y);
            //Map.Instance.mapTiles[positionInGrid.y, positionInGrid.x].GetComponent<SpriteRenderer>().color = Color.black;
            isFollowingPath = true;
        }
    }

    public void Select()
    {
        selectionIndicator.SetActive(true);
        SelectionInfoKeeper.Instance.Assign(this);
        SelectionInfoKeeper.Instance.Show();
    }
}
