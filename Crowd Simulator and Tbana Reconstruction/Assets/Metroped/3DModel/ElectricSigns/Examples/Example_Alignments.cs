//    Example - Text alignments, callback and direct dot access


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_Alignments : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		private static readonly string[] randomMessage = { "This", "is", "a", "secret", "message" };

		private Controller controller;
		private DisplayModel displayModel;

		private bool wordsToRandomPositions;

		void Start() {

			// Get both controller and display model
			controller = dotMatrix.GetController();
			displayModel = dotMatrix.GetDisplayModel();

			// Go through all alignments
			for (int vp = 0; vp<3; vp++) {
				for (int hp = 0; hp<3; hp++) {
					for (int ta = 0; ta<3; ta++) {
						TextCommand testText = new TextCommand((vp==0 ? "Top" : (vp==1 ? "Middle" : "Bottom")) +"\n"+ (hp==0 ? "Left" : (hp==1 ? "Center" : "Right"))) {
							VerPosition = (vp==0 ? TextCommand.VerPositions.Top : (vp==1 ? TextCommand.VerPositions.Middle : TextCommand.VerPositions.Bottom)),
							HorPosition = (hp==0 ? TextCommand.HorPositions.Left : (hp==1 ? TextCommand.HorPositions.Center : TextCommand.HorPositions.Right)),
							TextAlignment = (ta==0 ? TextCommand.TextAlignments.Left : (ta==1 ? TextCommand.TextAlignments.Center : TextCommand.TextAlignments.Right)),
							Movement = AbsCmdPosition.Movements.None
						};
						controller.AddCommand(testText);
						PauseCommand pause = new PauseCommand(1f);
						controller.AddCommand(pause);
					}
				}
			}

			// Clear
			controller.AddCommand(new ClearCommand());

			// Basic multiple line scrolling text
			string[] multiLine = new string[] { "Welcome", "back", "to 90s" };
			TextCommand text = new TextCommand(multiLine) {
				Font = TextCommand.Fonts.Large,
				Movement = AbsCmdPosition.Movements.MoveUpAndStop,
				DotsPerSecond = 20,
				VerPosition = AbsCmdPosition.VerPositions.Top,
				HorPosition = AbsCmdPosition.HorPositions.Left
			};
			controller.AddCommand(text);

			// Add callback "ScrollDone" that is called when Controller reaches this command
			controller.AddCommand(new CallbackCommand(ScrollDone));

			wordsToRandomPositions = false;

		}

		// Callback that is called by Controller 
		void ScrollDone() {
			wordsToRandomPositions = true;
		}

		void Update() {
			if (wordsToRandomPositions && Random.value<0.5f) {
				displayModel.SetDot(Random.Range(0, displayModel.Width), Random.Range(0, displayModel.Height), (Random.value<0.5f));
				if (Random.value<0.05f) {
					int rnd = Random.Range(0, randomMessage.Length);
					int[,] content = TextToContent.getContent(
						randomMessage[rnd],
						TextCommand.Fonts.Normal,
						(Random.value<0.5f), // Random fixed width
						(Random.value<0.5f), // Random bold
						Random.Range(0, 2) // Random char spacing
					);
					int widthInDots = content.GetLength(1);
					int x = Random.Range(-widthInDots+1, displayModel.Width-1); // Random X position
					int y = rnd*8;
					displayModel.SetPartialContent(content, x, y);
				}
			}
		}

	}

}
