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
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        UpdateUI();
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
