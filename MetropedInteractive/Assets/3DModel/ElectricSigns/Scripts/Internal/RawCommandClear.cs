//    DotMatrix - RawCommand


using UnityEngine;

namespace Leguar.DotMatrix.Internal {
	
	internal class RawCommandClear : RawCommand {
		
		private int targetState;
		
		private ClearCommand.Methods method;
		private int steps;
		private float secondsPerDot;
		
		private int counter;
		private int counterMax;
		
		internal RawCommandClear(int targetState) {
			this.targetState=targetState;
			method=ClearCommand.Methods.Instant;
			secondsPerDot=0f;
		}
		
		internal RawCommandClear(int targetState, ClearCommand.Methods method, int steps, float dotsPerSecond) {
			this.targetState=targetState;
			this.method=method;
			this.steps=steps;
			secondsPerDot=1f/dotsPerSecond;
			counter=-1;
		}
		
		internal RawCommandClear(int targetState, ClearCommand.Methods method, int steps, float dotsPerSecond, int counterMax) {
			this.targetState=targetState;
			this.method=method;
			this.steps=steps;
			secondsPerDot=1f/dotsPerSecond;
			counter=0;
			this.counterMax=counterMax;
		}

		internal override float runStep(DisplayModel displayModel, float timeToConsume) {

			if (method==ClearCommand.Methods.Instant) {
				displayModel.SetAll(targetState);
				return timeToConsume;
			}

			if (counter<0) {

				while (timeToConsume>=secondsPerDot*steps) {
					for (int n = 0; n<steps; n++) {
						runStepMovement(displayModel);
						timeToConsume-=secondsPerDot;
						if (isFinished(displayModel)) {
							return timeToConsume;
						}
					}
				}

			} else {

				if (steps==1) {

					while (timeToConsume>=secondsPerDot && counter<counterMax) {
						runStepClear(displayModel);
						timeToConsume-=secondsPerDot;
						counter++;
					}

				} else {

					int jump = Mathf.Min(steps, counterMax-counter);

					while (jump>0 && timeToConsume>=secondsPerDot*jump) {

						for (int n = 0; n<jump; n++) {
							runStepClear(displayModel);
							timeToConsume-=secondsPerDot;
							counter++;
						}

						jump = Mathf.Min(steps, counterMax-counter);

					}


				}

			}

			return timeToConsume;

		}

		private void runStepMovement(DisplayModel displayModel) {
			if (method==ClearCommand.Methods.MoveLeft) {
				displayModel.PushLeft();
				if (targetState>0) {
					displayModel.SetColumn(displayModel.Width-1, targetState);
				}
			} else if (method==ClearCommand.Methods.MoveRight) {
				displayModel.PushRight();
				if (targetState>0) {
					displayModel.SetColumn(0, targetState);
				}
			} else if (method==ClearCommand.Methods.MoveUp) {
				displayModel.PushUp();
				if (targetState>0) {
					displayModel.SetRow(displayModel.Height-1, targetState);
				}
			} else if (method==ClearCommand.Methods.MoveDown) {
				displayModel.PushDown();
				if (targetState>0) {
					displayModel.SetRow(0, targetState);
				}
			}
		}

		private void runStepClear(DisplayModel displayModel) {
			if (method==ClearCommand.Methods.ColumnByColumnFromLeft) {
				displayModel.SetColumn(counter, targetState);
			} else if (method==ClearCommand.Methods.ColumnByColumnFromRight) {
				displayModel.SetColumn(displayModel.Width-counter-1, targetState);
			} else if (method==ClearCommand.Methods.RowByRowFromTop) {
				displayModel.SetRow(counter, targetState);
			} else if (method==ClearCommand.Methods.RowByRowFromBottom) {
				displayModel.SetRow(displayModel.Height-counter-1, targetState);
			}
		}

		internal override bool isFinished(DisplayModel displayModel) {
			if (method==ClearCommand.Methods.Instant) {
				return true;
			}
			if (counter==-1) {
				return displayModel.IsAllDotsInState(targetState);
			}
			return (counter>=counterMax);
		}
		
	}
	
}
