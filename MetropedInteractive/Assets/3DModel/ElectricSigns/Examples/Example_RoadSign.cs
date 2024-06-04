//    Example - Forever looping roadsign


using UnityEngine;
using System;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_RoadSign : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		void Start() {

			// Simple arrows, using 2 colors (1 and 2) in addition to background color (0)

			int[,] arrowContent = new int[,] {
				{1,1,1,0,0,0,2,2,2,0,0,0},
				{0,1,1,1,0,0,0,2,2,2,0,0},
				{0,0,1,1,1,0,0,0,2,2,2,0},
				{0,0,0,1,1,1,0,0,0,2,2,2},
				{0,0,1,1,1,0,0,0,2,2,2,0},
				{0,1,1,1,0,0,0,2,2,2,0,0},
				{1,1,1,0,0,0,2,2,2,0,0,0}
			};

			// Add arrows to display

			Controller controller = dotMatrix.GetController();

			controller.AddCommand(new ContentCommand(arrowContent));

			// Cycle all the content on display to right, any dots on rightmost column will appear on leftmost column of display

			DisplayModel displayModel = dotMatrix.GetDisplayModel();

			controller.AddCommand(new CallbackCommand(new Action(delegate () {
				displayModel.CycleRight();
			})) {
				Repeat = true
			});

			// Take a short break

			controller.AddCommand(new PauseCommand(0.2f) {
				Repeat = true
			});

			// Two last commands repeats forever, we are done here

		}

	}

}
