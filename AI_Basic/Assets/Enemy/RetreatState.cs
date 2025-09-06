using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetreatState : BaseState
{
    public void EnterState(Enemy enemy)
    {
        Debug.Log("Entering Retreat State");
    }

    public void UpdateState(Enemy enemy)
    {
        Debug.Log("Updating Retreat State");
    }

    public void ExitState(Enemy enemy)
    {
        Debug.Log("Exiting Retreat State");
    }
}
