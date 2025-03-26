//    DotMatrix - Command - Clear


using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Command to clear or fill the display. Clearing may be instant or happen with some effect like scrolling all the content away from display,
	/// or clearing/filling display row by row or columns by column.
	/// </summary>
	public class ClearCommand : AbsCmdSpeed {
		
		/// <summary>
		/// List of methods how display can be cleared or filled. Default is instant.
		/// </summary>
		public enum Methods {
			/// <summary>Instant on next update.</summary>
			Instant,
			/// <summary>Move everything left until display is clear.</summary>
			MoveLeft,
			/// <summary>Move everything right until display is clear.</summary>
			MoveRight,
			/// <summary>Move everything up until display is clear.</summary>
			MoveUp,
			/// <summary>Move everything down until display is clear.</summary>
			MoveDown,
			/// <summary>Clear display column by column starting from left.</summary>
			ColumnByColumnFromLeft,
			/// <summary>Clear display column by column starting from right.</summary>
			ColumnByColumnFromRight,
			/// <summary>Clear display row by row starting from top.</summary>
			RowByRowFromTop,
			/// <summary>Clear display row by row starting from bottom.</summary>
			RowByRowFromBottom
		}
		
		private Methods method;
		
		/// <summary>
		/// Gets or sets the method how display is cleared or filled, one of the values from enum Methods.
		/// </summary>
		/// <value>
		/// Method to use.
		/// </value>
		public Methods Method {
			set {
				method=value;
			}
			get {
				return method;
			}
		}
		
		private int targetState;
		
		/// <summary>
		/// Creates new clear command.
		/// </summary>
		public ClearCommand() : this(0) {
		}
		
		/// <summary>
		/// Creates new clear or fill command.
		/// </summary>
		/// <remarks>
		/// This constructor is available mostly for backward compatibility for old versions.
		/// If parameter is false, this is same as ClearCommand() and also same as ClearCommand(0)
		/// If parameter is true, this is same as ClearCommand(1)
		/// </remarks>
		/// <param name="fill">
		/// If true, display is filled instead of cleared.
		/// </param>
		public ClearCommand(bool fill) : this (fill?1:0) {
		}

		/// <summary>
		/// Creates new clear or fill command.
		/// </summary>
		/// <param name="targetState">
		/// New wanted state (color/rotation) of all dots.
		/// </param>
		public ClearCommand(int targetState) {
			this.targetState=targetState;
			method=Methods.Instant;
		}

		internal override RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller) {
			float dotsPerSecond=this.getDotsPerSecond(controller.DefaultSpeedDotsPerSecond);
			if (method==Methods.Instant || dotsPerSecond<=0f) {
				return (new RawCommandClear(targetState));
			} else if (method==ClearCommand.Methods.MoveLeft || method==ClearCommand.Methods.MoveRight || method==ClearCommand.Methods.MoveUp || method==ClearCommand.Methods.MoveDown) {
				return (new RawCommandClear(targetState,method,base.DotsPerStep,dotsPerSecond));
			} else if (method==ClearCommand.Methods.ColumnByColumnFromLeft || method==ClearCommand.Methods.ColumnByColumnFromRight) {
				return (new RawCommandClear(targetState,method,base.DotsPerStep,dotsPerSecond,displayModel.Width));
			} else if (method==ClearCommand.Methods.RowByRowFromTop || method==ClearCommand.Methods.RowByRowFromBottom) {
				return (new RawCommandClear(targetState,method,base.DotsPerStep,dotsPerSecond,displayModel.Height));
			}
			// Should be never reached
			return null;
		}
		
	}
	
}
