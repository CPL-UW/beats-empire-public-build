using UnityEngine;
using UnityEditor;

public static class CorrelationRuleListUtility
{

    /// <summary>
    /// Create new asset from <see cref="ScriptableObject"/> type with unique name at
    /// selected folder in project window. Asset creation can be cancelled by pressing
    /// escape key when asset is initially being named.
    /// </summary>
    /// <typeparam name="T">Type of scriptable object.</typeparam>
    public static void CreateAsset<CorrelationRuleList>() where CorrelationRuleList : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<CorrelationRuleList>();
        ProjectWindowUtil.CreateAsset(asset, "New " + typeof(CorrelationRuleList).Name + ".asset");
    }

}