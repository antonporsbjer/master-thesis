//    DotMatrix - Display - 3D-object based for 3D world


using System;
using UnityEngine;
using Leguar.DotMatrix.Internal;

namespace Leguar.DotMatrix {
	
	/// <summary>
	/// Display where dots are based on actual 3D objects which rotates when turning on and off.
	/// </summary>
	public class Display_3D : DisplayObj {
		
		[Header("Dot size in units")]

		[SerializeField]
		private Vector3 dotSize=Vector3.one;
		
		/// <summary>
		/// Size of single dot.
		/// </summary>
		/// <value>
		/// Size of single dot as Vector3
		/// </value>
		public Vector3 DotSize {
			set {
				if (base.runTimeCheck(value,dotSize,"DotSize")) {
					dotSize=value;
				}
			}
			get {
				return dotSize;
			}
		}

		/// <summary>
		/// Different selections how dots are changed when their state changes.
		/// </summary>
		public enum StateChangeStyles {
			/// <summary>Rotate dot object to different rotation.</summary>
			RotateDot,
			/// <summary>Change dot object material.</summary>
			ChangeDotMaterial
		}

		[Header("State change style")]

		[SerializeField]
		private StateChangeStyles stateChangeStyle = StateChangeStyles.RotateDot;

		/// <summary>
		/// Set or get selection how dots change when their state changes.
		/// 
		/// By default this is StateChangeStyles.RotateDot which means that dots rotate to different rotation when their state changes between on, off or different additional states.
		/// When setting this to StateChangeStyles.ChangeDotMaterial, dot material is changed when its state changes.
		/// </summary>
		/// <value>
		/// One of the choices from enum StateChangeStyles
		/// </value>
		public StateChangeStyles StateChangeStyle {
			set {
				if (runTimeCheck(value, stateChangeStyle, "StateChangeStyle")) {
					stateChangeStyle=value;
				}
			}
			get {
				return stateChangeStyle;
			}
		}

		[Header("Dot rotations")]

		[SerializeField]
		private Vector3[] rotations=new Vector3[] {
			new Vector3(0f,90f,0f),
			new Vector3(0f,0f,0f)
		};

		/// <summary>
		/// Gets or sets the count of rotations this display's dots can have. Default and minimum is two rotations.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.ChangeDotMaterial
		/// </remarks>
		/// <value>
		/// Amount of different possible rotation states.
		/// </value>
		public int RotationCount {
			set {
				if (value!=rotations.Length && this.runTimeCheck(value,rotations.Length,"RotationCount")) {
					Vector3[] oldRotationsArray=rotations;
					rotations=new Vector3[value];
					for (int n=0; n<value; n++) {
						rotations[n]=(n<oldRotationsArray.Length?oldRotationsArray[n]:Vector3.zero);
					}
				}
			}
			get {
				return rotations.Length;
			}
		}

		/// <summary>
		/// Rotation of single dot object when it is turned to off-state.
		/// This have same effect than using SetRotation(0,Vector3) or GetRotation(0,Vector3) methods and is here for backward compatibility.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.ChangeDotMaterial
		/// </remarks>
		/// <value>
		/// Dot rotation when in off-state.
		/// </value>
		public Vector3 OffRotation {
			set {
				if (base.playingInitAndChanged(value,rotations[0])) {
					this.needFullStateUpdate=true;
				}
				rotations[0]=value;
			}
			get {
				return rotations[0];
			}
		}

		/// <summary>
		/// Rotation of single dot object when it is turned to on-state.
		/// This have same effect than using SetRotation(1,Vector3) or GetRotation(1,Vector3) methods and is here for backward compatibility.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.ChangeDotMaterial
		/// </remarks>
		/// <value>
		/// Dot rotation when in on-state.
		/// </value>
		public Vector3 OnRotation {
			set {
				if (base.playingInitAndChanged(value,rotations[1])) {
					this.needFullStateUpdate=true;
				}
				rotations[1]=value;
			}
			get {
				return rotations[1];
			}
		}

		/// <summary>
		/// Sets rotation used in dot objects when dot state is set to 'state'.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.ChangeDotMaterial
		/// </remarks>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' state rotations.
		/// </param>
		/// <param name="rotation">
		/// Rotation to use.
		/// </param>
		public void SetRotation(int state, Vector3 rotation) {
			if (base.playingInitAndChanged(rotation,rotations[state])) {
				this.needFullStateUpdate=true;
			}
			rotations[state]=rotation;
		}

		/// <summary>
		/// Gets rotation used in dot objects when dot state is set to 'state'.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.ChangeDotMaterial
		/// </remarks>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' state rotations.
		/// </param>
		/// <returns>
		/// Rotation used.
		/// </returns>
		public Vector3 GetRotation(int state) {
			return rotations[state];
		}

		[Header("Dot materials")]

		[SerializeField]
		private Material[] materials = new Material[] {
			null,
			null
		};

		/// <summary>
		/// Gets or sets the count of materials this display's dots can have. Default and minimum is two materials.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.RotateDot
		/// </remarks>
		/// <value>
		/// Amount of different possible material states.
		/// </value>
		public int MaterialCount {
			set {
				if (value!=materials.Length && this.runTimeCheck(value, materials.Length, "MaterialCount")) {
					Material[] oldMaterialsArray = materials;
					materials=new Material[value];
					for (int n = 0; n<value; n++) {
						materials[n]=(n<oldMaterialsArray.Length ? oldMaterialsArray[n] : null);
					}
				}
			}
			get {
				return materials.Length;
			}
		}

		/// <summary>
		/// Sets material used in dot objects when dot state is set to 'state'.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.RotateDot
		/// </remarks>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' states.
		/// </param>
		/// <param name="material">
		/// Material to use.
		/// </param>
		public void SetMaterial(int state, Material material) {
			if (base.playingInitAndChanged(material, materials[state])) {
				this.needFullStateUpdate=true;
			}
			materials[state]=material;
		}

		/// <summary>
		/// Gets material used in dot objects when dot state is set to 'state'.
		/// </summary>
		/// <remarks>
		/// This setting have no effect if StateChangeStyle is set to StateChangeStyles.RotateDot
		/// </remarks>
		/// <param name="state">
		/// 0 is dot 'off' state, 1 is default 'on' state, 2 and above are additional 'on' states.
		/// </param>
		/// <returns>
		/// Material used.
		/// </returns>
		public Material GetMaterial(int state) {
			return materials[state];
		}

		private StateChangeStyles editor_stateChangeStyle;

		internal override bool isNeedToRecreateEditorDots() {
			if (base.isNeedToRecreateEditorDots() || stateChangeStyle!=editor_stateChangeStyle) {
				// Save values used to create dots in editor mode
				editor_stateChangeStyle=stateChangeStyle;
				return true;
			}
			return false;
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
			bool materialChange = (stateChangeStyle==StateChangeStyles.ChangeDotMaterial);
			return (new DisplayDot_3D(this,x,y,dotObject.transform,(materialChange?dotObject.GetComponent<MeshRenderer>():null)));
		}
		
		internal override void setDisplayDotScaleAndPosition(DisplayDot displayDot) {
			((DisplayDot_3D)(displayDot)).setObjectScaleAndPosition(this);
		}
		
		protected override int getStateCount() {
			return RotationCount;
		}

	}
	
}
