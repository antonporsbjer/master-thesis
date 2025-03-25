//    Internal Camera Script for Examples_3D_1 scene


using UnityEngine;

namespace Leguar.DotMatrix.Example {

	public class InternalCameraScript1 : MonoBehaviour {

		public Vector3 cubePos;
		public Vector3 cubeRot;
		public Vector3 discPos;
		public Vector3 discRot;

		private Vector3 startPos;
		private Vector3 startRot;

		private float counter;

		void Start() {
			startPos = this.transform.localPosition;
			startRot = this.transform.localRotation.eulerAngles;
			counter = -20f;
		}

		void Update() {
			counter += Time.deltaTime;
			if (counter<0f || counter>40f) {
				return;
			}
			if (counter<15f) {
				float t = Mathf.Clamp01(counter<=5f ? counter*0.5f : (10f-counter)*0.5f);
				this.transform.localPosition = Vector3.Lerp(startPos, cubePos, t);
				this.transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRot, cubeRot, t));
			} else {
				float c = counter-15f;
				float t = Mathf.Clamp01(c<=5f ? c*0.5f : (15f-c)*0.5f);
				this.transform.localPosition = Vector3.Lerp(startPos, discPos, t);
				this.transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRot, discRot, t));
			}
		}

	}

}
