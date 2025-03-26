//    DotMatrix Display - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	[CustomEditor(typeof(Display_3D))]
	public class Display3D_Editor : Display_Editor {
		
		public override void OnInspectorGUI() {
			
			//  Undo
			
			Undo.RecordObject(target,"DotMatrix Display_3D Change");
			
			//  Variables
			
			Display_3D displayTarget=(Display_3D)(target);
			
			bool changes=false;

			// Display in editor
			
			base.createDotsInEditor(displayTarget,ref changes);

			// Display size in dots
			
			int totalDots=base.displaySizeInDots(displayTarget,ref changes);
			
			// Prefabs
			
			base.prefabs(displayTarget,ref changes);
			
			// Dot size in units
			
			EditorCommon.addHeader("Dot Size in Units");
			
			displayTarget.DotSize=EditorCommon.vector3Field("Dot Size",displayTarget.DotSize,true,ref changes);
			displayTarget.DotSpacing=EditorCommon.vector2Field("Dot Spacing",displayTarget.DotSpacing,false,ref changes);

			// Grouping

			base.grouping(displayTarget, ref changes);

			// States

			EditorCommon.addHeader("Dot States");

			displayTarget.StateChangeStyle=(Display_3D.StateChangeStyles)(EditorCommon.enumChoice("Dot State Change Style", displayTarget.StateChangeStyle, ref changes));

			if (displayTarget.StateChangeStyle==Display_3D.StateChangeStyles.RotateDot) {

				EditorCommon.addHeader("Dot States (Rotations)");

				displayTarget.RotationCount=EditorCommon.intSlider("Dot Rotation Count", displayTarget.RotationCount, 2, 6, ref changes);

				displayTarget.OffRotation=EditorCommon.vector3Field("Dot Off Rotation", displayTarget.OffRotation, false, ref changes);
				displayTarget.OnRotation=EditorCommon.vector3Field("Dot "+(displayTarget.RotationCount>2 ? "Default " : "")+"On Rotation", displayTarget.OnRotation, false, ref changes);
				for (int n = 2; n<displayTarget.RotationCount; n++) {
					displayTarget.SetRotation(n, EditorCommon.vector3Field("Dot On Rotation "+n, displayTarget.GetRotation(n), false, ref changes));
				}

			} else {

				EditorCommon.addHeader("Dot States (Materials)");

				displayTarget.MaterialCount=EditorCommon.intSlider("Dot Material Count", displayTarget.MaterialCount, 2, 6, ref changes);

				displayTarget.SetMaterial(0,EditorCommon.materialField("Dot Off Material", displayTarget.GetMaterial(0), ref changes));
				displayTarget.SetMaterial(1,EditorCommon.materialField("Dot "+(displayTarget.MaterialCount>2 ? "Default " : "")+"On Material", displayTarget.GetMaterial(1), ref changes));
				for (int n = 2; n<displayTarget.MaterialCount; n++) {
					displayTarget.SetMaterial(n, EditorCommon.materialField("Dot On Rotation "+n, displayTarget.GetMaterial(n), ref changes));
				}

			}

			// Real life problems

			base.realism(displayTarget,totalDots,(displayTarget.RotationCount>2),ref changes);
			
			// Possibly recreate or redraw display in editor
			
			if (changes) {
				EditorUtility.SetDirty(displayTarget);
			}
			
		}
		
	}
	
}
