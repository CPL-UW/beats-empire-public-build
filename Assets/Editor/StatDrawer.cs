//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(Stat))]
//public class StatDrawer : PropertyDrawer
//{
//    // Draw the property inside the given rect
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // Using BeginProperty / EndProperty on the parent property means that
//        // prefab override logic works on the entire property.
//        EditorGUI.BeginProperty(position, label, property);

//        // Draw label
//        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//        // Don't make child fields be indented
//        var indent = EditorGUI.indentLevel;
//        EditorGUI.indentLevel = 0;

//        // Calculate rects
//        var floatValRect = new Rect(position.x, position.y, 50, position.height);
//       var floatValRect2 = new Rect(position.x + 35, position.y, 50, position.height);
//        //var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

//        // Draw fields - passs GUIContent.none to each so they are drawn without labels
//        EditorGUI.PropertyField(floatValRect, property.FindPropertyRelative("floatVal"), GUIContent.none);

//        EditorGUI.PropertyField(floatValRect2, property.FindPropertyRelative("floatVal"), GUIContent.none);
//        //GUIContent gc = new GUIContent();
//        //gc.text = "Test";
//        //EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("affectedByStat"),gc);

//        //EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
//        //EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}