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
        Reentering,
        Taunting
    }

    Player player;
    State state;
    float maxDistance = 1f;
    void Start() {
        state = State.Ramming;
        player = GetComponent<Player>();
        robot.GetComponent<Robot>().onRobotCollide.AddListener(OnRobotCollide);
        robot.GetComponent<Health>().onDeath.AddListener(OnRobotDeath);
    }

    void FixedUpdate() {
        Vector2 targetPoint = Vector2.zero;
        Vector2 centerPoint = robot.position.xz();
        Vector2 playerPoint = player.transform.position.xz();

        if (!player.OverPlatform()) state = State.Reentering;

        if (state == State.Following) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 20f;
            maxDistance = 1f;
        } else if (state == State.Ramming) {
            targetPoint = enemyRobot.position.xz();
            maxDistance = 3f;
        } else if (state == State.PunchPositioning) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 15f + Vector2.right * 7.5f;
            maxDistance = 3f;
            if ((targetPoint - centerPoint).magnitude < 2f) {
                state = State.Punching;
            }
        } else if (state == State.Punching) {
            targetPoint = centerPoint + Vector2.left * 3.5f;
            maxDistance = 3.5f;
            if ((targetPoint - playerPoint).magnitude < 0.2f) {
                state = State.PunchPositioning;
                player.Submit();
            }
        } else if (state == State.Reentering) {
            if (player.OverPlatform()) state = !dead ? State.Ramming : State.Taunting;
            targetPoint = centerPoint;
            maxDistance = 100f;
            player.Submit();
        } else if (state == State.Taunting) {
            targetPoint = centerPoint;
            maxDistance = 5f;
            player.speedMultiplier *= 2.5f;
            player.platformVelocity = Vector2.zero;

            if ((targetPoint - playerPoint).magnitude < 0.2f) {
                player.Jump();
            }
        }

        // Debug.Log(state);

        Vector2 offset = Vector2.ClampMagnitude(targetPoint - centerPoint, maxDistance);
        player.MoveTowards(robot.position.xz() + offset);
    }

    void OnRobotCollide() {
        if (state == State.Ramming) {
            state = State.PunchPositioning;
        }
    }
    bool dead = false;
    void OnRobotDeath() {
        state = State.Taunting;
        dead = true;
    }
}
