using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    private bool _isMoving;
    private Vector3 _destination;

    public void EnterState(Enemy enemy)
    {
        _isMoving = false;
        //Debug.Log("Start Patrol");
        enemy.Animator.SetTrigger("PatrolState");
    }

    public void UpdateState(Enemy enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) < enemy.ChaseDistance)
        {
            enemy.SwitchState(enemy.ChaseState);
            //return;
        }
        if (!_isMoving)
        {
            _isMoving = true;
            int index = UnityEngine.Random.Range(0, enemy.WayPoints.Count);
            _destination = enemy.WayPoints[index].position;
            enemy.NavMeshAgent.destination = _destination;
            //Debug.Log($"Patrolling to waypoint {index}: {_destination}");
        }
        else
        {
            float distance = Vector3.Distance(_destination, enemy.transform.position);
            Debug.Log($"Distance to destination: {distance}");
            if (distance <= 1)
            {
                Debug.Log("Reached waypoint, picking new one.");
                _isMoving = false;
            }
        }
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Stop Patrol");
    }
}
