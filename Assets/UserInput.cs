using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class UserInput : MonoBehaviour
{
    Player player;
    void Start() {
        player = GetComponent<Player>();
    }

    void Update() {
        if (Input.GetButtonDown("Jump")) {
            player.Jump();
        }
        if (Input.GetButtonDown("Submit")) {
            player.Submit();
        }
        player.Move(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
    }
}
