using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeVariables : MonoBehaviour
{

	public float[] BonusInsightPercentFromUpgrades;
	public int[] BonusSurgeLengthWeeks;
	public int[] BonusChanceToMakeHit;
	public float[] BonusWeeksForSongSales;
	public float[] BonusWeeksDecayRates;
	public float[] BonusGetGenreArtistPercent;

	public float BooklineUnlockCost = 1000000f;
	public float MadhatterUnlockCost = 1000000f;
	public float KingsIsleUnlockCost = 1000000f;
	public float IronwoodUnlockCost = 1000000f;
	public float TheBronzUnlockCost = 1000000f;

	public float[] boroughCard1UnlockCosts = { 0, 0, 0 };
	public float[] boroughCard2UnlockCosts = { 10000f, 50000f, 100000f };
	public float[] boroughCard3UnlockCosts = { 10000f, 50000f, 100000f };

	public float[] genreCard1UnlockCosts = { 3000f, 5000f, 10000f };
	public float[] genreCard2UnlockCosts = { 3000f, 5000f, 10000f };
	public float[] genreCard3UnlockCosts = { 3000f, 5000f, 10000f };

}
