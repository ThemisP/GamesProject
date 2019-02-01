using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour {
    public float viewRadius; // How far the player can see
    [Range(0, 360)]
    public float viewAngle;
    public LayerMask enemyMask;
    public LayerMask objectMask;
    public List<Transform> visibleEnemies = new List<Transform>();

    public float meshResolution; // How many rays cast out
    public int edgeResolveIterations; // No of iterations for aproximating the edge of an object
    public float edgeDstThreshold;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    void Start() {
        viewMesh = new Mesh ();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    IEnumerator FindTargetsWithDelay(float delay) {
        while(true) {
            yield return new WaitForSeconds(delay);
            FindVisibleEnemies();
        }
    }

    /* A LateUpdate was used as the field of view 
    should be drawn after any action from the player */
    void LateUpdate() {
        DrawFieldOfView();
    }

    /* Go through all the enemies within the field of view and
    check whether or not their are obstructed by an object. 
    If they are not, they are added to the visibleEnemies list */
    void FindVisibleEnemies() {
        visibleEnemies.Clear();
        // a list with all the enemy objects that are within the view radius
        Collider[] enemiesInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, enemyMask);
        
        for(int i=0; i < enemiesInViewRadius.Length; i++) {
            Transform target = enemiesInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, dirToTarget, dstToTarget, objectMask)) {
                    visibleEnemies.Add(target);
                }
            }
        }
    }

    void DrawFieldOfView() {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution); // number of divisions of the mesh
        float stepAngleSize = viewAngle / stepCount; // size of each division

        List<Vector3> viewPoints = new List<Vector3> (); // points from the Raycasts
        ViewCastInfo oldViewCast = new ViewCastInfo() ;

        // go through all the divisions of the mesh
        for(int i = 0; i <= stepCount; i++) {
            // Raycast on that division
            float angle = transform.eulerAngles.y - viewAngle/2 + i*stepAngleSize;
            ViewCastInfo newViewCast = ViewCast (angle);

            if(i > 0) {
                /* if both raycasts hit an object, if the distance between
                 the hits is large enough, they are considered separate objects */
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector3.zero) {
                        viewPoints.Add(edge.pointA);
                    }
                    if(edge.pointB != Vector3.zero) {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount]; // vertices of the mesh
        int[] triangles = new int[(vertexCount-2)*3]; // the triangles that will form the mesh

        vertices[0] = Vector3.zero; // because mesh will be child of player
        for(int i = 1; i < vertexCount - 1; i++) {
            vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]);
            
            /* the triangles will contain vertices representing the center(player)
             and two consecutive points from the raycasts */
            if(i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /* Finds the edge of an object using a method similar to binary search. 
    For a number of iterations it gets closer and closer to the actual edge*/
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for(int i = 0; i < edgeResolveIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - maxViewCast.dst) > edgeDstThreshold;
            /* if the cast hits the object or the dist threshold is exceeded, 
            then the point becomes the new min, otherwise it becomes the new max */
            if(newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    // method that does a raycast and returns the results in the form of a ViewCastInfo
    ViewCastInfo ViewCast(float globalAngle) {
        Vector3 dir = DirFromAngle (globalAngle, true);
        RaycastHit hit;

        if(Physics.Raycast(transform.position, dir, out hit, viewRadius, objectMask)) {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
            return new ViewCastInfo(false, transform.position + dir * viewRadius, hit.distance, globalAngle);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if(!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    /* Struct representing the data obtained from a Raycast */
    public struct ViewCastInfo {
        public bool hit; // true if it hits an object
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    /* Struct representing the points close to the edge of an object used in the FindEdge method */
    public struct EdgeInfo {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}