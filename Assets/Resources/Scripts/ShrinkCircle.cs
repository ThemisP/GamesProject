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
    public bool Shrinking;

    #region Private Members
	private RenderCircleLine circle;
	private LineRenderer renderer;
	#endregion

    void Start()
    {   
        renderer = gameObject.GetComponent<LineRenderer>();
        circle = new RenderCircleLine(ref renderer, Segments, Radius, Radius);
        Circle = GameObject.FindGameObjectWithTag("Circle");
    }

    void Update ()
    {
        if(Time.time > 2f)
        {
            Shrinking = true;
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