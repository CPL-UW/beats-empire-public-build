using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfOverride))]
public class ShowIfOverrideDrawer : PropertyDrawer
{
	// This solution was inspired by http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5.

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (IsOverride(property))
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (IsOverride(property))
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}
		else
		{
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}

	private bool IsOverride(SerializedProperty property)
	{
		ShowIfOverride showAttribute = (ShowIfOverride) attribute;

		// Hacky. I want to access the sibling property, but there's no API for this. It would be
		// better to chop off the tail name and replace it with the dependency. Another day...
		string path = property.propertyPath.Replace(property.name, showAttribute.dependsOnProperty);

		SerializedProperty otherProperty = property.serializedObject.FindProperty(path);
		string selection = otherProperty.enumNames[otherProperty.enumValueIndex];
		return selection == "Override";
	}
}
