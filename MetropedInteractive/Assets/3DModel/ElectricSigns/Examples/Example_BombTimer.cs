//    Example - Bomb Timer


//*
using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_BombTimer : MonoBehaviour {

		private DotMatrix dotMatrixDisplayInScene;

		private float secondsInTimer;
		private float nextTimerUpdateCounter;

		void Start() {
			// Yet another way (compared to other examples) to get reference to DotMatrix object
			// This works since there is only one DotMatrix object in scene
			dotMatrixDisplayInScene = Object.FindObjectOfType<DotMatrix>();
			// Run 2 minutes (120 seconds)
			secondsInTimer = 120f;
			setCurrentTimeToDisplay();
			nextTimerUpdateCounter = 0f;
		}

		void Update() {

			if (secondsInTimer>0f) { // Timer is still running

				nextTimerUpdateCounter += Time.deltaTime;

				if (nextTimerUpdateCounter>=0.1f) { // Update display max 10 times per second

					secondsInTimer -= nextTimerUpdateCounter; // Decrease time of actual timer
					nextTimerUpdateCounter = 0f; // Next update in 0.1 seconds again

					if (secondsInTimer>0f) {
						setCurrentTimeToDisplay();
					} else {
						secondsInTimer = 0f;
						endOfTime();
					}

				}

			}

		}

		// This will update text on DotMatrix display based on value of 'secondsInTimer'
		private void setCurrentTimeToDisplay() {
			string timerText = getTextForDisplay(secondsInTimer);
			setTextToDisplay(timerText, TextCommand.HorPositions.Right);
		}

		// This will turn integer seconds to string of time, like 65 seconds -> "1:05.0"
		private string getTextForDisplay(float timeLeft) {
			int tenthOfSecondsLeft = (int)(timeLeft*10f);
			int minutes = tenthOfSecondsLeft/600;
			int seconds = (tenthOfSecondsLeft%600)/10;
			int secondFractions = tenthOfSecondsLeft%10;
			return (minutes+":"+(seconds<10 ? "0" : "")+seconds+"."+secondFractions);
		}

		// Set parameter text to DotMatrix display
		private void setTextToDisplay(string text, TextCommand.HorPositions horPosition) {
			TextCommand textCommand = new TextCommand(text) {
				HorPosition=horPosition
			};
			dotMatrixDisplayInScene.GetController().AddCommand(textCommand);
		}

		// Called when timer reaches 0:00.0
		private void endOfTime() {
			setTextToDisplay("BOOM!", TextCommand.HorPositions.Center);
			Debug.Log("BOOM!");
		}

	}

}
//*/


/*

//  As secondary example, whole above code could be replaced with this. This one adds all 1200 texts and 1/10 second pauses to Controller and let it do the magic. So no need for own Update() loop.

using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_BombTimer : MonoBehaviour {

		void Start() {

			DotMatrix dotMatrixDisplayInScene = Object.FindObjectOfType<DotMatrix>();
			Controller controller = dotMatrixDisplayInScene.GetController();

			for (int tenthOfSecondsInTimer = 1200; tenthOfSecondsInTimer>0; tenthOfSecondsInTimer--) {
				controller.AddCommand(new TextCommand(getTextForDisplay(tenthOfSecondsInTimer)) { HorPosition=TextCommand.HorPositions.Right });
				controller.AddCommand(new PauseCommand(0.1f));
			}

			controller.AddCommand(new TextCommand("BOOM!") { HorPosition=TextCommand.HorPositions.Center });
			controller.AddCommand(new CallbackCommand(endOfTime));

		}

		private string getTextForDisplay(int tenthOfSecondsLeft) {
			int minutes = tenthOfSecondsLeft/600;
			int seconds = (tenthOfSecondsLeft%600)/10;
			int secondFractions = tenthOfSecondsLeft%10;
			return (minutes+":"+(seconds<10 ? "0" : "")+seconds+"."+secondFractions);
		}

		private void endOfTime() {
			Debug.Log("BOOM!");
		}

	}

}

//*/
