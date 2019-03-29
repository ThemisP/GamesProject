using UnityEngine;
using System.Collections;

public class RenderCircleLine
{
	//private members
	private int _segments;
	private float _xradius;
	private float _yradius;
	private LineRenderer _renderer;

	#region Constructors
	public RenderCircleLine(ref LineRenderer renderer, int segments, float xradius, float yradius)
	{
		_renderer = renderer;
		_segments = segments;
		_xradius = xradius;
		_yradius = yradius;
		Draw(segments, _xradius, _yradius);
	}
	#endregion

	public void Draw(int segments, float xradius, float yradius)
	{
		_xradius = xradius;
		_yradius = yradius;
		_renderer.SetVertexCount(segments + 1);
		_renderer.useWorldSpace = false;
		CreatePoints();
	}

	private void CreatePoints ()
	{
		float x;
		float y;
		float z;
		float angle = 20f;

		for (int i = 0; i < (_segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * _xradius;
			z = Mathf.Cos (Mathf.Deg2Rad * angle) * _yradius;

            Vector3 position = new Vector3(x,0,z);
			_renderer.SetPosition (i, position );

			angle += (360f / _segments);
		}
	}
}