//    DotMatrix - Display - Display as UI element


using UnityEngine;
using UnityEngine.UI;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Display where dots are based on images that change colors when turning on and off.
	/// This display is meant to be used is UI element. It's position follows pivot point of RectTransform.
	/// Display (dots) size can be also set to fit inside the RectTransform so the actual size of display
	/// will change depending on UI layout settings.
	/// </summary>
	public class Display_UI : DisplayObj {
		
		[Header("Dot size in units")]

		[SerializeField]
		private Vector2 dotSize=Vector2.one;

		/// <summary>
		/// Size of single dot.
		/// </summary>
		/// <value>
		/// Size of single dot as Vector2
		/// </value>
		public Vector2 DotSize {
			set {
				if (base.runTimeCheck(value,dotSize,"DotSize")) {
					dotSize=value;
				}
			}
			get {
				return dotSize;
			}
		}

		[Header("Dot colors")]

		[SerializeField]
		private Color[] colors=new Color[] {
			new Color(0f,0f,0.25f,0.5f),
			new Color(0f,0f,1f,1f)
		};
		
		/// <summary>
		/// Gets or sets the count of colors this display's dots can have. Default and minimum is two colors.
		/// </summary>
		/// <value>
		/// Amount of different possible color states.
		/// </value>
		public int ColorCount {
			set {
				if (value!=colors.Length && this.runTimeCheck(value,colors.Length,"ColorCount")) {
					Color[] oldColorsArray=colors;
					colors=new Color[value];
					for (int n=0; n<value; n++) {
						colors[n]=(n<oldColorsArray.Length?oldColorsArray[n]:new Color(((n==1 || n==4 || n==5 || n==7)?1f:0f),((n==2 || n==4 || n==6 || n==7)?1f:0f),((n==3 || n==5 || n==6 || n==7)?1f:0f),1f));
					}
				}
			}
			get {
				return colors.Length;
			}
		}
		
		/// <summary>
		/// Color of single dot sprite when it is turned to off-state (color 0).
		/// This have same effect than using SetColor(0,Color) or GetColor(0,Color) methods and is here for backward compatibility.
		/// </summary>
		/// <value>
		/// Dot color when in off-state.
		/// </value>
		public Color OffColor {
			set {
				if (base.playingInitAndChanged(value,colors[0])) {
					this.needFullStateUpdate=true;
				}
				colors[0]=value;
			}
			get {
				return colors[0];
			}
		}
		
		/// <summary>
		/// Color of single dot sprite when it is turned to on-state (color 1).
		/// This have same effect than using SetColor(1,Color) or GetColor(1,Color) methods and is here for backward compatibility.
		/// </summary>
		/// <value>
		/// Dot color when in on-state.
		/// </value>
		public Color OnColor {
			set {
				if (base.playingInitAndChanged(value,colors[1])) {
					this.needFullStateUpdate=true;
				}
				colors[1]=value;
			}
			get {
				return colors[1];
			}
		}
		
		/// <summary>
		/// List of different types how to fit this display in UI RectTransform.
		/// </summary>
		public enum UIFits {
			/// <summary>Use dot size defined in Display.</summary>
			KeepDotSize,
			/// <summary>Shrink or enlarge all the dots so that they fill RectTransform of this component.</summary>
			FitToRectTransform,
			/// <summary>Shrink or enlarge all the dots so that they fill RectTransform of this component but keep original aspect ratio of dots.</summary>
			FitButKeepAspectRatio
		}

		[Header("UI Fit")]

		[SerializeField]
		private UIFits uiFit=UIFits.FitButKeepAspectRatio;

		/// <summary>
		/// Set or get UI fit type, one of the values from enum UIFits. When using type that tries to fit the display
		/// inside RectTransform, make sure this GameObject RectTransform width and height are not set to zero.
		/// </summary>
		/// <value>
		/// One of the choices from enum UIFits
		/// </value>
		public UIFits UIFit {
			set {
				if (base.runTimeCheck(value,uiFit,"UIFit")) {
					uiFit=value;
				}
			}
			get {
				return uiFit;
			}
		}

		/// <summary>
		/// List of positions inside RectTransform if display doesn't fill whole RectTransform area.
		/// </summary>
		public enum UIPositions {
			/// <summary>Position follows pivot point.</summary>
			FollowPivotPoint,
			/// <summary>Set display to upper left corner of RectTransform.</summary>
			UpperLeft,
			/// <summary>Set display to upper center position of RectTransform.</summary>
			UpperCenter,
			/// <summary>Set display to upper right corner of RectTransform.</summary>
			UpperRight,
			/// <summary>Set display to middle left position of RectTransform.</summary>
			MiddleLeft,
			/// <summary>Set display to middle center position of RectTransform.</summary>
			MiddleCenter,
			/// <summary>Set display to middle right position of RectTransform.</summary>
			MiddleRight,
			/// <summary>Set display to lower left corner of RectTransform.</summary>
			LowerLeft,
			/// <summary>Set display to lower center position of RectTransform.</summary>
			LowerCenter,
			/// <summary>Set display to lower right corner of RectTransform.</summary>
			LowerRight
		}

		[SerializeField]
		private UIPositions uiPosition=UIPositions.FollowPivotPoint;

		/// <summary>
		/// Set or get position of the display inside RectTransform, one of the values from enum UIPositions.
		/// This setting have no effect if UIFit is set FitToRectTransform because then display is filling whole RectTransform area.
		/// </summary>
		/// <value>
		/// One of the choices from enum UIPositions
		/// </value>
		public UIPositions UIPosition {
			set {
				if (base.runTimeCheck(value,uiPosition,"UIPosition")) {
					uiPosition=value;
				}
			}
			get {
				return uiPosition;
			}
		}

		private RectTransform rectTransform;

		/// <summary>
		/// Sets color used in dot sprites when dot state is set to 'index'.
		/// </summary>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' state colors.
		/// </param>
		/// <param name="color">
		/// Color to use.
		/// </param>
		public void SetColor(int state, Color color) {
			if (base.playingInitAndChanged(color,colors[state])) {
				this.needFullStateUpdate=true;
			}
			colors[state]=color;
		}

		/// <summary>
		/// Gets color used in dot sprites when dot state is set to 'index'.
		/// </summary>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' state colors.
		/// </param>
		/// <returns>
		/// Color used.
		/// </returns>
		public Color GetColor(int state) {
			return colors[state];
		}
		
		void LateUpdate() {
			if (base.playingAndInit()) {
				if (this.transform.hasChanged) {
					base.setSizeAndPositionOfAllDots();
					transform.hasChanged=false;
				}
			}
		}

		internal override GameObject createGroupObject(string groupObjectName, Transform parent) {
			GameObject groupObject=new GameObject(groupObjectName);
			setParent(groupObject,parent);
			RectTransform rectTransform=groupObject.AddComponent<RectTransform>();
			groupObject.transform.localPosition=Vector3.zero;
			groupObject.transform.localRotation=Quaternion.identity;
			groupObject.transform.localScale=Vector3.one;
			rectTransform.anchoredPosition3D=Vector3.zero;
			rectTransform.sizeDelta=Vector2.zero;
			rectTransform.anchorMin=Vector2.zero;
			rectTransform.anchorMax=Vector2.zero;
			rectTransform.pivot=Vector2.zero;
			return groupObject;
		}
		
		internal override void setParent(GameObject ntGameObject, Transform parent) {
			Transform got=ntGameObject.transform;
			got.SetParent(parent);
			got.localPosition=Vector3.zero;
			got.localRotation=Quaternion.identity;
			got.localScale=Vector3.one;
		}
		
		internal override DisplayDot createDisplayDot(GameObject dotObject, int x, int y) {
			Image image=dotObject.GetComponent<Image>();
			return (new DisplayDot_UI(this,x,y,image));
		}
		
		internal override void setDisplayDotScaleAndPosition(DisplayDot displayDot) {
			if (rectTransform==null) {
				rectTransform=this.GetComponent<RectTransform>();
			}
			((DisplayDot_UI)(displayDot)).setObjectScaleAndPosition(this,uiPosition,rectTransform);
		}

		protected override int getStateCount() {
			return ColorCount;
		}

		/// <summary>
		/// Check whatever this object is child of UI Canvas and tries to set the parent if not.
		/// If there is no canvas in hierarchy, this method will create one.
		/// If this object parent is changed, object's scale and anchored position will reset also.
		/// 
		/// Note that this method is normally used only internally in Unity Editor and there is no need to call this when application is running.
		/// If you create new UI-type DotMatrix from prefab runtime, rather make sure your scene already contains Canvas and then use normal
		/// myUIDotMatrix.transform.SetParent(myKnownCanvasOrOtherUIElement.transform) method to set parent as with any UI object.
		/// </summary>
		public void checkAndSetParent() {
			if (this.GetComponentInParent<Canvas>()==null) {
				Canvas canvas=getOrCreateCanvas();
				this.transform.SetParent(canvas.transform,false);
				this.transform.localScale=Vector3.one;
				this.GetComponent<RectTransform>().anchoredPosition3D=Vector3.zero;
			}
		}

		private Canvas getOrCreateCanvas() {
			Canvas canvas=Transform.FindObjectOfType<Canvas>();
			if (canvas==null) {
				GameObject canvasObject=new GameObject("Canvas");
				canvas=canvasObject.AddComponent<Canvas>();
				canvas.renderMode=RenderMode.ScreenSpaceOverlay;
				canvasObject.AddComponent<CanvasScaler>();
				canvasObject.AddComponent<GraphicRaycaster>();
				canvasObject.layer=this.gameObject.layer;
			}
			return canvas;
		}

	}
	
}
