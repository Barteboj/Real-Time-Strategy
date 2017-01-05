﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit
{
    public bool isAttacking = false;
    public Unit attackedUnit;
    public Building attackedBuilding;

    public float delayBetweenAttacks;

    public int damage;

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
                shortestPath = ASTARPathfinder.Instance.FindPath(positionInGrid, MapGridded.Instance.GetStrictFirstFreePlaceAround(MapGridded.WorldToMapPosition(attackedUnit.transform.position), 1, 1));
            }
            else
            {
                shortestPath = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(attackedBuilding.transform.position), attackedBuilding.width, attackedBuilding.height);
            }
            if (shortestPath != null)
            {
                if (shortestPath.Count == 0)
                {
                    RequestGoTo(positionInGrid);
                }
                else
                {
                    RequestGoTo(new IntVector2(shortestPath[shortestPath.Count - 1].x, shortestPath[shortestPath.Count - 1].y));
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
                IntVector2 checkedPosition = new IntVector2(positionInGrid.x - range + column, positionInGrid.y - range + row);
                if (attackedUnit != null)
                {
                    if (MapGridded.Instance.IsInMap(checkedPosition) && (MapGridded.Instance.mapGrid[checkedPosition.y, checkedPosition.x].unit == attackedUnit))
                    {
                        return true;
                    }
                }
                else if (attackedBuilding != null)
                {
                    if (MapGridded.Instance.IsInMap(checkedPosition) && (MapGridded.Instance.mapGrid[checkedPosition.y, checkedPosition.x].building == attackedBuilding))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public override void Update()
    {
        base.Update();
        if (isServer)
        {
            if (isAttacking && (attackedBuilding == null && attackedUnit == null) || ((attackedBuilding != null && attackedBuilding.actualHealth <= 0f) || (attackedUnit != null && attackedUnit.actualHealth <= 0f)))
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