using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// public delegate void EnemiesVisibilityChange(List<Transform> newEnemies);
public delegate void EnemyVisibility();

public class VisibleEnemies : MonoBehaviour
{
    public static List<Transform> visibleEnemies = new List<Transform>();
    // public static event EnemiesVisibilityChange OnEnemiesVisibilityChange;
    public static event EnemyVisibility OnEnemiesVisibilityChange;

    // public static void EnemiesVisibilityChange() {
    //     if (OnEnemiesVisibilityChange != null) OnEnemiesVisibilityChange(visibleEnemies);
    // }

    void Start() {
        StartCoroutine("FindEnemiesWithDelay", .2f);
    }

    public static void AddVisibleEnemy(Transform enemy)
    {
        if(!visibleEnemies.Contains(enemy))
        {
            visibleEnemies.Add(enemy);
        }
    }

    IEnumerator FindEnemiesWithDelay(float delay) {
        while(true) {
            yield return new WaitForSeconds(delay);
            VisibleEnemies.visibleEnemies.Clear();
            if (OnEnemiesVisibilityChange != null) OnEnemiesVisibilityChange();
            // FindVisibleEnemies();
        }
    }
}