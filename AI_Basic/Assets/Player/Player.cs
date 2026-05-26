using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    //void Start() { }

    [SerializeField]
    private float _speed;
    [SerializeField]
    [Range(0.005f, 0.1f)]
    private float _collisionSkin = 0.05f;
    [SerializeField]
    private int _maxSlideIterations = 2;
    [SerializeField]
    private LayerMask _movementBlockerLayers = ~0;
    [SerializeField]
    private Transform _camera;
    [SerializeField]
    private Transform _visualRoot;
    [SerializeField]
    private float _powerUpDuration;
    [SerializeField]
    private bool _useEnemyWaypointsForRespawn = true;
    [SerializeField]
    [Min(0f)]
    private float _safeRespawnDistanceFromEnemy = 6f;
    [SerializeField]
    private int _health;
    [SerializeField]
    private TMP_Text _healthText;
    [SerializeField]
    // Turn this off in the Inspector if you want the mouse cursor to stay unlocked.
    private bool _lockCursorOnStart = true;
    [SerializeField]
    // WebGL needs a user click before the browser allows cursor lock.
    private bool _relockCursorOnClick = true;

    private Rigidbody _rigidBody;
    private Quaternion _visualRootBaseLocalRotation;
    private Coroutine _powerupCoroutine;
    private bool _isPoweredUp;
    private Vector2 _moveInput;
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    private const float VisualForwardOffset = 180f;

    private void Awake()
    {
        _collisionSkin = Mathf.Clamp(_collisionSkin, 0.005f, 0.1f);
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (_visualRoot != null)
        {
            _visualRootBaseLocalRotation = _visualRoot.localRotation;
        }
    }

    private void OnValidate()
    {
        _collisionSkin = Mathf.Clamp(_collisionSkin, 0.005f, 0.1f);
        _maxSlideIterations = Mathf.Max(1, _maxSlideIterations);
    }

    private void Start()
    {
        UpdateUI();
        ApplyCursorState();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
#endif

        if (_relockCursorOnClick && ShouldLockCursor() && Input.GetMouseButtonDown(0))
        {
            ApplyCursorState();
        }

        _moveInput = CrossPlatformInput.GetMoveInput();
    }

    private void ApplyCursorState()
    {
        bool shouldLockCursor = ShouldLockCursor();

        Cursor.lockState = shouldLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLockCursor;
    }

    private bool ShouldLockCursor()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return _lockCursorOnStart && !Application.isMobilePlatform;
#else
        bool useTouchControls = Input.touchSupported || Application.isMobilePlatform;
        return _lockCursorOnStart && !useTouchControls;
