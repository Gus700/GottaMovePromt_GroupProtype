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
    public AudioSource backGroundMusic;

    // inititialize the array of movement types to with only one set to true
    private bool[] movementTypes = {true, false, false};

    // save polished camera for polished ripple effect
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject polishedCam;

    // Start is called before the first frame update
    void Start()
    {
        polishedCam.GetComponent<AudioListener>().enabled = false;
        polishedCam.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPolished() && !isDistinct() && backGroundMusic.isPlaying && (backGroundMusic.volume == 1f)) {
            StartCoroutine(FadeAudioSource.StartFade(backGroundMusic, 1f, 0.0f));
        } else if (isPolished() && (backGroundMusic.volume == 0f)) {
            StartCoroutine(FadeAudioSource.StartFade(backGroundMusic, 1f, 1.0f));
        }
        // keep public values updated based on movementTypes array
        originalMovement = movementTypes[0];
        polishedMovement = movementTypes[1];
        distinctMovement = movementTypes[2];
        // if "t" key is pressed then toggle the movement type
        if (Input.GetButtonDown("ToggleMovement")) {
            toggle();
            toggleSound.Play();
            // if background music is not playing then start music
            if (!backGroundMusic.isPlaying) {
                Debug.Log("musing starting");
                backGroundMusic.Play();
                //backGroundMusic.volume = 0f;
                // StartCoroutine(FadeAudioSource.StartFade(backGroundMusic, 2f, 1f));
            }
        }
        Debug.Log(backGroundMusic.volume);
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
        if (movementTypes[1] || movementTypes[2])
        {
            mainCam.GetComponent<AudioListener>().enabled = false;
            mainCam.SetActive(false);
            polishedCam.GetComponent<AudioListener>().enabled = true;
            polishedCam.SetActive(true);
        }
        else
        {
            mainCam.SetActive(true);
            mainCam.GetComponent<AudioListener>().enabled = true;
            polishedCam.SetActive(false);
            polishedCam.GetComponent<AudioListener>().enabled = false;
        }
    }

    public bool isPolished() { return movementTypes[1]; }
    public bool isDistinct() { return movementTypes[2]; }

        // taken from 
    //https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/#:~:text=You%20can%20fade%20audio%20in,script%20will%20do%20the%20rest. 
    public static class FadeAudioSource {
        public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
        {
            float currentTime = 0;
            float start = audioSource.volume;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
            yield break;
        }
    }
}
