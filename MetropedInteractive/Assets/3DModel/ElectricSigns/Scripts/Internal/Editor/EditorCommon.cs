//    DotMatrix - Editor Common


using System;
using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {
	
	public class EditorCommon {

		private static GUIStyle headerStyle;
		private static GUIStyle noteStyle;

		public static void addHeader(string labelText) {

			if (headerStyle==null) {
				headerStyle=new GUIStyle();
				headerStyle.fontStyle=FontStyle.Bold;
			}
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(labelText,headerStyle);

		}

		public static void addNote(string noteText) {
			
			if (noteText!=null) {
				if (noteStyle==null) {
					noteStyle=new GUIStyle();
					noteStyle.fontStyle=FontStyle.Italic;
					noteStyle.wordWrap=true;
				}
				EditorGUILayout.LabelField(noteText,noteStyle);
			}
			
		}

		public static bool boolToggle(string labelText, bool currentValue, ref bool changes) {
			bool newValue=EditorGUILayout.Toggle(labelText,currentValue);
			if (newValue!=currentValue) {
				changes=true;
			}
			return newValue;
		}

		public static int intField(string labelText, int currentValue, int minimum, ref bool changes) {
			int newValue=EditorGUILayout.IntField(labelText,currentValue);
			if (newValue<minimum) {
				newValue=minimum;
			}
			if (newValue!=currentValue) {
				changes=true;
			}
			return newValue;
		}

		public static int intSlider(string labelText, int currentValue, int minimum, int maximum, ref bool changes) {
			int newValue = EditorGUILayout.IntSlider(labelText, currentValue, minimum, maximum);
			if (newValue!=currentValue) {
				changes=true;
			}
			return newValue;
		}

		public static float floatField(string labelText, float currentValue) {
			float newValue=EditorGUILayout.FloatField(labelText,currentValue);
			if (newValue<0f) {
				newValue=0f;
			}
			return newValue;
		}

		public static float floatField(string labelText, float currentValue, float minimum, float maximum, ref bool changes) {
			float newValue = EditorGUILayout.FloatField(labelText, currentValue);
			if (newValue<minimum) {
				newValue=minimum;
			}
			if (newValue>maximum) {
				newValue=maximum;
			}
			if (newValue!=currentValue) {
				changes=true;
			}
			return newValue;
		}

		public static Vector2 vector2Field(string labelText, Vector2 currentVector, bool keepPositive, ref bool changes) {
			Vector2 newVector=EditorGUILayout.Vector2Field(labelText,currentVector);
			if (keepPositive) {
				if (newVector.x<=0f) {
					newVector.x=float.Epsilon;
				}
				if (newVector.y<=0f) {
					newVector.y=float.Epsilon;
				}
			}
			if (newVector.x!=currentVector.x || newVector.y!=currentVector.y) {
				changes=true;
			}
			return newVector;
		}

		public static Vector3 vector3Field(string labelText, Vector3 currentVector, bool keepPositive, ref bool changes) {
			Vector3 newVector=EditorGUILayout.Vector3Field(labelText,currentVector);
			if (keepPositive) {
				if (newVector.x<=0f) {
					newVector.x=float.Epsilon;
				}
				if (newVector.y<=0f) {
					newVector.y=float.Epsilon;
				}
				if (newVector.z<=0f) {
					newVector.z=float.Epsilon;
				}
			}
			if (newVector.x!=currentVector.x || newVector.y!=currentVector.y || newVector.z!=currentVector.z) {
				changes=true;
			}
			return newVector;
		}

		public static Material materialField(string labelText, Material currentMaterial, ref bool changes) {
			Material newMaterial = (Material)(EditorGUILayout.ObjectField(labelText, currentMaterial, typeof(Material), false));
			if (newMaterial!=currentMaterial) {
				changes=true;
			}
			return newMaterial;
		}

		public static Enum enumChoice(string labelText, Enum currentlySelected, ref bool changes) {
			Enum newSelected=EditorGUILayout.EnumPopup(labelText,currentlySelected);
			if (!newSelected.Equals(currentlySelected)) {
				changes=true;
			}
			return newSelected;
		}

		public static GameObject objectField(string labelText, GameObject currentObject, ref bool changes) {
			GameObject newObject=(GameObject)(EditorGUILayout.ObjectField(labelText,currentObject,typeof(GameObject),true));
			if (newObject!=currentObject) {
				changes=true;
			}
			return newObject;
		}

		public static Color colorField(string labelText, Color currentColor, ref bool changes) {
			Color newColor=EditorGUILayout.ColorField(labelText,currentColor);
			if (newColor!=currentColor) {
				changes=true;
			}
			return newColor;
		}

/*
		public static Color colorFieldWithoutAlpha(string labelText, Color currentColor, ref bool changes) {
			Color newColor = EditorGUILayout.ColorField(new GUIContent(labelText), currentColor, true, false, false, null);
			if (newColor!=currentColor) {
				newColor.a = 1f;
				changes=true;
			}
			return newColor;
		}
*/

		public static string textArea(string labelText, string contentText, ref bool changes) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(labelText);
			int indent=EditorGUI.indentLevel;
			EditorGUI.indentLevel=0;
			string newContentText=EditorGUILayout.TextArea(contentText);
			EditorGUI.indentLevel=indent;
			EditorGUILayout.EndHorizontal();
			if (newContentText!=contentText) {
				changes=true;
			}
			return newContentText;
		}

	}
	

}
