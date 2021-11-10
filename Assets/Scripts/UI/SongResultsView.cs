using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using TMPro;
using System.Linq;

public class SongResultsView : MonoBehaviour
{
    protected Dictionary<int, StatSubType> burroughLookup = new Dictionary<int, StatSubType>
    {
        {0, StatSubType.TURTLE_HILL},
        {1, StatSubType.MADHATTER},
        {2, StatSubType.IRONWOOD},
        {3, StatSubType.THE_BRONZ},
        {4, StatSubType.KINGS_ISLE},
        {5, StatSubType.BOOKLINE},
    };

    protected Dictionary<StatSubType, int> burroughLookupReverse = new Dictionary<StatSubType, int>
    {
        {StatSubType.TURTLE_HILL, 0},
        {StatSubType.MADHATTER, 1},
        {StatSubType.IRONWOOD, 2},
        {StatSubType.THE_BRONZ, 3},
        {StatSubType.KINGS_ISLE, 4},
        {StatSubType.BOOKLINE, 5},
    };

	public enum ResponseType
	{
		GoodLong,
		GoodShort,
		Mediocre,
		BadLong,
		BadShort,
		NotTrending,
		ActualMostPopular
	};

	public SongReleaseParameters parameters;
	public Colors colors;
    public Animator Animator;

    public TextMeshProUGUI BandNameLabel;
    public TextMeshProUGUI SongTitleLabel;

	public Image genreImage;
	public List<Sprite> genreSprites;
	public TextMeshProUGUI genreLabel;

    public TextMeshProUGUI TotalSalesLabel;
    public TextMeshProUGUI TotalFollowersLabel;

    public TextMeshProUGUI HUDCashLabel;
    public TextMeshProUGUI HUDFollowersLabel;

    public List<TextMeshProUGUI> BurroughSalesLabels;
    public List<TextMeshProUGUI> BurroughFollowersLabels;
	public List<TextMeshProUGUI> BurroughUnsuccessfulText;
	public List<TextMeshProUGUI> ResultsText;

	public FeedbackTexts exampleSentences;
	public TopChartsView topChartsView;
	public Heatmap Heatmap;

	public BoroughMapController[] boroughs;
	public SongReleaseTraitController[] traitControllers;
	public GameObject[] memberModels;

	public TextMeshProUGUI hitMeterLabel;
	public Image hitMeterLabelBackground;
	public Image[] hitMeterBoxes;
	public Color traitColor;

	public TextMeshProUGUI moodLabel;
	public TextMeshProUGUI topicLabel;

	private MarketingInsightObject thisInsight;
    public float TimeBetweenSteps;
    public float CountUpDelay;
    public float TimeToCountUp;

    private float totalSales;
    private float totalFollowers;

    private float currentCash;
    private float currentFollowers;

    private float newCash;
    private float newFollowers;

	private const float HIGH_APPEAL = 0.6f;
	private const float MED_APPEAL = 0.4f;
	private const float LOW_APPEAL = 0.3f;

	private Song lastSongReleased;

	void Start()
	{
		Animator.keepAnimatorControllerStateOnDisable = true;
	}

