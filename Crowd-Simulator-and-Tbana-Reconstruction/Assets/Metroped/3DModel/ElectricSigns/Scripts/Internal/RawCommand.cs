//    DotMatrix - RawCommand


namespace Leguar.DotMatrix.Internal {
	
	internal abstract class RawCommand {
		
		// Return remaining time to consume
		internal abstract float runStep(DisplayModel displayModel, float timeToConsume);
		
		internal abstract bool isFinished(DisplayModel displayModel);
		
	}

}
