using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menu;

    bool paused = false;
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            paused = !paused;
            menu.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
