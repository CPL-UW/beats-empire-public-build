using UnityEngine;
using System;

[Serializable]
public class StatTypeDropdown
{
    public int Type;
    public int SubType;

    public StatTypeDropdown()
    {
        this.Type = StatType.NONE_ID;
        this.SubType = StatSubType.NONE_ID;
    }
}

[Serializable]
public abstract class SubtypeDropdown
{
    public int SubType = StatSubType.NONE_ID;
}

[Serializable]
public class LocationSubtypeDropdown : SubtypeDropdown
{
    public int SuperType = StatType.LOCATION_ID;
}

[Serializable]
public class GenreSubtypeDropdown : SubtypeDropdown
{
    public int SuperType = StatType.GENRE_ID;
}

[Serializable]
public class GenreSubtypeDropdown_NoNone : GenreSubtypeDropdown
{
    public bool AllowNone = false;
}

[Serializable]
public class TopicSubtypeDropdown : SubtypeDropdown
{
    public int SuperType = StatType.TOPIC_ID;
}

[Serializable]
public class MoodSubtypeDropdown : SubtypeDropdown
{
    public int SuperType = StatType.MOOD_ID;
}