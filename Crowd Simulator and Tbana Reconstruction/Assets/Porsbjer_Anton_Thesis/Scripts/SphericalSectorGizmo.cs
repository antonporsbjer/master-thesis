using UnityEngine;

[ExecuteAlways]
public class SphericalSectorGizmo : MonoBehaviour
{
    public Vector3 axis = Vector3.forward;
    [Range(0f, 180f)] public float angle = 60f; // full angle in degrees
    public float radius = 5f;
    public int circleSteps = 36;
    public Color gizmoColor = Color.cyan;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Vector3 p = transform.position;
        Vector3 n = axis.normalized;
        float halfRad = Mathf.Deg2Rad * angle * 0.5f;
        float cosHalf = Mathf.Cos(halfRad);

        // Draw sphere boundary
        Gizmos.DrawWireSphere(p, radius);

        // Build orthonormal basis u,v perpendicular to n
        Vector3 u = Vector3.Cross(n, Vector3.up);
        if (u.sqrMagnitude < 1e-6f) u = Vector3.Cross(n, Vector3.right);
        u.Normalize();
        Vector3 v = Vector3.Cross(n, u);

        // draw cone rim (circle at angle on sphere)
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= circleSteps; i++)
        {
            float theta = (i / (float)circleSteps) * Mathf.PI * 2f;
            // direction: rotate from axis by half-angle
            // point direction = cos(half)*n + sin(half)*(cos(theta)*u + sin(theta)*v)
            Vector3 dir = cosHalf * n + Mathf.Sin(halfRad) * (Mathf.Cos(theta) * u + Mathf.Sin(theta) * v);
            Vector3 rim = p + dir * radius;
            if (i > 0)
                Gizmos.DrawLine(prev, rim);
            prev = rim;

            // draw lines from apex to rim (sparse)
            if (i % (circleSteps / 8 == 0 ? 1 : circleSteps / 8) == 0)
                Gizmos.DrawLine(p, rim);
        }
    }
}
