//    Example - Elevator


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_Elevator : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		[Header("Elevator parameters")]
		[Range(0.1f, 5f)]
		public float elevatorMoveSpeedSecondsPerFloor = 2f;
		[Range(1f, 15f)]
		public float elevatorStopTimeAtFloor = 5f;
		public bool clearDisplayBetweenFloors = true;

		private Controller controller;

		private int currentFloor;
		private int targetFloor;

		private float stateBetweenFloors;

		void Start() {
			currentFloor = 1;
			controller=dotMatrix.GetController();
			controller.DefaultSpeedDotsPerSecond = 30;
			controller.AddCommand(new TextCommand(currentFloorAsString()));
			targetFloor = 8;
		}

		void Update() {

			// Choose new target floor if old target floor is reached and DotMatrix Controller isn't doing anything any more
			if (targetFloor==currentFloor && controller.IsIdle()) {
				targetFloor = Random.Range(0, 9); // 0-8, may choose same floor where we are
				stateBetweenFloors = 0f;
			}

			// If target floor changed, give new commands to DotMatrix
			if (targetFloor!=currentFloor) {

				stateBetweenFloors += Time.deltaTime;

				// Is time to change floor number on display?
				if (stateBetweenFloors>elevatorMoveSpeedSecondsPerFloor) {

					stateBetweenFloors -= elevatorMoveSpeedSecondsPerFloor;

					// Going up or down?
					bool up = (targetFloor>currentFloor);
					currentFloor += (up ? 1 : -1);

					// Possibly add clear command first, scrolling to same directions as numbers
					if (clearDisplayBetweenFloors) {
						controller.AddCommand(new ClearCommand() {
							Method = (up ? ClearCommand.Methods.MoveDown : ClearCommand.Methods.MoveUp)
						});
					}

					// Scroll in new current floor number
					controller.AddCommand(new TextCommand(currentFloorAsString()) {
						Movement = (up ? TextCommand.Movements.MoveDownAndStop : TextCommand.Movements.MoveUpAndStop) // Direction depens whatever elevator is going up or down
					});

					// If reached target floor, add pause (kinda hack to use Controller pause command as timer how long elevator stays in one floor, but just as an example)
					if (currentFloor==targetFloor) {
						controller.AddCommand(new PauseCommand(elevatorStopTimeAtFloor));
					}

				}

			}

		}

		private string currentFloorAsString() {
			if (currentFloor==0) {
				return "B"; // Basement
			}
			return (""+currentFloor);
		}

	}

}
