using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetreatState : BaseState
{
    public void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Retreat State");
        enemy.Animator.SetTrigger("RetreatState");
    }

    public void UpdateState(Enemy enemy)
    {
        if (enemy.Player != null)
        {
            enemy.NavMeshAgent.destination = enemy.transform.position - enemy.Player.transform.position;
        }
        //Debug.Log("Updating Retreat State");
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Exiting Retreat State");
    }
}
