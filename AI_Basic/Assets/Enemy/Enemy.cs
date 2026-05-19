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
    [SerializeField]
    [Min(0f)]
    private float _respawnDelay = 3f;
    [SerializeField]
    [Min(0f)]
    private float _safeRespawnDistanceFromPlayer = 6f;

    private BaseState _currentState;
    public PatrolState PatrolState = new PatrolState();
    public ChaseState ChaseState = new ChaseState();
    public RetreatState RetreatState = new RetreatState();
    private Collider _collider;
    private Renderer[] _renderers;
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private bool _isDead;

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
        _collider = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
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
        if (_isDead)
        {
            return;
        }

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
        if (_isDead)
        {
            return;
        }

        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        _isDead = true;
        SetAliveState(false);

        yield return new WaitForSeconds(_respawnDelay);

        Vector3 respawnPosition = GetRespawnPosition();
        transform.rotation = _spawnRotation;

        if (NavMeshAgent != null)
        {
            NavMeshAgent.enabled = true;
            NavMeshAgent.Warp(respawnPosition);
            NavMeshAgent.ResetPath();
            NavMeshAgent.isStopped = false;
        }
        else
        {
            transform.position = respawnPosition;
        }

        _currentState = PatrolState;
        _currentState.EnterState(this);
        SetAliveState(true);
        _isDead = false;
    }

    private void SetAliveState(bool isAlive)
    {
        if (_collider != null)
        {
            _collider.enabled = isAlive;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].enabled = isAlive;
            }
        }

        if (NavMeshAgent != null)
        {
            if (!isAlive)
            {
                NavMeshAgent.isStopped = true;
                NavMeshAgent.ResetPath();
                NavMeshAgent.enabled = false;
            }
        }
    }

    private Vector3 GetRespawnPosition()
    {
        if (WayPoints == null || WayPoints.Count == 0)
        {
            return _spawnPosition;
        }

        List<Transform> safeCandidates = new List<Transform>();
        Transform bestFallbackCandidate = null;
        float bestDistanceSquared = -1f;
        float safeDistanceSquared = _safeRespawnDistanceFromPlayer * _safeRespawnDistanceFromPlayer;
        Vector3 playerPosition = Player != null ? Player.transform.position : transform.position;

        for (int i = 0; i < WayPoints.Count; i++)
        {
            Transform waypoint = WayPoints[i];
            if (waypoint == null)
            {
                continue;
            }

            float distanceSquared = (waypoint.position - playerPosition).sqrMagnitude;
            if (distanceSquared >= safeDistanceSquared)
            {
                safeCandidates.Add(waypoint);
            }

            if (distanceSquared > bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                bestFallbackCandidate = waypoint;
            }
        }

        if (safeCandidates.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, safeCandidates.Count);
            return safeCandidates[randomIndex].position;
        }

        if (bestFallbackCandidate != null)
        {
            return bestFallbackCandidate.position;
        }

        return _spawnPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDead)
        {
            return;
        }

        if (_currentState != RetreatState)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<Player>().Dead();
            }
        }
    }
}
