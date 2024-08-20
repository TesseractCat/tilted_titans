using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent onStart = new();
    public UnityEvent onLose = new();

    [Header("References")]
    public Robot robot;
    public Robot enemyRobot;
    public UserInput player;
    public GameObject enemyPlayer;
    public GameObject mainMenuCamera;
    public GameObject gameCamera;
    public InfoPanel info;

    bool started = false;
    bool lost = false;

    void Start() {
        robot.GetComponent<Health>().onDeath.AddListener(() => {
            IEnumerator Helper() {
                yield return new WaitForSeconds(0.5f);
                onLose.Invoke();
                Time.timeScale = 0f;
                info.Show("<color=red>you lost</color>\npress (start)/[escape]\nto try again");
                lost = true;
            }
            StartCoroutine(Helper());
        });
    }

    bool paused;
    void Update() {
        if (!started) {
            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel")) {
                started = true;
                onStart.Invoke();
                robot.enabled = true;
                player.enabled = true;
                player.transform.position = new Vector3(0, player.transform.position.y, 0);
                enemyRobot.gameObject.SetActive(true);
                enemyPlayer.SetActive(true);

                mainMenuCamera.SetActive(false);
                gameCamera.SetActive(true);
            }
        } else {
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
}
