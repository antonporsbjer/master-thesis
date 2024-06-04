//    DotMatrix Display - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	[CustomEditor(typeof(Display_UI))]
	public class DisplayUI_Editor : Display_Editor {

		void OnEnable() {

			Display_UI displayTarget = (Display_UI)(target);

			// Possibly automatically add to Canvas
			if (shouldCheckAndSetParent(displayTarget)) {
				displayTarget.checkAndSetParent();
			}

		}

		private bool shouldCheckAndSetParent(Display_UI displayTarget) {
			if (Application.isPlaying) {
				return false;
			}
#if UNITY_2018_3_OR_NEWER
			// Could use UnityEditor.Experimental.SceneManagement.PrefabStageUtility to test whatever prefab is actually in Scene or Prefab editor but worried of using packages
			// called "Experimental". So playing safe and returning true only in clear cases (prefab in scene) as this is just for extra convenience for user to auto-create canvas when needed.
			return (PrefabUtility.GetPrefabAssetType(displayTarget)==PrefabAssetType.Regular && PrefabUtility.GetPrefabInstanceStatus(displayTarget)==PrefabInstanceStatus.Connected);
#else
			// Previously (PrefabUtility.GetPrefabType(displayTarget)!=PrefabType.Prefab) that affected also objects in scene that no longer were prefabs.
			// But changed to this form to have identical functionality with Unity 2018.3 and newer.
			return (PrefabUtility.GetPrefabType(displayTarget)==PrefabType.PrefabInstance);
#endif
		}

		public override void OnInspectorGUI() {

			//  Undo

			Undo.RecordObject(target,"DotMatrix Display_UI Change");

			//  Variables

			Display_UI displayTarget=(Display_UI)(target);

			bool changes=false;

			// Display in editor
			
			base.createDotsInEditor(displayTarget,ref changes);

			// Display size in dots

			int totalDots=base.displaySizeInDots(displayTarget,ref changes);

			// Prefabs

			base.prefabs(displayTarget,ref changes);

			// Dot size in units
			
			EditorCommon.addHeader("Dot Size in Units");

			displayTarget.DotSize=EditorCommon.vector2Field("Dot Size",displayTarget.DotSize,true,ref changes);
			displayTarget.DotSpacing=EditorCommon.vector2Field("Dot Spacing",displayTarget.DotSpacing,false,ref changes);

			displayTarget.UIFit=(Display_UI.UIFits)(EditorCommon.enumChoice("Fit to RectTransform",displayTarget.UIFit,ref changes));

			int indentLevel=EditorGUI.indentLevel;
			EditorGUI.indentLevel=indentLevel+1;

			if (displayTarget.UIFit!=Display_UI.UIFits.KeepDotSize) {
				Rect goRect=displayTarget.GetComponent<RectTransform>().rect;
				if (goRect.width<=0f || goRect.height<=0f) {
					Debug.LogWarning("DotMatrix ("+displayTarget.gameObject.name+"): "+
						"'Fit to UI' is enabled, but GameObject's RectTransform 'width' or 'height' is zero or negative. Display dots will not be visible.\n"+
						"Change 'Fit to UI' to 'Keep Dot Size' or change RectTransform so that it have positive width and height.",displayTarget.gameObject);
				}
			}

			if (displayTarget.UIFit!=Display_UI.UIFits.FitToRectTransform) {
				displayTarget.UIPosition=(Display_UI.UIPositions)(EditorCommon.enumChoice("Position inside rect",displayTarget.UIPosition,ref changes));
			}

			EditorGUI.indentLevel=indentLevel;

			// Grouping

			base.grouping(displayTarget, ref changes);

			// State colors

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
		
	}
	
}
