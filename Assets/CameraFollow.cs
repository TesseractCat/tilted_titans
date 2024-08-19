using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform follow;
    public float followSpeed;

    Vector3 offset;
    Vector3 pos;
    void Start() {
        offset = transform.position - follow.position;
        pos = transform.position;
    }

    void FixedUpdate() {
        pos = Vector3.Lerp(
            pos, follow.position + offset,
            Time.fixedDeltaTime * followSpeed
        );
        if (screenshakeTime > 0f) {
            transform.position = pos + Random.insideUnitSphere * screenshakeAmount * (screenshakeTime/screenshakeDuration);
            screenshakeTime -= Time.fixedDeltaTime;
        } else {
            transform.position = pos;
        }
    }
    public float screenshakeAmount;
    public float screenshakeDuration;
    float screenshakeTime;
    public void Screenshake() {
        screenshakeTime = screenshakeDuration;
    }
}
