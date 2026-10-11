using UnityEngine;
using System.Collections;

public class CustomNodeLined : CustomNode {

	public override Vector3 getTargetPoint(Vector3 origin, int agentID = 0) {

		Vector3 A = transform.TransformPoint(new Vector3 (0.5f, 0, 0));
		Vector3 B = transform.TransformPoint (new Vector3 (-0.5f, 0, 0));
		Vector3 AB = B - A;
		float dis = Vector3.Dot ((origin - A), AB) / AB.sqrMagnitude;
		float t;
		if (dis < 0f || dis > 1f) {
			// Evenly distribute agents approaching from outside the line bounds across the inner 80%
			float hash = agentID != 0 ? Mathf.Abs(Mathf.Sin(agentID * 12.9898f)) : Random.value;
			t = Mathf.Lerp(0.1f, 0.9f, hash);
		} else {
			t = Mathf.Clamp(dis, 0.05f, 0.95f);
		}
		return A + AB * t;
	}
}
