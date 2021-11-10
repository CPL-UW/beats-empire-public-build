using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfSmallInstrument))]
public class ShowIfSmallInstrumentDrawer : PropertyDrawer
{
	// This solution was inspired by http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5.

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (IsSmallInstrument(property))
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (IsSmallInstrument(property))
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}
		else
		{
			return -EditorGUIUtility.standardVerticalSpacing;
		}
	}

	private bool IsSmallInstrument(SerializedProperty property)
	{
		// Hacky. I want to access the sibling property, but there's no API for this. It would be
		// better to chop off the tail name and replace it with the dependency. Another day...
		string path = property.propertyPath.Replace(property.name, "instrument");

		SerializedProperty otherProperty = property.serializedObject.FindProperty(path);
		InstrumentChoice instrument = (InstrumentChoice) otherProperty.enumValueIndex;
		return instrument == InstrumentChoice.Any ||
			   instrument == InstrumentChoice.Guitar ||
			   instrument == InstrumentChoice.Bass ||
			   instrument == InstrumentChoice.Vocals;
	}
}
