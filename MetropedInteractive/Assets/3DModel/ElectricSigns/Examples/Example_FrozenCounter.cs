//    Example - FrozenCounter (actual delay effect is done by DotMatrix Display parameters, this script is unaware that display is trailing/lagging/ghosting)


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_FrozenCounter : MonoBehaviour {

		private Controller controller;
		private int counter;

		void Start() {

			// Just another way (compared to other examples) to get reference to DotMatrix object this script wants to control
			DotMatrix dotMatrix = GameObject.Find("/Frozen Counter/DotMatrix_Sprite").GetComponent<DotMatrix>();

			controller = dotMatrix.GetController();

			controller.AddCommand(new TextCommand("Frozen") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = AbsCmdPosition.HorPositions.Center,
				DotsPerSecond = 15f
			});

			controller.AddCommand(new PauseCommand(2f));

			controller.AddCommand(new TextCommand("and trailing") {
				Movement = TextCommand.Movements.MoveLeftAndPass,
				DotsPerSecond = 15f
			});

			counter = 0;

		}

		void Update() {
			if (controller.IsIdle()) { // Important check: Without this new text commands would be added to Controller also when Controller is running pause and clear command (below), causing command queue to grow forever
				// Add new number to display
				counter++;
				controller.AddCommand(new TextCommand(""+counter));
				// On every 1000, pause and scroll text away before continuing
				if (counter%1000==0) {
					controller.AddCommand(new PauseCommand(1f));
					controller.AddCommand(new ClearCommand() {
						Method = ClearCommand.Methods.MoveRight,
						DotsPerSecond = 15f
					});
				}
			}
		}

	}

}
