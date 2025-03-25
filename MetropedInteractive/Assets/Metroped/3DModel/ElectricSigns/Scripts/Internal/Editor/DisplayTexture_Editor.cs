//    DotMatrix Display - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	[CustomEditor(typeof(Display_Texture))]
	public class DisplayTexture_Editor : Display_Editor {

//		private static bool alreadyConfirmed = false;

		public override void OnInspectorGUI() {

			//  Undo

			Undo.RecordObject(target,"DotMatrix Display_Texture Change");

			//  Variables

			Display_Texture displayTarget = (Display_Texture)(target);

			bool changes=false;

			// Target material for generated texture

			EditorCommon.addHeader("Target Material (optional)");

			Material newMaterial = (Material)(EditorGUILayout.ObjectField("Target Material for generated Texture", displayTarget.TargetMaterial, typeof(Material), true));
			if (newMaterial != displayTarget.TargetMaterial) {
				if (newMaterial == null || confirmMaterialSet()) {
					if (displayTarget.TargetMaterial != null) {
						displayTarget.TargetMaterial.mainTexture = null;
					}
					displayTarget.TargetMaterial = newMaterial;
					changes = true;
				}
			}

			// Display in editor

			if (displayTarget.TargetMaterial == null && displayTarget.CreateDotsInEditor) {
				displayTarget.CreateDotsInEditor = false;
				changes = true;
			}

			EditorGUI.BeginDisabledGroup(displayTarget.TargetMaterial == null);
			bool change = false;
			base.createDotsInEditor(displayTarget,ref change);
			if (change) {
				if (!displayTarget.CreateDotsInEditor && displayTarget.TargetMaterial != null) {
					displayTarget.TargetMaterial.mainTexture = null;
				}
				changes = true;
			}
			EditorGUI.EndDisabledGroup();

			// Display size in dots

			int totalDots = base.displaySizeInDots(displayTarget,ref changes);

			// Colors

			EditorCommon.addHeader("Dot States (Colors)");

			displayTarget.ColorCount=EditorCommon.intSlider("Dot Color Count",displayTarget.ColorCount,2,8,ref changes);

			displayTarget.OffColor=EditorCommon.colorField("Dot Off Color",displayTarget.OffColor,ref changes);
			displayTarget.OnColor=EditorCommon.colorField("Dot "+(displayTarget.ColorCount>2?"Default ":"")+"On Color",displayTarget.OnColor,ref changes);
			for (int n=2; n<displayTarget.ColorCount; n++) {
				displayTarget.SetColor(n,EditorCommon.colorField("Dot On Color "+n,displayTarget.GetColor(n),ref changes));
			}

			// Real life problems

			base.realism(displayTarget,totalDots,(displayTarget.ColorCount>2),ref changes);

			// Possibly recreate or redraw display in editor

			if (changes) {
				EditorUtility.SetDirty(displayTarget);
			}

		}

		private bool confirmMaterialSet() {
			return true;
	/*
			if (alreadyConfirmed) {
				return true;
			}
			bool ok = EditorUtility.DisplayDialog(
				"Set target Material?",
				"This material's texture will be updated with generated texture. Make sure you assign material that is not used for anything else",
				"Yes", "No");
			if (ok) {
				alreadyConfirmed = true; // No need to ask continuously
			}
			return ok;
	*/
		}

	}

}
