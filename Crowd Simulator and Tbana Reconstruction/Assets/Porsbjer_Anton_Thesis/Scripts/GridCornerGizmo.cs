using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCornerGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        Vector3 basePosition = transform.position;
        Vector3 topPosition = basePosition + Vector3.up * 3f;

        // Draw small sphere at the base
        Gizmos.DrawSphere(basePosition, 0.2f);

        // Draw a 3-unit line upwards
        Gizmos.DrawLine(basePosition, topPosition);

        // Draw small sphere on top
        Gizmos.DrawSphere(topPosition, 0.2f);
    }
}
