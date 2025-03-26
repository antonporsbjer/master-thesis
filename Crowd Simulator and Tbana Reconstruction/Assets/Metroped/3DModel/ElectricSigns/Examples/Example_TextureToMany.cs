//    Example - Using Texture based DotMatrix, adding same texture to multiple objects


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_TextureToMany : MonoBehaviour {

		public DotMatrix dotMatrix; // Reference is set in Unity Editor inspector

		public Renderer quad1Renderer;
		public Renderer quad2Renderer;
		public Renderer quad3Renderer;

		void Start() {

			// Init DotMatrix right away, need to be done before trying to get texture
			dotMatrix.Init();

			// Get texture from DotMatrix display
			Display_Texture display = (Display_Texture)(dotMatrix.GetDisplay());
			Texture2D texture = display.GetGeneratedTexture();

			// Set it to multiple targets
			quad1Renderer.material.mainTexture = texture;
			quad2Renderer.material.mainTexture = texture;
			quad3Renderer.material.mainTexture = texture;

			// From now on, can do all the same things as other DotMatrix displays
			TextCommand textCommand = new TextCommand("-- Same texture, different shader --") {
				Font = TextCommand.Fonts.Large,
				VerPosition = TextCommand.VerPositions.Bottom,
				Movement = TextCommand.Movements.MoveLeftAndPass,
				DotsPerSecond = 20,
				Repeat = true
			};
			dotMatrix.GetController().AddCommand(textCommand);

		}

	}

}
