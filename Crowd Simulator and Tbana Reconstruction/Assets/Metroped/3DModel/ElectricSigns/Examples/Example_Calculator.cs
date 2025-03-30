//    Example - Calculator


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_Calculator : MonoBehaviour {

		private DotMatrix dotMatrix;
		private Controller controller;

		private int charactersPerRow;

		private string calcEntry;
		private int calcOne;
		private int calcTotal;
		private bool errorState;

		void Start() {

			dotMatrix = Object.FindObjectOfType<DotMatrix>();
			controller = dotMatrix.GetController();

			controller.AddCommand(new TextCommand("Calc v1.1") {
				CharSpacing=0, // Zero spacing between characters since dots in target DotMatrix are already grouped so that there is space between characters and lines
				VerPosition=AbsCmdPosition.VerPositions.Top,
				HorPosition=AbsCmdPosition.HorPositions.Left,
				Movement=AbsCmdPosition.Movements.MoveLeftAndStop,
				DotsPerSecond=25f,
				DotsPerStep=5 // Make scrolling to jump 5 dots per update, this way characters do not end up between dot groups
			});

			controller.AddCommand(new PauseCommand(2f));

			controller.AddCommand(new ClearCommand() {
				Method=ClearCommand.Methods.MoveLeft,
				DotsPerSecond=25f,
				DotsPerStep=5,
			});

			controller.AddCommand(new CallbackCommand(calcClear));

			charactersPerRow = dotMatrix.GetDisplay().WidthInDots/5; // Just assuming that display width is set to divisible by 5

//			clear();

		}

		private void setTextToDisplay(string textRow1, string textRow2) {

			// Set first row to left side just by adding spaces
			int rowOneLength = textRow1.Length;
			if (rowOneLength<charactersPerRow) {
				textRow1 += new string(' ', charactersPerRow-rowOneLength);
			}

			// Create new text command
			TextCommand textCommand = new TextCommand(textRow1+"\n"+textRow2);

			// Use normal size font (width 5, height 7)
			textCommand.Font = TextCommand.Fonts.Normal;

			// Set both character and line spacing to 0 since in this display each character already have space because Dot Grouping is enabled
			textCommand.CharSpacing = 0;
			textCommand.LineSpacing = 0;

			// Set all content to right side
			textCommand.HorPosition = TextCommand.HorPositions.Right;
			textCommand.TextAlignment = TextCommand.TextAlignments.Right;

			// Set text to display
			controller.AddCommand(textCommand);

		}

		//  Extremely minimalistic calculator functions follows

		public void clearClicked(int number) {
			cancelStartUpAnimationIfRunning();
			calcClear();
		}

		public void numberClicked(int number) {
			cancelStartUpAnimationIfRunning();
			if (errorState) {
				return;
			}
			calcEntry+=""+number;
			int currentTotal;
			try {
				checked {
					calcOne=calcOne*10+number;
					currentTotal=calcTotal+calcOne;
				}
			}
			catch (System.OverflowException) {
				calcError();
				return;
			}
			setTextToDisplay(calcEntry, "="+currentTotal);
		}

		public void plusClicked(int number) {
			cancelStartUpAnimationIfRunning();
			if (errorState) {
				return;
			}
			if (calcEntry.Length>0) {
				if (calcEntry[calcEntry.Length-1]=='+') {
					calcError();
					return;
				}
			}
			calcEntry+="+";
			try {
				checked {
					calcTotal+=calcOne;
				}
			}
			catch (System.OverflowException) {
				calcError();
				return;
			}
			calcOne=0;
			setTextToDisplay(calcEntry, "="+calcTotal);
		}

		private void calcClear() {
			calcEntry="";
			calcTotal=0;
			calcOne=0;
			setTextToDisplay("", "0");
			errorState=false;
		}

		private void calcError() {
			setTextToDisplay("*ERROR*", "");
			errorState=true;
		}

		private void cancelStartUpAnimationIfRunning() {
			if (!controller.IsIdle()) { // Still running start-up animation
				Debug.Log("CANCEL");
				controller.ClearCommandsAndStop();
				calcClear();
			}
		}

	}

}
