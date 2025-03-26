//    Example - ImageToDisplay (using provided ImageToContent script)


using UnityEngine;
using Leguar.DotMatrix;

namespace Leguar.DotMatrix.Example {

	public class Example_ImageToDisplay : MonoBehaviour {

		public DotMatrix dotMatrixForSlideShow;
		public Texture2D[] slideShowTextures;
		public DotMatrix dotMatrixForScrollingText;
		public Texture2D scrollingTextTexture;

		void Start() {
			startSlideShow();
			startScrollingText();
		}

		private void startSlideShow() {

			// Get controller and display of first DotMatrix

			Controller controller = dotMatrixForSlideShow.GetController();
			Display_Sprite display = (Display_Sprite)(dotMatrixForSlideShow.GetDisplay());

			// Run slideshow (turn 32*32 textures to content that are added to DotMatrix display which is also size 32*32)

			for (int n = 0; n<slideShowTextures.Length; n++) {

				int[,] content = ImageToContent.getContent(slideShowTextures[n], display);

				controller.AddCommand(new ContentCommand(content) { Repeat=true });
				controller.AddCommand(new PauseCommand(5f) { Repeat=true });

			}

//			dotMatrixForSlideShow.GetDisplayModel().SetFullContent(ImageToContent.getContent(slideShowTextures[0], display));

		}

		private void startScrollingText() {

			// Get controller and display of second DotMatrix

			Controller controller = dotMatrixForScrollingText.GetController();
			Display_UI display = (Display_UI)(dotMatrixForScrollingText.GetDisplay()); // For sake of example, this second DotMatrix in scene is UI version, but everything works just like in Sprite version

			// Turn texture containing text to content and scroll it on display

			int[,] content = ImageToContent.getContent(scrollingTextTexture, display);

			controller.AddCommand(new ContentCommand(content) {
				Movement=ContentCommand.Movements.MoveLeftAndPass,
				Repeat=true
			});
			controller.AddCommand(new PauseCommand(2f) {
				Repeat=true
			});

			// For comparison, scroll normal text too

			controller.AddCommand(new TextCommand("This text is using normal text command") {
				Movement=TextCommand.Movements.MoveLeftAndPass,
				Font=TextCommand.Fonts.Large,
				Repeat=true
			});
			controller.AddCommand(new PauseCommand(2f) {
				Repeat=true
			});
			
			// Scroll also slideshow images here. 'ShrinkKeepingAspectRatio' makes them fit inside display even textures are larger than this display height.
			// Images will appear green since their palette is changed to this DotMatrix display palette.

			for (int n = 0; n<slideShowTextures.Length; n++) {

				content = ImageToContent.getContent(slideShowTextures[n], display, ImageToContent.ResizeModes.ShrinkKeepingAspectRatio);

				controller.AddCommand(new ContentCommand(content) {
					HorPosition=ContentCommand.HorPositions.Right,
					Movement=TextCommand.Movements.MoveLeftAndStop,
					Repeat=true
				});

			}

			controller.AddCommand(new ClearCommand() {
				Method=ClearCommand.Methods.MoveLeft,
				Repeat=true
			});
			controller.AddCommand(new PauseCommand(3f) {
				Repeat=true
			});

		}

	}

}
