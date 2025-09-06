using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    public List<Transform> WayPoints = new List<Transform>();
    [SerializeField]
    public float ChaseDistance;
    [SerializeField]
    public Player Player;

    private BaseState _currentState;
    public PatrolState PatrolState = new PatrolState();
    public ChaseState ChaseState = new ChaseState();
    public RetreatState RetreatState = new RetreatState();

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent NavMeshAgent;

    public void SwitchState(BaseState state)
    {
        _currentState.ExitState(this);
        _currentState = state;
        _currentState.EnterState(this);
    }

    private void Awake()
    {
        _currentState = PatrolState;
        _currentState.EnterState(this);
        NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    private void Start()
    {
        if (Player != null)
        {
            Player.OnPowerUpStart += StartRetreat;
            Player.OnPowerUpStop += StopRetreat;
        }
    }

    private void Update()
    {
        if (_currentState != null)
        {
            _currentState.UpdateState(this);
        }
    }

    private void StartRetreat()
    {
        SwitchState(RetreatState);
    }

    private void StopRetreat()
    {
        SwitchState(PatrolState);
    }
}
