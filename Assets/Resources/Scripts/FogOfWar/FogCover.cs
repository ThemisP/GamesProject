using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogCover : MonoBehaviour
{
    Renderer renderer;


    void Start()
    {
        renderer = GetComponent<Renderer>();
        FieldOfView.OnEnemiesVisibilityChange += FieldOfViewOnEnemiesVisibilityChange;
    }

    void OnDestroy()
    {
        FieldOfView.OnEnemiesVisibilityChange -= FieldOfViewOnEnemiesVisibilityChange;
    }

    void FieldOfViewOnEnemiesVisibilityChange(List<Transform> newEnemies)
    {
        renderer.enabled = newEnemies.Contains(transform);
    }
}