    private IEnumerator CoReveal()
    {
		yield return new WaitForSeconds(TimeBetweenSteps);
		Animator.SetBool("SongRating", true);

		yield return new WaitForSeconds(2 * TimeBetweenSteps);
		Animator.SetBool("Notifications", true);

		yield return new WaitForSeconds(TimeBetweenSteps);
		if (thisInsight == null)
			this.Animator.SetInteger("TrendType", 0);
		else if (thisInsight.successful)
		{
			if (thisInsight.insightType == MarketingInsights.InsightType.MostPopular)
				this.Animator.SetInteger("TrendType", 1);
			else if (thisInsight.insightType == MarketingInsights.InsightType.IsTrending)
				this.Animator.SetInteger("TrendType", 2);
		}
		else
			this.Animator.SetInteger("TrendType", 3);

		yield return new WaitForSeconds(TimeBetweenSteps);
		Animator.SetBool("Sales", true);
		StartCoroutine(Footilities.CoLerp(TimeToCountUp, 0, totalSales * GameRefs.I.m_dataSimulationManager.DataSimulationVariables.CashPerSale, x => TotalSalesLabel.text = Utilities.FormatNumberForDisplay(Mathf.Round(x))));
		StartCoroutine(Footilities.CoLerp(TimeToCountUp, currentCash, newCash, x => HUDCashLabel.text = string.Format("${0:#,##0}", Mathf.Round(x))));

		yield return new WaitForSeconds(TimeBetweenSteps);
		Animator.SetBool("Fans", true);
		StartCoroutine(Footilities.CoLerp(TimeToCountUp, 0, totalFollowers, x => TotalFollowersLabel.text = Utilities.FormatNumberForDisplay(Mathf.Round(x))));
		StartCoroutine(Footilities.CoLerp(TimeToCountUp, currentFollowers, newFollowers, x => HUDFollowersLabel.text = Utilities.FormatNumberForDisplay(Mathf.Round(x))));

		yield return new WaitForSeconds(TimeBetweenSteps);
		Animator.SetBool("ContinueButton", true);

		GameRefs.I.hudController.Unlock();
    }

	public void Show()
	{
		gameObject.SetActive(true);
		GameRefs.I.hudController.ToResultsMode();
	}

