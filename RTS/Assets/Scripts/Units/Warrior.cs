﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit
{
    public bool isAttacking = false;
    public Unit attackedUnit;
    public Building attackedBuilding;

    public void StartAttack(Unit unitToAttack)
    {
        attackedUnit = unitToAttack;
        attackedBuilding = null;
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
            Debug.Log("Following");
            List<MapGridElement> shortestPath;
            shortestPath = ASTARPathfinder.Instance.FindNearestEntrancePath(positionInGrid, MapGridded.WorldToMapPosition(attackedUnit.transform.position), 1, 1);
            if (shortestPath.Count == 0)
            {
                RequestGoTo(positionInGrid);
            }
            else
            {
                RequestGoTo(new IntVector2(shortestPath[shortestPath.Count - 1].x, shortestPath[shortestPath.Count - 1].y));
            }
            
        }
        else
        {
            Debug.Log("Attacking");
        }
    }

    public bool CheckIfEnemyIsInAttackRange()
    {

        bool isAttackingUnit = attackedUnit != null;
        for (int row = 0; row <= 2* range; ++row)
        {
            for (int column = 0; column <= 2 * range; ++column)
            {
                IntVector2 checkedPosition = new IntVector2(positionInGrid.x - range + column, positionInGrid.y - range + row);
                if (MapGridded.Instance.IsInMap(checkedPosition) && (MapGridded.Instance.mapGrid[checkedPosition.y, checkedPosition.x].unit == attackedUnit))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void Update()
    {
        base.Update();
        if (isAttacking)
        {
            Attack();
        }
    }
}