using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableManager : MonoBehaviour
{
    // Start is called before the first frame update
    private List<Pickable> _pickablesList = new List<Pickable>();
    
    private void Start()
    {
        initializePickableList();
    }

    // Update is called once per frame
    private void initializePickableList()
    {
        //Pickable[] pickableObjects = gameObject.FindObjectsOfType<Pickable>();
        Pickable[] pickableObjects = FindObjectsOfType<Pickable>();
        for (int i = 0; i < pickableObjects.Length; i++)
        {
            _pickablesList.Add(pickableObjects[i]);
            pickableObjects[i].OnPicked += OnPickableCollected;
        }
        Debug.Log("Pickables in the scene: " + _pickablesList.Count);
    }

    private void OnPickableCollected(Pickable pickable)
    {
        _pickablesList.Remove(pickable);
        Destroy(pickable.gameObject);
        Debug.Log("Pickable Collected: " + pickable.name);
        if (_pickablesList.Count <= 0)
        {
            Debug.Log("All Pickables Collected");
        }
    }
}