    public void ProcessNewSongAndShowResults(Song song, GameController controller, DataSimulationManager sim, MarketingInsightObject marketingInsight)
    {
		GameRefs.I.hudController.Lock();
		GameRefs.I.hudController.ToResultsMode();
		Animator.SetInteger("TargetBorough", 0);
		Animator.SetBool("SongRating", false);
        Animator.SetBool("Notifications", false);
        Animator.SetInteger("TrendType", 0);
        Animator.SetBool("Sales", false);
        Animator.SetBool("Fans", false);
        Animator.SetBool("ContinueButton", false);

		genreImage.sprite = genreSprites[song.Artist.GetGenre().ID - StatSubType.ROCK_ID];
		genreLabel.text = song.Artist.GetGenre().Name;
		moodLabel.SetIText(song.mood.Name);
		topicLabel.SetIText(song.topic.Name);

		if (song.TargetedLocation != StatSubType.NONE)
        {
            this.Animator.SetInteger("TargetBorough", this.burroughLookupReverse[song.TargetedLocation] + 1);
        }
        else
        {
            this.Animator.SetInteger("TargetBorough", 0);
        }

		for (int i = 0; i < 6; i++)
		{
			StatSubType borough = burroughLookup[i];
			boroughs[i].isSelected = borough == song.TargetedLocation;
			boroughs[i].isLocked = !GameRefs.I.m_marketingView.IsBoroughUnlocked(borough);
			boroughs[i].hasPrediction = marketingInsight != null && marketingInsight.location == borough;
		}

        this.BandNameLabel.text = song.Artist.Name;
        this.SongTitleLabel.text = string.Format("\"{0}\"", song.Name);

		song.Artist.Incarnate(memberModels);

        this.currentCash = controller.GetCash();
        this.currentFollowers = sim.GetPopulationData().Sum(x => x.GetPendingFollowers());

		thisInsight = marketingInsight;
		sim.ProcessInitialSongSales(song, thisInsight);

        Dictionary<StatSubType, int> newFollowers = sim.ProcessNewFollowers(song, marketingInsight);
        
        int totalSales = 0;
        int totalNewFollowers = 0;

		Heatmap.UpdateSaturation();

		foreach (StatSubType location in StatSubType.GetFilteredList(StatType.LOCATION_ID))
        {
            if (location.ID == StatSubType.RANDOM_ID || location.ID == StatSubType.NONE_ID)
            {
                continue;
            }

            int locationSales = song.TurnData[0][location].Sales;
            int locationNewFollowers = newFollowers[location];
			int i = burroughLookupReverse[location];

            totalSales += locationSales;
            totalNewFollowers += locationNewFollowers;

			boroughs[i].salesDelta = locationSales;
			boroughs[i].fansDelta = locationNewFollowers;

			string responseSentence = GetResponseSentence(song, location, marketingInsight);
			
			ResultsText[burroughLookupReverse[location]].text = responseSentence;
        }

		// Show prediction feedback only for selected borough.
		if (marketingInsight != null)
		{
			GameRefs.I.m_gameController.previousMarketingInsights.Add(marketingInsight);
			GameRefs.I.m_gameController.marketingInsightList.Remove(marketingInsight);

			StatSubType location = marketingInsight.location;
			int selectedBoroughIndex = burroughLookupReverse[location];
			BoroughMapController borough = boroughs[selectedBoroughIndex];

			if (marketingInsight.successful)
			{
				if (marketingInsight.insightType == MarketingInsights.InsightType.IsTrending)
				{
					float bonus = sim.GetUpgradeBonusPercent(location) + GameRefs.I.m_gameController.SongReleaseVariables.BonusFansPercent;
					borough.SetPrediction(MarketingInsights.InsightType.IsTrending, bonus);
				}
				else
				{
					float bonus = sim.GetUpgradeBonusPercent(location) + GameRefs.I.m_gameController.SongReleaseVariables.BonusFansPercent;
					borough.SetPrediction(MarketingInsights.InsightType.MostPopular, bonus);
				}
			}
			else
			{
				if (marketingInsight.statType.SuperType == StatType.TOPIC)
				{
					BurroughUnsuccessfulText[this.burroughLookupReverse[location]].text = string.Format("Songs about {0} weren't {1}.", Utilities.InterceptText(marketingInsight.statType.Name).ToUpper(), marketingInsight.insightType == MarketingInsights.InsightType.IsTrending ? "trending up" : "the most popular");
				}
				else
				{
					BurroughUnsuccessfulText[this.burroughLookupReverse[location]].text = string.Format("{0} songs weren't {1}.", Utilities.InterceptText(marketingInsight.statType.Name).ToUpper(), marketingInsight.insightType == MarketingInsights.InsightType.IsTrending ? "trending up" : "the most popular");
				}
			}
		}

		for (int i = 0; i < traitControllers.Length; ++i)
		{
			if (marketingInsight != null && marketingInsight.statType.SuperType.ID - StatType.MOOD_ID == i)
			{
				traitControllers[i].ShowPrediction(marketingInsight.insightType);
			}
			else
			{
				traitControllers[i].HidePrediction();
			}
		}

		this.totalSales = totalSales;
		this.totalFollowers = totalNewFollowers;

		float cashIncome = Mathf.Floor(sim.DataSimulationVariables.CashPerSale * totalSales);

		controller.OnNewSongRelease(song, cashIncome);
		lastSongReleased = song;

		this.newCash = controller.GetCash();
		this.newFollowers = sim.GetPopulationData().Sum(x => x.GetPendingFollowers());

		this.HUDCashLabel.text = string.Format("${0:#,##0}", this.currentCash);
		this.HUDFollowersLabel.text = Utilities.FormatNumberForDisplay(this.currentFollowers);

		for (int i = 0; i < hitMeterBoxes.Length; i++)
		{
			hitMeterBoxes[i].color = (song.Quality >= i) ? parameters.activeHitMeterBoxColor : parameters.inactiveHitMeterBoxColor;
		}

		int nBars = song.Quality + 1;
		for (int i = 0; i < parameters.hitMeterThresholds.Length; ++i)
		{
			if (nBars >= parameters.hitMeterThresholds[i].barCount)
			{
				hitMeterLabel.text = parameters.hitMeterThresholds[i].label;
				if (i == 0)
				{
					hitMeterLabel.fontSize = parameters.maxHitnessLabelFontSize;
					hitMeterLabel.color = parameters.maxHitnessLabelColor;
					hitMeterLabelBackground.color = parameters.maxHitnessLabelBackgroundColor;
				}
				else
				{
					hitMeterLabel.fontSize = parameters.hitnessLabelFontSize;
					hitMeterLabel.color = parameters.hitnessLabelColor;
					hitMeterLabelBackground.color = parameters.hitnessLabelBackgroundColor;
				}
				break; // There, I did it.
			} 
		}

		for (int i = 0; i < 6; i++)
		{
			boroughs[i].Synchronize();
		}

		StartCoroutine(CoReveal());
	}

