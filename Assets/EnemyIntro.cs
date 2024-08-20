using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyIntro : MonoBehaviour
{
    public UnityEvent onIntroDone = new();
    public float riseSpeed;
    public Vector3 startOffset;

    Vector3 startPos;
    Vector3 endPos;
    float startTime;
    void Start() {
        startPos = transform.position + startOffset;
        endPos = transform.position;
        startTime = Time.time;
    }

    void Update() {
        float t = (Time.time - startTime) * riseSpeed;
        if (t < 1f) {
            transform.position = Vector3.Lerp(startPos, endPos, t);
        } else {
            transform.position = endPos;
            onIntroDone.Invoke();
            this.enabled = false;
        }
    }
}
