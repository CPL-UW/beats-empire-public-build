using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Utility;

public class DataSimulationManager : MonoBehaviour
{
    private static DataSimulationManager _instance;
    public static DataSimulationManager Instance
    {
        get
        {
            //If _instance is null then we find it from the scene 
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<DataSimulationManager>();
            return _instance;
        }
    }

    [Header("- Generated within Application -")]
    public List<Person> m_PersonsGenerated = new List<Person>();
    public List<Person> m_PersonsGeneratedCurrentTurn = new List<Person>(); //this holds the person data with rule applied + surge applied for the current turn
    public List<Person> m_PersonAvgsByLocationCurrentTurn = new List<Person>(); //this holds 1 record per location person data avgs for the current turn so we have light calculations

    public List<Song> m_SongsGenerated = new List<Song>();
    public List<ListenData> m_ListensCurrentTurn = new List<ListenData>();//this holds all listens from the current turn
    public List<ListenData> m_ListensAllTurns = new List<ListenData>();//this holds all listens from a time range we decide upon

    [Header("- Editable Simulation Values -")]
    public string[] simulated_Names;
    [Space]
    public List<CorrelationRuleList> m_CorrelationRuleList = new List<CorrelationRuleList>();

    private List<PopulationData> m_PopulationByLocation = new List<PopulationData>();

    [Space]
    public int m_CurrentTurn = 0;

    [Header("- Non-Editable Runtime References -")]
    public PersonPanel personPanel;
    public Release release;
    public List<Stat> StatDatabase = new List<Stat>();
    [SerializeField]
    private StatList[] StatLists;

    public List<SurgeGenerator> SurgeGenerators;

    public GameInitializationVariables GameInitializationVariables;
    public DataSimulationVariables DataSimulationVariables;
	public DataCollectionParameters dataCollectionParameters;

    public List<Surge> activeSurges;

    private class MarketData
    {
        public float Fatigue;
        public float Hype;
    }

    private Dictionary<StatSubType, MarketData> marketData;

    private Dictionary<StatSubType, List<float>> cachedPreferenceData;
    private Dictionary<StatSubType, Dictionary<StatSubType, List<float>>> cachedLocationData;
    private Dictionary<StatSubType, List<float>> cachedIndustryPreferenceData;
    private Dictionary<StatSubType, Dictionary<StatSubType, List<float>>> cachedIndustryLocationData;
	private Dictionary<StatSubType, List<Stat>> boroughToPrefs;

    public void Init()
    {
        LoadStats();
        this.activeSurges = new List<Surge>();
		boroughToPrefs = new Dictionary<StatSubType, List<Stat>>();

        this.cachedPreferenceData = new Dictionary<StatSubType, List<float>>();
        this.cachedLocationData = new Dictionary<StatSubType, Dictionary<StatSubType, List<float>>>();

        this.cachedIndustryPreferenceData = new Dictionary<StatSubType, List<float>>();
        this.cachedIndustryLocationData = new Dictionary<StatSubType, Dictionary<StatSubType, List<float>>>();

        this.marketData = new Dictionary<StatSubType, MarketData>();

        foreach (StatSubType genre in StatSubType.GetFilteredList(StatType.GENRE, false))
        {
            this.marketData[genre] = new MarketData();
        }

        foreach (StatSubType topic in StatSubType.GetFilteredList(StatType.TOPIC, false))
        {
            this.marketData[topic] = new MarketData();
        }

        foreach (StatSubType mood in StatSubType.GetFilteredList(StatType.MOOD, false))
        {
            this.marketData[mood] = new MarketData();
        }

        foreach (StatSubType location in StatSubType.GetFilteredList(StatType.LOCATION, false))
        {
            this.cachedLocationData[location] = new Dictionary<StatSubType, List<float>>();
            this.cachedIndustryLocationData[location] = new Dictionary<StatSubType, List<float>>();
        }

        foreach (StatSubType statSubType in StatSubType.List)
        {
            this.cachedPreferenceData[statSubType] = new List<float>();
            this.cachedIndustryPreferenceData[statSubType] = new List<float>();

            foreach (StatSubType location in StatSubType.GetFilteredList(StatType.LOCATION, false))
            {
                this.cachedLocationData[location][statSubType] = new List<float>();
                this.cachedIndustryLocationData[location][statSubType] = new List<float>();
            }
        }
    }

    public Dictionary<StatSubType, List<float>> GetCachedPreferenceData()
    {
        return this.cachedPreferenceData;
    }

    public Dictionary<StatSubType, List<float>> GetCachedLocationData(StatSubType location)
    {
        return this.cachedLocationData[location];
    }

    public Dictionary<StatSubType, List<float>> GetCachedIndustryPreferenceData()
    {
        return this.cachedIndustryPreferenceData;
    }

    public Dictionary<StatSubType, List<float>> GetCachedIndustryLocationData(StatSubType location)
    {
        return this.cachedIndustryLocationData[location];
    }

    public float GetSongAppeal(Song song)
    {
        float sum = 0;

        foreach (StatSubType stat in song.Stats)
        {
            sum += this.cachedPreferenceData[stat][this.cachedPreferenceData[stat].Count - 1];
        }

        return sum / song.Stats.Count;
    }

	public void PrecomputeAggregatePreferences()
	{
		List<StatSubType> locations = StatSubType.GetFilteredList(StatType.LOCATION_ID, false);
		foreach (StatSubType location in locations)
		{
			boroughToPrefs[location] = GenerateAggregatePreference(location, StatSubType.NONE);
		}
	}

    public float GetSongAppealInLocation(Song song, StatSubType location)
    {
		float oldSum = 0f;
        float sum = 0f;
		Dictionary<StatType, float> statAppeals = new Dictionary<StatType, float>();
		List<Stat> aggregateLocationStats = boroughToPrefs[location];

        foreach (Stat stat in aggregateLocationStats)
        {
            if (song.Stats.Contains(stat.statSubType))
            {
                float marketFeelings = (1 + Mathf.Pow(this.marketData[stat.statSubType].Hype, this.DataSimulationVariables.HypeDiminishingFactor)) / (1 + this.marketData[stat.statSubType].Fatigue);
				oldSum += stat.floatVal * marketFeelings;
				statAppeals.Add(stat.statType, stat.floatVal * marketFeelings);
				//if(song.TargetedLocation == location)
					//Debug.LogFormat("{1} appeal: {0} interestRating: {2}", stat.floatVal * marketFeelings, Utility.Utilities.InterceptText(stat.statSubType.Name), stat.statType == StatType.GENRE ? locationInterests[0] : (stat.statType == StatType.MOOD ? locationInterests[1] : locationInterests[2]));
			}
        }

		float[] locationInterests = DataSimulationVariables.GetInterests(location);
		sum = statAppeals[StatType.GENRE] * locationInterests[0] + 
				statAppeals[StatType.MOOD] * locationInterests[1] + 
				statAppeals[StatType.TOPIC] * locationInterests[2];

		return sum;
    }

