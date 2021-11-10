using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenrePersonBucket
{
    public Person InternalPerson;
    public StatSubType Genre;
    public float Population;
}

[System.Serializable]
public class Person
{
    public string personName;
    public List<Stat> stats = new List<Stat>();
    public List<string> surgesApplied = new List<string>();

    public Stat GetStatBySubType(StatSubType subtype)
    {
        return stats.Where(x => x.statSubType == subtype).FirstOrDefault();
    }

	public void SetStatBySubType(StatSubType subtype, float val)
	{
		stats.Where(x => x.statSubType == subtype).FirstOrDefault().floatVal = val;
	}

	public Stat GetStatByType(StatType statType)
    {
        return stats.Where(x => x.statType == statType).FirstOrDefault();
    }
}
