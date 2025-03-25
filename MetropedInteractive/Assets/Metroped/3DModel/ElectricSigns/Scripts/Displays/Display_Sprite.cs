//    DotMatrix - Display - Sprite-based for 3D/2D world


using UnityEngine;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Display where dots are based on flat sprites that change colors when turning on and off.
	/// </summary>
	public class Display_Sprite : DisplayObj {

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

		internal override GameObject createGroupObject(string groupObjectName, Transform parent) {
			GameObject groupObject=new GameObject(groupObjectName);
			setParent(groupObject,parent);
			groupObject.transform.localPosition=Vector3.zero;
			groupObject.transform.localRotation=Quaternion.identity;
			groupObject.transform.localScale=Vector3.one;
			return groupObject;
		}
		
		internal override void setParent(GameObject ntGameObject, Transform parent) {
			Transform got=ntGameObject.transform;
			got.parent=parent;
			got.localPosition=Vector3.zero;
			got.localRotation=Quaternion.identity;
			got.localScale=Vector3.one;
		}
		
		internal override DisplayDot createDisplayDot(GameObject dotObject, int x, int y) {
			SpriteRenderer spriteRenderer=dotObject.GetComponent<SpriteRenderer>();
			return (new DisplayDot_Sprite(this,x,y,spriteRenderer));
		}
		
		internal override void setDisplayDotScaleAndPosition(DisplayDot displayDot) {
			((DisplayDot_Sprite)(displayDot)).setObjectScaleAndPosition(this);
		}

		protected override int getStateCount() {
			return ColorCount;
		}

	}

}