	public Dictionary<StatSubType, float> GetSpecificAppealsInLocation(Song song, StatSubType location, out float overallAppeal)
	{
		overallAppeal = 0f;
		Dictionary<StatSubType, float> appeals = new Dictionary<StatSubType, float>();

		List<Stat> aggregateLocationStats = this.GenerateAggregatePreference(location, StatSubType.NONE);
		float[] locationInterests = new float[3];

		switch (location.ID)
		{
			case StatSubType.BOOKLINE_ID: locationInterests = DataSimulationVariables.BooklineInterests; break;
			case StatSubType.IRONWOOD_ID: locationInterests = DataSimulationVariables.IronwoodInterests; break;
			case StatSubType.MADHATTER_ID: locationInterests = DataSimulationVariables.MadhatterInterests; break;
			case StatSubType.TURTLE_HILL_ID: locationInterests = DataSimulationVariables.TurtleHillInterests; break;
			case StatSubType.KINGS_ISLE_ID: locationInterests = DataSimulationVariables.KingsIsleInterests; break;
			case StatSubType.THE_BRONZ_ID: locationInterests = DataSimulationVariables.BronzInterests; break;
			default: break;
		}

		foreach (Stat stat in aggregateLocationStats)
		{
			if (song.Stats.Contains(stat.statSubType) && DataSimulationVariables.getBoroughInterests(location)[stat.statType == StatType.GENRE ? 0 : (stat.statType == StatType.MOOD ? 1 : 2)])
			{
				float marketFeelings = (1 + Mathf.Pow(this.marketData[stat.statSubType].Hype, this.DataSimulationVariables.HypeDiminishingFactor)) / (1 + this.marketData[stat.statSubType].Fatigue);
				overallAppeal += stat.floatVal * marketFeelings;
				appeals.Add(stat.statSubType, stat.floatVal * marketFeelings);
			}
		}

		overallAppeal /= appeals.Count;

		return appeals;
	}

	public StatSubType GetMostPopularTraitInLocation(StatSubType location, StatType mainType)
	{
		Dictionary<StatSubType, List<float>> data;
		//data = DataSimulationManager.Instance.GetCachedIndustryPreferenceData();
		data = DataSimulationManager.Instance.GetCachedIndustryLocationData(location);

		StatSubType highestType = StatSubType.NONE;
		float highestValue = 0f;
		foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
		{
			// Need to check all of the types of this super to see whose is the highest
			if (kvp.Key.SuperType == mainType)
			{
				List<float> values = kvp.Value.Cast<float>().ToList();
				float lastValue = values[values.Count - 1];
				if (lastValue > highestValue)
				{
					highestValue = lastValue;
					highestType = kvp.Key;
				}
			}
		}
		return highestType;
	}

	public void ProcessInitialSongSales(Song song, MarketingInsightObject insight)
    {
		PrecomputeAggregatePreferences();
        song.TurnData = new List<Dictionary<StatSubType, Song.MarketData>>();

        Dictionary<StatSubType, Song.MarketData> newListens = new Dictionary<StatSubType, Song.MarketData>();

        float totalSales = 0;
        float totalPopulation = 0;

        song.BellCurveOffset = 1 / this.BellCurveCalculation(0, song.Quality/3f, song.Quality/3f);

        foreach (PopulationData data in this.m_PopulationByLocation)
        {
            float appeal = this.GetSongAppealInLocation(song, data.Location);
            int initialSales = Mathf.RoundToInt(this.CalculateSongSalesAtLocation(song, data) * this.DataSimulationVariables.WordOfMouthSalesMultiplier * ((-3f / (song.Quality + 3f)) + 1.1f));
			//Debug.LogFormat("Song Quality: {0}, Quality Multiplier: {1}", song.Quality, ((-3f / (song.Quality + 3f)) + 1.1f)); //Multiplier approaches 1 as song quality increases
			
			if((float)initialSales > data.GetPopulation())
			{
				Debug.Log("Sales clamped to population");
				initialSales = (int)data.GetPopulation();
			}
			else if(initialSales < 0)
			{
				Debug.Log("Negative sales forced to 0 (Something went wrong???)");
				initialSales = 0;
			}

			//initialSales = Mathf.RoundToInt(Mathf.Clamp((float)initialSales, 0f, data.GetPopulation()));
			if(GameRefs.I.forceNextSongToSales >= 0)
				initialSales = GameRefs.I.forceNextSongToSales;

			int bonusAmount = 0;
			if (insight != null && insight.successful && insight.insightType == MarketingInsights.InsightType.MostPopular && data.Location == insight.songAttached.TargetedLocation)
			{
				bonusAmount = Mathf.RoundToInt((float)initialSales * (GameRefs.I.m_gameController.SongReleaseVariables.BonusSalesPercent + GetUpgradeBonusPercent(song.TargetedLocation)) / 100f);
			}
				
			initialSales += bonusAmount;

			if(GameRefs.I.m_marketingView.IsBoroughUnlocked(data.Location))
			{
				newListens[data.Location] = new Song.MarketData
				{
					Sales = initialSales,
					Listens = Mathf.RoundToInt(appeal * data.GetFollowers() * (song.StarRating / 5f) * this.DataSimulationVariables.WordOfMouthListensMultiplier),
					Appeal = appeal,
				};
			}
			else
			{
				newListens[data.Location] = new Song.MarketData
				{
					Sales = 0,
					Listens = 0,
					Appeal = 0f,
				};
			}     
			
            totalSales += GameRefs.I.m_marketingView.IsBoroughUnlocked(data.Location) ? initialSales : 0;
            totalPopulation += data.GetPopulation();
        }

        foreach (StatSubType subType in song.Stats)
        {
            this.marketData[subType].Fatigue += 5f * (totalSales / totalPopulation);
            this.marketData[subType].Hype += (float)song.Quality * (totalSales / totalPopulation);
        }

        song.TurnData.Add(newListens);
    }

	public void ProcessNextAllSongsSales(List<Song> songs, ref int totalSales, ref int totalListens)
	{
		PrecomputeAggregatePreferences();
		foreach (Song song in songs)
		{
			ProcessNextSongSales(song, ref totalSales, ref totalListens);
		}
	}

    public void ProcessNextSongSales(Song song, ref int totalSales, ref int totalListens)
    {
        Dictionary<StatSubType, Song.MarketData> newListens = new Dictionary<StatSubType, Song.MarketData>();

		int i = 0;
        foreach (PopulationData data in this.m_PopulationByLocation)
        {
            float appeal = this.GetSongAppealInLocation(song, data.Location);
            int sales = Mathf.RoundToInt(this.CalculateSongSalesAtLocation(song, data) * this.DataSimulationVariables.WordOfMouthSalesMultiplier * ((-3f / (song.Quality + 3f)) + 1.1f));
            int listens = Mathf.RoundToInt(appeal * data.GetFollowers() * (song.StarRating / 5f) * this.DataSimulationVariables.WordOfMouthListensMultiplier);

			if (!GameRefs.I.m_marketingView.IsBoroughUnlocked(data.Location))
			{
				sales = 0;
				listens = 0;
			}

			newListens[data.Location] = new Song.MarketData
			{
				Sales = sales,
				Listens = listens,
				Appeal = appeal,
			};


			totalSales += sales;
            totalListens += listens;
        }

        song.TurnData.Add(newListens);
    }

