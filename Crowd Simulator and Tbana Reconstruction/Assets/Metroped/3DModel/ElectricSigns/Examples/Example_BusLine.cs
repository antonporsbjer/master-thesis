//    Example - Bus line info


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_BusLine : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;
		private TextCommand nextStopMessage;

		void Start() {

			// Get controller

			controller = dotMatrix.GetController();

			controller.DefaultSpeedDotsPerSecond = 45f;

			// Set basic loop that is repeated forever

			TextCommand text = new TextCommand("Bus Line 42A") {
				Movement = TextCommand.Movements.MoveUpAndStop,
				DotsPerSecond = 25f,
				Repeat = true
			};

			PauseCommand pause = new PauseCommand(7f) {
				Repeat = true
			};

			ClearCommand clear = new ClearCommand() {
				Method = ClearCommand.Methods.MoveUp,
				DotsPerSecond = 25f,
				Repeat = true
			};

			nextStopMessage = new TextCommand("(placeholder)");

			controller.AddCommand(text);
			controller.AddCommand(pause);
			controller.AddCommand(clear);
			controller.AddCommand(nextStopMessage);

			// Set first next stop

			setRandomNextStop();

		}

		void Update() {
			// Just randomly change next stop
			if (Random.value<0.0005f) {
				setRandomNextStop();
			}
		}

		private void setRandomNextStop() {
			// Create next text command which is displaying next stop
			TextCommand newNextStopMessage = new TextCommand("Next stop: "+getRandomNextStop()) {
				Movement = TextCommand.Movements.MoveLeftAndPass,
				Repeat = true
			};
			// Replace old next stop text in queue with new one
			controller.ReplaceCommand(nextStopMessage, newNextStopMessage);
			nextStopMessage=newNextStopMessage;
		}

		private string getRandomNextStop() {
			string[] stops = new string[] { "Kamppi", "Siilitie", "Puotila", "Rastila", "Vuosaari", "Kontula" };
			int rnd = Random.Range(0, stops.Length);
			return stops[rnd];
		}

	}

}
