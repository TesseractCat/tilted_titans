using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Robot robot;
    public Robot enemyRobot;
    public InfoPanel info;

    void Start() {
        robot.GetComponent<Health>().onDeath.AddListener(() => {
            Time.timeScale = 0f;
            info.Show("<color=red>you lost</color>\npress (start)/[escape]\nto try again");
        });
    }
}
