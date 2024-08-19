using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public InfoPanel info;

    bool paused = false;
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            paused = !paused;
            if (paused) {
                info.Show("paused");
            } else {
                info.Hide();
            }
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
