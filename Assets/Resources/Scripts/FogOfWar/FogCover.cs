using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogCover : MonoBehaviour
{
    //Renderer renderer;
    private Renderer[] renderers;
    private Canvas[] canvas;

    void Start()
    {
        // renderer = GetComponent<Renderer>();
        renderers = GetComponentsInChildren<Renderer>();
        canvas = GetComponentsInChildren<Canvas>();
        // VisibleEnemies.OnEnemiesVisibilityChange += FieldOfViewOnEnemiesVisibilityChange;
    }

    void OnDestroy()
    {
        // VisibleEnemies.OnEnemiesVisibilityChange -= FieldOfViewOnEnemiesVisibilityChange;
    }
    void Update()
    {
        FieldOfViewOnEnemiesVisibilityChange();
    }

    void FieldOfViewOnEnemiesVisibilityChange()
    {   
        // renderer.enabled = VisibleEnemies.visibleEnemies.Contains(transform);
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = VisibleEnemies.visibleEnemies.Contains(transform);
        }

        foreach(Canvas c in canvas)
        {
            c.enabled = VisibleEnemies.visibleEnemies.Contains(transform);
        }
    }
}