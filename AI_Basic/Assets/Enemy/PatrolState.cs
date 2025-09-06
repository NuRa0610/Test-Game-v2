using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    public void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Patrol State");
    }

    public void UpdateState(Enemy enemy)
    {
        Debug.Log("Updating Patrol State");
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Exiting Patrol State");
    }
}
