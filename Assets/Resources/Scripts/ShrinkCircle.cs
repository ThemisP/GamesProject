using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkCircle : MonoBehaviour
{
    public static ShrinkCircle circleAccess;
    [Range(0, 360)]
	public int Segments;
    [Range(0, 5000)]
    public float Radius;
    public float SpeedShrinking;
    public GameObject Circle;
    bool Shrinking;
    public List<float> PauseDurations = new List<float> {5.0f, 5.0f, 5.0f, 5.0f};
    public List<float> Stages = new List<float> {0.7f, 0.4f, 0.2f, 0.0f};
    public int currentStageIndex = 0;

    #region Private Members
    private float startingRadius;
	private RenderCircleLine circle;
	private LineRenderer renderer;
    private float startPauseTime;
    private float initialRadius;
    private bool workingCircle;
    // private float startShrinkingTime;
	#endregion

    void Start()
    {
        circleAccess = this;
        workingCircle = false;
        startingRadius = Radius;
        renderer = gameObject.GetComponent<LineRenderer>();
        circle = new RenderCircleLine(ref renderer, Segments, Radius, Radius);
        Circle = GameObject.FindGameObjectWithTag("Circle");
        startPauseTime = Time.time;
        initialRadius = Radius;
        Circle.transform.localScale = new Vector3 (Radius, 1, Radius);
    }

    void Update ()
    {
        if (workingCircle) {
            if (!Shrinking) {
                if ((Time.time - startPauseTime) >= PauseDurations[currentStageIndex]) {
                    Shrinking = true;
                }
            } else {
                if (Radius <= initialRadius * Stages[currentStageIndex]) {
                    Shrinking = false;
                    startPauseTime = Time.time;
                    if ((currentStageIndex + 1) < Stages.Count)
                        currentStageIndex++;
                }
            }


            if (Shrinking) {
                Radius = Mathf.Lerp(Radius, 0, Time.deltaTime * SpeedShrinking);
                circle.Draw(Segments, Radius, Radius);
                Circle.transform.localScale = new Vector3(Radius, 1, Radius);
            }
        }
    }

    public void StartCircle() {
        currentStageIndex = 0;
        Radius = startingRadius;
        workingCircle = true;
    }

    public void StopCircle() {
        workingCircle = false;
    }

    public bool isInCircle(Transform obj)
    {
        if (!workingCircle) return true;
        float dist = Mathf.Sqrt(Mathf.Pow(transform.position.x - obj.position.x, 2) + Mathf.Pow(transform.position.z - obj.position.z, 2));
        //float dist = Vector3.Distance(transform.position, obj.position);
        if (dist < Radius) {
            return true;
        }
        return false;
    }
}