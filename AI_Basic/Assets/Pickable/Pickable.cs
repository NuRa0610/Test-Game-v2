using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    [SerializeField]
    private PickableType _pickableType;
    public Action<Pickable> OnPicked;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (OnPicked != null)
            {
                OnPicked(this);
            }
            // Pick up the item
            //Destroy(gameObject);
            //Debug.Log("Pickable Triggered: " + pickableType);
        }
    }

    // Update is called once per frame
}
