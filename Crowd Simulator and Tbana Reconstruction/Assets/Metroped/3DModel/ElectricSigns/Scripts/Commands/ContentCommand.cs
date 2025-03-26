//    DotMatrix - Command - Content


using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Command to add free content on display. Content is defined by two dimensional int array where each value represent wanted state of dot.
	/// </summary>
	public class ContentCommand : AbsCmdPosition {
		
		private int[,] content;
		
		/// <summary>
		/// Create new content command. By default content is displayed in middle and center of the display.
		/// </summary>
		/// <param name="content">
		/// Free content to be added on screen.
		/// </param>
		public ContentCommand(int[,] content) {
			this.content=content;
		}
		
		internal override RawCommand getRawCommand(DotMatrix dotMatrix, DisplayModel displayModel, Controller controller) {
			return base.getRawCommand(dotMatrix,displayModel,controller,content);
		}
		
	}

}
