//    DotMatrix - Command - Callback


using System;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Command to call certain action when this command is reached. Useful if you need to get notification when DotMatrix have reached certain point of command queue.
	/// </summary>
	public class CallbackCommand : AbsCmd {
		
		private Action callback;
		
		/// <summary>
		/// Creates new callback command. When Controller reaches this command, callback is called and Controller continues from next command in queue.
		/// </summary>
		/// <param name="callback">
		/// Action to be called.
		/// </param>
		public CallbackCommand(Action callback) {
			this.callback=callback;
		}
		
		internal override RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller) {
			return (new RawCommandCallback(callback));
		}
		
	}

}
