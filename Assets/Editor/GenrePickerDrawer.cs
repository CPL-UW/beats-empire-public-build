using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GenrePicker))]
public class GenrePickerDrawer : PropertyDrawer
{
	static StatSubType[] genres = {
		StatSubType.ROCK,
		StatSubType.POP,
		StatSubType.RANDB,
		StatSubType.HIP_HOP,
		StatSubType.RAP,
		StatSubType.ELECTRONIC,
	};
	static string[] options = genres.Select(genre => genre.Name).ToArray();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		int index = property.intValue;
		property.intValue = EditorGUI.Popup(position, label.text, index, options);
		property.serializedObject.ApplyModifiedProperties();
	}
}
