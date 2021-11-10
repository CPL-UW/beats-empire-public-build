using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Enumeration : IComparable
{
    public readonly string Name;
    public readonly int ID;

    protected Enumeration()
    {
    }

    protected Enumeration(int id, string name)
    {
        ID = id;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        var otherValue = obj as Enumeration;
        if (otherValue == null)
        {
            return false;
        }
        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = ID.Equals(otherValue.ID);
        return typeMatches && valueMatches;
    }

    public int CompareTo(object other)
    {
        return ID.CompareTo(((Enumeration)other).ID);
    }

    // Other utility methods ...
}

[Serializable]
public class StatType : Enumeration
{
    public const int NONE_ID = 0;
    public const int MOOD_ID = 1;
    public const int GENRE_ID = 2;
    public const int TOPIC_ID = 3;
    public const int BAND_QUALITY_ID = 4;
    public const int LOCATION_ID = 5;
    public const int DISTRIBUTION_POINTS_ID = 6;
    public const int RANDOM_ID = 7;

    public static readonly StatType NONE = new StatType(StatType.NONE_ID, "None");
    public static readonly StatType RANDOM = new StatType(StatType.RANDOM_ID, "Random");
    public static readonly StatType MOOD = new StatType(StatType.MOOD_ID, "Mood");
    public static readonly StatType GENRE = new StatType(StatType.GENRE_ID, "Genre");
    public static readonly StatType TOPIC = new StatType(StatType.TOPIC_ID, "Topic");
    public static readonly StatType BAND_QUALITY = new StatType(StatType.BAND_QUALITY_ID, "Band Quality");
    public static readonly StatType LOCATION = new StatType(StatType.LOCATION_ID, "Location");
    public static readonly StatType DISTRIBUTION_POINTS = new StatType(StatType.DISTRIBUTION_POINTS_ID, "Distribution Points");

    protected StatType() { }

    public StatType(int id, string name)
        : base(id, name)
    {
    }

    public static readonly List<StatType> List =  new List<StatType>
    {
        StatType.NONE,
        StatType.MOOD,
        StatType.GENRE,
        StatType.TOPIC,
        StatType.BAND_QUALITY,
        StatType.LOCATION,
        StatType.DISTRIBUTION_POINTS,
        StatType.RANDOM,
    };
}

[Serializable]
public class StatSubType : Enumeration
{
    public StatType SuperType;
    public Color GraphColor;

    public const int NONE_ID = 0;
    public const int RANDOM_ID = 1;

    public static readonly StatSubType NONE = new StatSubType(StatSubType.NONE_ID, "None", StatType.NONE, Color.black);
    public static readonly StatSubType RANDOM = new StatSubType(StatSubType.RANDOM_ID, "Random", StatType.RANDOM, Color.black);

    public const int MOOD1_ID = 2;
    public const int MOOD2_ID = 3;
    public const int MOOD3_ID = 4;
    public const int MOOD4_ID = 5;
    public const int MOOD5_ID = 6;
    public const int MOOD6_ID = 7;

    public static readonly StatSubType MOOD1 = new StatSubType(StatSubType.MOOD1_ID, "[Mood_1]", StatType.MOOD, Color.black);
    public static readonly StatSubType MOOD2 = new StatSubType(StatSubType.MOOD2_ID, "[Mood_2]", StatType.MOOD, Color.black);
    public static readonly StatSubType MOOD3 = new StatSubType(StatSubType.MOOD3_ID, "[Mood_3]", StatType.MOOD, Color.black);
    public static readonly StatSubType MOOD4 = new StatSubType(StatSubType.MOOD4_ID, "[Mood_4]", StatType.MOOD, Color.black);
    public static readonly StatSubType MOOD5 = new StatSubType(StatSubType.MOOD5_ID, "[Mood_5]", StatType.MOOD, Color.black);
    public static readonly StatSubType MOOD6 = new StatSubType(StatSubType.MOOD6_ID, "[Mood_6]", StatType.MOOD, Color.black);

    public const int BOOKLINE_ID = 8;
    public const int THE_BRONZ_ID = 9;
    public const int IRONWOOD_ID = 10;
    public const int KINGS_ISLE_ID = 11;
    public const int MADHATTER_ID = 12;
    public const int TURTLE_HILL_ID = 13;

