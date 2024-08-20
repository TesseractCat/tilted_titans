using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(Player))]
public class EnemyInput : MonoBehaviour
{
    [Header("Settings")]
    public float avoidanceDist;

    [Header("References")]
    public Transform robot;
    Transform enemyRobot;

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
    bool punchingRight = false;
    void Start() {
        enemyRobot = GameObject.Find("Robot").transform;
        state = State.Ramming;
        player = GetComponent<Player>();
        robot.GetComponent<Robot>().onRobotCollide.AddListener(OnRobotCollide);
        robot.GetComponent<Health>().onDeath.AddListener(OnRobotDeath);
    }

    void FixedUpdate() {
        Vector2 targetPoint = Vector2.zero;
        Vector2 centerPoint = robot.position.xz();
        Vector2 playerPoint = player.transform.position.xz();

        List<Vector2> otherRobotPoints = GameObject.FindGameObjectsWithTag("Robot")
            .Where(r => r.name == "Enemy Robot"  && r != robot.gameObject)
            .Select(r => r.transform.position.xz())
            .ToList();

        if (!player.OverPlatform()) state = State.Reentering;

        if (state == State.Following) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 20f;
            maxDistance = 1f;
        } else if (state == State.Ramming) {
            targetPoint = enemyRobot.position.xz();
            maxDistance = 3f;
        } else if (state == State.PunchPositioning) {
            targetPoint = enemyRobot.position.xz() + Vector2.up * 15f + (punchingRight ? Vector2.right : Vector2.left) * 7.5f;
            maxDistance = 3f;
            if ((targetPoint - centerPoint).magnitude < 3.5f) {
                state = State.Punching;
            }
        } else if (state == State.Punching) {
            targetPoint = centerPoint + (punchingRight ? Vector2.left : Vector2.right) * 3.5f;
            maxDistance = 3.5f;
            if ((targetPoint - playerPoint).magnitude < 0.2f) {
                state = State.PunchPositioning;
                punchingRight = Random.Range(0, 2) == 0;
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

        // Avoid other enemy robots
        if (state != State.Reentering && state != State.Taunting) {
            foreach (Vector2 orp in otherRobotPoints) {
                if (Vector2.Distance(centerPoint, orp) < avoidanceDist) {
                    //offset = Vector2.ClampMagnitude(-(orp - centerPoint), maxDistance);
                    targetPoint += -(orp - centerPoint) * 10f;
                }
            }
        }

        Vector2 offset = Vector2.ClampMagnitude(targetPoint - centerPoint, maxDistance);

        player.MoveTowards(robot.position.xz() + offset);
    }

    void OnRobotCollide() {
        if (state == State.Ramming) {
            state = State.PunchPositioning;
            punchingRight = Random.Range(0, 2) == 0;
        }
    }
    bool dead = false;
    void OnRobotDeath() {
        state = State.Taunting;
        dead = true;
        FindObjectOfType<GameManager>().EnemyKilled();
    }
}
