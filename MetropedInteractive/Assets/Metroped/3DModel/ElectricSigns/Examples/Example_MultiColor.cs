//    Example - Multiple colors


using UnityEngine;
using System;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_MultiColor : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;
		private DisplayModel displayModel;

		private bool runUpdate;

		private float sine;
		private int saw;
		private float linesUpdate;

		void Start() {

			// Get controller & displaymodel

			controller = dotMatrix.GetController();
			displayModel = dotMatrix.GetDisplayModel();

			controller.DefaultTextFont = TextCommand.Fonts.Normal;
			controller.DefaultSpeedDotsPerSecond = 20f;

			// Some colorful texts

			TextCommand text;

			text = new TextCommand("Yellow") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Center,
				TextColor = 4 // Color 4 is defined as yellow for this display in Unity editor inspector
			};
			controller.AddCommand(text);

			text = new TextCommand("Blue") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Center,
				TextColor = 3 // Color 3 is defined as blue for this display in Unity editor inspector
			};
			controller.AddCommand(text);

			text = new TextCommand("Green") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Center,
				TextColor = 2 // Color 2 is defined as green for this display in Unity editor inspector
			};
			controller.AddCommand(text);

			text = new TextCommand("Red") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Center,
				TextColor = 1 // Color 1 is defined as red for this display in Unity editor inspector
			};
			controller.AddCommand(text);

			text = new TextCommand(" Inverse Green ") {
				Movement = TextCommand.Movements.MoveLeftAndPass,
				TextColor = 0,
				BackColor = 2
			};
			controller.AddCommand(text);

			controller.AddCommand(new TextCommand("R") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Right,
				TextColor = 1
			});
			controller.AddCommand(new TextCommand("G") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Right,
				TextColor = 2
			});
			controller.AddCommand(new TextCommand("B") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Right,
				TextColor = 3
			});

			// Scroll away

			controller.AddCommand(new ClearCommand() {
				Method = ClearCommand.Methods.MoveLeft
			});

			// Notify after done

			linesUpdate = 0f;
			runUpdate = false;

			controller.AddCommand(new CallbackCommand(new Action(delegate () {
				runUpdate = true;
			})));

		}

		void Update() {
			if (runUpdate) {
				linesUpdate -= Time.deltaTime;
				if (linesUpdate<=0f) {
					displayModel.PushLeft();
					sine += 0.11f;
					displayModel.SetDot(displayModel.Width-1, (int)((Mathf.Sin(sine)+1f)*0.49f*displayModel.Height), 4);
					saw = (saw+1) % ((displayModel.Height-1)*2);
					displayModel.SetDot(displayModel.Width-1, (saw<displayModel.Height ? saw : (displayModel.Height-1)*2-saw), 3);
					linesUpdate = 0.02f;
				}
			}
		}

	}

}
