using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickableManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Player _player;
    [SerializeField]
    private ScoreManager _scoreManager;

    private List<Pickable> _pickablesList = new List<Pickable>();
    
    private void Start()
    {
        initializePickableList();
    }

    // Update is called once per frame
    private void initializePickableList()
    {
        //Pickable[] pickableObjects = gameObject.FindObjectsOfType<Pickable>();
        //_scoreManager.SetMaxScore(_pickablesList.Count);
        Pickable[] pickableObjects = FindObjectsOfType<Pickable>();
        for (int i = 0; i < pickableObjects.Length; i++)
        {
            _pickablesList.Add(pickableObjects[i]);
            pickableObjects[i].OnPicked += OnPickableCollected;
        }
        _scoreManager.SetMaxScore(_pickablesList.Count);
        //Debug.Log("Pickables in the scene: " + _pickablesList.Count);
    }

    private void OnPickableCollected(Pickable pickable)
    {
        _pickablesList.Remove(pickable);
        Destroy(pickable.gameObject);
        //Debug.Log("Pickable Collected: " + pickable.name);
        
        if (_pickablesList.Count <= 0)
        {
            //Debug.Log("All Pickables Collected");
            SceneManager.LoadScene("WinScreen");
        }

        if (pickable.PickableType == PickableType.PowerUp)
        {
            _player?.PickPowerUp();
        }

        if (_scoreManager != null)
        {
            _scoreManager.AddScore(1);
        }

    }
}
