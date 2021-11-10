using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BandGenerators))]
public class BandGeneratorsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		BandGenerators generators = (BandGenerators) target;
		if (GUILayout.Button("Reset to Defaults"))
		{
			generators.Reset();
		}
		if (GUILayout.Button("Reset Feature Pose"))
		{
			generators.ResetFeaturePose();
		}
	}
}	
