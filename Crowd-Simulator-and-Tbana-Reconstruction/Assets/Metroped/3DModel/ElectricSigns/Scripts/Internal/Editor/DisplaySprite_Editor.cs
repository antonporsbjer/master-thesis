//    DotMatrix Display - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	[CustomEditor(typeof(Display_Sprite))]
	public class DisplaySprite_Editor : Display_Editor {

		public override void OnInspectorGUI() {

			//  Undo

			Undo.RecordObject(target,"DotMatrix Display_Sprite Change");

			//  Variables

			Display_Sprite displayTarget=(Display_Sprite)(target);

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

			// Grouping

			base.grouping(displayTarget, ref changes);

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
		
	}
	
}
