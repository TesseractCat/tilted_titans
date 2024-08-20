using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent onLose = new();

    [Header("References")]
    public Robot robot;
    public Robot enemyRobot;
    public InfoPanel info;

    void Start() {
        robot.GetComponent<Health>().onDeath.AddListener(() => {
            IEnumerator Helper() {
                yield return new WaitForSeconds(0.5f);
                onLose.Invoke();
                Time.timeScale = 0f;
                info.Show("<color=red>you lost</color>\npress (start)/[escape]\nto try again");
            }
            StartCoroutine(Helper());
        });
    }
}
