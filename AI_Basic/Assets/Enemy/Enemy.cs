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
    [Min(0f)]
    public float ChaseExitBuffer = 0.5f;
    [SerializeField]
    public Player Player;
    [SerializeField]
    private float _forwardOffset = 180f;

    private BaseState _currentState;
    public PatrolState PatrolState = new PatrolState();
    public ChaseState ChaseState = new ChaseState();
    public RetreatState RetreatState = new RetreatState();

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent NavMeshAgent;
    [HideInInspector]
    public Animator Animator; //{ get; private set; }

    public void SwitchState(BaseState state)
    {
        _currentState.ExitState(this);
        _currentState = state;
        _currentState.EnterState(this);
    }

    private void Awake()
    {
        _currentState = PatrolState;
        Animator = GetComponent<Animator>();
        _currentState.EnterState(this);
        NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (NavMeshAgent != null)
        {
            NavMeshAgent.updateRotation = false;
        }
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

        FaceMovementDirection();
    }

    private void FaceMovementDirection()
    {
        if (NavMeshAgent == null)
        {
            return;
        }

        Vector3 direction = NavMeshAgent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up) *
            Quaternion.Euler(0f, _forwardOffset, 0f);
    }

    private void StartRetreat()
    {
        SwitchState(RetreatState);
    }

    private void StopRetreat()
    {
        SwitchState(PatrolState);
    }

    public void Dead()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_currentState != RetreatState)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().Dead();
            }
        }
    }
}
