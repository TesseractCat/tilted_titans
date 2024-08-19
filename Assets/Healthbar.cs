using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    RectTransform bar;
    Vector2 size;
    void Start() {
        bar = GetComponent<RectTransform>();
        size = bar.sizeDelta;
    }
    public void ApplyDamage(float amount, float health) {
        bar.sizeDelta = new Vector2(size.x * health, size.y);
    }
}
