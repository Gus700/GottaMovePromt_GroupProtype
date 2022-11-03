using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCollision : MonoBehaviour
{
    public GameObject currentObject;
    public bool active;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        active = currentObject.activeSelf;
    }
    
    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Player") {
            currentObject.SetActive(false);
            Debug.Log("yup collecting");
        }
    }
}
