//    Example - Blinking clock (showing imaginary gametime following time scale)


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_BlinkingClock : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;

		private float displayZero;

		private float timeInGameWorld;
		private float updateCounter;
		private bool blink;

		void Start() {

			controller = dotMatrix.GetController();

			// Blink "00:00" by repeating 4 commands until Update() will clear the command queue
			controller.AddCommand(new TextCommand("00:00") { Repeat = true }); // Text 00:00
			controller.AddCommand(new PauseCommand(0.5f) { Repeat = true }); // Wait half second
			controller.AddCommand(new ClearCommand() { Repeat = true }); // Clear
			controller.AddCommand(new PauseCommand(0.5f) { Repeat = true }); // Wait half second

			displayZero = 10f;
			timeInGameWorld = 43200f;
			updateCounter = 0f;
			blink = false;

		}

		void FixedUpdate() {
			timeInGameWorld += Time.fixedDeltaTime;
		}

		void Update() {

			// Just blinking 00:00 ?
			if (displayZero>0f) {
				displayZero -= Time.deltaTime;
				if (displayZero>0f) {
					// Continue blinking zero
					return;
				}
				// End blinking, clear controller command queue
				controller.ClearCommands();
			}

			// Display imaginary game world time, blinking ':' between
			updateCounter -= Time.deltaTime;
			if (updateCounter<=0f) {
				int seconds = Mathf.RoundToInt(timeInGameWorld);
				int minutes = (seconds/60)%60;
				int hours = (seconds/(60*60))%24;
				string str = hours.ToString("00")+(blink ? ":" : " ")+minutes.ToString("00");
				controller.ClearCommands(); // Make sure any old commands are not in queue
				controller.AddCommand(new TextCommand(str));
				updateCounter = 0.5f;
				blink = !blink;
			}

		}

	}

}
