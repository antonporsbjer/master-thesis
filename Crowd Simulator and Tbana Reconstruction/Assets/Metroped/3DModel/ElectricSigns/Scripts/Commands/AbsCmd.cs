//    DotMatrix - AbsCmd


using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Abstract base class for all commands to be added to Controller.
	/// </summary>
	public abstract class AbsCmd {
		
		private bool repeat;
		
		/// <summary>
		/// Get or set a value indicating whether this command should repeat. If repeat is true, command is added back to Controller queue
		/// immediately after its execution starts.
		/// </summary>
		/// <value>
		/// True if command repeats, false otherwise.
		/// </value>
		public bool Repeat {
			set {
				repeat=value;
			}
			get {
				return repeat;
			}
		}
		
		internal AbsCmd() {
			repeat=false;
		}
		
		internal abstract RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller);
		
	}

}
