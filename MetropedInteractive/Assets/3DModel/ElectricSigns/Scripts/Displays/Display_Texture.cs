//    DotMatrix - Display - Texture-based


using UnityEngine;
using System.Collections.Generic;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Display where display output is written to texture (Texture2D).
	/// </summary>
	public class Display_Texture : Display {

		[Header("Target material")]

		[SerializeField]
		private Material targetMaterial = null;

		/// <summary>
		/// Gets or sets the target material where texture is added. Usually set in Unity Inspector and DotMatrix will automatically set texture to this material when application starts.
		/// </summary>
		/// <value>
		/// Material where to add generated texture.
		/// </value>
		public Material TargetMaterial {
			set {
				targetMaterial = value;
				if (this.playingAndInit()) {
					targetMaterial.mainTexture = generatedRunTimeTexture;
				}
			}
			get {
				return targetMaterial;
			}
		}

		[Header("Dot colors")]

		[SerializeField]
		private Color[] colors=new Color[] {
			new Color(0.25f,0f,0f,0.75f),
			new Color(1f,0f,0f,1f)
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
		/// Color of single dots when it is turned to off-state (color 0).
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
		/// Color of single dots when it is turned to on-state (color 1).
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
		/// Sets color used in dots when dot state is set to 'index'.
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
		/// Gets color used in dots when dot state is set to 'index'.
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

		private Texture2D generatedEditorTexture;
		private Texture2D generatedRunTimeTexture;
		private bool runTimeUpdateLoopChanges;

		private Material editor_targetMaterial;

		/// <summary>
		/// Gets the texture containing dots of this display. Texture will be constantly updated when dots on display changes.
		/// Note that DotMatrix object need to be initialized before calling this method, either by calling DotMatrix.Init() or by setting it to auto-initialize on start.
		/// </summary>
		/// <returns>
		/// Texture2D object which width and height matches dot size of DotMatrix display.
		/// </returns>
		public Texture2D GetGeneratedTexture() {
			if (generatedRunTimeTexture == null) {
				Debug.LogError("DotMatrix ("+this.gameObject.name+"): Generated texture doesn't exist. Make sure DotMatrix is initialized before calling Display_Texture.getGeneratedTexture()", this.gameObject);
			}
			return generatedRunTimeTexture;
		}

		protected override void Update() {
			if (Application.isPlaying) {
				return;
			}
			base.Update();
			if (CreateDotsInEditor && generatedEditorTexture != null) {
				generatedEditorTexture.Apply();
			}
		}

		internal override void update(float deltaTime) {
			runTimeUpdateLoopChanges=false;
			base.update(deltaTime);
			if (runTimeUpdateLoopChanges) {
				generatedRunTimeTexture.Apply();
			}
		}

		internal void updateTexture(int drawX, int drawY, Color color) {
			if (!Application.isPlaying) {
				if (CreateDotsInEditor && generatedEditorTexture != null) {
					generatedEditorTexture.SetPixel(drawX, drawY, color);
				}
			} else {
				generatedRunTimeTexture.SetPixel(drawX, drawY, color);
				runTimeUpdateLoopChanges=true;
			}
		}

		internal override bool isNeedToRecreateEditorDots() {
			if (base.isNeedToRecreateEditorDots() || generatedEditorTexture==null || targetMaterial!=editor_targetMaterial) {
				editor_targetMaterial=targetMaterial;
				return true;
			}
			return false;
		}

		protected override void setEditorDisplayContent() {
			base.setEditorDisplayContent(false, 0, 0);
		}

		protected override void createDots(int width, int height, int borderDots, bool editorPreview) {

			if (editorPreview) {
				generatedEditorTexture = createTexture(width, height);
			} else {
				generatedRunTimeTexture = createTexture(width, height);
			}

			displayDots=new DisplayDot[height, width];
			displayBorderDots=(borderDots>0 ? new List<DisplayDot>() : null);

			for (int y = -borderDots; y<height+borderDots; y++) {

				for (int x = -borderDots; x<width+borderDots; x++) {

					DisplayDot displayDot = createDisplayDot(x, y);
					displayDot.setNewStateInstantly(0);

					if (y>=0 && y<height && x>=0 && x<width) {
						displayDots[y, x]=displayDot;
					} else {
						displayBorderDots.Add(displayDot);
					}

				}

			}

		}

		private Texture2D createTexture(int width, int height) {

			int w = width + BorderDots*2;
			int h = height + BorderDots*2;
			Texture2D texture = new Texture2D(w, h);

			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;

			Color[] colorArray = texture.GetPixels();
			for (int n = 0; n < colorArray.Length; n++) {
				colorArray[n] = colors[0];
			}
			texture.SetPixels(colorArray);
			texture.Apply();

			texture.hideFlags = HideFlags.HideAndDontSave;

			if (targetMaterial != null) {
				targetMaterial.mainTexture = texture;
			}

			return texture;

		}

		private DisplayDot createDisplayDot(int x, int y) {
			return (new DisplayDot_Texture(this, x, y));
		}

		protected override int getStateCount() {
			return ColorCount;
		}

		protected override void destroyEditorDots(bool immediate) {
			if (generatedEditorTexture != null) {
				if (immediate) {
					DestroyImmediate(generatedEditorTexture);
				} else {
					Destroy(generatedEditorTexture);
				}
			}
		}

	}

}
