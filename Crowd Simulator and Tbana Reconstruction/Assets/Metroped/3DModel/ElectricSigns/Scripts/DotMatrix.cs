//    DotMatrix


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Main class of DotMatrix. This script is attached to all DotMatrix prefabs (3D, sprite based and UI).
	/// </summary>
	public class DotMatrix : MonoBehaviour {
		
		[Header("Auto Init")]
		/// <summary>
		/// Defines whatever DotMatrix should be initialized immediately when this script's Start function is called by Unity engine.
		/// Set false if you wish to have more control when DotMatrix display (dots) is created or you need to change some of the values
		/// (like width and height) during runtime before creating the display.
		/// </summary>
		public bool initOnStart=true;
		
		private bool doneInit;

		private DisplayModel displayModel;
		private Controller controller;
		private Display display;

		void Start() {
			if (initOnStart) {
				Init();
			}
		}
		
		// Single update to make sure controller and display are updated in correct order (without touching script execution order)
		void Update() {
			// Nothing to do if not yet initialized
			if (!doneInit) {
				return;
			}
			// Update controller commands, then display
			controller.update(Time.deltaTime);
			display.update(Time.deltaTime);
		}
		
		/// <summary>
		/// Initializes this DotMatrix display. This will create the actual visible display and create Controller and DisplayModel. This method does nothing if this
		/// instance is already initialized. After initialization, many parameters can't be changed any more (like width and height of display) and changing some
		/// parameters may cause undesired or delayed effects. Generally it is not recommended to change any parameters after initialization.
		/// </summary>
		public void Init() {
			
			// Already done?
			if (doneInit) {
				return;
			}

			// Actual init
			GetDisplay(); // Make sure 'display' is set
			displayModel=new DisplayModel(display.WidthInDots,display.HeightInDots);
			controller=new Controller(this,displayModel);
			display.init(this,displayModel);
			doneInit=true;
			
		}
		
		/// <summary>
		/// Resets the DotMatrix. This will stop executing any commands, clear the Controller command queue and clear the display.
		/// This instance will return to same state where it was after Init() was done.
		/// </summary>
		public void Reset() {
			if (!doneInit) {
				Init();
				return;
			}
			controller.ClearCommandsAndStop();
			displayModel.Clear();
		}
		
		/// <summary>
		/// Gets the DotMatrix Controller. This will also cause this DotMatrix to initialize if it haven't done yet.
		/// </summary>
		/// <returns>
		/// The controller that can be used to send commands to DotMatrix display.
		/// </returns>
		public Controller GetController() {
			if (!doneInit) {
				Init();
			}
			return controller;
		}
		
		/// <summary>
		/// Gets the DotMatrix DisplayModel. This will also cause this DotMatrix to initialize if it haven't done yet.
		/// </summary>
		/// <returns>
		/// The display model that can be used to access display directly without using Controller.
		/// </returns>
		public DisplayModel GetDisplayModel() {
			if (!doneInit) {
				Init();
			}
			return displayModel;
		}

		/// <summary>
		/// Gets the DotMatrix Display. This will not cause DotMatrix to initialize.
		/// If this DotMatrix object was created from prefab on runtime, you can use this method to get the display and
		/// change display parameters and then call Init() method in this class to create the display dots.
		/// </summary>
		/// <returns>
		/// Reference to display that is attached to this game object. You can cast resulting Display object to
		/// Display_Sprite, Display_UI, Display_3D or Display_Texture depending of the type of attached display.
		/// </returns>
		public Display GetDisplay() {
			if (display==null) {
				display=this.GetComponent<Display>();
				if (display==null) {
					Debug.LogError("DotMatrix ("+this.gameObject.name+"): There is no Display script attached to this gameobject",this.gameObject);
				}
			}
			return display;
		}

	}

}