    public static readonly StatSubType BOOKLINE = new StatSubType(StatSubType.BOOKLINE_ID, "Brower", StatType.LOCATION, Color.black);
    public static readonly StatSubType THE_BRONZ = new StatSubType(StatSubType.THE_BRONZ_ID, "Gorman", StatType.LOCATION, Color.black);
    public static readonly StatSubType IRONWOOD = new StatSubType(StatSubType.IRONWOOD_ID, "Ironwood", StatType.LOCATION, Color.black);
    public static readonly StatSubType KINGS_ISLE = new StatSubType(StatSubType.KINGS_ISLE_ID, "Morris", StatType.LOCATION, Color.black);
    public static readonly StatSubType MADHATTER = new StatSubType(StatSubType.MADHATTER_ID, "Uptown", StatType.LOCATION, Color.black);
    public static readonly StatSubType TURTLE_HILL = new StatSubType(StatSubType.TURTLE_HILL_ID, "Turtle Hill", StatType.LOCATION, Color.black);

    public const int TOPIC1_ID = 14;
    public const int TOPIC2_ID = 15;
    public const int TOPIC3_ID = 16;
    public const int TOPIC4_ID = 17;
    public const int TOPIC5_ID = 18;
    public const int TOPIC6_ID = 19;

    public static readonly StatSubType TOPIC1 = new StatSubType(StatSubType.TOPIC1_ID, "[Topic_1]", StatType.TOPIC, Color.black);
    public static readonly StatSubType TOPIC2 = new StatSubType(StatSubType.TOPIC2_ID, "[Topic_2]", StatType.TOPIC, Color.black);
    public static readonly StatSubType TOPIC3 = new StatSubType(StatSubType.TOPIC3_ID, "[Topic_3]", StatType.TOPIC, Color.black);
    public static readonly StatSubType TOPIC4 = new StatSubType(StatSubType.TOPIC4_ID, "[Topic_4]", StatType.TOPIC, Color.black);
    public static readonly StatSubType TOPIC5 = new StatSubType(StatSubType.TOPIC5_ID, "[Topic_5]", StatType.TOPIC, Color.black);
    public static readonly StatSubType TOPIC6 = new StatSubType(StatSubType.TOPIC6_ID, "[Topic_6]", StatType.TOPIC, Color.black);

    public const int AMBITION_ID = 20;
    public const int RELIABILITY_ID = 21;
    public const int SPEED_ID = 22;
    public const int PERSISTENCE_ID = 23;

    public static readonly StatSubType AMBITION = new StatSubType(StatSubType.AMBITION_ID, "Ambition", StatType.BAND_QUALITY, Color.black);
    public static readonly StatSubType RELIABILITY = new StatSubType(StatSubType.RELIABILITY_ID, "Reliability", StatType.BAND_QUALITY, Color.black);
    public static readonly StatSubType SPEED = new StatSubType(StatSubType.SPEED_ID, "Speed", StatType.BAND_QUALITY, Color.black);
    public static readonly StatSubType PERSISTENCE = new StatSubType(StatSubType.PERSISTENCE_ID, "Persistence", StatType.BAND_QUALITY, Color.black);
	public static readonly StatSubType SONGQUALITY = new StatSubType(StatSubType.SONGQUALITY_ID, "", StatType.BAND_QUALITY, Color.black);

	public const int ROCK_ID = 24;
    public const int POP_ID = 25;
    public const int RANDB_ID = 26;
    public const int HIP_HOP_ID = 27;
    public const int RAP_ID = 28;
    public const int ELECTRONIC_ID = 29;

    public static readonly StatSubType ROCK = new StatSubType(StatSubType.ROCK_ID, "Rock", StatType.GENRE, Color.black);
    public static readonly StatSubType POP = new StatSubType(StatSubType.POP_ID, "Pop", StatType.GENRE, Color.black);
    public static readonly StatSubType RANDB = new StatSubType(StatSubType.RANDB_ID, "R&B", StatType.GENRE, Color.black);
    public static readonly StatSubType HIP_HOP = new StatSubType(StatSubType.HIP_HOP_ID, "Hip Hop", StatType.GENRE, Color.black);
    public static readonly StatSubType RAP = new StatSubType(StatSubType.RAP_ID, "Rap", StatType.GENRE, Color.black);
    public static readonly StatSubType ELECTRONIC = new StatSubType(StatSubType.ELECTRONIC_ID, "Electronic", StatType.GENRE, Color.black);

