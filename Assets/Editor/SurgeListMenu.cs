using UnityEngine;
using UnityEditor;

static class SurgeListMenu
{

    [MenuItem("Assets/Create/Scriptable Object/Surge List")]
    public static void CreateYourScriptableObject()
    {
        SurgeListUtility.CreateAsset<SurgeList>();
    }

}