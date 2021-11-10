using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongReleaseVariables : MonoBehaviour
{
    public float ExtraStatRollsPerSpeed;

    public int MinimumTurns;
    public int PersistenceLowMaxOffset;
    public int PersistenceHighMaxOffset;
	public float BonusFansPercent;
	public float BonusSalesPercent;
    public float SuccessRateFloor;
	public int BonusFansFixMin;
	public int BonusFansFixMax;
	public int[] SpotSalesThresholds;
	public int LowEndChartPos;
	public int HighEndChartPos;
	public int SalesToNextPos;
	public int SalelsToNextNextPos;

    public List<int> FollowersNeededPerLevel;
}
