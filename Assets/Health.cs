using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent<float, float> onDamageTaken;
    public UnityEvent onDeath;
    public UnityEvent onHalfHealth;

    float health = 1f;
    bool dead = false;
    public void Damage(float amount) {
        health -= amount;
        health = Mathf.Clamp01(health);
        if (health > 0f) {
            onDamageTaken.Invoke(amount, health);
            if (health <= 0.5f && health + amount > 0.5f) {
                onHalfHealth.Invoke();
            }
        } else {
            onDamageTaken.Invoke(amount, health);
            if (!dead) {
                dead = true;
                onDeath.Invoke();
            }
        }
    }
    public void Heal(float amount) {
        health += amount;
        health = Mathf.Clamp01(health);
    }
}