    public Dictionary<StatSubType, int> ProcessNewFollowers(Song song, MarketingInsightObject insight)
    {
		Dictionary<StatSubType, int> newFollowers = new Dictionary<StatSubType, int>();

		foreach (PopulationData data in this.m_PopulationByLocation)
        {
            float appeal = song.GetTurnAppeal(0, data.Location);

            int mostRecentSales = song.TurnData[song.TurnData.Count - 1][data.Location].Sales;

            float conversionRate;

            if (appeal >= this.DataSimulationVariables.FollowerAppealThreshold)
            {
                conversionRate = (DataSimulationVariables.FollowerConversionFactor *
                    (DataSimulationVariables.FanRatingFloor + (1- DataSimulationVariables.FanRatingFloor) * (song.StarRating / 5f)) * (appeal - DataSimulationVariables.FollowerAppealThreshold)) / 
                    (1 - DataSimulationVariables.FollowerAppealThreshold);
            }
            else 
            {
                conversionRate = (DataSimulationVariables.FollowerConversionFactor *
                    (DataSimulationVariables.FanRatingFloor + (1 - DataSimulationVariables.FanRatingFloor) * (1 - song.StarRating / 5f)) * (appeal - DataSimulationVariables.FollowerAppealThreshold)) /
                    (1 - DataSimulationVariables.FollowerAppealThreshold);
            }

            float convertibles = 1 - data.GetFollowers() / data.GetPopulation();

			int newFollowersAtLocation = Mathf.RoundToInt(mostRecentSales * conversionRate * convertibles);
			int bonusFollowers = 0;
			if (insight != null && insight.successful && insight.insightType == MarketingInsights.InsightType.IsTrending && insight.songAttached.TargetedLocation == data.Location)
			{
				bonusFollowers = Mathf.RoundToInt((float)newFollowersAtLocation * 
					((GameRefs.I.m_gameController.SongReleaseVariables.BonusFansPercent + GetUpgradeBonusPercent(song.TargetedLocation)) / 100f));
				int newFans = newFollowersAtLocation + bonusFollowers;
				// Hack because we don't want them to get "bonus" negative fans, so we'll give them a small amount
				if(newFans < 0)
				{
					Debug.Log("Forcing positive fan gain.");
					newFans = UnityEngine.Random.Range(GameRefs.I.m_gameController.SongReleaseVariables.BonusFansFixMin, GameRefs.I.m_gameController.SongReleaseVariables.BonusFansFixMax);
				}
					
				
				newFollowers[data.Location] = newFans;
			}
			else
			{
				if (GameRefs.I.m_marketingView.IsBoroughUnlocked(data.Location))
					newFollowers[data.Location] = newFollowersAtLocation;
				else
					newFollowers[data.Location] = 0;
			}
				
			
            data.AddPendingFollowers((float)newFollowers[data.Location]);
            //song.Artist.EarnFollowers(data.Location, newFollowers[data.Location]);
        }
        return newFollowers;
    }

    /// <summary>
    /// Start over with a clean backing list
    /// </summary>
    public void ClearPersonData()
    {
        m_PersonsGenerated.Clear();
    }

    /// <summary>
    /// Generate Raw Person objects and populate the data table
    /// </summary>
    public void GeneratePersonsData()
    {
        ClearPersonData();
        int quantityPersons = personPanel.GetQuantity();

        //find out how many locations
        List<Stat> locationsStatList = GetAllSubStatsOfType(StatType.LOCATION);

        //how many per location
        int quantityPersonsPerLocation = quantityPersons / locationsStatList.Count;


        for (int locIndex = 0; locIndex < locationsStatList.Count; locIndex++)
        {
            StatSubType assignedLocation = locationsStatList[locIndex].statSubType;
            for (int i = 0; i < quantityPersonsPerLocation; i++)
            {
                //add a person to the
                Person toAdd = GenerateOnePerson(assignedLocation);
                ApplyRules(ref toAdd); //apply correlation rules upon generating a person
                m_PersonsGenerated.Add(toAdd);

            }
        }
        //Need people in this first, so it goes under the person generating.
        ProcessSurges();
        //export the data for this - not required, just a way to check the data
        ExportPersonData(m_PersonsGenerated, "Consumers_Generated");

        MessagePanel.Instance.CallOpen("Successfully Generated New Consumers. View them in the DataSimulationManager.PersonsGenerated. CSV exported to Assets/Exports with raw consumer data.");
    }

    public void GeneratePeople(int peoplePerLocation)
    {
        ClearPersonData();
        
        //find out how many locations
        List<Stat> locationsStatList = GetAllSubStatsOfType(StatType.LOCATION);

        //how many per location
        int quantityPersonsPerLocation = peoplePerLocation;

        for (int locIndex = 0; locIndex < locationsStatList.Count; locIndex++)
        {
            StatSubType assignedLocation = locationsStatList[locIndex].statSubType;
            for (int i = 0; i < quantityPersonsPerLocation; i++)
            {
                //add a person to the
                Person toAdd = GenerateOnePerson(assignedLocation);
                ApplyRules(ref toAdd); //apply correlation rules upon generating a person
                m_PersonsGenerated.Add(toAdd);

            }
        }
    }


