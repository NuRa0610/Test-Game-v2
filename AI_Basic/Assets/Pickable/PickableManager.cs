using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (pickableObjects[i].PickableType == PickableType.Coin)
            {
                _pickablesList.Add(pickableObjects[i]);
            }

            pickableObjects[i].OnPicked += OnPickableCollected;
        }
        _scoreManager.SetMaxScore(_pickablesList.Count);
        //Debug.Log("Pickables in the scene: " + _pickablesList.Count);
    }

    private void OnPickableCollected(Pickable pickable)
    {
        if (pickable.PickableType == PickableType.Coin)
        {
            _pickablesList.Remove(pickable);

            if (_scoreManager != null)
            {
                _scoreManager.AddScore(1);
            }

            if (_pickablesList.Count <= 0)
            {
                //Debug.Log("All Pickables Collected");
                StageFlow.LoadWinScreen();
            }
        }

        if (pickable.PickableType == PickableType.PowerUp)
        {
            _player?.PickPowerUp();
        }

        Destroy(pickable.gameObject);
        //Debug.Log("Pickable Collected: " + pickable.name);

    }
}