#endif
    }

    private void FixedUpdate()
    {
        if (_camera == null)
        {
            return;
        }

        Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, Vector3.up).normalized;
        Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
        Vector3 movementDirection = (cameraRight * _moveInput.x) + (cameraForward * _moveInput.y);

        if (movementDirection.sqrMagnitude > 1f)
        {
            movementDirection.Normalize();
        }

        Vector3 movementDelta = movementDirection * _speed * Time.fixedDeltaTime;

        if (movementDelta.sqrMagnitude <= 0f)
        {
            return;
        }

        Vector3 resolvedDelta = ResolveMovementWithSliding(movementDelta);
        FaceMovementDirection(resolvedDelta);
        _rigidBody.MovePosition(_rigidBody.position + resolvedDelta);
    }

    private void FaceMovementDirection(Vector3 direction)
    {
        if (_visualRoot == null)
        {
            return;
        }

        Vector3 localDirection = _visualRoot.parent != null
            ? _visualRoot.parent.InverseTransformDirection(direction)
            : direction;
        localDirection.y = 0f;

        if (localDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float yaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        _visualRoot.localRotation = Quaternion.Euler(0f, yaw + VisualForwardOffset, 0f) *
            _visualRootBaseLocalRotation;
    }

    private Vector3 ResolveMovementWithSliding(Vector3 desiredDelta)
    {
        Vector3 resolved = Vector3.zero;
        Vector3 remaining = desiredDelta;
        int iterationCount = Mathf.Max(1, _maxSlideIterations);

        for (int i = 0; i < iterationCount; i++)
        {
            float distance = remaining.magnitude;
            if (distance <= 0.0001f)
            {
                break;
            }

            Vector3 direction = remaining / distance;

            if (TryGetMovementBlockerHit(direction, distance + _collisionSkin, out RaycastHit hit))
            {
                float safeDistance = Mathf.Max(0f, hit.distance - _collisionSkin);
                Vector3 movePart = direction * safeDistance;
                resolved += movePart;

                // Slide the remaining movement along the hit surface.
                Vector3 leftover = remaining - movePart;
                remaining = Vector3.ProjectOnPlane(leftover, hit.normal);

                // Prevent micro-jitter when blocked very close to a wall/corner.
                if (safeDistance <= 0.0001f && remaining.sqrMagnitude <= 0.0001f)
                {
                    break;
                }
            }
            else
            {
                resolved += remaining;
                break;
            }
        }

        return resolved;
    }

    private bool TryGetMovementBlockerHit(Vector3 direction, float distance, out RaycastHit closestHit)
    {
        RaycastHit[] hits = _rigidBody.SweepTestAll(
            direction,
            distance,
            QueryTriggerInteraction.Ignore);

        closestHit = default;
        bool hasHit = false;
        float closestDistance = float.MaxValue;
        int blockerMask = _movementBlockerLayers.value;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            int hitLayerMask = 1 << hitCollider.gameObject.layer;
            if ((blockerMask & hitLayerMask) == 0)
            {
                continue;
            }

            if (hits[i].distance < closestDistance)
            {
                closestDistance = hits[i].distance;
                closestHit = hits[i];
                hasHit = true;
            }
        }

        return hasHit;
    }

    public void PickPowerUp()
    {
        //Debug.Log("Power Up Collected");
        if (_powerupCoroutine != null)
        {
            StopCoroutine(_powerupCoroutine);
        }
        _powerupCoroutine = StartCoroutine(StartPowerUp());
    }

    private IEnumerator StartPowerUp()
    {
        _isPoweredUp = true;

        if (OnPowerUpStart != null)
        {
            OnPowerUpStart();
        }
        //Debug.Log("Power Up Started");
        yield return new WaitForSeconds(_powerUpDuration);
        //Debug.Log("Power Up Ended");
        _isPoweredUp = false;

        if (OnPowerUpStop != null)
        {
            OnPowerUpStop();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isPoweredUp)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponent<Enemy>().Dead();
            }
        }
    }

    private void UpdateUI()
    {
        _healthText.text = "Health: " + _health;
    }

    public void Dead()
    {
        _health -= 1;
        
        if (_health > 0)
        {
            Vector3 respawnPosition = GetRespawnPosition();
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            _rigidBody.position = respawnPosition;
            transform.position = respawnPosition;
        }

        else
        {
            _health = 0;
            StageFlow.LoadLoseScreen();
        }
        
        UpdateUI();
    }

    private Vector3 GetRespawnPosition()
    {
        List<Transform> candidates = CollectRespawnCandidates();
        if (candidates.Count == 0)
        {
            return transform.position;
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        List<Transform> safeCandidates = new List<Transform>();
        float bestNearestEnemyDistance = -1f;
        Transform bestFallbackCandidate = candidates[0];
        float safeDistanceSquared = _safeRespawnDistanceFromEnemy * _safeRespawnDistanceFromEnemy;

        for (int i = 0; i < candidates.Count; i++)
        {
            Transform candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            float nearestEnemyDistanceSquared = GetNearestEnemyDistanceSquared(candidate.position, enemies);
            if (nearestEnemyDistanceSquared >= safeDistanceSquared)
            {
                safeCandidates.Add(candidate);
            }

            if (nearestEnemyDistanceSquared > bestNearestEnemyDistance)
            {
                bestNearestEnemyDistance = nearestEnemyDistanceSquared;
                bestFallbackCandidate = candidate;
            }
        }

        if (safeCandidates.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, safeCandidates.Count);
            return safeCandidates[randomIndex].position;
        }

        return bestFallbackCandidate.position;
    }

    private List<Transform> CollectRespawnCandidates()
    {
        List<Transform> candidates = new List<Transform>();
        HashSet<Transform> uniqueCandidates = new HashSet<Transform>();

        if (!_useEnemyWaypointsForRespawn)
        {
            return candidates;
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || enemy.WayPoints == null)
            {
                continue;
            }

            for (int j = 0; j < enemy.WayPoints.Count; j++)
            {
                Transform waypoint = enemy.WayPoints[j];
                if (waypoint != null && uniqueCandidates.Add(waypoint))
                {
                    candidates.Add(waypoint);
                }
            }
        }

        return candidates;
    }

    private static float GetNearestEnemyDistanceSquared(Vector3 position, Enemy[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            return float.MaxValue;
        }

        float nearestDistanceSquared = float.MaxValue;
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            float distanceSquared = (enemy.transform.position - position).sqrMagnitude;
            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
            }
        }

        return nearestDistanceSquared;
    }
}
