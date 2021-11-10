using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Surge
{
	public int GeneratorIndex;
	public int UniqueID;
    public SurgeData SurgeType;
    public int Age;
	public int AdditionalLength;

    public bool AffectsLocation;
    public bool AffectsGenreCommunity;
    public StatSubType AffectedLocation;
    public StatSubType AffectedGenreCommunity;
    public StatSubType AffectedSubType;

	public Surge(SurgeData type, SavedData saved)
	{
		SurgeType = type;
		GeneratorIndex = saved.GeneratorIndex;
		UniqueID = saved.UniqueID;
		Age = saved.Age;
		AdditionalLength = saved.AdditionalLength;
		AffectsLocation = saved.AffectsLocation;
		AffectsGenreCommunity = saved.AffectsGenreCommunity;
		AffectedLocation = StatSubType.List[saved.AffectedLocation];
		AffectedGenreCommunity = StatSubType.List[saved.AffectedGenreCommunity];
		AffectedSubType = StatSubType.List[saved.AffectedSubType];
	}

    public Surge(SurgeData type, StatSubType subType, int bonusLength, int generatorIndex, int chosenSurge)
    {
		this.GeneratorIndex = generatorIndex;
		this.UniqueID = chosenSurge;
		this.SurgeType = type;
        this.Age = 0;
		this.AdditionalLength = bonusLength;
		this.AffectsLocation = this.SurgeType.location.SubType != StatSubType.NONE_ID;
        this.AffectsGenreCommunity = this.SurgeType.genreCommunity.SubType != StatSubType.NONE_ID;
		this.AffectedSubType = subType;

        if (this.SurgeType.location.SubType == StatSubType.RANDOM_ID)
        {
            this.AffectedLocation = StatSubType.GetRandomSubType(StatType.LOCATION_ID);
        }
        else
        {
            this.AffectedLocation = StatSubType.List[this.SurgeType.location.SubType];
        }

        if (this.SurgeType.genreCommunity.SubType == StatSubType.RANDOM_ID)
        {
            this.AffectedGenreCommunity = StatSubType.GetRandomSubType(StatType.GENRE_ID);
        }
        else
        {
            this.AffectedGenreCommunity = StatSubType.List[this.SurgeType.genreCommunity.SubType];
        }
    }

	[System.Serializable]
	public class SavedData
	{
		public int GeneratorIndex;
		public int UniqueID;
		public int Age;
		public int AdditionalLength;
		public bool AffectsLocation;
		public bool AffectsGenreCommunity;
		public int AffectedLocation;
		public int AffectedGenreCommunity;
		public int AffectedSubType;
	}

	public SavedData GetSavedData()
	{
		SavedData data = new SavedData();

		data.GeneratorIndex = GeneratorIndex;
		data.UniqueID = UniqueID;
		data.Age = Age;
		data.AdditionalLength = AdditionalLength;
		data.AffectsGenreCommunity = AffectsGenreCommunity;
		data.AffectsLocation = AffectsLocation;
		data.AffectedLocation = AffectedLocation.ID;
		data.AffectedGenreCommunity = AffectedGenreCommunity.ID;
		data.AffectedSubType = AffectedSubType.ID;

		return data;
	}

    public float GetCurrentModifier()
    {
        return this.SurgeType.curve.Evaluate((float)this.Age / (this.SurgeType.surgeLength + this.AdditionalLength));
    }

    public bool AffectsBucket(StatSubType location, StatSubType genreCommunity)
    {
        if (this.AffectsLocation && this.AffectedLocation != location)
        {
            return false;
        }

        if (this.AffectsGenreCommunity && this.AffectedGenreCommunity != genreCommunity)
        {
            return false;
        }

        return true;
    }
}

[System.Serializable]
public class SurgeData
{
    public string surgeTitle;
	public int uniqueID;
    [Header("Leaving both as 'None' will cause surge to affect everyone.")]

    [Header("Which location is this restricted to?")]
    public LocationSubtypeDropdown location;

    [Header("Which genre comminuty is this restricted to?")]
    public GenreSubtypeDropdown genreCommunity;

    [Header("Which Stat Sub Type is affected?")]
    public StatTypeDropdown affectedSubStatType;

    [Header("How many turn steps are there in this surge?")]
    public int surgeLength;

    [Header("Draw the curve, add key for shaping, ending at a non-starting value will permanently change affected buckets.")]
    public AnimationCurve curve;
}


