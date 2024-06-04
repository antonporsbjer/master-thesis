//    Example - Characters and manual graphics


using UnityEngine;
using System;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_Characters : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private Controller controller;

		void Start() {

			// Get controller

			controller = dotMatrix.GetController();

			// Hello you!

			string userName = null;
			try {
				userName = Environment.UserName;
			}
			catch (Exception) {
			}
			if (string.IsNullOrEmpty(userName)) {
				userName = "you";
			}

			TextCommand text = new TextCommand("Hello "+userName) {
				Movement = TextCommand.Movements.MoveUpAndStop,
				FixedWidth = false
			};
			controller.AddCommand(text);

			// Pause, then clear

			PauseCommand pause = new PauseCommand(5f);
			controller.AddCommand(pause);

			controller.AddCommand(new ClearCommand());

			// Show all characters
			controller.AddCommand(new TextCommand("ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789 .,!?:;-_'\"#€$%&/\\()<>=+*@|[]{}^~°¡¿`´’ �?ÀÂÄÃÅÆÉÈÊË�?ÌÎ�?ÓÒÔÖÕØŒÚÙÛÜ�?ŸẞÇÑŠŽ "+"\u0436") { // Last character is example of char that doesn't exist in Characters class
				Movement = TextCommand.Movements.MoveLeftAndPass,
				CharSpacing = 2
			});

			// Some variable width text

			controller.AddCommand(new PauseCommand(1f));
			text = new TextCommand("Variable width!!!") {
				Movement = TextCommand.Movements.MoveRightAndStop,
				HorPosition = TextCommand.HorPositions.Right,
				FixedWidth = false
			};
			controller.AddCommand(text);

			controller.AddCommand(new PauseCommand(2f));
			text = new TextCommand(" & ") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = TextCommand.HorPositions.Right,
				FixedWidth = false
			};
			controller.AddCommand(text);
			text = new TextCommand("bold") {
				Movement = TextCommand.Movements.MoveLeftAndStop,
				HorPosition = TextCommand.HorPositions.Right,
				FixedWidth = false,
				Bold = true
			};
			controller.AddCommand(text);

			// Pause again, re-using earlier pause command

			controller.AddCommand(pause);

			// Clear slowly

			controller.AddCommand(new ClearCommand() {
				Method = ClearCommand.Methods.MoveDown,
				DotsPerSecond = 1
			});

			// Example of smaller font

			controller.AddCommand(new TextCommand("This text would fit display which height is just 5 dots") {
				Movement = TextCommand.Movements.MoveLeftAndPass,
				Font = TextCommand.Fonts.Small,
				FixedWidth = false
			});

			// Add some manual graphics (little heart)

			controller.AddCommand(new ContentCommand(
				new int[,] {
					{0,1,1,0,1,1,0},
					{1,1,1,1,1,1,1},
					{1,1,1,1,1,1,1},
					{0,1,1,1,1,1,0},
					{0,0,1,1,1,0,0},
					{0,0,0,1,0,0,0}
				}
				) {
					VerPosition = AbsCmdPosition.VerPositions.Bottom,
					Movement = AbsCmdPosition.Movements.MoveDownAndStop,
					DotsPerSecond = 7
				}
			);

		}

	}

}
