using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StatTypeDropdown))]
public class StatTypeDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty propType = property.FindPropertyRelative("Type");
        SerializedProperty propSubType = property.FindPropertyRelative("SubType");

        EditorGUI.BeginProperty(position, label, property);

        var typeRect = new Rect(position.x, position.y, position.width / 2, position.height);
        var subTypeRect = new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height);

        List<string> typeOptions = new List<string>();

        foreach (StatType type in StatType.List)
        {
            typeOptions.Add(type.Name);
        }

        propType.intValue = EditorGUI.Popup(typeRect, propType.intValue, typeOptions.ToArray());

        if (propType.intValue != StatType.NONE_ID && propType.intValue != StatType.RANDOM_ID)
        {
            int currentSubType = 0;

            List<StatSubType> validSubTypes = StatSubType.GetFilteredList(propType.intValue);

            List<string> subTypeOptions = new List<string>();

            for (int i = 0; i < validSubTypes.Count; i++)
            {
                subTypeOptions.Add(validSubTypes[i].Name);

                if (validSubTypes[i].ID == propSubType.intValue)
                {
                    currentSubType = i;
                }
            }

            currentSubType = EditorGUI.Popup(subTypeRect, currentSubType, subTypeOptions.ToArray());

            propSubType.intValue = validSubTypes[currentSubType].ID;
        }        

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(SubtypeDropdown), true)]
public class SubtypeDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty propSubType = property.FindPropertyRelative("SubType");

        int superTypeToUse = StatType.NONE_ID;

        bool allowNone = true;

        switch (property.type)
        {
            case "TopicSubtypeDropdown":
                superTypeToUse = StatType.TOPIC_ID;
                break;
            case "MoodSubtypeDropdown":
                superTypeToUse = StatType.MOOD_ID;
                break;
            case "GenreSubtypeDropdown":
                superTypeToUse = StatType.GENRE_ID;
                break;
            case "GenreSubtypeDropdown_NoNone":
                superTypeToUse = StatType.GENRE_ID;
                allowNone = false;
                break;
            case "LocationSubtypeDropdown":
                superTypeToUse = StatType.LOCATION_ID;
                break;
        }

        EditorGUI.BeginProperty(position, label, property);

        var subTypeRect = new Rect(position.x, position.y, position.width / 2, position.height);

        int currentSubType = 0;

        List<StatSubType> validSubTypes = StatSubType.GetFilteredList(superTypeToUse);

        if (!allowNone)
        {
            validSubTypes.Remove(StatSubType.NONE);
        }

        List<string> subTypeOptions = new List<string>();

        for (int i = 0; i < validSubTypes.Count; i++)
        {
            subTypeOptions.Add(validSubTypes[i].Name);

            if (validSubTypes[i].ID == propSubType.intValue)
            {
                currentSubType = i;
            }
        }

        currentSubType = EditorGUI.Popup(subTypeRect, currentSubType, subTypeOptions.ToArray());

        propSubType.intValue = validSubTypes[currentSubType].ID;

        EditorGUI.EndProperty();
    }
}