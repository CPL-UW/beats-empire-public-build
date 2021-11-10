using UnityEngine;
using UnityEditor;

static class CorrelationRuleMenu
{

    [MenuItem("Assets/Create/Scriptable Object/Correlation Rule List")]
    public static void CreateYourScriptableObject()
    {
        CorrelationRuleListUtility.CreateAsset<CorrelationRuleList>();
    }

}