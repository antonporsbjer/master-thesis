//    Example - 5x5 rotating line using direct DisplayModel access


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_5x5 : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private DisplayModel displayModel;

		private int state;
		private float runner;

		private float rotation;

		void Start() {
			// This example doesn't use DotMatrix Controller at all, but instead push data directly to DisplayModel
			displayModel = dotMatrix.GetDisplayModel();
			// Run example
			state = 0;
			runner = 0f;
			rotation = 0f;
		}

		void Update() {

			runner -= Time.deltaTime;
			if (runner<=0f) {
				state = (state+1)%4;
				if (state==0) {
					// Fill middle row
					displayModel.Clear();
					displayModel.FillRow(2);
				} else if (state==1) {
					// One way to set data (dot by dot)
					displayModel.Clear();
					for (int n = 0; n<5; n++) {
						displayModel.SetDot(n, n, true);
					}
				} else if (state==2) {
					// Fill center column
					displayModel.Clear();
					displayModel.FillColumn(2);
				} else if (state==3) {
					// Another way to set data (set all dots at once)
					displayModel.SetFullContent(new int[,]{
						{0,0,0,0,1},
						{0,0,0,1,0},
						{0,0,1,0,0},
						{0,1,0,0,0},
						{1,0,0,0,0}});
				}
				runner += 0.5f;
			}

			rotation = (rotation+Time.deltaTime*0.2f)%Mathf.PI;
			this.transform.parent.transform.localRotation = Quaternion.Euler(new Vector3(0f, (1f+Mathf.Cos(rotation))*180f, 0f));

		}

	}

}
