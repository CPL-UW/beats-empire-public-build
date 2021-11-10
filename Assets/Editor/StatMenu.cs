using UnityEngine;
using UnityEditor;

static class StatMenu
{

    [MenuItem("Assets/Create/Scriptable Object/Stat List")]
    public static void CreateYourScriptableObject()
    {
        StatListUtility.CreateAsset<StatList>();
    }

}