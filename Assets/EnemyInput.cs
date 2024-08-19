using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(Player))]
public class EnemyInput : MonoBehaviour
{
    public Transform robot;
    public Transform enemyRobot;

    enum State {
        Following,
        Ramming,
        PunchPositioning,
        Punching,
        Reentering
    }

    Player player;
    State state;
    float maxSpeed = 1f;
    void Start() {
        state = State.Ramming;
        player = GetComponent<Player>();
        robot.GetComponent<Robot>().onRobotCollide.AddListener(OnRobotCollide);
    }

    void FixedUpdate() {
        Vector2 targetPoint = Vector2.zero;
        Vector2 centerPoint = robot.position.xz();
        Vector2 playerPoint = player.transform.position.xz();

        if (!player.OverPlatform()) state = State.Reentering;

        if (state == State.Following) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 20f;
            maxSpeed = 1f;
        } else if (state == State.Ramming) {
            targetPoint = enemyRobot.position.xz();
            maxSpeed = 3f;
        } else if (state == State.PunchPositioning) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 15f + Vector2.right * 7.5f;
            maxSpeed = 3f;
            if ((targetPoint - centerPoint).magnitude < 2f) {
                state = State.Punching;
            }
        } else if (state == State.Punching) {
            targetPoint = centerPoint + Vector2.left * 3.5f;
            maxSpeed = 3.5f;
            if ((targetPoint - playerPoint).magnitude < 0.2f) {
                state = State.PunchPositioning;
                player.Submit();
            }
        } else if (state == State.Reentering) {
            if (player.OverPlatform()) state = State.Ramming;
            targetPoint = centerPoint;
            maxSpeed = 100f;
            player.Submit();
        }

        // Debug.Log(state);

        Vector2 offset = Vector2.ClampMagnitude(targetPoint - centerPoint, maxSpeed);
        player.MoveTowards(robot.position.xz() + offset);
    }

    void OnRobotCollide() {
        if (state == State.Ramming) {
            state = State.PunchPositioning;
        }
    }
}
