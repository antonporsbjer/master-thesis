//    DotMatrix - DisplayDot - Texture


namespace Leguar.DotMatrix.Internal {
	
	using UnityEngine;
	
	public class DisplayDot_Texture : DisplayDot {
		
		private Display_Texture displayTexture;

		private int fromState;
		private int toState;
		private Color fromColor;
		private Color toColor;
		private float stateChangeElapsed;

		private Color currentColor;

		internal DisplayDot_Texture(Display_Texture displayTexture, int x, int y) : base(displayTexture, x,y) {
			this.displayTexture=displayTexture;
		}

		internal override void setVisibleStateInstantly(int state) {
			fromState=toState=state;
			fromColor=toColor=displayTexture.GetColor(state);
			updateCurrentColor(toColor);
		}

		internal override void setNewVisibleTargetState(int state) {
			fromState=toState;
			toState=state;
			fromColor=currentColor;
			toColor=displayTexture.GetColor(state);
			stateChangeElapsed=0f;
		}

		internal override bool updateVisibleState(float deltaTime) {
			stateChangeElapsed+=deltaTime;
			float delay=base.getDelay(fromState,toState);
			if (delay<=0f || stateChangeElapsed>=delay) {
				updateCurrentColor(toColor);
				return false; // Done, no need for new updates until new target state is set
			}
			float percent=stateChangeElapsed/delay;
			updateCurrentColor(Color.Lerp(fromColor, toColor, percent));
			return true; // Need new updates to reach final color
		}

		private void updateCurrentColor(Color color) {
			currentColor = color;
			displayTexture.updateTexture(this.getDrawX(), this.getDrawY(), currentColor);
		}

	}

}
