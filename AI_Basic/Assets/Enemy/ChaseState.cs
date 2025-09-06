using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BaseState
{
    public void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Chase State");
    }

    public void UpdateState(Enemy enemy)
    {
        Debug.Log("Updating Chase State");
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Exiting Chase State");
    }
}
