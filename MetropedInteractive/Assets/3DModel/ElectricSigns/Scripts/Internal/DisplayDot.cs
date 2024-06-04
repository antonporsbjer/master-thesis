//    DotMatrix - Display - DisplayDot


using UnityEngine;

namespace Leguar.DotMatrix.Internal {
	
	public abstract class DisplayDot {
		
		private Display display;
		private int x,y;

		private int brokenState; // This dot is "stuck" in this state, no matter what updates comes from
		private int currentState; // Real wanted state of this dot which is used if 'brokenState' is -1

		internal DisplayDot(Display display, int x, int y) {
			this.display=display;
			this.x=x;
			this.y=y;
			brokenState=-1;
			currentState=0;
		}

		protected int getClampedX() {
			return Mathf.Clamp(x,0,display.WidthInDots-1);
		}

		protected int getClampedY() {
			return Mathf.Clamp(y, 0, display.HeightInDots-1);
		}

		protected int getDrawX() {
			return (x+display.BorderDots);
		}
		
		protected int getDrawY() {
			int borderDots=display.BorderDots;
			int drawY=y+borderDots;
			drawY=display.HeightInDots+borderDots*2-1-drawY; // Swap Y coordinate so that first row (y=0) is on top
			return drawY;
		}

		protected float getDelay(int fromState, int toState) {
			return (toState==0?display.OffDelaySeconds:(fromState==0?display.OnDelaySeconds:display.ChangeDelaySeconds));
		}

		internal void setBroken(int brokenState) {
			this.brokenState=brokenState;
			resetState();
		}

		internal int getBroken() {
			return brokenState;
		}

		internal bool isBroken() {
			return (brokenState>=0);
		}

		internal void setNewStateInstantly(int state) {
			currentState=state;
			resetState();
		}

		internal void setNewTargetState(int state) {
			if (state!=currentState) {
				currentState=state;
				if (!isBroken()) {
					setNewVisibleTargetState(currentState);
				}
			}
		}

		internal void resetState() {
			setVisibleStateInstantly(isBroken()?brokenState:currentState);
		}

		internal abstract void setVisibleStateInstantly(int state);
		internal abstract void setNewVisibleTargetState(int state);
		internal abstract bool updateVisibleState(float deltaTime);

	}

}
