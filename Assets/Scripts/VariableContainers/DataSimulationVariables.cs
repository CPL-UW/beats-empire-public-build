using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSimulationVariables : MonoBehaviour
{
    public int TurtleHillPopulation;
    public int MadhatterPopulation;
    public int IronwoodPopulation;
    public int BronzPopulation;
    public int KingsIslePopulation;
    public int BooklinePopulation;
    public float TargetedLocationMultiplier;
    public float CashPerSale;
    public float CashPerListen;
    public float DecayRate;
    public float HypeDecayRate;
    public float FatigueDecayRate;
    public float HypeDiminishingFactor;
    public float IndustryListensOffset;
    public float IndustryListensRandomizer;
    public float IndustryListensFactor;
    public float FollowerConversionFactor;
    public float FollowerAppealThreshold;
    public float WordOfMouthSalesMultiplier;
    public float WordOfMouthListensMultiplier;
    public float FanRatingFloor;
	public float GraphYAxisMultiplier;

	[Header("Weights summed to equal 1.0")]
	[Header("0 Genre, 1 Mood, 2 Topic")]
	public float[] TurtleHillInterests;
	public float[] KingsIsleInterests;
	public float[] BronzInterests;
	public float[] IronwoodInterests;
	public float[] BooklineInterests;
	public float[] MadhatterInterests;

	public bool[] TurtleHillCaresAbout;
	public bool[] KingsIsleCaresAbout;
	public bool[] BronzCaresAbout;
	public bool[] IronwoodCaresAbout;
	public bool[] BooklineCaresAbout;
	public bool[] MadhatterCaresAbout;

	public string TurtleHillTrendString = "Slow";
	public string KingsIsleTrendString = "Fast";
	public string TheBronzTrendString = "Slow";
	public string IronwoodTrendString = "Slow";
	public string BooklineTrendString = "Fast";
	public string MadhatterTrendString = "Slow";

	private float[][] boroughInterests;

	void Start()
	{
		boroughInterests = new float[6][];
		boroughInterests[StatSubType.BOOKLINE_ID - StatSubType.BOOKLINE_ID] = BooklineInterests;
		boroughInterests[StatSubType.THE_BRONZ_ID - StatSubType.BOOKLINE_ID] = BronzInterests;
		boroughInterests[StatSubType.IRONWOOD_ID - StatSubType.BOOKLINE_ID] = IronwoodInterests;
		boroughInterests[StatSubType.KINGS_ISLE_ID - StatSubType.BOOKLINE_ID] = KingsIsleInterests;
		boroughInterests[StatSubType.MADHATTER_ID - StatSubType.BOOKLINE_ID] = MadhatterInterests;
		boroughInterests[StatSubType.TURTLE_HILL_ID - StatSubType.BOOKLINE_ID] = TurtleHillInterests;
	}

	public float[] GetInterests(StatSubType borough)
	{
		return boroughInterests[borough.ID - StatSubType.BOOKLINE_ID];
	}

	public bool[] getBoroughInterests(StatSubType borough)
	{
		switch (borough.ID)
		{
			case StatSubType.TURTLE_HILL_ID: return TurtleHillCaresAbout;
			case StatSubType.KINGS_ISLE_ID: return KingsIsleCaresAbout;
			case StatSubType.THE_BRONZ_ID: return BronzCaresAbout;
			case StatSubType.IRONWOOD_ID: return IronwoodCaresAbout;
			case StatSubType.BOOKLINE_ID: return BooklineCaresAbout;
			case StatSubType.MADHATTER_ID: return MadhatterCaresAbout;
			default: return TurtleHillCaresAbout;
		}
	}
}
