//    DotMatrix - Display - DisplayObj


using UnityEngine;
using System.Collections.Generic;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {

	/// <summary>
	/// Extended abstract base class for all the display types that uses separate dot objects (3D, sprite based and UI).
	/// </summary>
	public abstract class DisplayObj : Display {

		private const string EDITOR_GROUP_NAME = "Generated EditorDots";
		private const string RUNTIME_GROUP_NAME = "Generated RunTimeDots";

		[Header("Prefabs")]

		[SerializeField]
		private GameObject dotPrefab;

		/// <summary>
		/// Prefab for a single dot in display.
		/// </summary>
		/// <value>
		/// Prefab object.
		/// </value>
		public GameObject DotPrefab {
			set {
				if (runTimeCheck(value, dotPrefab, "DotPrefab")) {
					dotPrefab=value;
				}
			}
			get {
				return dotPrefab;
			}
		}

		/// <summary>
		/// Different selections how dots' layers are set.
		/// </summary>
		public enum DotLayers {
			/// <summary>Keep dots layer same as prefab layer.</summary>
			KeepDotPrefabLayer,
			/// <summary>Set dots layer same as DotMatrix display layer.</summary>
			CopyDotMatrixDisplayLayer
		}

		[SerializeField]
		private DotLayers dotLayer = DotLayers.KeepDotPrefabLayer;

		/// <summary>
		/// Set or get selection how each dot layer is set when they are created runtime or in editor.
		/// 
		/// By default this is DotLayers.KeepPrefabLayer which means that dots' layers are not changed after they are created from dot prefab.
		/// When setting this to DotLayers.CopyDotMatrixLayer, each dot layer is set same as actual DotMatrix gameobject layer.
		/// </summary>
		/// <value>
		/// One of the choices from enum DotLayers
		/// </value>
		public DotLayers DotLayer {
			set {
				if (runTimeCheck(value, dotLayer, "DotLayer")) {
					dotLayer=value;
				}
			}
			get {
				return dotLayer;
			}
		}

		[Header("Dot size in units")]

		[SerializeField]
		private Vector2 dotSpacing = Vector2.zero;

		/// <summary>
		/// Spacing between dots. 0 means no spacing. Negative values will cause dots to overlap each other but may result undesired visual glitches.
		/// </summary>
		/// <value>
		/// Spacing between dots as Vector2
		/// </value>
		public Vector2 DotSpacing {
			set {
				if (runTimeCheck(value, dotSpacing, "DotSpacing")) {
					dotSpacing=value;
				}
			}
			get {
				return dotSpacing;
			}
		}

		[Header("Dot grouping")]

		[SerializeField]
		private bool dotGroupingEnabled = false;

		/// <summary>
		/// Set or get the setting whatever dots are grouped, ie there is extra space after certain amount of dots. Typically this is used to create visual effect that display is actually
		/// made of multiple smaller displays, for example each letter in is in its own dot group.
		/// </summary>
		/// <remarks>
		/// This is special mode of display, there are several things to note when using this:<br>
		/// - When adding text to display and you want each letter to be in its own dot group, make sure font size matches the dot group size and character/line spacing in TextCommand is set to 0.<br>
		/// - Scrolling text may look broken since scrolling still happens dot by dot.
		/// </remarks>
		/// <value>
		/// Dot grouping enabled setting.
		/// </value>
		public bool DotGroupingEnabled {
			set {
				if (runTimeCheck(value, dotGroupingEnabled, "DotGroupingEnabled")) {
					dotGroupingEnabled=value;
				}
			}
			get {
				return dotGroupingEnabled;
			}
		}

		[SerializeField]
		private int dotGroupSizeHorizontal = 5;

		/// <summary>
		/// Set or get how many dots are in each group horizontally.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if 'DotGroupingEnabled' is false.
		/// </remarks>
		/// <value>
		/// Number of dots in each group horizontally.
		/// </value>
		public int DotGroupSizeHorizontal {
			set {
				if (runTimeCheck(value, dotGroupSizeHorizontal, "DotGroupSizeHorizontal")) {
					dotGroupSizeHorizontal=value;
				}
			}
			get {
				return dotGroupSizeHorizontal;
			}
		}

		[SerializeField]
		private int dotGroupSizeVertical = 7;

		/// <summary>
		/// Set or get how many dots are in each group vertically.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if 'DotGroupingEnabled' is false.
		/// </remarks>
		/// <value>
		/// Number of dots in each group vertically.
		/// </value>
		public int DotGroupSizeVertical {
			set {
				if (runTimeCheck(value, dotGroupSizeVertical, "DotGroupSizeVertical")) {
					dotGroupSizeVertical=value;
				}
			}
			get {
				return dotGroupSizeVertical;
			}
		}

		[SerializeField]
		private Vector2 dotGroupRelativeSpacing = Vector2.one;

		/// <summary>
		/// Set or get relative spacing between dot groups. 1.0 means space is equal to one dot.
		/// 0.0 means no space at all which equals to not having dot grouping enabled at all.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if 'DotGroupingEnabled' is false.
		/// </remarks>
		/// <value>
		/// Relative spacing between dot groups.
		/// </value>
		public Vector2 DotGroupRelativeSpacing {
			set {
				if (runTimeCheck(value, dotGroupRelativeSpacing, "DotGroupRelativeSpacing")) {
					dotGroupRelativeSpacing=value;
				}
			}
			get {
				return dotGroupRelativeSpacing;
			}
		}

		private GameObject editor_dotPrefab;
		private DotLayers editor_dotLayer;

		internal override bool isNeedToRecreateEditorDots() {
			if (base.isNeedToRecreateEditorDots() || this.transform.Find(EDITOR_GROUP_NAME)==null || dotPrefab!=editor_dotPrefab || dotLayer!=editor_dotLayer) {
				editor_dotPrefab=dotPrefab;
				editor_dotLayer=dotLayer;
				return true;
			}
			return false;
		}

		protected override void setEditorDisplayContent() {
			base.setEditorDisplayContent(dotGroupingEnabled, dotGroupSizeHorizontal, dotGroupSizeVertical);
		}

		protected override void createDots(int width, int height, int borderDots, bool editorPreview) {

			displayDots=new DisplayDot[height, width];
			displayBorderDots=(borderDots>0 ? new List<DisplayDot>() : null);

			GameObject groupObject = createGroupObject((editorPreview ? EDITOR_GROUP_NAME : RUNTIME_GROUP_NAME), this.transform, editorPreview);

			GameObject borderObject = null;
			if (borderDots>0) {
				borderObject=createGroupObject("Border", groupObject.transform, editorPreview);
			}

			for (int y = -borderDots; y<height+borderDots; y++) {

				GameObject rowObject = null;
				if (y>=0 && y<height) {
					rowObject=createGroupObject("Row "+y, groupObject.transform, editorPreview);
				}

				for (int x = -borderDots; x<width+borderDots; x++) {

					GameObject dotObject = (GameObject)(Instantiate(dotPrefab));
					if (y>=0 && y<height && x>=0 && x<width) {
						dotObject.name="Col "+x;
						setParent(dotObject, rowObject.transform);
					} else {
						dotObject.name="Border dot";
						setParent(dotObject, borderObject.transform);
					}

					setDotLayerAndHideFlags(dotObject, editorPreview);

					DisplayDot displayDot = createDisplayDot(dotObject, x, y);
					setDisplayDotScaleAndPosition(displayDot);
					displayDot.setNewStateInstantly(0);

					if (y>=0 && y<height && x>=0 && x<width) {
						displayDots[y, x]=displayDot;
					} else {
						displayBorderDots.Add(displayDot);
					}

				}

			}

		}

		private void setDotLayerAndHideFlags(GameObject dotObject, bool editorPreview) {
			if (dotLayer==DotLayers.CopyDotMatrixDisplayLayer) {
				dotObject.layer=this.gameObject.layer;
			}
			if (editorPreview) {
				dotObject.hideFlags=HideFlags.DontSave;
			}
			foreach (Transform child in dotObject.transform) {
				setDotLayerAndHideFlags(child.gameObject, editorPreview);
			}
		}

		protected override void setSizeAndPositionOfAllDots() {
			// Set size and position
			for (int y = 0; y<HeightInDots; y++) {
				for (int x = 0; x<WidthInDots; x++) {
					setDisplayDotScaleAndPosition(displayDots[y, x]);
				}
			}
			// Same for border dots
			if (displayBorderDots!=null) {
				foreach (DisplayDot displayBorderDot in displayBorderDots) {
					setDisplayDotScaleAndPosition(displayBorderDot);
				}
			}
		}

		protected override void destroyEditorDots(bool immediate) {
			foreach (Transform t in this.transform) {
				if (t.name.Equals(EDITOR_GROUP_NAME)) {
					if (immediate) {
						DestroyImmediate(t.gameObject);
					} else {
						Destroy(t.gameObject);
					}
				}
			}
		}

		protected GameObject createGroupObject(string groupObjectName, Transform parent, bool editorPreview) {
			GameObject groupObject = createGroupObject(groupObjectName, parent);
			groupObject.layer=parent.gameObject.layer;
			if (editorPreview) {
				groupObject.hideFlags=HideFlags.DontSave;
			}
			return groupObject;
		}

		internal abstract GameObject createGroupObject(string groupObjectName, Transform parent);
		internal abstract void setParent(GameObject gameObject, Transform parent);

		internal abstract DisplayDot createDisplayDot(GameObject dotObject, int x, int y);
		internal abstract void setDisplayDotScaleAndPosition(DisplayDot displayDot);

	}

}
