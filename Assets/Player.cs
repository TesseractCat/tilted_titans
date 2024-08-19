using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    public float slideAmount;
    public Vector2 joltAmount;
    public bool showTooltips;

    [System.NonSerialized]
    public Vector2 platformVelocity;

    [Header("References")]
    public GameObject warningPanel;
    public RectTransform tooltip;
    public Transform shadow;
    public Transform model;

    [Header("Events")]
    public UnityEvent onJump = new();
    public UnityEvent onLand = new();

    public void Jump() {
        if (onGround && Time.time - jumpSquatStart > jumpSquatTime) {
            onJump.Invoke();
            velocity.y = jumpHeight;
            transform.position = transform.position + new Vector3(0f, 0.1f, 0f);
            onGround = false;
            if (onPlatform)
                airVelocity = platformVelocity;
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
        if (onPlatform)
            airVelocity += platformVelocity;
        onGround = false;
    }
    public void Submit() {
        if (interactable) {
            interactable.GetComponent<Interactable>().onInteract.Invoke();
        }
    }

    void Start() {
        shadow.parent = null;
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
    float offmechTime = 0;
    void FixedUpdate() {
        if (warningPanel) {
            if (!OverPlatform()) {
                offmechTime += Time.fixedDeltaTime;
                if (offmechTime > 1f) {
                    warningPanel.SetActive(true);
                }
            } else {
                warningPanel.SetActive(false);
                offmechTime = 0f;
            }
        }
        float targetSpeed = Mathf.Clamp01(dir.magnitude) * speedMultiplier;
        Vector2 targetDir = dir.normalized;

        // Facing

        if (Vector2.Dot(facingDir, targetDir) < 0 && speed < 0.1f) { // >90deg difference
            facingDir = targetDir;
        } else {
            facingDir = Vector2.Lerp(facingDir, targetDir, Time.fixedDeltaTime * turnSpeed);
        }

        if (facingDir.magnitude > 0.05f)
            model.rotation = Quaternion.LookRotation(new Vector3(facingDir.x, 0f, facingDir.y), Vector3.up);

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

        // Apply velocity
        transform.position += velocity * Time.fixedDeltaTime;
        if (!onGround) {
            transform.position += new Vector3(airVelocity.x, 0f, airVelocity.y) * Time.fixedDeltaTime;
        }
        if (onPlatform) {
            transform.position += new Vector3(platformVelocity.x, 0f, platformVelocity.y) * Time.fixedDeltaTime * slideAmount;
        }

        // Ground collision
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position + new Vector3(0, 1, 0), Vector3.down), out hit, 15f, ~0, QueryTriggerInteraction.Ignore)) {
            // Snap harder to the ground if we haven't jumped
            // ...goal is to avoid annoying stepping/falling effect while the platform tilts
            if ((!onGround && hit.distance <= 1.05f) || (onGround && hit.distance <= 1.15f)) {
                transform.position = hit.point;
                velocity.y = 0f;
                if (!onGround) {
                    jumpSquatStart = Time.time;
                    onLand.Invoke();
                }
                onGround = true;
                onPlatform = hit.collider.gameObject.tag == "Platform";
                airVelocity = Vector2.zero;
            } else {
                onGround = false;
                onPlatform = false;
            }

            shadow.gameObject.SetActive(true);
            shadow.position = hit.point + hit.normal * 0.05f;
            shadow.rotation = Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.right), hit.normal);
        } else {
            shadow.gameObject.SetActive(false);
        }
    }

    public bool OnPlatform() {
        return this.onPlatform;
    }
    public bool OverPlatform() {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position + new Vector3(0, 1, 0), Vector3.down), out hit, 15f, ~0, QueryTriggerInteraction.Ignore)) {
            if (hit.collider.tag == "Platform")
                return true;
        }
        return false;
    }

    Collider interactable;
    void OnTriggerEnter(Collider c) {
        if (c.GetComponent<Interactable>()) {
            interactable = c;
            if (showTooltips) {
                tooltip.GetComponentInChildren<TMP_Text>().text =
                    $"{interactable.GetComponent<Interactable>().interactText}\n(A)/[Ret]";
                tooltip.gameObject.SetActive(true);
            }
        }
    }
    void OnTriggerStay(Collider c) {
        if (c == interactable) {
            if (showTooltips)
                tooltip.anchoredPosition = Camera.main.WorldToViewportPoint(c.transform.position + Vector3.up * 1.5f) * new Vector2(800, 600);
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
