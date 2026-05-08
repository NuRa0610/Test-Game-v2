using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    private Transform _camera;
    [SerializeField]
    private float _powerUpDuration;
    [SerializeField]
    private Transform _respawnPoint;
    [SerializeField]
    private int _health;
    [SerializeField]
    private TMP_Text _healthText;

    private Rigidbody _rigidBody;
    private Coroutine _powerupCoroutine;
    private bool _isPoweredUp;
    private Vector2 _moveInput;
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    private void Awake()
    {
        _collisionSkin = Mathf.Clamp(_collisionSkin, 0.005f, 0.1f);
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnValidate()
    {
        _collisionSkin = Mathf.Clamp(_collisionSkin, 0.005f, 0.1f);
        _maxSlideIterations = Mathf.Max(1, _maxSlideIterations);
    }

    private void Start()
    {
        UpdateUI();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxis("Horizontal");
        _moveInput.y = Input.GetAxis("Vertical");
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
        _rigidBody.MovePosition(_rigidBody.position + resolvedDelta);
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

            if (_rigidBody.SweepTest(
                    direction,
                    out RaycastHit hit,
                    distance + _collisionSkin,
                    QueryTriggerInteraction.Ignore))
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
            transform.position = _respawnPoint.position;
        }

        else
        {
            _health = 0;
            SceneManager.LoadScene("LoseScreen");
        }
        
        UpdateUI();
    }
}
