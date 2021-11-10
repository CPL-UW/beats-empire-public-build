using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializationVariables : MonoBehaviour
{
    public int StartingCash;
    public int StartingWeeks;
	public float BaseChanceForArtist;
	[Tooltip("Will be divided by 100. This number is a percentage.")]
	public float StartingFansPercentofPop;
	public int initialStorageSlotCount;
	public bool isSampledMaximally;
	public int weeksBeforeRemindingUpgrades;
	public int weeksBeforeRemindingBoroughs;
	public int weeksBeforeRemindingDataCollect;

	public const int BOOKLINE_ID = 8;
	public const int THE_BRONZ_ID = 9;
	public const int IRONWOOD_ID = 10;
	public const int KINGS_ISLE_ID = 11;
	public const int MADHATTER_ID = 12;
	public const int TURTLE_HILL_ID = 13;

	public int MaxSongQuality = 15;
	public List<int> DebugBandPoints;
}
