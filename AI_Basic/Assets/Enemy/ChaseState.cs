using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BaseState
{
    public void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Chase State");
        enemy.Animator.SetTrigger("ChaseState");
    }

    public void UpdateState(Enemy enemy)
    {
        if (enemy.Player != null)
        {
            enemy.NavMeshAgent.destination = enemy.Player.transform.position;
            if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) < enemy.ChaseDistance)
            {
                enemy.SwitchState(enemy.PatrolState);
            }
            //return;
        }
        Debug.Log("Updating Chase State");
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Exiting Chase State");
    }
}
