//    DotMatrix - AbsCmdPosition


using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Abstract base class for all commands that have content which need to be positioned on display, such as text.
	/// </summary>
	public abstract class AbsCmdPosition : AbsCmdSpeed {
		
		/// <summary>
		/// List of possible horizontal positions of the content. Default is center.
		/// </summary>
		public enum HorPositions {
			/// <summary> Content is aligned to center of display.</summary>
			Center,
			/// <summary> Content is aligned to left side of display.</summary>
			Left,
			/// <summary> Content is aligned to right side of display.</summary>
			Right
		}
		
		private HorPositions horPosition;
		
		/// <summary>
		/// Set or get the horizontal position of the content.
		/// </summary>
		/// <value>
		/// Wanted horizontal position, one of the values from enum HorPositions.
		/// </value>
		public HorPositions HorPosition {
			set {
				horPosition=value;
			}
			get {
				return horPosition;
			}
		}
		
		/// <summary>
		/// List of possible vertical positions of the content. Default is middle.
		/// </summary>
		public enum VerPositions {
			/// <summary> Content is aligned to middle of display.</summary>
			Middle,
			/// <summary> Content is aligned to top of display.</summary>
			Top,
			/// <summary> Content is aligned to bottom of display.</summary>
			Bottom
		}
		
		private VerPositions verPosition;
		
		/// <summary>
		/// Set or get the vertical position of the content.
		/// </summary>
		/// <value>
		/// Wanted vertical position, one of the values from enum VerPositions.
		/// </value>
		public VerPositions VerPosition {
			set {
				verPosition=value;
			}
			get {
				return verPosition;
			}
		}
		
		/// <summary>
		/// List of possible movement types. Default is none, meaning all content will instantly appear on display.
		/// </summary>
		public enum Movements {
			/// <summary>Not moving, content is set to its location instantly on next update.</summary>
			None,
			/// <summary>Move from right to left and stop when wanted horizontal position is reached.</summary>
			MoveLeftAndStop,
			/// <summary>Move from right to left and keep moving until content is away from display. Horizontal position have no effect.</summary>
			MoveLeftAndPass,
			/// <summary>Move from left to right and stop when wanted horizontal position is reached.</summary>
			MoveRightAndStop,
			/// <summary>Move from left to right and keep moving until content is away from display. Horizontal position have no effect.</summary>
			MoveRightAndPass,
			/// <summary>Move from down to up and stop when wanted vertical position is reached.</summary>
			MoveUpAndStop,
			/// <summary>Move from down to up and keep moving until content is away from display. Vertical position have no effect.</summary>
			MoveUpAndPass,
			/// <summary>Move from up to down and stop when wanted vertical position is reached.</summary>
			MoveDownAndStop,
			/// <summary>Move from up to down and keep moving until content is away from display. Vertical position have no effect.</summary>
			MoveDownAndPass
		}
		
		private Movements movement;
		
		/// <summary>
		/// Set or get movement type of the content.
		/// </summary>
		/// <value>
		/// Wanted movement type, one of the values from enum Movements.
		/// </value>
		public Movements Movement {
			set {
				movement=value;
			}
			get {
				return movement;
			}
		}
		
		internal AbsCmdPosition() {
			horPosition=HorPositions.Center;
			verPosition=VerPositions.Middle;
			movement=Movements.None;
		}
		
		internal RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller, int[,] content) {
			
			int totalHeight=content.GetLength(0);
			int totalWidth=content.GetLength(1);
			
			Movements movement=this.Movement;
			float dotsPerSecond=this.getDotsPerSecond(controller.DefaultSpeedDotsPerSecond);
			if (dotsPerSecond<=0f) {
				movement=Movements.None;
			}
			
			int posX=0;
			int posY=0;
			if (this.HorPosition==HorPositions.Right) {
				posX=displayModel.Width-totalWidth;
			} else if (this.HorPosition==HorPositions.Center) {
				posX=(displayModel.Width-totalWidth)/2;
			}
			if (this.VerPosition==VerPositions.Bottom) {
				posY=displayModel.Height-totalHeight;
			} else if (this.VerPosition==VerPositions.Middle) {
				posY=(displayModel.Height-totalHeight)/2;
			}
			
			if (movement==Movements.None) {
				return (new RawCommandContent(content,posX,posY));
			}
			
			int push=0;
			
			if (movement==Movements.MoveLeftAndStop) {
				if (this.HorPosition==HorPositions.Left) {
					push=displayModel.Width;
				} else if (this.HorPosition==HorPositions.Right) {
					push=totalWidth;
				} else if (this.HorPosition==HorPositions.Center) {
					push=(displayModel.Width+totalWidth)/2;
				}
			} else if (movement==Movements.MoveRightAndStop) {
				if (this.HorPosition==HorPositions.Left) {
					push=totalWidth;
				} else if (this.HorPosition==HorPositions.Right) {
					push=displayModel.Width;
				} else if (this.HorPosition==HorPositions.Center) {
					push=(displayModel.Width+totalWidth)/2;
				}
			} else if (movement==Movements.MoveLeftAndPass || movement==Movements.MoveRightAndPass) {
				push=displayModel.Width+totalWidth;
			} else if (movement==Movements.MoveUpAndStop) {
				if (this.VerPosition==VerPositions.Top) {
					push=displayModel.Height;
				} else if (this.VerPosition==VerPositions.Bottom) {
					push=totalHeight;
				} else if (this.VerPosition==VerPositions.Middle) {
					push=(displayModel.Height+totalHeight)/2;
				}
			} else if (movement==Movements.MoveDownAndStop) {
				if (this.VerPosition==VerPositions.Top) {
					push=totalHeight;
				} else if (this.VerPosition==VerPositions.Bottom) {
					push=displayModel.Height;
				} else if (this.VerPosition==VerPositions.Middle) {
					push=(displayModel.Height+totalHeight)/2;
				}
			} else if (movement==Movements.MoveUpAndPass || movement==Movements.MoveDownAndPass) {
				push=displayModel.Height+totalHeight;
			}
			
			return (new RawCommandContent(content,posX,posY,movement,push,base.DotsPerStep,dotsPerSecond));
			
		}
		
	}

}