    /// <summary>
    /// Generate the person with randomized + weighted values
    /// </summary>
    private Person GenerateOnePerson(StatSubType location)
    {
        Person nextPerson = new Person();
        nextPerson.personName = GetRandomValue(simulated_Names);
        //nextPerson.stats.Add(CreateStat(StatType.LOCATION, GetRandomSubType(StatType.LOCATION), 0));
        nextPerson.stats.Add(CreateStat(StatType.LOCATION, location, 0));
        //add entry for each mood
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD1, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD5, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD2, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD4, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD3, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.MOOD, StatSubType.MOOD6, GetNormal()));

        //add entry for each topic
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC2, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC3, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC4, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC1, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC6, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.TOPIC, StatSubType.TOPIC5, GetNormal()));

        //add entry for Genre 
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.RAP, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.HIP_HOP, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.ROCK, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.RANDB, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.POP, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.GENRE, StatSubType.ELECTRONIC, GetNormal()));

        //add entry for song type prefs
        nextPerson.stats.Add(CreateStat(StatType.BAND_QUALITY, StatSubType.RELIABILITY, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.BAND_QUALITY, StatSubType.PERSISTENCE, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.BAND_QUALITY, StatSubType.SPEED, GetNormal()));
        nextPerson.stats.Add(CreateStat(StatType.BAND_QUALITY, StatSubType.AMBITION, GetNormal()));

        return nextPerson;
    }

    public int IncrementTurn(int nTurns)
    {
		StatType[] sampledStats = {
			StatType.MOOD,
			StatType.TOPIC,
			StatType.GENRE
		};

		for (int i = 0; i < nTurns; ++i)
		{
			m_CurrentTurn += 1;
			this.ProcessSurges();

			foreach (MarketData data in this.marketData.Values)
			{
				data.Fatigue -= this.DataSimulationVariables.FatigueDecayRate;
				data.Hype -= this.DataSimulationVariables.HypeDecayRate;

				if (data.Fatigue < 0)
				{
					data.Fatigue = 0;
				}

				if (data.Hype < 0)
				{
					data.Hype = 0;
				}
			}

			CacheNewAggregatePreferenceStats();

			foreach (StatType statType in sampledStats)
			{
				TraitSampling sampling = GameRefs.I.traitSamplings[statType];
				if (sampling.iteration == -1)
				{
					GameRefs.I.isSampledAt[statType].Add(null);
				}
				else
				{
					GameRefs.I.isSampledAt[statType].Add(sampling.iteration == 0);
				}

				if (GameRefs.I.isSampledAt[statType].Count > 31)
					GameRefs.I.isSampledAt[statType].RemoveAt(0);
				AdvanceSamplingIteration(statType);
			}
		}

        if (m_CurrentTurn < 0) m_CurrentTurn = 0;//we don't allow negative turns
        return m_CurrentTurn;
    }

	private void AdvanceSamplingIteration(StatType statType)
	{
		TraitSampling sampling = GameRefs.I.traitSamplings[statType];
		int iteration = sampling.iteration;
		int period = dataCollectionParameters.frequencies[sampling.slotCount].period;
		if (iteration >= 0)
		{
			sampling.iteration = (iteration + 1) % period;
		}
	}

	[ContextMenu("Print Surge IDs")]
	public void testSurges()
	{
		for(int i = 0; i < SurgeGenerators.Count; i++)
		{
			foreach(SurgeData sd in SurgeGenerators[i].SurgePool.Contents)
			{
				Debug.Log(sd.uniqueID);
			}
		}
	}

	public void LoadSurges(List<Surge.SavedData> loadedSurges)
	{
		activeSurges.Clear();
		for(int i = 0; i < loadedSurges.Count; i++)
		{
			this.activeSurges.Add(new Surge(SurgeGenerators[loadedSurges[i].GeneratorIndex].GetSurgeDataByUniqueID(loadedSurges[i].UniqueID), 
				loadedSurges[i]));
		}
	}

	public void LoadPersonFloatVals(float[] floatsIn)
	{
		List<StatSubType> subTypeList = new List<StatSubType>();
		int index = 0;
		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.ROCK_ID + i]);

		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.MOOD1_ID + i]);

		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.TOPIC1_ID + i]);

		for (int i = 0; i < this.m_PopulationByLocation.Count; i++)
		{
			for (int j = 0; j < this.m_PopulationByLocation[i].GenreBuckets.Count; j++)
			{
				for(int k = 0; k < subTypeList.Count; k++)
				{
					this.m_PopulationByLocation[i].GenreBuckets[j].InternalPerson.SetStatBySubType(subTypeList[k], floatsIn[index++]);
				}
			}
		}
	}

	public float[] SavePersonFloatVals()
	{
		List<StatSubType> subTypeList = new List<StatSubType>();
		List<float> floats = new List<float>();

		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.ROCK_ID + i]);

		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.MOOD1_ID + i]);

		for (int i = 0; i < 6; i++)
			subTypeList.Add(StatSubType.List[StatSubType.TOPIC1_ID + i]);

		for (int i = 0; i < this.m_PopulationByLocation.Count; i++)
		{
			for (int j = 0; j < this.m_PopulationByLocation[i].GenreBuckets.Count; j++)
			{
				for (int k = 0; k < subTypeList.Count; k++)
				{
					floats.Add(m_PopulationByLocation[i].GenreBuckets[j].InternalPerson.GetStatBySubType(subTypeList[k]).floatVal);
				}
			}
		}

		return floats.ToArray();
	}

	public void ProcessSurges()
    {
        List<Surge> surgesToRemove = new List<Surge>();

        foreach (Surge surge in this.activeSurges)
        {
            surge.Age++;

            if (surge.Age >= (surge.SurgeType.surgeLength + surge.AdditionalLength))
            {
                surgesToRemove.Add(surge);

                for (int i = 0; i < this.m_PopulationByLocation.Count; i++)
                {
                    for (int j = 0; j < this.m_PopulationByLocation[i].GenreBuckets.Count; j++)
                    {
                        if (this.m_PopulationByLocation[i].GenreBuckets[j].Population > 0)
                        {
                            if (surge.AffectsBucket(this.m_PopulationByLocation[i].Location, this.m_PopulationByLocation[i].GenreBuckets[j].Genre))
                            {
                                this.m_PopulationByLocation[i].GenreBuckets[j].InternalPerson.GetStatBySubType(surge.AffectedSubType).floatVal *= (1 + surge.GetCurrentModifier());
                            }
                        }
                    }
                }
            }
        }

        foreach (Surge surge in surgesToRemove)
        {
            this.activeSurges.Remove(surge);
        }

        for (int i = 0; i < this.SurgeGenerators.Count; i++)
        {
			int chosenSurge = -1;
            SurgeData data = this.SurgeGenerators[i].AdvanceTurnAndGetPendingSurge(this.activeSurges, out chosenSurge);

            if (data != null)
            {
				StatSubType subType = StatSubType.NONE;

				if (data.affectedSubStatType.Type == StatType.RANDOM_ID || data.affectedSubStatType.SubType == StatSubType.RANDOM_ID)
				{
					StatSubType location = StatSubType.List[data.location.SubType];
					List<StatSubType> validSubTypes = StatSubType.GetFilteredList(data.affectedSubStatType.Type, false).Where(x => !activeSurges.Where(surge => surge.AffectedLocation == location).Select(y => y.AffectedSubType).ToList().Contains(x)).ToList();
					if (validSubTypes.Count > 0)
					{
						subType = validSubTypes[UnityEngine.Random.Range(0, validSubTypes.Count)];
					}
				}
				else
				{
					subType = StatSubType.List[data.affectedSubStatType.SubType];
				}

				if (subType != StatSubType.NONE)
				{
					int bonusLength = 0;
					bonusLength += GetBonusSurgeWeeks(StatSubType.List[data.location.SubType]);
					this.activeSurges.Add(new Surge(data, subType, bonusLength, i, chosenSurge));
				}
            }
        }
    }

    /// <summary>
    /// CALL THIS TO GET RANDOM SUBTYPE BASED ON THE STAT TYPE
    /// </summary>
    public StatSubType GetRandomSubType(StatType statType)
    {
        StatSubType chosenStatSubtype = StatSubType.BOOKLINE;
        List<StatSubType> items = new List<StatSubType>();

        switch (statType.ID)
        {

            case StatType.LOCATION_ID:
                items = new List<StatSubType>();
                items.Add(StatSubType.BOOKLINE);
                items.Add(StatSubType.THE_BRONZ);
                items.Add(StatSubType.IRONWOOD);
                items.Add(StatSubType.KINGS_ISLE);
                items.Add(StatSubType.MADHATTER);
                items.Add(StatSubType.TURTLE_HILL);
                chosenStatSubtype = items[UnityEngine.Random.Range(0, items.Count)];
                break;
            case StatType.MOOD_ID:
                items = new List<StatSubType>();
				items.Add(StatSubType.MOOD1);
				items.Add(StatSubType.MOOD2);
                items.Add(StatSubType.MOOD3);
                items.Add(StatSubType.MOOD4);
                items.Add(StatSubType.MOOD5);
				items.Add(StatSubType.MOOD6);
				chosenStatSubtype = items[UnityEngine.Random.Range(0, items.Count)];
                break;

            case StatType.TOPIC_ID:
                items = new List<StatSubType>();
                items.Add(StatSubType.TOPIC5);
                items.Add(StatSubType.TOPIC2);
                items.Add(StatSubType.TOPIC4);
                items.Add(StatSubType.TOPIC3);
                items.Add(StatSubType.TOPIC1);
                items.Add(StatSubType.TOPIC6);
                chosenStatSubtype = items[UnityEngine.Random.Range(0, items.Count)];
                break;
        }

        return chosenStatSubtype;
    }

    /// <summary>
    /// apply all active rules to this person
    /// </summary>
    private void ApplyRules(ref Person p)
    {
        foreach (CorrelationRule corr in m_CorrelationRuleList[0].correlationRuleList)
        {
            //if the rule is not active, we dont apply it
            if (!corr.m_IsActive) continue;

            bool shouldApplyRule = false;

            //if this is a location triggerred surge and we match the location:
            if (corr.m_LocationTrigger.SubType == StatSubType.NONE_ID || corr.m_LocationTrigger.SubType == p.GetStatByType(StatType.LOCATION).statSubType.ID)
            {
                shouldApplyRule = true;
            }

            //if (corr.m_SubStatTrigger.SubType != StatSubType.NONE_ID)
            //{
            //    //this is only needed for the above/below threshold triggers
            //    float matchingStatVal = p.GetStatBySubType(StatSubType.List[corr.m_SubStatTrigger.SubType]).floatVal;
            //
            //    //if this is an above threshold triggered surge and we are above the threshold
            //    if (corr.m_TriggeredIfAboveThreshold & matchingStatVal > corr.m_SubStatThreshhold)
            //    {
            //        shouldApplyRule = true;
            //    }
            //
            //    //if this is a below threshhold triggered surge and we are below the threshold
            //    if (corr.m_TriggeredIfBelowThreshold & matchingStatVal < corr.m_SubStatThreshhold)
            //    {
            //        shouldApplyRule = true;
            //    }
            //}

            //we did not match any of the trigger values so we are not going to apply the modifier
            if (!shouldApplyRule) continue;

            //Get the stat to affect
            Stat personAffectedStat = p.GetStatBySubType(corr.GetAffectedSubType());

            //Debug and check values
            //Debug.Log("Rule Applied on " + p.personName + " | "
            //    + " match: " + corr.affectedByStat.statType + " " + corr.affectedByStat.statSubType + " | "
            //    + " " + corr.affectedStat.statSubType
            //    + " modified by " + corr.affectedStat.floatVal + "\n"
            //    + " Change Amount: " + modifier + " | "
            //    + " Starting Val: " + personAffectedStat.floatVal + " | "
            //    + " Ending Val: " + (personAffectedStat.floatVal += modifier));

            //apply the modifer
            personAffectedStat.floatVal += corr.m_ModifierVal;
            //apply clamp

            personAffectedStat.floatVal = Mathf.Clamp(personAffectedStat.floatVal, 0, 1);
        }

    }

    /// <summary>
    /// Snap fresh copy of person data, apply any modifier data eg:surge and then save in m_PersonsGeneratedCurrentTurn
    /// </summary>
    public void EvaluatePersonDataAtTurn(int turnToEval)
    {
        //Debug.Log("EvaluatePersonDataAtTurn:" + turnToEval);
        //loop through Person Generated
        //apply rule for each
        //apply surge for each
        //export the entire set (dont affect base data

        //Clear the list
        m_PersonsGeneratedCurrentTurn.Clear();
        //Deep copy the raw person list
        m_PersonsGeneratedCurrentTurn = Utilities.DeepClone(m_PersonsGenerated);
        //Now m_PersonsGeneratedCurrentTurn is not a reference to m_PersonsGenerated. We can modify the data
        for (int i = 0; i < m_PersonsGeneratedCurrentTurn.Count; i++)
        {
            Person nextPerson = m_PersonsGeneratedCurrentTurn[i];
            ApplyRules(ref nextPerson);
        }
    }

    /// <summary>
    /// Calculate how many listens we should have. Applied to the averages of each location
    /// </summary>
    public void EvaluateListensDataAtTurn(int turnIndex)
    {
        Debug.Log("EvaluateListenDataAtTurn():" + turnIndex);
        //loop through songs
        //loop through person
        //store listen data into m_ListensAllTurns
        m_ListensCurrentTurn.Clear();
        foreach (Song nextSong in m_SongsGenerated)
        {
            foreach (Person p in m_PersonAvgsByLocationCurrentTurn)
            {
                float sumOfStatVals = 0;
                StatSubType nextLocation = p.GetStatByType(StatType.LOCATION).statSubType;

                foreach (StatSubType nextStat in nextSong.Stats)
                {
                    // Debug.Log("Next SubType:" + nextStat.statSubType);
                    sumOfStatVals += p.GetStatBySubType(nextStat).floatVal;
                }

                float nextAppeal = sumOfStatVals / nextSong.Stats.Count();
                float nextListens = 0;

                //word of mouth requirements
                float prevWordOfMouth = 0;
                ListenData listenDataPrevTurn = m_ListensAllTurns.Where(x => x.location == nextLocation & x.song == nextSong & x.turn == turnIndex - 1).FirstOrDefault();
                if (listenDataPrevTurn != null)
                {
                    prevWordOfMouth = listenDataPrevTurn.wordOfMouth;
                    if (turnIndex == 1) prevWordOfMouth = listenDataPrevTurn.listens;
                }

                float latencyWordOfMouth = 1;
                float marketImpen = 1;
                float population = m_PopulationByLocation.Where(x => x.Location == nextLocation).FirstOrDefault().GetPopulation();
                float nextWordOfMouth = GetWordOfMouth(prevWordOfMouth, nextAppeal, turnIndex, latencyWordOfMouth, population, marketImpen);
                float nextFreshness = GetFreshness(nextSong.StarRating / 5, turnIndex, 1);

                //apply turn zero logic here...things that cannot happen the same on initial turn
                if (turnIndex == 0)
                {
                    nextListens = 0;
                    nextWordOfMouth = 0;
                    nextFreshness = 0;
                }
                else
                {
                    nextListens = nextWordOfMouth * nextFreshness;
                }

                ListenData nextListenData = new ListenData()
                {
                    locationString = p.GetStatByType(StatType.LOCATION).statSubType.ToString(),
                    location = nextLocation,
                    song = nextSong,
                    starRating = nextSong.StarRating,
                    turn = turnIndex,
                    appeal = nextAppeal,
                    listens = nextListens,
                    wordOfMouth = nextWordOfMouth,
                    freshness = nextFreshness,
                };
                m_ListensCurrentTurn.Add(nextListenData);
            }
        }
    }

    /// <summary>
    /// Generate Location Averages
    /// </summary>
    public void EvaluateLocationAveragesAtTurn()
    {
        if (m_PersonsGeneratedCurrentTurn.Count == 0)
        {
            Debug.Log("EvaluateLocationAveragesAtTurn() Leaving. No turn data to process.");
            return; //no turn data - quit trying to evaluate averages
        }

        List<Stat> allLocations = GetAllSubStatsOfType(StatType.LOCATION);

        //Loop Through Locations
        foreach (Stat item in allLocations)
        {
            List<Stat> locationAverages = new List<Stat>(); //this will hold all of our averaged stats for this location
            //get the persons for next location
            List<Person> personByLocation = m_PersonsGeneratedCurrentTurn.Where(x => x.GetStatByType(StatType.LOCATION).statSubType == item.statSubType).ToList<Person>();
            float totalPersons = personByLocation.Count;

            //loop through each stat (use first person as template)
            for (int statIndex = 0; statIndex < personByLocation[0].stats.Count(); statIndex++)
            {
                Stat nextStatAvg = new Stat();
                float totalStatVal = 0;

                //loop through persons for next location
                //average the stat and store it
                for (int personIndex = 0; personIndex < totalPersons; personIndex++)
                {
                    Person nextPerson = personByLocation[personIndex];
                    totalStatVal += nextPerson.stats[statIndex].floatVal;
                }

                //Note we can use the [0] index person to copy everything except the value
                nextStatAvg = new Stat()
                {
                    floatVal = totalStatVal / totalPersons,
                    inspectorTitle = personByLocation[0].stats[statIndex].inspectorTitle,
                    statType = personByLocation[0].stats[statIndex].statType,
                    statSubType = personByLocation[0].stats[statIndex].statSubType,
                };

                locationAverages.Add(nextStatAvg);
            }//end stat loop

            //Create a new person and put it in averages for stats m_PersonAvgsByLocation
            Person averageByLocation = new Person()
            {
                personName = "averaged_persons",
                stats = locationAverages
            };

            m_PersonAvgsByLocationCurrentTurn.Add(averageByLocation);
        }
    }

    /// <summary>
    /// Generate Location Averages
    /// </summary>
    public void CollapsePopulationData()
    {
        if (m_PersonsGenerated.Count == 0)
        {
            Debug.Log("EvaluateLocationAveragesAtTurn() Leaving. No turn data to process.");
            return; //no turn data - quit trying to evaluate averages
        }

        List<Stat> allLocations = GetAllSubStatsOfType(StatType.LOCATION);
        List<Stat> allGenres = GetAllSubStatsOfType(StatType.GENRE);

        //Loop Through Locations
        foreach (Stat item in allLocations)
        {
            Person templatePerson = this.GenerateOnePerson(StatSubType.BOOKLINE);

            //get the persons for next location
            List<Person> personByLocation = m_PersonsGenerated.Where(x => x.GetStatByType(StatType.LOCATION).statSubType == item.statSubType).ToList<Person>();
            float totalPersons = personByLocation.Count;

            Dictionary<StatSubType, List<Person>> genreBuckets = new Dictionary<StatSubType, List<Person>>();
            List<GenrePersonBucket> bucketedPersons = new List<GenrePersonBucket>();

            foreach (Stat genre in allGenres)
            {
                genreBuckets[genre.statSubType] = personByLocation.Where(x => x.stats.Where(y => y.statType == StatType.GENRE).OrderBy(z => z.floatVal).Last().statSubType == genre.statSubType).ToList();
            }

            foreach (KeyValuePair<StatSubType, List<Person>> kvp in genreBuckets)
            {
                List<Stat> genreAverages = new List<Stat>(); //this will hold all of our averaged stats for this location

                //loop through each stat (use first person as template)
                for (int statIndex = 0; statIndex < templatePerson.stats.Count(); statIndex++)
                {
                    Stat nextStatAvg = new Stat();
                    float totalStatVal = 0;

                    //loop through persons for next location
                    //average the stat and store it
                    for (int personIndex = 0; personIndex < kvp.Value.Count; personIndex++)
                    {
                        totalStatVal += kvp.Value[personIndex].stats[statIndex].floatVal;
                    }

                    //Note we can use the [0] index person to copy everything except the value
                    nextStatAvg = new Stat()
                    {
                        floatVal = kvp.Value.Count > 0 ? totalStatVal / kvp.Value.Count : 0,
                        inspectorTitle = templatePerson.stats[statIndex].inspectorTitle,
                        statType = templatePerson.stats[statIndex].statType,
                        statSubType = templatePerson.stats[statIndex].statSubType,
                    };

                    genreAverages.Add(nextStatAvg);
                }//end stat loop

                float population = kvp.Value.Count / totalPersons;

                switch (item.statSubType.ID)
                {
                    case StatSubType.TURTLE_HILL_ID:
                        population *= this.DataSimulationVariables.TurtleHillPopulation;
                        break;
                    case StatSubType.MADHATTER_ID:
                        population *= this.DataSimulationVariables.MadhatterPopulation;
                        break;
                    case StatSubType.IRONWOOD_ID:
                        population *= this.DataSimulationVariables.IronwoodPopulation;
                        break;
                    case StatSubType.THE_BRONZ_ID:
                        population *= this.DataSimulationVariables.BronzPopulation;
                        break;
                    case StatSubType.KINGS_ISLE_ID:
                        population *= this.DataSimulationVariables.KingsIslePopulation;
                        break;
                    case StatSubType.BOOKLINE_ID:
                        population *= this.DataSimulationVariables.BooklinePopulation;
                        break;
                }


                //Create a new person and put it in averages for stats m_PersonAvgsByLocation
                GenrePersonBucket averageByGenre = new GenrePersonBucket
                {
                    InternalPerson = new Person()
                    {
                        personName = "averaged_persons",
                        stats = genreAverages,
                    },
                    Population = population,
                    Genre = kvp.Key,
                };

                bucketedPersons.Add(averageByGenre);
            }

			m_PopulationByLocation.Add(new PopulationData(item.statSubType, bucketedPersons,  this.GameInitializationVariables.StartingFansPercentofPop / 100f));
        }
    }

    public List<Stat> GenerateAggregatePreference(StatSubType specifiedLocation, StatSubType specifiedGenreCommunity)
    {
        List<Stat> aggregateStats = new List<Stat>();

        foreach (StatSubType statSubType in StatSubType.List)
        {
            if (statSubType == StatSubType.NONE || statSubType == StatSubType.RANDOM || statSubType.SuperType == StatType.LOCATION || statSubType.SuperType == StatType.DISTRIBUTION_POINTS)
            {
                continue;
            }

            float statSum = 0;
            float populationSum = 0;

            /* for (int i = 0; i < this.m_PopulationByLocation.Count; i++) */
            /* { */
                /* for (int j = 0; j < this.m_PopulationByLocation[i].GenreBuckets.Count; j++) */
                /* { */
                    /* if (this.m_PopulationByLocation[i].GenreBuckets[j].Population <= 0 ||  */
                        /* (specifiedLocation != StatSubType.NONE && specifiedLocation != this.m_PopulationByLocation[i].Location) || */
                        /* (specifiedGenreCommunity != StatSubType.NONE && specifiedLocation != this.m_PopulationByLocation[i].GenreBuckets[j].Genre)) */
                    /* { */
                        /* continue; */
                    /* } */

                    /* float surgeModifier = 0; */

                    /* foreach (Surge surge in this.activeSurges.Where(x => x.AffectedSubType == statSubType && */
                    /* (x.AffectsBucket(this.m_PopulationByLocation[i].Location, this.m_PopulationByLocation[i].GenreBuckets[j].Genre)))) */
                    /* { */
                        /* surgeModifier += surge.GetCurrentModifier(); */
                    /* } */

                    /* statSum += (this.m_PopulationByLocation[i].GenreBuckets[j].InternalPerson.GetStatBySubType(statSubType).floatVal + surgeModifier) * */
                        /* this.m_PopulationByLocation[i].GenreBuckets[j].Population; */

                    /* populationSum += this.m_PopulationByLocation[i].GenreBuckets[j].Population; */
                /* } */
            /* } */

			foreach (PopulationData population in m_PopulationByLocation)
			{
				foreach (GenrePersonBucket bucket in population.GenreBuckets)
				{
                    if (bucket.Population <= 0 || 
                        (specifiedLocation != StatSubType.NONE && specifiedLocation != population.Location) ||
                        (specifiedGenreCommunity != StatSubType.NONE && specifiedLocation != bucket.Genre))
                    {
                        continue;
                    }

					float surgeModifier = 0;
					foreach (Surge surge in activeSurges)
					{
						if (surge.AffectedSubType == statSubType && surge.AffectsBucket(population.Location, bucket.Genre))
						{
							surgeModifier += surge.GetCurrentModifier();
						}
					}

                    /* float surgeModifier = activeSurges.Where(x => x.AffectedSubType == statSubType && (x.AffectsBucket(population.Location, bucket.Genre))).Select(x => x.GetCurrentModifier()).Sum(); */
                    statSum += (bucket.InternalPerson.GetStatBySubType(statSubType).floatVal + surgeModifier) * bucket.Population;
                    populationSum += bucket.Population;
                }
            }

            aggregateStats.Add(new Stat()
            {
                floatVal = statSum / populationSum,
                inspectorTitle = statSubType.Name,
                statType = statSubType.SuperType,
                statSubType = statSubType,
            });
        }

        return aggregateStats;
    }

    public void CacheNewAggregatePreferenceStats()
    {
        foreach (Stat stat in this.GenerateAggregatePreference(StatSubType.NONE, StatSubType.NONE))
        {
            this.cachedPreferenceData[stat.statSubType].Add(Mathf.Max(0, stat.floatVal));
			if (cachedPreferenceData[stat.statSubType].Count > 31)
				cachedPreferenceData[stat.statSubType].RemoveAt(0);

			this.cachedIndustryPreferenceData[stat.statSubType].Add(Mathf.Max(0, this.DataSimulationVariables.IndustryListensOffset + stat.floatVal * this.DataSimulationVariables.IndustryListensFactor) + (UnityEngine.Random.value - 0.5f) * 2 * this.DataSimulationVariables.IndustryListensRandomizer);
			if (cachedIndustryPreferenceData[stat.statSubType].Count > 31)
				cachedIndustryPreferenceData[stat.statSubType].RemoveAt(0);
		}

        foreach (StatSubType subType in StatSubType.GetFilteredList(StatType.LOCATION, false))
        {
            foreach (Stat stat in this.GenerateAggregatePreference(subType, StatSubType.NONE))
            {
                this.cachedLocationData[subType][stat.statSubType].Add(Mathf.Max(0, stat.floatVal));
				this.cachedIndustryLocationData[subType][stat.statSubType].Add(Mathf.Max(0, this.DataSimulationVariables.IndustryListensOffset + stat.floatVal * this.DataSimulationVariables.IndustryListensFactor) + (UnityEngine.Random.value - 0.5f) * 2 * this.DataSimulationVariables.IndustryListensRandomizer);

				// Limit these arrays to 31 weeks
				if (this.cachedLocationData[subType][stat.statSubType].Count > 31)
					this.cachedLocationData[subType][stat.statSubType].RemoveAt(0);
				if (this.cachedIndustryLocationData[subType][stat.statSubType].Count > 31)
					this.cachedIndustryLocationData[subType][stat.statSubType].RemoveAt(0);
			}
        }
    }

    /// <summary>
    /// Calculate Listens and add to a list of data
    /// </summary>
    public void EvaluateListensDataUpToTurn()
    {
        Debug.Log("EvaluateListensDataUpToTurn");
        if (m_SongsGenerated.Count == 0)
        {
            MessagePanel.Instance.CallOpen("Wait! You do not have any songs generated. Go to the Song Generator tab and create one.");
            return;
        }
        if (m_CurrentTurn == 0)
        {
            MessagePanel.Instance.CallOpen("Wait! You are at turn zero. Advance turns before calculating this.");
            return;
        }

        //for each turn from 0 to now
        for (int turnIndex = 0; turnIndex <= m_CurrentTurn; turnIndex++)
        {
            //populate the list of turn data with current turn index
            EvaluatePersonDataAtTurn(turnIndex);
            //evaluate averages for the turn this cursor is on
            EvaluateLocationAveragesAtTurn();
            //evaluate listens for this turn and put them into a list 
            EvaluateListensDataAtTurn(turnIndex);
            //add these into the all turns list
            for (int listenIndex = 0; listenIndex < m_ListensCurrentTurn.Count; listenIndex++)
            {
                m_ListensAllTurns.Add(m_ListensCurrentTurn[listenIndex]);
            }
        }
        //calculate person turn data
        //calculate averages
        //add affinities and divide by numbe
        MessagePanel.Instance.CallOpen("Listens generated up to the current turn. View them in the DataSimulationManager m_ListensAllTurns.");

    }

    private float GetWordOfMouth(float wordOfMouthPrevTurn, float thisTurnAppeal, float numTurnsSinceRelease, float latencyWordOfMouth, float population, float marketImpenetrability)
    {
        float wordOfMouth = wordOfMouthPrevTurn * (1 + Mathf.Pow(thisTurnAppeal, (numTurnsSinceRelease * latencyWordOfMouth)) * (Mathf.Pow((1 - (wordOfMouthPrevTurn / population)), marketImpenetrability)));
        //Debug.Log("Word Of Mouth:" + wordOfMouth);
        return wordOfMouth;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="starRating">Should come in as a finished float percent!</param>
    private float GetFreshness(float starRating, float numTurnsSinceRelease, float decayRate)
    {
        float freshness = Mathf.Pow(starRating, (numTurnsSinceRelease / decayRate));
        //(Star Rating / 5 Star Maximum) ^ (Number of Turns Since Release / Decay Rate)
        return freshness;
    }

    /// <summary>
    /// Create a new stat
    /// </summary>
    public Stat CreateStat(StatType nextStatType, StatSubType nextStatSubtype, float nextVal)
    {
        Stat nextStat = new Stat
        {
            inspectorTitle = nextStatSubtype.ToString(),
            statSubType = nextStatSubtype,
            statType = nextStatType,
            floatVal = nextVal
        };
        return nextStat;
    }

    /// <summary>
    /// Returns a list of all stats based on type - ideal for things like populating a dropdown menu
    /// </summary>
    /// <param name="statType"></param>
    /// <returns></returns>
    public List<Stat> GetAllSubStatsOfType(StatType statType)
    {
        List<Stat> matchingStats = new List<Stat>();
        foreach (Stat s in StatDatabase)
        {
            if (s.statType == statType)
            {
                matchingStats.Add(s);
            }
        }

        return matchingStats;
    }

    /// <summary>
    /// Simple 0 to 1f to use for now while generating data
    /// </summary>
    /// <returns></returns>
    double sigma = .25;
    double mean = .5f;
    private float GetNormal()
    {
        float x = (float)Utilities.NextGaussian(mean, sigma); //using Next Gaussian for Normal Dist
        x = Mathf.Abs(x); //Abs for always positive, float for ease of use
        x = Mathf.Clamp(x, 0, 1); //clamp keeps the outliers trimmed to between 0 and 1
        return x;
    }

    private void LoadStats()
    {
        for (int statListIndex = 0; statListIndex < StatLists.Length; statListIndex++)
        {
            //Debug.Log("StatListIndex:" + statListIndex);
            for (int statIndex = 0; statIndex < StatLists[statListIndex].Contents.Count; statIndex++)
            {
                //Debug.Log("Stat:" + StatLists[statListIndex].statList[statIndex].name);
                StatDatabase.Add(StatLists[statListIndex].Contents[statIndex]);
                StatSubType.List[StatLists[statListIndex].Contents[statIndex].statSubType.ID].GraphColor = StatLists[statListIndex].Contents[statIndex].Color;
            }
        }
    }

    private string GetRandomValue(string[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    private float GetLocationPopulation(StatSubType locationRequested)
    {
        return m_PopulationByLocation.Where(x => x.Location == locationRequested).FirstOrDefault().GetPopulation();
    }

    public void ExportPersonData(List<Person> toExport, string fileName)
    {
        if (toExport.Count == 0)
        {
            Debug.Log("ExportPersonData() Leaving, no data in toExport");
        }

        if (fileName == "") fileName = "Generated_Persons.csv";

        string filePath = Application.dataPath + "/Exports/" + fileName + ".csv";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        //This is the writer, it writes to the filepath
        StreamWriter writer = new StreamWriter(filePath);

        //This is writing the line of the type, name, damage... etc... (I set these)
        writer.WriteLine("Type,Name,Location,Lyrics,Melody,Beat,Voice,Rap, Rock, Ska, Hip Hop, Folk, Happy, Sad, Excited, Nostalgic, Angry, Love, Success, Faith, Fame, Pets, Acoustic Guitar, Electric Guitar, Maracas, Drums, Synthesizer, Piano");
        //This loops through everything in the inventory and sets the file to these.
        for (int i = 0; i < toExport.Count; ++i)
        {

            writer.WriteLine(m_PersonsGenerated[i].GetType().ToString() + ","
                + toExport[i].personName + ","
                + toExport[i].GetStatByType(StatType.LOCATION).statSubType.ToString() + ","

                + toExport[i].GetStatBySubType(StatSubType.SPEED).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.RELIABILITY).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.PERSISTENCE).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.AMBITION).floatVal + ","

                + toExport[i].GetStatBySubType(StatSubType.RAP).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.ROCK).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.POP).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.HIP_HOP).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.RANDB).floatVal + ","

                + toExport[i].GetStatBySubType(StatSubType.MOOD2).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.MOOD3).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.MOOD5).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.MOOD4).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.MOOD1).floatVal + ","

                + toExport[i].GetStatBySubType(StatSubType.TOPIC1).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.TOPIC2).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.TOPIC4).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.TOPIC3).floatVal + ","
                + toExport[i].GetStatBySubType(StatSubType.TOPIC6).floatVal + ",");
        }
        writer.Flush();
        //This closes the file
        writer.Close();
        Debug.Log("File Generated Successfully: " + filePath);
    }


    public void ExportListenData(List<ListenData> toExport, string fileName)
    {
        if (toExport.Count == 0)
        {
            Debug.Log("ExportPersonData() Leaving, no data in toExport");
        }

        if (fileName == "") fileName = "Generated_ListenData.csv";

        string filePath = Application.dataPath + "/Exports/" + fileName + ".csv";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        //This is the writer, it writes to the filepath
        StreamWriter writer = new StreamWriter(filePath);
        //This is writing the line of the type, name, damage... etc... (I set these)
        writer.WriteLine("Turn,SongName,Location,Listens, Followers,Appeal,Word Of Mouth,Freshness");
        //This loops through everything in the inventory and sets the file to these.
        for (int i = 0; i < toExport.Count; ++i)
        {

            writer.WriteLine(toExport[i].turn + ","
            + toExport[i].song.Name + ","
            + toExport[i].location.ToString() + ","
            + toExport[i].listens + ","
            //+ toExport[i].song.m_Followers.Where(x => x.location == toExport[i].location).FirstOrDefault().quantityFollowers + ","
            + toExport[i].appeal + ","
            + toExport[i].wordOfMouth + ","
            + toExport[i].appeal);
        }
        writer.Flush();
        //This closes the file
        writer.Close();
        Debug.Log("File Generated Successfully: " + filePath);
    }
    
    public void CloseDataTablePanel()
    {
        personPanel.ToggleMe(false);
    }

    public float GetTotalFollowerCount()
    {
        return this.m_PopulationByLocation.Sum(x => x.GetPendingFollowers());
    }

    public List<PopulationData> GetPopulationData()
    {
        return this.m_PopulationByLocation;
    }

    public void CommitFollowers()
    {
        foreach (PopulationData data in this.m_PopulationByLocation)
        {
            data.CommitPendingFollowers();
        }
    }

    public int CalculateSongSalesAtLocation(Song song, PopulationData data)
    {
        float curveValue = this.BellCurveCalculation(
                GameRefs.I.m_gameController.currentTurn - song.TurnReleased,
                song.StarRating * (this.DataSimulationVariables.DecayRate + GetBonusSalesWeeks(song.Artist.GetGenre(), true)),
                song.StarRating * (this.DataSimulationVariables.DecayRate + GetBonusSalesWeeks(song.Artist.GetGenre(), true))
            );

		if(song.BellCurveOffset == 0f)
		{
			song.BellCurveOffset = 1 / this.BellCurveCalculation(0, song.Quality/3f, song.Quality/3f);
		}

        float appeal = this.GetSongAppealInLocation(song, data.Location);

		float followersAtSongLocation = data.GetFollowers();

		float sales = appeal * followersAtSongLocation * curveValue * song.BellCurveOffset;

        if (song.TargetedLocation == data.Location)
        {
            sales *= this.DataSimulationVariables.TargetedLocationMultiplier;
        }

        return Mathf.RoundToInt(sales);
    }

    public float BellCurveCalculation(float x, float mean, float standardDeviation)
    {
        return Mathf.Pow((float)Math.E, -Mathf.Pow(x - mean, 2) / (2 * Mathf.Pow(standardDeviation, 2))) / (standardDeviation * Mathf.Sqrt(2 * Mathf.PI));
    }

    public float GetFollowerSaturation(StatSubType location)
    {
        return this.m_PopulationByLocation.Where(x => x.Location == location).FirstOrDefault().GetFollowers() / this.GetLocationPopulation(location);
    }

	public float GetUpgradeBonusPercent(StatSubType location)
	{
		int numUpgrades = GameRefs.I.m_marketingView.GetNumUnlocks(location, MarketingView.BoroughUnlockType.BonusInsight);
		if (numUpgrades > 0)
			return GameRefs.I.m_upgradeVariables.BonusInsightPercentFromUpgrades[numUpgrades - 1];
		else
			return 0;
	}

	public int GetBonusSurgeWeeks(StatSubType location)
	{
		int numUpgrades = GameRefs.I.m_marketingView.GetNumUnlocks(location, MarketingView.BoroughUnlockType.BonusSurgeLength);
		if (numUpgrades > 0)
			return GameRefs.I.m_upgradeVariables.BonusSurgeLengthWeeks[numUpgrades - 1];
		else
			return 0;
	}

	public int GetBonusChanceToMakeHit(StatSubType genre)
	{
		int numUpgrades = GameRefs.I.m_marketingView.GetNumUnlocks(genre, MarketingView.GenreUnlockType.CreatingHit);
		if (numUpgrades > 0)
			return GameRefs.I.m_upgradeVariables.BonusChanceToMakeHit[numUpgrades - 1];
		else
			return 0;
	}

	// This should be for text only
	public float GetBonusSalesWeeks(StatSubType genre, bool actualMathNumbers)
	{
		int numUpgrades = GameRefs.I.m_marketingView.GetNumUnlocks(genre, MarketingView.GenreUnlockType.ExtraSales);
		if (numUpgrades > 0)
		{
			if(actualMathNumbers)
				return GameRefs.I.m_upgradeVariables.BonusWeeksDecayRates[numUpgrades - 1];
			else
				return GameRefs.I.m_upgradeVariables.BonusWeeksForSongSales[numUpgrades - 1];
		}
		else
			return 0;
	}

	public float GetBonusArtistPercent(StatSubType genre)
	{
		int numUpgrades = GameRefs.I.m_marketingView.GetNumUnlocks(genre, MarketingView.GenreUnlockType.AttractArtists);
		if (numUpgrades > 0)
			return GameRefs.I.m_upgradeVariables.BonusGetGenreArtistPercent[numUpgrades - 1];
		else
			return 0;
	}
}
