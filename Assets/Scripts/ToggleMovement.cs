using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleMovement : MonoBehaviour
{
    // initialize movement booleans, we want only one to be true at all times
    public bool originalMovement;
    public bool polishedMovement;
    public bool distinctMovement;
    public AudioSource toggleSound;

    // inititialize the array of movement types to with only one set to true
    private bool[] movementTypes = {true, false, false};

    // save polished camera for polished ripple effect
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject polishedCam;

    // Start is called before the first frame update
    void Start()
    {
        polishedCam.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // keep public values updated based on movementTypes array
        originalMovement = movementTypes[0];
        polishedMovement = movementTypes[1];
        distinctMovement = movementTypes[2];
        // if "t" key is pressed then toggle the movement type
        if (Input.GetButtonDown("ToggleMovement")) {
            toggle();
            toggleSound.Play();
        }
    }

    // toggles which element in the array is set to true in the private array, movementTypes
    void toggle () {
        for (int i = 0; i < movementTypes.Length; i++) {
            if (movementTypes[i] == true && i != (movementTypes.Length - 1)) {
                movementTypes[i] = false;
                movementTypes[i + 1] = true;
                break;
            } else if (movementTypes[i] == true && i == (movementTypes.Length - 1)) {
                movementTypes[i] = false;
                movementTypes[0] = true;
                break;
            }
        }

        CameraToggle(); // toggles to polished camera when in polished mode
    }

    void CameraToggle()
    {
        if (movementTypes[1])
        {
            mainCam.SetActive(false);
            polishedCam.SetActive(true);
        }
        else
        {
            mainCam.SetActive(true);
            polishedCam.SetActive(false);
        }
    }

    public bool isPolished() { return movementTypes[1]; }
    public bool isDistinct() { return movementTypes[2]; }
}