    public const int PURCHASE_ID = 30;
    public const int LISTENS_ID = 31;
	public const int SONGQUALITY_ID = 32;

    public static readonly StatSubType PURCHASE = new StatSubType(StatSubType.PURCHASE_ID, "Purchase", StatType.DISTRIBUTION_POINTS, Color.black);
    public static readonly StatSubType LISTENS = new StatSubType(StatSubType.LISTENS_ID, "Listens", StatType.DISTRIBUTION_POINTS, Color.black);

    protected StatSubType() { }

    public StatSubType(int id, string name, StatType superType, Color graphColor)
        : base(id, name)
    {
        this.SuperType = superType;
        this.GraphColor = graphColor;
    }

    public static readonly List<StatSubType> List = new List<StatSubType>
    {
        StatSubType.NONE,
        StatSubType.RANDOM,
        StatSubType.MOOD1,
        StatSubType.MOOD2,
        StatSubType.MOOD3,
        StatSubType.MOOD4,
        StatSubType.MOOD5,
        StatSubType.MOOD6,
        StatSubType.BOOKLINE,
        StatSubType.THE_BRONZ,
        StatSubType.IRONWOOD,
        StatSubType.KINGS_ISLE,
        StatSubType.MADHATTER,
        StatSubType.TURTLE_HILL,
        StatSubType.TOPIC1,
        StatSubType.TOPIC2,
        StatSubType.TOPIC3,
        StatSubType.TOPIC4,
        StatSubType.TOPIC5,
        StatSubType.TOPIC6,
        StatSubType.AMBITION,
        StatSubType.RELIABILITY,
        StatSubType.SPEED,
        StatSubType.PERSISTENCE,
        StatSubType.ROCK,
        StatSubType.POP,
        StatSubType.RANDB,
        StatSubType.HIP_HOP,
        StatSubType.RAP,
        StatSubType.ELECTRONIC,
        StatSubType.PURCHASE,
        StatSubType.LISTENS,
    };

    public static List<StatSubType> GetFilteredList(int superTypeID, bool allowWildcards = true)
    {
        List<StatSubType> validSubTypes = new List<StatSubType>();

        foreach (StatSubType subType in StatSubType.List)
        {
            if (subType.SuperType.ID == superTypeID && subType.SuperType != StatType.RANDOM)
            {
                validSubTypes.Add(subType);
                continue;
            }

            if (allowWildcards && (subType.ID == StatSubType.NONE_ID || subType.ID == StatSubType.RANDOM_ID))
            {
                validSubTypes.Add(subType);
                continue;
            }

            if (superTypeID == StatType.RANDOM_ID && (subType.SuperType != StatType.NONE && subType.SuperType != StatType.RANDOM && subType.SuperType != StatType.LOCATION && subType.SuperType != StatType.DISTRIBUTION_POINTS))
            {
                validSubTypes.Add(subType);
                continue;
            }
        }

        return validSubTypes;
    }

    public static List<StatSubType> GetFilteredList(StatType superType, bool allowWildcards = true)
    {
        return StatSubType.GetFilteredList(superType.ID, allowWildcards);
    }

    public static StatSubType GetRandomSubType(int superTypeID)
    {
        return StatSubType.GetFilteredList(superTypeID, false).OrderBy(x => UnityEngine.Random.value).First();
    }

    public static StatSubType GetRandomSubType(StatType superType)
    {
        return StatSubType.GetRandomSubType(superType.ID);
    }

	public static StatSubType GetTypeFromString(string str)
	{
		foreach(StatSubType type in List)
		{
			if (type.Name == str)
				return type;

			if (Utility.Utilities.InterceptText(type.Name) == str)
				return type;
		}

		return StatSubType.NONE;
	}
}
