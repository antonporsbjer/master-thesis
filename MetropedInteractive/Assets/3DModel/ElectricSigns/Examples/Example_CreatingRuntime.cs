//    Example - Creating runtime from DotMatrix prefab


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_CreatingRuntime : MonoBehaviour {

		public GameObject dotMatrixPrefab; // Reference to prefab is set in Unity Editor inspector

		private Display_Sprite spriteBasedDisplay;

		void Start() {

			// Create new DotMatrix display from prefab
			GameObject dotMatrixObject = (GameObject)(Instantiate(dotMatrixPrefab));
			dotMatrixObject.transform.parent = this.transform;
			dotMatrixObject.transform.localPosition = Vector3.zero;

			// Get main script
			DotMatrix dotMatrix = dotMatrixObject.GetComponent<DotMatrix>();
			// Set desired values, these can be changed as long as DotMatrix is not initialized by calling Init or by getting reference to Controller/DisplayModel
			spriteBasedDisplay = (Display_Sprite)(dotMatrix.GetDisplay());
			spriteBasedDisplay.WidthInDots = 120;
			spriteBasedDisplay.HeightInDots = 11;
			spriteBasedDisplay.OffColor = new Color(0.0f, 0.1f, 0.0f);
			spriteBasedDisplay.OnColor  = new Color(0.1f, 0.8f, 0.1f);
			spriteBasedDisplay.DotSize = Vector2.one*0.65f;

			// This will also init the display (create dots)
			Controller controller = dotMatrix.GetController();
			// Set default font for all text commands
			controller.DefaultTextFont = TextCommand.Fonts.Large;

			// Show this text once (no repeat)
			controller.AddCommand(new TextCommand("This didn't exist in editor but was created from prefab") {
				Movement = TextCommand.Movements.MoveLeftAndPass, // Move from right to left until away from display
				FixedWidth = false // Use variable width font
			});

			// Repeat this text forever
			controller.AddCommand(new TextCommand("The quick brown fox jumps over the lazy dog  ----  ") {
				Movement = TextCommand.Movements.MoveLeftAndStop, // Move from right to left and end when text have reached it's wanted horizontal position
				HorPosition = AbsCmdPosition.HorPositions.Right, // Align text to right so it will move fully visible when moving from right to left
				FixedWidth = false, // Use variable width font
				Repeat = true // Repeat so same text will scroll forever without noticeable stops
			});
			// Change display color before running above text command again
			controller.AddCommand(new CallbackCommand(SetRandomColor) {
				Repeat = true
			});

		}

		public void SetRandomColor() {
			float rnd = Random.value;
			if (rnd<0.5) {
				spriteBasedDisplay.OffColor = new Color(0.1f, 0.2f, 0.1f);
				spriteBasedDisplay.OnColor  = new Color(0.2f, 0.9f, 0.2f);
			} else if (rnd<0.8) {
				spriteBasedDisplay.OffColor = new Color(0.1f, 0.1f, 0.2f);
				spriteBasedDisplay.OnColor  = new Color(0.2f, 0.2f, 1.0f);
			} else {
				spriteBasedDisplay.OffColor = new Color(0.1f, 0.1f, 0.0f);
				spriteBasedDisplay.OnColor  = new Color(0.8f, 0.8f, 0.1f);
			}
		}

	}

}
