using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit
{
    private bool isAttacking = false;
    public bool IsAttacking
    {
        get
        {
            return isAttacking;
        }
    }
    private Unit attackedUnit;
    private Building attackedBuilding;
    [SerializeField]
    private float delayBetweenAttacks;
    [SerializeField]
    private int damage;
    private float timeFromLastTryToHit = 0f;
    private bool justContactedWithEnemy = true;

    public void StartAttack(Unit unitToAttack)
    {
        attackedUnit = unitToAttack;
        attackedBuilding = null;
        isAttacking = true;
    }

    public void StartAttack(Building buildingToAttack)
    {
        attackedBuilding = buildingToAttack;
        attackedUnit = null;
        isAttacking = true;
    }

    public void StopAttack()
    {
        attackedUnit = null;
        attackedBuilding = null;
        isAttacking = false;
    }

    public void Attack()
    {
        if (!CheckIfEnemyIsInAttackRange())
        {
            justContactedWithEnemy = true;
            List<MapGridElement> shortestPath;
            if (attackedUnit != null)
            {
                shortestPath = ASTARPathfinder.Instance.FindPathForUnit(positionInGrid, attackedUnit);
            }
            else
            {
                shortestPath = ASTARPathfinder.Instance.FindPathForBuilding(positionInGrid, attackedBuilding);
            }
            if (shortestPath != null)
            {
                if (shortestPath.Count == 0)
                {
                    RequestGoTo(shortestPath);
                }
                else
                {
                    RequestGoTo(shortestPath);
                }
            }
        }
        else
        {
            if (justContactedWithEnemy)
            {
                justContactedWithEnemy = false;
                timeFromLastTryToHit = 0f;
                StopFollowingPath();
            }
            timeFromLastTryToHit += Time.deltaTime;
            if (timeFromLastTryToHit >= delayBetweenAttacks)
            {
                if (attackedUnit != null)
                {
                    attackedUnit.GetHit(damage, this);
                }
                else
                {
                    attackedBuilding.GetHit(damage, this);
                }
                timeFromLastTryToHit = 0f;
            }
        }
    }

    public bool CheckIfEnemyIsInAttackRange()
    {
        for (int row = 0; row <= 2* range; ++row)
        {
            for (int column = 0; column <= 2 * range; ++column)
            {
                IntVector2 checkedPosition = new IntVector2(positionInGrid.X - range + column, positionInGrid.Y - range + row);
                if (attackedUnit != null)
                {
                    if (MapGridded.Instance.IsInMap(checkedPosition) && (MapGridded.Instance.MapGrid[checkedPosition.Y, checkedPosition.X].Unit == attackedUnit))
                    {
                        return true;
                    }
                }
                else if (attackedBuilding != null)
                {
                    if (MapGridded.Instance.IsInMap(checkedPosition) && (MapGridded.Instance.MapGrid[checkedPosition.Y, checkedPosition.X].Building == attackedBuilding))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    protected override void Update()
    {
        base.Update();
        if (isServer)
        {
            if (isAttacking && (attackedBuilding == null && attackedUnit == null) || ((attackedBuilding != null && attackedBuilding.ActualHealth <= 0f) || (attackedUnit != null && attackedUnit.ActualHealth <= 0f)))
            {
                StopAttack();
            }
            if (isAttacking)
            {
                Attack();
            }
        }
    }
}