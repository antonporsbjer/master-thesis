//    DotMatrix - AbsCmdSpeed


using UnityEngine;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Abstract base class for all commands that may be executed with different speeds, such as scrolling text.
	/// </summary>
	public abstract class AbsCmdSpeed : AbsCmd {
		
		private float dotsPerSecond;
		private bool dotsPerSecondSet;
		
		/// <summary>
		/// Set moving content speed. If this is not set, default value from Controller is used.
		/// </summary>
		/// <value>
		/// Speed in dots per second.
		/// </value>
		public float DotsPerSecond {
			set {
				dotsPerSecond=value;
				dotsPerSecondSet=true;
			}
			get {
				if (!dotsPerSecondSet) {
					Debug.LogError("Unable to get command 'DotsPerSecond' since value is not set. Command will use Controller default value when executed.");
				}
				return dotsPerSecond;
			}
		}

		private int dotsPerStep;

		/// <summary>
		/// Set moving content to jump in steps. Default is 1 for normal dot-by-dot scrolling and clearing effects.
		/// Setting this to higher value make scrolling or clearing happen in jumps which is useful especially in displays with grouped dots.
		/// </summary>
		/// <value>
		/// How many dots content moves at once.
		/// </value>
		public int DotsPerStep {
			set {
				if (value<1) {
					Debug.LogError("Unable to set command 'DotsPerStep' to "+value+", value must be positive.");
					return;
				}
				dotsPerStep=value;
			}
			get {
				return dotsPerStep;
			}
		}

		internal AbsCmdSpeed() {
			dotsPerSecondSet=false;
			dotsPerStep=1;
		}
		
		internal float getDotsPerSecond(float controllerDefault) {
			return (dotsPerSecondSet?dotsPerSecond:controllerDefault);
		}
		
	}

}
