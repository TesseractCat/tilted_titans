using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float speedMultiplier;
    public float brakingMultiplier;
    public float acceleration;
    public float deceleration;
    public float turnSpeed;
    public float jumpHeight;
    public float jumpSquatTime;
    public float gravity;
    public float airDrag;
    public Vector2 joltAmount;
    public bool showTooltips;

    [System.NonSerialized]
    public Vector2 platformVelocity;

    [Header("References")]
    public RectTransform tooltip;

    public void Jump() {
        if (onGround && Time.time - jumpSquatStart > jumpSquatTime) {
            velocity.y = jumpHeight;
            transform.position = transform.position + new Vector3(0f, 0.1f, 0f);
            onGround = false;
            // if (onPlatform) {
            //     velocity += new Vector3(platformVelocity.x, 0f, platformVelocity.y);
            // }
            onPlatform = false;
        }
    }

    public void Move(Vector2 dir) {
        this.dir = dir;
    }
    public void MoveTowards(Vector2 point) {
        this.dir = -new Vector2(
            transform.position.x - point.x,
            transform.position.z - point.y
        );
    }
    public void Jolt(Vector2 joltDir) {
        transform.position = transform.position + new Vector3(0f, 0.1f, 0f);
        velocity.y = joltAmount.y;
        airVelocity = joltDir.normalized * joltAmount.x;
        onGround = false;
    }
    public void Submit() {
        if (interactable) {
            interactable.GetComponent<Interactable>().onInteract.Invoke();
        }
    }

    [System.NonSerialized]
    public Vector2 dir = Vector2.zero;
    Vector3 velocity = Vector3.zero;
    Vector2 airVelocity = Vector2.zero;
    Vector2 facingDir = Vector3.right;
    float jumpSquatStart = 0f;
    bool onGround = true;
    bool onPlatform = true;
    float speed = 0f;
    void FixedUpdate() {
        float targetSpeed = dir.magnitude * speedMultiplier;
        Vector2 targetDir = dir.normalized;

        // Facing
        facingDir = Vector2.Lerp(facingDir, targetDir, Time.fixedDeltaTime * turnSpeed);

        // Movement
        if (dir.magnitude < 0.05f) { // Braking
            speed *= brakingMultiplier;
        } else {
            if (targetSpeed > speed) { // Accelerate
                speed = Mathf.Min(speed + acceleration, targetSpeed);
            } else { // Decelerate
                speed = Mathf.Max(speed - deceleration, targetSpeed);
            }
        }

        velocity.x = speed * facingDir.x;
        velocity.z = speed * facingDir.y;

        // Gravity
        velocity -= new Vector3(0f, gravity, 0f) * Time.fixedDeltaTime;

        // Friction/drag
        airVelocity *= airDrag;

        // Ground collision
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position + new Vector3(0, 1, 0), Vector3.down), out hit, 2f, ~0, QueryTriggerInteraction.Ignore)) {
            if (hit.distance <= 1.05f) {
                transform.position = hit.point;
                velocity.y = 0f;
                if (!onGround)
                    jumpSquatStart = Time.time;
                onGround = true;
                onPlatform = hit.collider.gameObject.tag == "Platform";
                airVelocity = Vector2.zero;
            } else {
                onGround = false;
                onPlatform = false;
            }
        }

        // Apply velocity
        transform.position += velocity * Time.fixedDeltaTime;
        if (!onGround) {
            transform.position += new Vector3(airVelocity.x, 0f, airVelocity.y) * Time.fixedDeltaTime;
        }
        if (true) {
            transform.position += new Vector3(platformVelocity.x, 0f, platformVelocity.y);
        }
    }

    public bool OnPlatform() {
        return this.onPlatform;
    }

    Collider interactable;
    void OnTriggerEnter(Collider c) {
        if (c.GetComponent<Interactable>()) {
            interactable = c;
            if (showTooltips)
                tooltip.gameObject.SetActive(true);
        }
    }
    void OnTriggerStay(Collider c) {
        if (c == interactable) {
            if (showTooltips)
                tooltip.anchoredPosition = Camera.main.WorldToScreenPoint(c.transform.position + Vector3.up * 1.5f);
        }
    }
    void OnTriggerExit(Collider c) {
        if (c == interactable) {
            if (showTooltips)
                tooltip.gameObject.SetActive(false);
            interactable = null;
        }
    }
}
