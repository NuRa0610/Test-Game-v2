using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    //void Start() { }

    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _camera;
    [SerializeField]
    private float _powerUpDuration;

    private Rigidbody _rigidBody;
    private Coroutine _powerupCoroutine;
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 horizontalDirection = horizontalInput * _camera.right;
        Vector3 verticalDirection = verticalInput * _camera.forward;
        verticalDirection.y = 0;
        horizontalDirection.y = 0;
        Vector3 movementDirection = horizontalDirection + verticalDirection;
        _rigidBody.MovePosition(_rigidBody.position + movementDirection * _speed * Time.deltaTime);
        //Debug.Log("Horizontal Input: " + horizontalInput);
        //Debug.Log("Vertical Input: " + verticalInput);
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
        if (OnPowerUpStart != null)
        {
            OnPowerUpStart();
        }
        //Debug.Log("Power Up Started");
        yield return new WaitForSeconds(_powerUpDuration);
        //Debug.Log("Power Up Ended");
        if (OnPowerUpStop != null)
        {
            OnPowerUpStop();
        }
    }
}
