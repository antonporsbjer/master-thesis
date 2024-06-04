//    DotMatrix - Custom Editor


using UnityEngine;
using UnityEditor;

namespace Leguar.DotMatrix {

	[CustomEditor(typeof(DotMatrix))]
	public class DotMatrix_Editor : Editor {

		public override void OnInspectorGUI() {

			Undo.RecordObject(target,"DotMatrix Change");

			DotMatrix dmTarget=(DotMatrix)(target);

			EditorCommon.addHeader("Auto Init");

			dmTarget.initOnStart=EditorGUILayout.Toggle("Init on start",dmTarget.initOnStart);

			int indentLevel=EditorGUI.indentLevel;
			EditorGUI.indentLevel=indentLevel+1;
			if (dmTarget.initOnStart) {
				EditorCommon.addNote("When application is playing, DotMatrix inits and display dots are created immediately when this gameobject starts (default setting)");
			} else {
				EditorCommon.addNote("When application is playing, DotMatrix dots are not created until Init() method of this script is called");
			}
			EditorGUI.indentLevel=indentLevel;

		}

	}

}
