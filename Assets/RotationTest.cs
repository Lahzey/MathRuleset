using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour {

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 direction;
    [SerializeField] private Vector3 directionAlt;
    [SerializeField] private Vector3 rotation;
    private Vector3 newDirection;
    private Vector3 newDirectionAlt;

    private void OnValidate() {
        newDirection = Quaternion.Euler(rotation) * direction;
        newDirectionAlt = Quaternion.Euler(rotation) * directionAlt;
    }

    private void Update() {
        Debug.DrawLine(position, position + direction.normalized * 10f, Color.red);
        Debug.DrawLine(position, position + newDirection.normalized * 10f, Color.green);
        Debug.DrawLine(position, position + directionAlt.normalized * 10f, new Color(1, 0, .5f));
        Debug.DrawLine(position, position + newDirectionAlt.normalized * 10f, new Color(0, 1, .5f));
    }
}
