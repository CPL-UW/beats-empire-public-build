using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class CorrelationRule 
{
    //[Header("Please observe stat rank: ", order = 0)]
    //[Space(-10, order = 1)]
    //[Header("1. Location", order = 2)]
    //[Space(-10, order = 3)]
    //[Header("2. Genre.", order = 4)]
    //[Space(-10, order = 5)]
    //[Header("3. Mood.", order = 6)]
    //[Space(-10, order = 7)]
    //[Header("4. Topic, Instrument", order = 8)]
    //[Space(-10, order = 9)]
    //[Header("5. Song Quality Preferences", order = 10)]
    //[Space(10, order = 11)]
    public string ruleTitle;
	
	public bool m_IsActive = true;

    [Header("Trigger By Location Value")]
    public LocationSubtypeDropdown m_LocationTrigger;

    //[Header("Trigger By SubStat and Threshold")]
    //public bool m_TriggeredIfAboveThreshold;
    //public bool m_TriggeredIfBelowThreshold;
    //public StatTypeDropdown m_SubStatTrigger;

    //[Range(-1, 1f)]
    //public float m_SubStatThreshhold;


    [Header("Which Stat Sub Type is affected?")]
    public StatTypeDropdown m_AffectedSubStatType;
    [Range(-1, 1f)]
    public float m_ModifierVal;

    private StatSubType cachedAffectedSubType = StatSubType.NONE;

    public StatSubType GetAffectedSubType()
    {
        if (this.cachedAffectedSubType == null || this.cachedAffectedSubType.ID == StatSubType.NONE_ID)
        {
            if (this.m_AffectedSubStatType.SubType == StatSubType.RANDOM_ID || this.m_AffectedSubStatType.Type == StatType.RANDOM_ID)
            {
                this.cachedAffectedSubType = StatSubType.GetRandomSubType(this.m_AffectedSubStatType.Type);
            }
            else
            {
                this.cachedAffectedSubType = StatSubType.List[this.m_AffectedSubStatType.SubType];
            }
        }

        return this.cachedAffectedSubType;
    }
}

 
// [CustomEditor(typeof(CorrelationRule))]
//public class MyScriptEditor : Editor
//{
//    override public void OnInspectorGUI()
//    {

//        var myScript = target as CorrelationRule;
//        myScript.m_IsActive = GUILayout.Toggle(myScript.m_IsActive, "Flag");

//        if (myScript.m_IsActive)
//            myScript.m_ModifierVal = EditorGUILayout.FloatField("I field:", myScript.m_ModifierVal);

//    }
//}

[System.Serializable]
public class AffectedStat
{
    [Header("- Ajust this stat on Consumer -")]
    public StatType statType; //Mood
    public StatSubType statSubType; //Angry Mad Sad
    [Range(-1f, 1f)]
    public float floatVal;
}

[System.Serializable]
public class AffectedByStat
{
    public StatType statType; //Mood
    public StatSubType statSubType; //Angry Mad Sad
    
}