	public void ContinueToTopCharts()
	{
		Animator.SetInteger("TargetBorough", 0);
		Animator.SetBool("SongRating", false);
        Animator.SetBool("Notifications", false);
        Animator.SetInteger("TrendType", 0);
        Animator.SetBool("Sales", false);
        Animator.SetBool("Fans", false);
        /* Animator.SetBool("ContinueButton", false); */
	}

	public void ReallyContinueToTopCharts()
	{
		topChartsView.gameObject.SetActive(true);
		topChartsView.CalculateChartPosition(lastSongReleased);
		this.gameObject.SetActive(false);
	}

	public string GetResponseSentence(Song song, StatSubType loc, MarketingInsightObject insight)
	{
		if (!GameRefs.I.m_marketingView.IsBoroughUnlocked(loc))
			return "";

		float overallAppeal;
		string response = "";

		FeedbackObject responseSentence;
		DataSimulationManager sim = GameRefs.I.m_dataSimulationManager;
		Dictionary<StatSubType, float> appeals = sim.GetSpecificAppealsInLocation(song, loc, out overallAppeal);

		// Get highest and lowest appeal for later
		StatSubType highestTrait = StatSubType.NONE;
		float highestTraitValue = 0f;
		StatSubType lowestTrait = StatSubType.NONE;
		float lowestTraitValue = 5f;

		StatSubType mostPopularGenre = sim.GetMostPopularTraitInLocation(loc, StatType.GENRE);
		StatSubType mostPopularTopic = sim.GetMostPopularTraitInLocation(loc, StatType.TOPIC);
		StatSubType mostPopularMood = sim.GetMostPopularTraitInLocation(loc, StatType.MOOD);

		foreach (KeyValuePair<StatSubType, float> appeal in appeals)
		{
			if (appeal.Value > highestTraitValue)
			{
				highestTrait = appeal.Key;
				highestTraitValue = appeal.Value;
			}

			if (appeal.Value < lowestTraitValue)
			{
				lowestTrait = appeal.Key;
				lowestTraitValue = appeal.Value;
			}
		}

		StatSubType highestTraitSansQuality = highestTrait;
		StatSubType lowestTraitSansQuality = lowestTrait;

		/* Debug.LogFormat("{0}: {1}", "< highestTrait", highestTrait); */
		/* Debug.LogFormat("{0}: {1}", "< highestTraitValue", highestTraitValue); */
		/* Debug.LogFormat("{0}: {1}", "< lowestTrait", lowestTrait); */
		/* Debug.LogFormat("{0}: {1}", "< lowestTraitValue", lowestTraitValue); */

		// Treat hit meter like a trait
		float qualityValue = song.Quality / (float) GameRefs.I.m_gameInitVars.MaxSongQuality;
		if (qualityValue > highestTraitValue)
		{
			highestTrait = StatSubType.SONGQUALITY;
			highestTraitValue = qualityValue;
		}
		if (qualityValue < lowestTraitValue)
		{
			lowestTrait = StatSubType.SONGQUALITY;
			lowestTraitValue = qualityValue;
		}

		/* Debug.LogFormat("{0}: {1}", "> highestTrait", highestTrait); */
		/* Debug.LogFormat("{0}: {1}", "> highestTraitValue", highestTraitValue); */
		/* Debug.LogFormat("{0}: {1}", "> lowestTrait", lowestTrait); */
		/* Debug.LogFormat("{0}: {1}", "> lowestTraitValue", lowestTraitValue); */

		// Hit Meter can't be used for both low and high.

		if (insight == null || insight.location != loc) // No insight
		{
			if (overallAppeal >= HIGH_APPEAL)
			{
				if (qualityValue >= 0.3f) // (Line 13)
				{
					Debug.LogFormat("{1} {0} (Line 13)", "GoodLong.[HighestAppealingTrait]", loc.Name);
					responseSentence = get_response_sentence(highestTrait, ResponseType.GoodLong);
					response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
				}
				else // < 0.3f (Line 14)
				{
					Debug.LogFormat("{1} {0} (Line 14)", "GoodShort.[HighestAppealingTrait] + Conjunction + BadShort.HitMeter", loc.Name);
					responseSentence = get_response_sentence(highestTraitSansQuality, ResponseType.GoodShort);
					response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTraitSansQuality.Name).ToUpper() + "</color>" + responseSentence.string2;
					response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
					responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
					response += responseSentence.string1 + responseSentence.string2;
				}
			}
			else if (overallAppeal >= LOW_APPEAL)
			{
				if (highestTraitValue >= 0.6f)
				{
					if (lowestTrait == StatSubType.SONGQUALITY) // (Line 16)
					{
						Debug.LogFormat("{1} {0} (Line 16)", "GoodShort.[HighestAppealingTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(highestTraitSansQuality, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTraitSansQuality.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
					else // Lowest trait is song quality (hit meter) (line 15)
					{
						Debug.LogFormat("{1} {0} (Line 15)", "GoodShort.[HighestAppealingTrait] + Conjunction + BadShort.[LowestAppealingTrait]", loc.Name);
						responseSentence = get_response_sentence(highestTrait, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(lowestTrait, ResponseType.BadShort);
						response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
				}
				else
				{
					if (lowestTrait == highestTrait)
					{
						Debug.LogFormat("{1} {0} (new Line 4)", "MediocreShort.[PredictedTrait] + .", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.Mediocre);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTrait.Name).ToUpper() + "</color>" + responseSentence.string2 + ".";
					}
					else if (lowestTrait == StatSubType.SONGQUALITY) // (Line 18)
					{
						Debug.LogFormat("{1} {0} (Line 18)", "Mediocre.[HighestAppealingTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(highestTraitSansQuality, ResponseType.Mediocre);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTraitSansQuality.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
					else // Lowest trait is song quality (hit meter) (line 17)
					{
						Debug.LogFormat("{1} {0} (Line 17)", "Mediocre.[HighestAppealingTrait] + Conjunction + BadShort.[LowestAppealingTrait]", loc.Name);
						responseSentence = get_response_sentence(highestTrait, ResponseType.Mediocre);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(highestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(lowestTrait, ResponseType.BadShort);
						response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
				}
			}
			else // Low appeal
			{
				if(qualityValue >= 0.3f) // (Line 19)
				{
					Debug.LogFormat("{1} {0} (Line 19)", "GoodShort.HitMeter + Conjunction + BadShort.[LowestAppealingTrait]", loc.Name);
					responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.GoodShort);
					response = responseSentence.string1 + responseSentence.string2;
					response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
					responseSentence = get_response_sentence(lowestTraitSansQuality, ResponseType.BadShort);
					response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTraitSansQuality.Name).ToUpper() + "</color>" + responseSentence.string2;
				}
				else
				{
					if (lowestTrait == StatSubType.SONGQUALITY) // (Line 21)
					{
						Debug.LogFormat("{1} {0} (Line 21)", "BadLong.HitMeter", loc.Name);
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadLong);
						response = responseSentence.string1 + responseSentence.string2;
					}
					else // (line 20)
					{
						Debug.LogFormat("{1} {0} (Line 20)", "BadLong.[LowestAppealingTrait]", loc.Name);
						responseSentence = get_response_sentence(lowestTrait, ResponseType.BadLong);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
				}
			}
		}
		else if(insight.insightType == MarketingInsights.InsightType.MostPopular)
		{
			if(insight.successful)
			{
				if(overallAppeal >= HIGH_APPEAL)
				{
					if(qualityValue >= 0.3f) // (Line 2) GoodLong.[PredictedTrait]
					{
						Debug.LogFormat("{1} {0} (Line 2)", "GoodLong.[PredictedTrait]", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodLong);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
					else // < 0.3f (Line 3) GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter
					{
						Debug.LogFormat("{1} {0} (Line 3)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
				}
				else
				{
					if (lowestTrait == insight.statType)
					{
						Debug.LogFormat("{1} {0} (new Line 4)", "MediocreShort.[PredictedTrait] + .", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.Mediocre);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2 + ".";
					}
					else if(lowestTrait != StatSubType.SONGQUALITY) // (Line 4) GoodShort.[PredictedTrait] + Conjunction + BadShort.[LowestAppealingTrait]
					{
						Debug.LogFormat("{1} {0} (Line 4)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.[LowestAppealingTrait]", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(lowestTrait, ResponseType.BadShort);
						response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
					else // Lowest trait is song quality (hit meter) (line 5) GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter
					{
						Debug.LogFormat("{1} {0} (Line 5)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
				}
			}
			else // Insight unsuccessful
			{
				// Check appeal of the predicted trait
				if (!appeals.ContainsKey(insight.statType))
					Debug.LogErrorFormat("Appeals: {0}  InsightStatType: {1}", appeals, insight.statType);
				else if (appeals[insight.statType] >= 0.3f) // (line 6) GoodShort.[PredictedTrait] + Conjunction + ActualMostPopular.[MostPopular]
				{
					Debug.LogFormat("{1} {0} (Line 6)", "GoodShort.[PredictedTrait] + Conjunction + ActualMostPopular.[MostPopular]", loc.Name);
					responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
					response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
					response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];

					StatSubType mostPop;
					if (insight.statType.SuperType == StatType.GENRE)
						mostPop = mostPopularGenre;
					else if (insight.statType.SuperType == StatType.MOOD)
						mostPop = mostPopularMood;
					else
						mostPop = mostPopularTopic;

					responseSentence = get_response_sentence(mostPop, ResponseType.ActualMostPopular);
					response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(mostPop.Name).ToUpper() + "</color>" + responseSentence.string2;
				}
				else // (Line 7) BadLong.[PredictedTrait]
				{
					Debug.LogFormat("{1} {0} (Line 7)", "BadLong.[PredictedTrait]", loc.Name);
					responseSentence = get_response_sentence(insight.statType, ResponseType.BadLong);
					response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
				}
			}
		}
		else if(insight.insightType == MarketingInsights.InsightType.IsTrending)
		{
			if (insight.successful)
			{
				if (overallAppeal >= HIGH_APPEAL)
				{
					if (qualityValue >= 0.3f) // (Line 8)
					{
						Debug.LogFormat("{1} {0} (Line 8)", "GoodLong.[PredictedTrait]", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodLong);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
					else // < 0.3f (Line 9)
					{
						Debug.LogFormat("{1} {0} (Line 9)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
				}
				else
				{
					if (lowestTrait == insight.statType) // New line 11
					{
						Debug.LogFormat("{1} {0} (new Line 11)", "MediocreShort.[PredictedTrait] + .", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.Mediocre);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2 + ".";
					}
					else if (lowestTrait == StatSubType.SONGQUALITY) // (Line 11)
					{
						Debug.LogFormat("{1} {0} (Line 11)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.HitMeter", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(StatSubType.SONGQUALITY, ResponseType.BadShort);
						response += responseSentence.string1 + responseSentence.string2;
					}
					else // Lowest trait is song quality (hit meter) (line 10)
					{
						Debug.LogFormat("{1} {0} (Line 10)", "GoodShort.[PredictedTrait] + Conjunction + BadShort.[LowestAppealingTrait]", loc.Name);
						responseSentence = get_response_sentence(insight.statType, ResponseType.GoodShort);
						response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
						response += exampleSentences.conjunctions[Random.Range(0, exampleSentences.conjunctions.Length)];
						responseSentence = get_response_sentence(lowestTrait, ResponseType.BadShort);
						response += responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(lowestTrait.Name).ToUpper() + "</color>" + responseSentence.string2;
					}
				}
			}
			else // Insight unsuccessful (line 12)
			{
				Debug.LogFormat("{1} {0} (Line 12)", "NotTrendingUp.[PredictedTrait]", loc.Name);
				responseSentence = get_response_sentence(insight.statType, ResponseType.NotTrending);
				response = responseSentence.string1 + "<#FFC930>" + Utilities.InterceptText(insight.statType.Name).ToUpper() + "</color>" + responseSentence.string2;
			}
		}

		return "\"" + response + "\"";
	}

	private FeedbackObject get_response_sentence(StatSubType statType, ResponseType responseType)
	{
		if (responseType == ResponseType.GoodLong)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.goodLongGenre[Random.Range(0, exampleSentences.goodLongGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.goodLongMood[Random.Range(0, exampleSentences.goodLongMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.goodLongTopic[Random.Range(0, exampleSentences.goodLongTopic.Length)];
			else if (statType.SuperType == StatType.BAND_QUALITY)
				return exampleSentences.goodLongHitMeter[Random.Range(0, exampleSentences.goodLongHitMeter.Length)];
			else
				return null;
		}
		else if(responseType == ResponseType.BadLong)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.badLongGenre[Random.Range(0, exampleSentences.badLongGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.badLongMood[Random.Range(0, exampleSentences.badLongMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.badLongTopic[Random.Range(0, exampleSentences.badLongTopic.Length)];
			else if (statType.SuperType == StatType.BAND_QUALITY)
				return exampleSentences.badLongHitMeter[Random.Range(0, exampleSentences.badLongHitMeter.Length)];
			else
				return null;
		}
		else if (responseType == ResponseType.GoodShort)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.goodShortGenre[Random.Range(0, exampleSentences.goodShortGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.goodShortMood[Random.Range(0, exampleSentences.goodShortMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.goodShortTopic[Random.Range(0, exampleSentences.goodShortTopic.Length)];
			else if (statType.SuperType == StatType.BAND_QUALITY)
				return exampleSentences.goodShortHitMeter[Random.Range(0, exampleSentences.goodShortHitMeter.Length)];
			else
				return null;
		}
		else if (responseType == ResponseType.BadShort)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.badShortGenre[Random.Range(0, exampleSentences.badShortGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.badShortMood[Random.Range(0, exampleSentences.badShortMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.badShortTopic[Random.Range(0, exampleSentences.badShortTopic.Length)];
			else if (statType.SuperType == StatType.BAND_QUALITY)
				return exampleSentences.badShortHitMeter[Random.Range(0, exampleSentences.badShortHitMeter.Length)];
			else
				return null;
		}
		else if (responseType == ResponseType.Mediocre)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.mediocreGenre[Random.Range(0, exampleSentences.mediocreGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.mediocreMood[Random.Range(0, exampleSentences.mediocreMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.mediocreTopic[Random.Range(0, exampleSentences.mediocreTopic.Length)];
			else if (statType.SuperType == StatType.BAND_QUALITY)
				return exampleSentences.mediocreHitMeter[Random.Range(0, exampleSentences.mediocreHitMeter.Length)];
			else
				return null;
		}
		else if (responseType == ResponseType.ActualMostPopular)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.actualMostPopGenre[Random.Range(0, exampleSentences.actualMostPopGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.actualMostPopMood[Random.Range(0, exampleSentences.actualMostPopMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.actualMostPopTopic[Random.Range(0, exampleSentences.actualMostPopTopic.Length)];
			else
				return null;
		}
		else if (responseType == ResponseType.NotTrending)
		{
			if (statType.SuperType == StatType.GENRE)
				return exampleSentences.notTrendingGenre[Random.Range(0, exampleSentences.notTrendingGenre.Length)];
			else if (statType.SuperType == StatType.MOOD)
				return exampleSentences.notTrendingMood[Random.Range(0, exampleSentences.notTrendingMood.Length)];
			else if (statType.SuperType == StatType.TOPIC)
				return exampleSentences.notTrendingTopic[Random.Range(0, exampleSentences.notTrendingTopic.Length)];
			else
				return null;
		}

		return null;
	}
}
