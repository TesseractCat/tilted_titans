using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UnityEvent onStart = new();
    public UnityEvent onLose = new();

    [Header("References")]
    public GameObject enemyPrefab;
    public Transform enemySpawnPoint;
    public Robot robot;
    public UserInput player;
    public GameObject mainMenuCamera;
    public GameObject gameCamera;
    public InfoPanel info;

    bool started = false;
    bool lost = false;

    void Start() {
        robot.GetComponent<Health>().onDeath.AddListener(() => {
            IEnumerator Helper() {
                yield return new WaitForSeconds(0.5f);
                foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>())
                    audioSource.Stop();
                onLose.Invoke();
                Time.timeScale = 0f;
                if (enemiesKilled == 0) {
                    info.Show("<color=red>you lost</color>\npress (start)/[return]\nto try again");
                } else if (enemiesKilled == 1) {
                    info.Show("<color=green>you defeated your opponent</color>\nbut there were more robots to fight!\npress (start)/[return]\nto keep going");
                } else {
                    info.Show($"<color=#90D5FF>you defeated {enemiesKilled} robots</color>\nbut the mecha horde seems endless...\npress (start)/[return]\nto re-enter the fray");
                }
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
                Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);

                mainMenuCamera.SetActive(false);
                gameCamera.SetActive(true);
            }
        } else if (!lost) {
            if (Input.GetButtonDown("Cancel")) {
                paused = !paused;
                if (paused) {
                    info.Show("paused");
                } else {
                    info.Hide();
                }
                Time.timeScale = paused ? 0f : 1f;
            }
        } else {
            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel")) {
                Time.timeScale = 1f;
                SceneManager.LoadScene("Main");
            }
        }
    }

    int enemiesKilled = 0;
    public void EnemyKilled() {
        enemiesKilled++;
        Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
    }
}
