//    DotMatrix - RawCommand - Callback


using System;

namespace Leguar.DotMatrix.Internal {
	
	internal class RawCommandCallback : RawCommand {
		
		private Action callback;
		
		internal RawCommandCallback(Action callback) {
			this.callback=callback;
		}
		
		internal override float runStep(DisplayModel displayModel, float timeToConsume) {
			callback();
			return timeToConsume;
		}
		
		internal override bool isFinished(DisplayModel displayModel) {
			return true;
		}
		
	}

}
