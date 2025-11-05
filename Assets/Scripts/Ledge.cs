using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class Ledge : MonoBehaviour
{
    [SerializeField] Transform leftBound, rightBound;
    [SerializeField] float width, height;
    public float offset;

    Vector3 center, direction, boxSize;
    Vector3 DEBUG_GRAB_POINT = Vector3.zero;
    BoxCollider boxCollider;
    Quaternion rotation;

    public Vector3 GetLeftEdge() => leftBound.position;
    public Vector3 GetRightEdge() => rightBound.position;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            CalculateLedge();
        }
    }

    // Find the grab point along the line between the bounds
    // based on the ledgeCheckPont position
    public Vector3 CalculateGrabPosition()
    {
        Vector3 pos = PlayerManager.Instance.ledgeCheckPoint.position;

        Vector3 a = leftBound.position;
        Vector3 b = rightBound.position;

        Vector3 ab = b - a;
        Vector3 ap = pos - a;

        float t = Vector3.Dot(ap, ab.normalized) / ab.magnitude;
        t = Mathf.Clamp01(t);

        Vector3 closestPoint = a + ab * t;
        DEBUG_GRAB_POINT = closestPoint;

        return closestPoint;
    }

    void CalculateLedge()
    {
        if (!leftBound || !rightBound || !boxCollider) return;
        
        // Box Collider uses local scale not world scale vectors
        Vector3 localCenter = (leftBound.localPosition + rightBound.localPosition) / 2f;
        boxCollider.center = localCenter;

        float length = Vector3.Distance(leftBound.localPosition, rightBound.localPosition);
        Vector3 localSize = new Vector3(length, height, width);
        boxCollider.size = localSize;
    }

    void OnDrawGizmos()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            return;
        }

        if (leftBound == null || rightBound == null) return;

        CalculateLedge();

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(leftBound.position, 0.1f);
        Gizmos.DrawSphere(rightBound.position, 0.1f);

        if (DEBUG_GRAB_POINT != Vector3.zero)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawSphere(DEBUG_GRAB_POINT, 0.1f);
        }

        // Draw in world space
        Gizmos.color = Color.green;
        Vector3 worldCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 worldSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, boxCollider.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, worldSize);
    }
}//EndScript