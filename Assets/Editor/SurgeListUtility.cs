using UnityEngine;
using UnityEditor;

public static class SurgeListUtility
{

    /// <summary>
    /// Create new asset from <see cref="ScriptableObject"/> type with unique name at
    /// selected folder in project window. Asset creation can be cancelled by pressing
    /// escape key when asset is initially being named.
    /// </summary>
    /// <typeparam name="T">Type of scriptable object.</typeparam>
    public static void CreateAsset<SurgeList>() where SurgeList : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<SurgeList>();
        ProjectWindowUtil.CreateAsset(asset, "New " + typeof(SurgeList).Name + ".asset");
    }

}