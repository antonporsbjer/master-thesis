//    Example - Hello World


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_HelloWorld : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;

		void Start() {

			controller = dotMatrix.GetController();
			controller.DefaultSpeedDotsPerSecond = 20;

			controller.AddCommand(new TextCommand("Hello"));
			controller.AddCommand(new PauseCommand(2f));
			controller.AddCommand(new ClearCommand(1) { Method = ClearCommand.Methods.RowByRowFromBottom });
			controller.AddCommand(new ClearCommand(0) { Method = ClearCommand.Methods.RowByRowFromBottom });
			controller.AddCommand(new TextCommand("World"));
			controller.AddCommand(new PauseCommand(2f));
			controller.AddCommand(new ClearCommand(1) { Method = ClearCommand.Methods.RowByRowFromTop });
			controller.AddCommand(new ClearCommand(0) { Method = ClearCommand.Methods.RowByRowFromTop });

			controller.AddCommand(new TextCommand("I am\nmade of"));
			controller.AddCommand(new PauseCommand(2.5f));
			controller.AddCommand(new TextCommand("cubes with\ndark and"));
			controller.AddCommand(new PauseCommand(3.5f));
			controller.AddCommand(new TextCommand("bright\nsides"));
			controller.AddCommand(new PauseCommand(2.5f));
			controller.AddCommand(new ClearCommand());

			controller.AddCommand(new TextCommand("* EOM *") { Repeat = true });
			controller.AddCommand(new PauseCommand(1f) { Repeat = true });
			controller.AddCommand(new TextCommand("  EOM  ") { Repeat = true });
			controller.AddCommand(new PauseCommand(1f) { Repeat = true });

		}

	}

}
