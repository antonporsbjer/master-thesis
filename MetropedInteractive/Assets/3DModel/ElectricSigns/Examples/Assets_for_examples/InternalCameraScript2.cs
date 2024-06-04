//    Internal Camera Script for Examples_3D_2 scene


using UnityEngine;

namespace Leguar.DotMatrix.Example {

	public class InternalCameraScript2 : MonoBehaviour {

		public Vector3[] positions;
		public Quaternion[] rotations;

		private int currentIndex;

		void Start() {
			currentIndex=0;
		}

		public void buttonClicked() {
			currentIndex = (currentIndex+1)%positions.Length;
			this.transform.localPosition=positions[currentIndex];
			this.transform.localRotation=rotations[currentIndex];
		}

	}

}
