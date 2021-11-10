using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Stat
{
    public string inspectorTitle; //nice to have because then the element in the inspector turns into a title rather than saying "Element 0" 
    public StatTypeDropdown statTypeDropdown;
    public Color Color;
	public Color DarkColor;
	public Color LightColor;

    public StatType statType
    {
        get
        {
            if (this.cachedType == null)
            {
                this.cachedType = StatType.List[this.statTypeDropdown.Type];
            }

            return this.cachedType;
        }

        set
        {
            this.cachedType = value;
        }
    }

    public StatSubType statSubType
    {
        get
        {
            if (this.cachedSubType == null)
            {
                this.cachedSubType = StatSubType.List[this.statTypeDropdown.SubType];
            }

            return this.cachedSubType;
        }

        set
        {
            this.cachedSubType = value;
        }
    }

    [System.NonSerialized]
    private StatType cachedType = null;
    [System.NonSerialized]
    private StatSubType cachedSubType = null;

    [Header("Stored Value for the Stat, Or % affected")]
    [Range(-10f, 10f)]
    public float floatVal;

    /// <summary>
    /// CALL THIS TO RETURN RANK VAL - PREVENT CIRCULAR RULE REFERENCES
    /// </summary>
    int GetRank(StatType nextType)
    {
        int rank = 100;
        switch (nextType.ID)
        {
            case StatType.LOCATION_ID:
                rank = 1;
                break;
            case StatType.GENRE_ID:
                rank = 2;
                break;
            case StatType.MOOD_ID:
                rank = 3;
                break;
            case StatType.TOPIC_ID:
                rank = 4;
                break;
            case StatType.BAND_QUALITY_ID:
                rank = 5;
                break;
        }
        return rank;
    }

}
