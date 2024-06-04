//    DotMatrix Display - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	public class Display_Editor : Editor {

		// For typical display sizes this should be far more than enough.
		// This limit just prevents accidental dot amounts that could jam the whole editor (trying to generate millions of dots or so).
		private const int MAX_ALLOWED_DOT_AMOUNT = 10000;

		// Texture based display generally have no problem showing bigger displays too.
		private const int MAX_ALLOWED_DOT_AMOUNT_FOR_TEXTURE_BASED_DISPLAY = 100000;

		// Typical border sizes are from 0-3, so this should be enough.
		// If there's need for wider borders, consider making separated static gameobjects instead of generating huge amount of empty dots around the display.
		private const int MAX_ALLOWED_BORDER_DOTS = 10;

		public void createDotsInEditor(Display displayTarget, ref bool changes) {

			EditorCommon.addHeader("Editor");

			displayTarget.CreateDotsInEditor=EditorCommon.boolToggle("Create Dots in Editor",displayTarget.CreateDotsInEditor,ref changes);

			if (displayTarget.CreateDotsInEditor) {

				int indentLevel=EditorGUI.indentLevel;
				EditorGUI.indentLevel=indentLevel+1;
				displayTarget.ContentInEditor=(Display.ContentsInEditor)(EditorCommon.enumChoice("Content In Editor",displayTarget.ContentInEditor,ref changes));

				if (displayTarget.ContentInEditor==Display.ContentsInEditor.Text) {

					EditorGUI.indentLevel=indentLevel+2;
					displayTarget.TextInEditorFont=(TextCommand.Fonts)(EditorCommon.enumChoice("Font",displayTarget.TextInEditorFont,ref changes));
					displayTarget.TextInEditorText=EditorCommon.textArea("Text",displayTarget.TextInEditorText,ref changes);

				}

				EditorGUI.indentLevel=indentLevel;

			}

		}

		public int displaySizeInDots(Display displayTarget, ref bool changes) {

			EditorCommon.addHeader("Display Size in Dots");

			bool widthChange=false;
			bool heightChange=false;
			displayTarget.WidthInDots=EditorCommon.intField("Display Width",displayTarget.WidthInDots,1,ref widthChange);
			displayTarget.HeightInDots=EditorCommon.intField("Display Height",displayTarget.HeightInDots,1,ref heightChange);

			int totalDots=displayTarget.WidthInDots*displayTarget.HeightInDots;
			if (widthChange || heightChange) {
				int limit = (displayTarget is DisplayObj ? MAX_ALLOWED_DOT_AMOUNT : MAX_ALLOWED_DOT_AMOUNT_FOR_TEXTURE_BASED_DISPLAY);
				if (totalDots > limit) {
					string asString = (displayTarget is DisplayObj ? "MAX_ALLOWED_DOT_AMOUNT" : "MAX_ALLOWED_DOT_AMOUNT_FOR_TEXTURE_BASED_DISPLAY");
					Debug.LogWarning("DotMatrix ("+displayTarget.gameObject.name+"): Dot amount is over "+limit+", limiting display size.\n"+
					                 "If you need to break this limit, change '"+asString+"' in file 'DisplayEditor.cs'.",displayTarget.gameObject);
					if (widthChange && !heightChange && displayTarget.WidthInDots <= limit) {
						displayTarget.HeightInDots=Mathf.Max((int)(1.0f*limit/displayTarget.WidthInDots),1);
					} else if (heightChange && !widthChange && displayTarget.HeightInDots<=limit) {
						displayTarget.WidthInDots=Mathf.Max((int)(1.0f*limit/displayTarget.HeightInDots),1);
					} else {
						GUI.FocusControl(null);
						float m=Mathf.Sqrt(1.0f*totalDots/limit);
						displayTarget.WidthInDots=Mathf.Clamp((int)(displayTarget.WidthInDots/m), 1, limit);
						displayTarget.HeightInDots=Mathf.Clamp((int)(displayTarget.HeightInDots/m), 1, limit);
					}
					totalDots=displayTarget.WidthInDots*displayTarget.HeightInDots;
				}
				changes=true;
			}

			displayTarget.BorderDots=EditorCommon.intSlider("Display Borders",displayTarget.BorderDots,0,MAX_ALLOWED_BORDER_DOTS,ref changes);

			return totalDots;

		}

		public void prefabs(DisplayObj displayTarget, ref bool changes) {

			EditorCommon.addHeader("Prefabs");

			displayTarget.DotPrefab=EditorCommon.objectField("Dot Prefab",displayTarget.DotPrefab,ref changes);

			displayTarget.DotLayer=(DisplayObj.DotLayers)(EditorCommon.enumChoice("Dots layer", displayTarget.DotLayer, ref changes));

		}

		public void grouping(DisplayObj displayTarget, ref bool changes) {

			EditorCommon.addHeader("Dot Grouping");

			displayTarget.DotGroupingEnabled=EditorCommon.boolToggle("Enable dot grouping", displayTarget.DotGroupingEnabled, ref changes);

			if (displayTarget.DotGroupingEnabled) {

				int maxX = Mathf.Max(displayTarget.WidthInDots,2); // Mathf.Max((displayTarget.WidthInDots+1)/2,2);
				int maxY = Mathf.Max(displayTarget.HeightInDots,2); // Mathf.Max((displayTarget.HeightInDots+1)/2,2);
				displayTarget.DotGroupSizeHorizontal=EditorCommon.intSlider("Group dot count horizontal", displayTarget.DotGroupSizeHorizontal, 2, maxX, ref changes);
				displayTarget.DotGroupSizeVertical=EditorCommon.intSlider("Group dot count vertical", displayTarget.DotGroupSizeVertical, 2, maxY, ref changes);
				displayTarget.DotGroupRelativeSpacing=EditorCommon.vector2Field("Relative spacing between groups", displayTarget.DotGroupRelativeSpacing, true, ref changes);

			}

		}

		public void realism(Display displayTarget, int totalDots, bool moreThanTwoStates, ref bool changes) {

			EditorCommon.addHeader("Realism");

			bool offChange=false;
			bool onChange=false;
			displayTarget.BrokenAlwaysOffDots=EditorCommon.intSlider("Broken Always Off Dots",displayTarget.BrokenAlwaysOffDots,0,totalDots,ref offChange);
			displayTarget.BrokenAlwaysOnDots=EditorCommon.intSlider("Broken Always On Dots",displayTarget.BrokenAlwaysOnDots,0,totalDots,ref onChange);

			if (offChange || onChange) {
				int totalBroken=displayTarget.BrokenAlwaysOffDots+displayTarget.BrokenAlwaysOnDots;
				if (totalBroken>totalDots) {
					if (offChange) {
						displayTarget.BrokenAlwaysOnDots=totalDots-displayTarget.BrokenAlwaysOffDots;
					} else {
						displayTarget.BrokenAlwaysOffDots=totalDots-displayTarget.BrokenAlwaysOnDots;
					}
				}
				changes=true;
			}

			EditorCommon.addNote("Following delay settings have effect only when application is playing");
			displayTarget.OffDelaySeconds=EditorCommon.floatField("Turn Off Delay Seconds",displayTarget.OffDelaySeconds);
			displayTarget.OnDelaySeconds=EditorCommon.floatField("Turn On Delay Seconds",displayTarget.OnDelaySeconds);
			if (moreThanTwoStates) {
				displayTarget.ChangeDelaySeconds=EditorCommon.floatField("Change (On to On) Delay Seconds",displayTarget.ChangeDelaySeconds);
			}

		}

	}
	
}
