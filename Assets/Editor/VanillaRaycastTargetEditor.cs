using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(VanillaRaycastTarget), false)]
public class VanillaRaycastTargetEditor : GraphicEditor {
	public override void OnInspectorGUI()
	{
		base.serializedObject.Update();
		EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
		base.RaycastControlsGUI();
		base.serializedObject.ApplyModifiedProperties();
	}
}
