using UnityEngine;

public class CollectibleGesture : MonoBehaviour
{
    [Header("Bob")]
    [Min(0f)]
    [SerializeField]
    private float _bobHeight = 0.12f;
    [Min(0f)]
    [SerializeField]
    private float _bobSpeed = 1.5f;
    [Min(0f)]
    [SerializeField]
    private float _randomSpeedRange = 0.25f;
    [Min(1)]
    [SerializeField]
    private int _bobCyclesBeforeFlip = 2;

    [Header("Flip")]
    [SerializeField]
    private float _flipDegrees = 180f;
    [Min(0.01f)]
    [SerializeField]
    private float _flipDuration = 0.18f;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _speedMultiplier;
    private float _bobProgress;
    private float _flipProgress;
    private float _currentFlipAngle;
    private float _flipStartAngle;
    private float _flipTargetAngle;
    private bool _isFlipping;

    private void Awake()
    {
        float randomRange = Mathf.Max(0f, _randomSpeedRange);
        _speedMultiplier = Random.Range(Mathf.Max(0.1f, 1f - randomRange), 1f + randomRange);
    }

    private void OnEnable()
    {
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        _bobProgress = 0f;
        _flipProgress = 0f;
        _currentFlipAngle = 0f;
        _isFlipping = false;
    }

    private void Update()
    {
        if (_isFlipping)
        {
            UpdateFlip();
            return;
        }

        UpdateBob();
    }

    private void UpdateBob()
    {
        float bobSpeed = Mathf.Max(0f, _bobSpeed) * _speedMultiplier;
        _bobProgress += Time.deltaTime * bobSpeed;

        float bobOffset = Mathf.Sin(_bobProgress * Mathf.PI * 2f) * _bobHeight;
        transform.localPosition = _startPosition + Vector3.up * bobOffset;
        transform.localRotation = Quaternion.Euler(0f, _currentFlipAngle, 0f) * _startRotation;

        if (_bobProgress >= Mathf.Max(1, _bobCyclesBeforeFlip))
        {
            _bobProgress = 0f;
            transform.localPosition = _startPosition;
            StartFlip();
        }
    }

    private void StartFlip()
    {
        _isFlipping = true;
        _flipProgress = 0f;
        _flipStartAngle = _currentFlipAngle;
        _flipTargetAngle = _currentFlipAngle + _flipDegrees;
    }

    private void UpdateFlip()
    {
        float duration = Mathf.Max(0.01f, _flipDuration);
        _flipProgress += Time.deltaTime / duration;

        float t = Mathf.Clamp01(_flipProgress);
        float easedT = Mathf.SmoothStep(0f, 1f, t);
        _currentFlipAngle = Mathf.Lerp(_flipStartAngle, _flipTargetAngle, easedT);

        transform.localPosition = _startPosition;
        transform.localRotation = Quaternion.Euler(0f, _currentFlipAngle, 0f) * _startRotation;

        if (t >= 1f)
        {
            _currentFlipAngle = Mathf.Repeat(_flipTargetAngle, 360f);
            _isFlipping = false;
        }
    }
}
