using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Robot : MonoBehaviour
{
    [Header("Settings")]
    public float tiltSpeed;
    public float movementSpeedModifier;
    public float maxTilt;
    public float deadzone;
    public float cardinalBias;
    public float collisionJoltModifier;
    public float collisionKnockback;

    public float collisionDamage;
    public float punchDamage;

    [Header("References")]
    public Transform platform;
    public Transform player;

    [Header("Events")]
    public UnityEvent onRobotCollide = new();

    Quaternion platformStartRot;
    Health health;
    Animator animator;
    void Start() {
        platformStartRot = platform.localRotation;
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
    }

    Vector2 tiltAmount;
    Vector2 velocity;
    void FixedUpdate() {
        Vector2 diff = Vector2.zero;
        // Vector2 playerDiff = new Vector2(transform.position.x, transform.position.z)
        //     - new Vector2(player.transform.position.x, player.transform.position.z);
        Vector3 itp = transform.InverseTransformPoint(player.transform.position);
        Vector2 playerDiff = new Vector2(itp.x, itp.z);
        if (!player.GetComponent<Player>().OnPlatform()) playerDiff = Vector2.zero;

        diff -= playerDiff;
        if (diff.magnitude < deadzone) {
            diff = Vector2.zero;
        }
        tiltAmount = Vector2.Lerp(tiltAmount, diff, tiltSpeed);
        tiltAmount = Vector3.ClampMagnitude(tiltAmount, maxTilt);

        platform.localRotation = Quaternion.FromToRotation(
            Vector3.up,
            Vector3.up - new Vector3(-tiltAmount.y, 0, tiltAmount.x)
        ) * platformStartRot;

        Vector2 movement = Quaternion.Euler(0,0,transform.rotation.eulerAngles.y) * BiasToCardinal(-tiltAmount * movementSpeedModifier, cardinalBias);
        transform.position += new Vector3(movement.x, 0f, movement.y);
        transform.position += new Vector3(velocity.x, 0f, velocity.y);
        velocity *= 0.75f;
        player.GetComponent<Player>().platformVelocity = movement/Time.fixedDeltaTime;
    }

    Vector2 BiasToCardinal(Vector2 vec, float biasStrength) {
        Vector2 n = vec.normalized;

        Vector2[] dirs = {
            Vector2.up, Vector2.down, Vector2.right, Vector2.left,
            Vector2.up + Vector2.right, Vector2.right + Vector2.down,
            Vector2.down + Vector2.left, Vector2.left + Vector2.up
        };

        int maxIdx = dirs.Select((dir, idx) => (Vector2.Dot(n, dir.normalized), idx)).Max().idx;

        return Vector2.Lerp(n, dirs[maxIdx], biasStrength) * vec.magnitude;
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.tag == "Robot") {
            Vector2 collisionDir = (transform.position.xz() - c.transform.position.xz()).normalized;
            this.Hit(collisionDir, collisionDamage);
        }
    }

    public void Hit(Vector2 dir, float damage) {
        velocity = dir * collisionKnockback;
        if (player.GetComponent<Player>().OverPlatform())
            player.GetComponent<Player>().Jolt(dir * collisionJoltModifier);
        onRobotCollide.Invoke();
        health.Damage(damage);
    }

    public void PunchRight() {
        IEnumerator Helper() {
            animator.SetTrigger("PunchRight");
            yield return new WaitForSeconds(0.4f);
            Ray punchRay = new Ray(transform.position + Vector3.up * 6f, transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(punchRay, out hit, 25f)) {
                if (hit.collider.tag == "Robot") {
                    hit.collider.GetComponent<Robot>().Hit(transform.forward.xz(), punchDamage);
                }
            }
        }
        StartCoroutine(Helper());
    }
}
