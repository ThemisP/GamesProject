using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkCircle : MonoBehaviour
{   
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
	private RenderCircleLine circle;
	private LineRenderer renderer;
    private float startPauseTime;
    private float initialRadius;
    // private float startShrinkingTime;
	#endregion

    void Start()
    {   
        renderer = gameObject.GetComponent<LineRenderer>();
        circle = new RenderCircleLine(ref renderer, Segments, Radius, Radius);
        Circle = GameObject.FindGameObjectWithTag("Circle");
        startPauseTime = Time.time;
        initialRadius = Radius;
        Circle.transform.localScale = new Vector3 (Radius, 1, Radius);
    }

    void Update ()
    {
        if(!Shrinking)
        {
            if((Time.time - startPauseTime) >= PauseDurations[currentStageIndex])
            {
                Shrinking = true;
            }
        }
        else
        {
            if(Radius <= initialRadius * Stages[currentStageIndex])
            {
                Shrinking = false;
                startPauseTime = Time.time;
                if((currentStageIndex + 1) < Stages.Count)
                    currentStageIndex++;
            }
        }


        if(Shrinking)
        {
            Radius = Mathf.Lerp(Radius, 0, Time.deltaTime * SpeedShrinking);
            circle.Draw(Segments, Radius, Radius);
            Circle.transform.localScale = new Vector3 (Radius, 1, Radius);
        }
    }

    public bool isInCircle(Transform obj)
    {
        float dist = Vector3.Distance(transform.position, obj.position);
        if(dist < Radius)
            return true;
        else return false;
    }
}