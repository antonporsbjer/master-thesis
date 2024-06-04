//    Example - Scroll HI! and swap color of random dots that are turned on

//    This example is mostly just to show 3D-object based display using different materials as dot states, instead of rotating 3D objects as dots


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_HiWithColorSwap : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;
		private DisplayModel displayModel;

		private float nextColorSwap;

		void Start() {

			controller = dotMatrix.GetController();
			displayModel = dotMatrix.GetDisplayModel();

			controller.AddCommand(new TextCommand("HI!  HI!") {
				Bold = true,
				HorPosition = TextCommand.HorPositions.Left,
				Movement = TextCommand.Movements.MoveLeftAndPass,
				DotsPerSecond = 2f,
				Repeat = true
			});

			nextColorSwap=0f;

		}

		void Update() {

			nextColorSwap-=Time.deltaTime;
			if (nextColorSwap<=0f) {
				for (int n = 0; n<20; n++) { // Try max 20 times
					int x = Random.Range(0, displayModel.Width);
					int y = Random.Range(0, displayModel.Height);
					int currentState = displayModel.GetDotState(x, y);
					if (currentState>0) {
						displayModel.SetDot(x, y, (currentState==1 ? 2 : 1)); // Set dot state between 1 <-> 2
						break;
					}
				}
				nextColorSwap=0.1f; // Do color swapping once per 1/10 seconds
			}

		}

	}

}
