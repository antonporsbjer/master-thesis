//    DotMatrix - RawCommand


using UnityEngine;

namespace Leguar.DotMatrix.Internal {
	
	internal class RawCommandDelay : RawCommand {
		
		private float delay;
		private bool finished;
		
		internal RawCommandDelay(float delay) {
			this.delay=delay;
			finished=false;
		}
		
		internal override float runStep(DisplayModel displayModel, float timeToConsume) {
			if (timeToConsume>=delay) {
				timeToConsume-=delay;
				finished=true;
			}
			return timeToConsume;
		}
		
		internal override bool isFinished(DisplayModel displayModel) {
			return finished;
		}
		
	}

}
