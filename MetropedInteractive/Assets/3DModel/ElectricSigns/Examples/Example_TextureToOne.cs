//    Example - Using Texture based DotMatrix which set the texture to target material automatically


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_TextureToOne : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		public Transform cubeTransform;
		private float counter;

		void Start() {

			// There's nothing to set up. DotMatrix script in scene already have target material defined where it will add the texture when application starts.

			ShowRandomColoredText();

		}

		private void ShowRandomColoredText() {

			int randomTextColorIndex = Random.Range(1, 8);

			Controller controller = dotMatrix.GetController();
			Display_Texture display = (Display_Texture)(dotMatrix.GetDisplay());

			display.OffColor = Color.Lerp(display.GetColor(randomTextColorIndex), Color.black, 0.8f);

			TextCommand textCommand = new TextCommand("Texture added to cube") {
				Movement = TextCommand.Movements.MoveLeftAndPass,
				TextColor = randomTextColorIndex
			};
			controller.AddCommand(textCommand);
			controller.AddCommand(new CallbackCommand(ShowRandomColoredText));

		}

		void Update() {
			// Just for sake of example, rotate cube around
			counter += Time.deltaTime * 0.25f;
			while (counter >= 2f) {
				counter -= 2f;
			}
			cubeTransform.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp01(counter)*360f, 180f+Mathf.Clamp01(counter-1)*360f, 0f));
		}

	}

}
