using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Song
{
    public class MarketData
    {
        public int Sales;
        public int Listens;
        public float Appeal;
    }

	[System.Serializable]
	public class LoggedData
	{
		public string name;
		public string artistName;
		public string targetedLocation;
		public float starRating;
		public bool ReadyToRelease;
		public bool DoneRecording;
		public int TurnsRecorded;
		public int TurnReleased;

		public int Quality;
		public int totalRecordingTurns;
		public int genreID;
		public int topicID;
		public int moodID;
	}

    public string Name;
    public float StarRating = 0;
    [Header("Randomized Artist name for now in the prototype")]
    public Band Artist;
    public Band.Instrument Instrument;
	public int NumRecordingTurns;
    public int TurnOfCreation;
    public int TurnsRecorded;
	public int TurnReleased;

	public bool IsKnownDoneRecording;
	public bool IsKnownStartedRecording;
    public bool ReadyToRelease;
    public bool DoneRecording;
	public bool WasInstantHit = false;

    [Header("How many followers at song release time?")]
    public int Quality = 0;
    public int RollsMade = 0;
    public List<StatSubType> Stats = new List<StatSubType>();
    public StatSubType TargetedLocation;
	public string TargetedLocationName;
    public List<Dictionary<StatSubType, MarketData>> TurnData = new List<Dictionary<StatSubType, MarketData>>();

    public float BellCurveOffset;

    public void RollForStats(SongReleaseVariables releaseVariables)
    {
        this.RollsMade += 1;

        float valueNeeded = releaseVariables.SuccessRateFloor + (1 - releaseVariables.SuccessRateFloor) * this.Quality / (float)GameRefs.I.m_gameInitVars.MaxSongQuality;

        List<float> rolls = new List<float>();

        for (int i = 0; i < 6 - this.Artist.AmbitionScore; i++)
        {
            rolls.Add(Random.value / (6 - this.Artist.AmbitionScore));
        }

        for (int i = 0; i < this.Artist.ReliabilityScore; i++)
        {
            float reroll = Random.value / (6 - this.Artist.AmbitionScore);

            rolls = rolls.OrderBy(x => x).ToList();

            if (rolls[0] < reroll)
            {
                rolls[0] = reroll;
            }
        }

		// Bonus upgrade: Chance to automatically fill meter.
		if (RollsMade <= 1 && (Random.Range(0, 100) < GameRefs.I.m_dataSimulationManager.GetBonusChanceToMakeHit(Artist.GetGenre())))
		{
			this.Quality = GameRefs.I.m_gameInitVars.MaxSongQuality;
			WasInstantHit = true;
		}
		else
		{
			if (rolls.Sum() >= valueNeeded)
			{
				this.Quality += 1;

				if (this.Quality > GameRefs.I.m_gameInitVars.MaxSongQuality)
				{
					this.Quality = GameRefs.I.m_gameInitVars.MaxSongQuality;
				}
			}
		}

    }

    public void CalculateStarRating()
    {
        this.StarRating = (this.Quality) / (GameRefs.I.m_gameInitVars.MaxSongQuality / 5);

        if (this.StarRating == 0)
        {
            this.StarRating = 0.5f;
        }
    }

    public float GetTurnAppeal(int turnIndex, StatSubType possibleLocation)
    {
        if (possibleLocation.SuperType == StatType.LOCATION)
        {
            return this.TurnData[turnIndex][possibleLocation].Appeal;
        }
        else
        {
            return (float)this.TurnData[turnIndex].Average(x => x.Value.Appeal);
        }
    }

    public float GetTurnSales(int turnIndex, StatSubType possibleLocation)
    {
        if (possibleLocation.SuperType == StatType.LOCATION)
        {
            return this.TurnData[turnIndex][possibleLocation].Sales;
        }
        else
        {
            return (float)this.TurnData[turnIndex].Average(x => x.Value.Sales);
        }
    }

    public float GetTurnListens(int turnIndex, StatSubType possibleLocation)
    {
        if (possibleLocation.SuperType == StatType.LOCATION)
        {
            return this.TurnData[turnIndex][possibleLocation].Listens;
        }
        else
        {
            return (float)this.TurnData[turnIndex].Average(x => x.Value.Listens);
        }
    }

    public float GetCumulativeTurnSales(int turnIndex, StatSubType possibleLocation)
    {
        float sum = 0;

        for (int i = 0; i <= turnIndex; i++)
        {
            if (possibleLocation.SuperType == StatType.LOCATION)
            {
                sum += this.TurnData[i][possibleLocation].Sales;
            }
            else
            {
                sum += (float)this.TurnData[i].Average(x => x.Value.Sales);
            }
        }

        return sum;
    }

	public LoggedData GetDataForLogs()
	{
		LoggedData data = new LoggedData();
		data.name = Name;
		data.artistName = Artist.Name;
		data.targetedLocation = TargetedLocation.Name;
		data.ReadyToRelease = ReadyToRelease;
		data.TurnsRecorded = TurnsRecorded;
		data.DoneRecording = DoneRecording;
		data.TurnReleased = TurnReleased;

		for(int i = 0; i < Stats.Count; i++)
		{
			if (Stats[i].SuperType == StatType.GENRE)
				data.genreID = Stats[i].ID;
			if (Stats[i].SuperType == StatType.MOOD)
				data.moodID = Stats[i].ID;
			if (Stats[i].SuperType == StatType.TOPIC)
				data.topicID = Stats[i].ID;
		}
		data.totalRecordingTurns = NumRecordingTurns;
		data.Quality = Quality;

		return data;
	}

    public int GetMaximumRecordingTurns()
    {
		return NumRecordingTurns;
    }

	public void SetMaxRecordingTurns()
	{
		NumRecordingTurns = this.Artist.PersistenceScore + Random.Range(GameRefs.I.m_songReleaseVariables.PersistenceLowMaxOffset, GameRefs.I.m_songReleaseVariables.PersistenceHighMaxOffset + 1); // Random function high end is exclusive
	}

	public StatSubType mood {
		get
		{
			foreach (StatSubType stat in Stats)
			{
				if (stat.SuperType == StatType.MOOD)
					return stat;
			}
			return null;
		}
	}

	public StatSubType topic {
		get
		{
			foreach (StatSubType stat in Stats)
			{
				if (stat.SuperType == StatType.TOPIC)
					return stat;
			}
			return null;
		}
	}
}
