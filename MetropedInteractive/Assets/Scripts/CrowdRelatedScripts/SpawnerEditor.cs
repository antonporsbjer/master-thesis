using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spawner)), CanEditMultipleObjects]
public class SpawnerEditor : Editor {

    private Main mainScript;
    private SerializedObject mainSerializedObject;

    public SerializedProperty
        spawnMethod_Prop,
        circleRadius_Prop,
        numberOfAgents_Prop,
        numberOfDiscRows_Prop,
        useGroupedAgents_Prop,
        percentTwo_Prop,
		percentThree_Prop,
		percentFour_Prop,
        rowAmount_Prop,
		rowLength_Prop,
        useSimpleAgents_Prop,
		individualAgents_Prop,
		agentSpawnColor_Prop,
		groupAgentSpawnColor_Prop,
		agentEditorContainer_Prop,
		customGoal_Prop,
		spawnRate_Prop,
		waitingAgents_Prop;
	
	private SerializedProperty
		avoidanceRadius_Prop,
		planeSize_Prop;



    void OnEnable () {

        mainScript = FindObjectOfType<Main>();
        if (mainScript != null)
        {
            mainSerializedObject = new SerializedObject(mainScript);
			avoidanceRadius_Prop = mainSerializedObject.FindProperty ("agentAvoidanceRadius");
			planeSize_Prop = mainSerializedObject.FindProperty ("planeSize"); 
        } else { Debug.Log("Main script not found"); }

        circleRadius_Prop = serializedObject.FindProperty ("circleRadius");
        numberOfAgents_Prop = serializedObject.FindProperty ("numberOfAgents");
        numberOfDiscRows_Prop = serializedObject.FindProperty ("numberOfDiscRows");
        useGroupedAgents_Prop = serializedObject.FindProperty ("useGroupedAgents");
        percentTwo_Prop = serializedObject.FindProperty ("percentOfTwoInGroup");
		percentThree_Prop = serializedObject.FindProperty ("percentOfThreeInGroup");
		percentFour_Prop = serializedObject.FindProperty ("percentOfFourInGroup");
        rowAmount_Prop = serializedObject.FindProperty ("rows");
		rowLength_Prop = serializedObject.FindProperty ("rowLength");
        spawnMethod_Prop = serializedObject.FindProperty ("spawnMethod");
        useSimpleAgents_Prop = serializedObject.FindProperty ("useSimpleAgents");
		individualAgents_Prop = serializedObject.FindProperty ("individualAgents");
		agentSpawnColor_Prop = serializedObject.FindProperty ("agentSpawnColor");
		groupAgentSpawnColor_Prop = serializedObject.FindProperty ("groupAgentSpawnColor");
		agentEditorContainer_Prop = serializedObject.FindProperty ("agentEditorContainer");
		customGoal_Prop = serializedObject.FindProperty ("customGoal");
		spawnRate_Prop = serializedObject.FindProperty ("spawnRate");
		waitingAgents_Prop = serializedObject.FindProperty ("waitingAgents");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update ();
        mainSerializedObject.Update();

		EditorGUILayout.PropertyField(agentSpawnColor_Prop);
		EditorGUILayout.PropertyField(groupAgentSpawnColor_Prop);
		EditorGUILayout.PropertyField(agentEditorContainer_Prop);
		EditorGUILayout.PropertyField(customGoal_Prop);
		EditorGUILayout.Space();
		

        EditorGUILayout.PropertyField(spawnMethod_Prop);

		Main.Method m = (Main.Method)spawnMethod_Prop.enumValueIndex;
		switch( m ) { 
		case Main.Method.uniformSpawn:
			EditorGUILayout.PropertyField (numberOfAgents_Prop);
			break;
		case Main.Method.circleSpawn:        
			EditorGUILayout.PropertyField (circleRadius_Prop);   
			EditorGUILayout.IntSlider (numberOfAgents_Prop, 0, (int)(2*Mathf.PI*circleRadius_Prop.floatValue/(avoidanceRadius_Prop.floatValue*2f)));
			break;
			
		case Main.Method.discSpawn:  
			EditorGUILayout.PropertyField(circleRadius_Prop); 
			EditorGUILayout.IntSlider (numberOfDiscRows_Prop, 0, (int)((planeSize_Prop.floatValue*5-circleRadius_Prop.floatValue)/(avoidanceRadius_Prop.floatValue*2f)));
			break;

		case Main.Method.continuousSpawn:     
			EditorGUILayout.PropertyField(spawnRate_Prop);
			EditorGUILayout.PropertyField(waitingAgents_Prop);
			EditorGUILayout.PropertyField (useSimpleAgents_Prop);
			EditorGUILayout.PropertyField (useGroupedAgents_Prop);
			if (useGroupedAgents_Prop.boolValue) {
				float diff0 = 1.0f;
				float diff1 = 1.0f - individualAgents_Prop.floatValue;
				float diff2 = 1.0f - individualAgents_Prop.floatValue -percentTwo_Prop.floatValue; 
				float diff3 = 1.0f - individualAgents_Prop.floatValue -percentTwo_Prop.floatValue - percentThree_Prop.floatValue;
				EditorGUILayout.Slider (individualAgents_Prop, 0.0f, diff0);
				EditorGUILayout.Slider (percentTwo_Prop, 0.0f, diff1);
				EditorGUILayout.Slider (percentThree_Prop, 0.0f, diff2);
				EditorGUILayout.Slider (percentFour_Prop, 0.0f, diff3);
			}

			break;

		case Main.Method.areaSpawn:
			EditorGUILayout.PropertyField (rowAmount_Prop);
			EditorGUILayout.PropertyField (rowLength_Prop);
			EditorGUILayout.PropertyField (useSimpleAgents_Prop);
			break;
		}

        serializedObject.ApplyModifiedProperties ();
        mainSerializedObject.ApplyModifiedProperties();

    }

}