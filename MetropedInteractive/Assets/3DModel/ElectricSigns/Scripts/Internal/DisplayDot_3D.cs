//    DotMatrix - DisplayDot - 3D-object


namespace Leguar.DotMatrix.Internal {
	
	using UnityEngine;
	
	public class DisplayDot_3D : DisplayDot {
		
		private Display_3D display3D;
		private Transform dotTransform;

		private int fromState;
		private int toState;
		private Quaternion fromRotation;
		private Quaternion toRotation;
		private float stateChangeElapsed;

		private MeshRenderer dotMeshRenderer;
		private Material toMaterial;

		internal DisplayDot_3D(Display_3D display3D, int x, int y, Transform dotTransform, MeshRenderer dotMeshRenderer) : base(display3D,x,y) {
			this.display3D=display3D;
			this.dotTransform=dotTransform;
			this.dotMeshRenderer=dotMeshRenderer;
		}
		
		internal void setObjectScaleAndPosition(Display_3D display) {
			int drawX = base.getDrawX();
			int drawY = base.getDrawY();
			Vector3 dotSize = display.DotSize;
			dotTransform.localScale=dotSize;
			Vector2 dotSpacing = display.DotSpacing;
			float ux = drawX*(dotSize.x+dotSpacing.x)+dotSize.x*0.5f;
			float uy = drawY*(dotSize.y+dotSpacing.y)+dotSize.y*0.5f;
			if (display.DotGroupingEnabled) {
				int xGroup = base.getClampedX()/display.DotGroupSizeHorizontal;
				ux+=xGroup*(dotSize.x*display.DotGroupRelativeSpacing.x);
				int yGroup = base.getClampedY()/display.DotGroupSizeVertical;
				uy-=yGroup*(dotSize.y*display.DotGroupRelativeSpacing.y);
			}
			dotTransform.localPosition=new Vector3(ux, uy, 0f);
		}

		internal override void setVisibleStateInstantly(int state) {
			fromState=toState=state;
			if (dotMeshRenderer==null) {
				fromRotation=toRotation=Quaternion.Euler(display3D.GetRotation(state));
				dotTransform.localRotation=toRotation;
			} else {
				toMaterial=display3D.GetMaterial(state);
				dotMeshRenderer.material=toMaterial;
			}
		}
		
		internal override void setNewVisibleTargetState(int state) {
			fromState=toState;
			toState=state;
			if (dotMeshRenderer==null) {
				fromRotation=dotTransform.localRotation;
				toRotation=Quaternion.Euler(display3D.GetRotation(state));
			} else {
				toMaterial=display3D.GetMaterial(state);
			}
			stateChangeElapsed=0f;
		}
		
		internal override bool updateVisibleState(float deltaTime) {
			stateChangeElapsed+=deltaTime;
			float delay=base.getDelay(fromState,toState);
			if (delay<=0f || stateChangeElapsed>=delay) {
				if (dotMeshRenderer==null) {
					dotTransform.localRotation=toRotation;
				} else {
					dotMeshRenderer.material=toMaterial;
				}
				return false; // Done, no need for new updates until new target state is set
			}
			if (dotMeshRenderer==null) {
				float percent = stateChangeElapsed/delay;
				dotTransform.localRotation=Quaternion.Lerp(fromRotation, toRotation, percent);
			}
			return true; // Need new updates to reach final state
		}

	}

}
