using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandTierList : MonoBehaviour
{
    [System.Serializable]
    public class BandTier
    {
        public int MinimumPoints;
        public int MaximumPoints;
        public int MinimumFollowers;
    }

    public List<BandTier> Tiers;

	public int NumStartingBands;
    public int BaseBandCost;
    public int AdditionalBandCostPerPoint;
	public float AdditionalBandCostMultiplierPerPoint;
    public int CommonBandAvailabilityMinimumDuration;
    public int CommonBandAvailabilityMaximumDuration;
    public int RareBandAvailabilityMinimumDuration;
    public int RareBandAvailabilityMaximumDuration;
    public float WorseTierRate;
    public float BetterTierRate;

    [Header("Upgrade Costs")]
    public float MultiplierPerUpgrade;
    public float UpgradeBaseCost;

    private List<BandGenerator> validTemplates;
	private List<BandGenerator> usedTemplates;

    public void Init()
    {
        this.validTemplates = GameRefs.I.bandGenerators.Clone();
		this.usedTemplates = new List<BandGenerator>();
    }

    public BandGenerator GetRandomBandTemplate(StatSubType specificGenre)
    {
		BandGenerator randomTemplate;

		if(specificGenre.SuperType == StatType.GENRE)
		{
			List<BandGenerator> genreTemplates = new List<BandGenerator>();
			for(int i = 0; i < validTemplates.Count; i++)
			{
				if (validTemplates[i].RealGenre == specificGenre)
					genreTemplates.Add(validTemplates[i]);
			}

			if(genreTemplates.Count == 0)
			{
				for (int i = 0; i < usedTemplates.Count; i++)
				{
					if (usedTemplates[i].RealGenre == specificGenre)
						genreTemplates.Add(usedTemplates[i]);
				}
			}

			randomTemplate = genreTemplates[Random.Range(0, genreTemplates.Count)];
		}
		else
		{
			// If we go through all bands, we'll just start to reuse them indefinitely.
			if (validTemplates.Count > 0)
			{
				randomTemplate = this.validTemplates[Random.Range(0, this.validTemplates.Count)];
				this.usedTemplates.Add(randomTemplate);
				this.validTemplates.Remove(randomTemplate);
			}
			else
			{
				randomTemplate = this.usedTemplates[Random.Range(0, this.usedTemplates.Count)];
			}
		}
		
        return randomTemplate;
    }
}
