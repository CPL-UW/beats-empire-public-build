using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Utility;

public class SongRecordingPanel : ArtistViewPanel
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

	public RecordingParameters parameters;
	public GameObject recordingFullBox;
	public Colors colors;
    public SongReleaseVariables SongReleaseVariables;
    public MarketingInsights MarketingInsights;
    public TextMeshProUGUI SongNameLabel, SongRecordButtonLabel, SongReleaseButtonLabel, SongIsRecordingLabel;
	public Animator animator;
	public Release releaseView;

    public Color UnfilledDotColor;
    public Color InactiveHitBarColor;

    public List<Image> RecordingDots;
    public List<Image> RecordingBars;
    public Button RecordSongButton, ReleaseSongButton, ReleaseFinishedSongButton, RandomSongNameButton;

	public ToggleGroup MoodToggleGroup;
	public ToggleGroup TopicToggleGroup;
	public ToggleGroup LocationToggleGroup;
    public List<Toggle> MoodToggles, TopicToggles, LocationTargetToggles;
    public List<TextMeshProUGUI> moodToggleLabels;
    public List<TextMeshProUGUI> topicToggleLabels;
	public GameObject moodPredictionIcon;
	public GameObject topicPredictionIcon;
	public GameObject genrePredictionIcon;
	public GameObject moodMostPopularIcon;
	public GameObject moodTrendingUpIcon;
	public GameObject topicMostPopularIcon;
	public GameObject topicTrendingUpIcon;
	public GameObject genreMostPopularIcon;
	public GameObject genreTrendingUpIcon;
	public TextMeshProUGUI moodLabel;
	public TextMeshProUGUI topicLabel;
	public Image moodIcon;
	public Image topicIcon;

	public Image weekCounterBackground;
    public Transform SongProgressRoot;
    public Transform SongTitleRoot;
    public Transform WhatsHotRoot;
    public TextMeshProUGUI PlanYourSongText;
    public TextMeshProUGUI WhatsHotText;

    public Heatmap Heatmap;
	public HeatmapTooltipControl HeatmapTooltip;

    private List<Color> hitBarColors;
    private List<float> burroughSaturation;

    private string defaultNameText = "Click to generate a song title";
	private bool isProgrammaticLocationToggle = false;

    public void Init(GameController controller)
    {
        hitBarColors = new List<Color>();

        for (int i = 0; i < RecordingBars.Count; i++)
        {
            hitBarColors.Add(RecordingBars[i].color);
        }
    }
	
    public void OnViewOpened(bool resetOptions)
    {
        foreach (Toggle toggle in LocationTargetToggles)
        {
            toggle.interactable = true;
            toggle.isOn = false;
        }

		if (resetOptions)
		{
			foreach (Band b in GameRefs.I.m_gameController.GetSignedBands())
			{
				MarketingInsightObject insight = GetInsightFromBand(currentBand);

				if (insight == null)
				{
					b.preReleaseLocation = StatSubType.NONE;
					b.preReleaseMood = StatSubType.NONE;
					b.preReleaseTopic = StatSubType.NONE;
					b.preReleaseName = null;
				}
			}
		}

        RecordSongButton.interactable = false;
        SongRecordButtonLabel.text = "CHOOSE A BOROUGH";
		GameRefs.I.m_gameController.TopChartsHud.UpdateCircles();
        burroughSaturation = GameRefs.I.m_gameController.GetBurroughSaturation();
    }

    public override void AssignBand(Band band)
    {
        base.AssignBand(band);

        RefreshStats();

        bool bandIsRecording = currentBand.IsRecordingSong();

        SongProgressRoot.gameObject.SetActive(bandIsRecording);
        RandomSongNameButton.interactable = !bandIsRecording;

        if (bandIsRecording)
        {
            Song recordingSong = band.GetRecordingSong();
			int maximumRecordingTurns = recordingSong.GetMaximumRecordingTurns();
			SongNameLabel.SetIText(recordingSong.Name);
			GameRefs.I.m_recordingAudio.RequestSong(recordingSong);
			releaseView.SetBPM(GameRefs.I.currentBPM);

			int minimumRecordingTurns = SongReleaseVariables.MinimumTurns;

            ReleaseSongButton.gameObject.SetActive(!recordingSong.DoneRecording);
            ReleaseFinishedSongButton.gameObject.SetActive(recordingSong.DoneRecording);

            for (int i = 0; i < RecordingBars.Count; i++)
            {
                RecordingBars[i].color = (recordingSong.Quality >= i) ? hitBarColors[i] : InactiveHitBarColor;
				if (recordingSong.Quality >= RecordingBars.Count - 1)
				{
					ReleaseSongButton.gameObject.SetActive(!recordingSong.DoneRecording);
					ReleaseFinishedSongButton.gameObject.SetActive(recordingSong.DoneRecording);
				}
            }

			for (int i = 0; i < RecordingDots.Count; i++)
			{
				Color colorToUse;
				if (recordingSong.DoneRecording)
				{
					colorToUse = colors.dotFinishedColor;
				}
				else if (i < recordingSong.TurnsRecorded)
				{
					colorToUse = colors.dotUnfinishedColor;
				}
				else
				{
					colorToUse = colors.dotUnfilledColor;
				}

				RecordingDots[i].color = colorToUse;
				RecordingDots[i].gameObject.SetActive(i < maximumRecordingTurns);
			}
			weekCounterBackground.color = recordingSong.DoneRecording ? colors.dotFinishedColor : colors.dotUnfinishedColor;

			ReleaseSongButton.interactable = (recordingSong.TurnsRecorded >= minimumRecordingTurns);

            SongIsRecordingLabel.gameObject.SetActive(recordingSong.TurnsRecorded < maximumRecordingTurns);

            if (recordingSong.TurnsRecorded < minimumRecordingTurns)
            {
                SongReleaseButtonLabel.SetIText("SONG IS RECORDING");
            }
            else
            {
                SongReleaseButtonLabel.SetIText("RELEASE SONG EARLY");
            }

			Heatmap.UpdateSaturation(releaseView.targetedBoroughId); // new List<float> { 0, 0, 0, 0, 0, 0 });

			for (int i = 0; i < LocationTargetToggles.Count; i++)
            {
                LocationTargetToggles[i].interactable = false;
                LocationTargetToggles[i].isOn = burroughLookup[i] == recordingSong.TargetedLocation;
            }

			recordingFullBox.SetActive(false);
			RecordSongButton.gameObject.SetActive(true);
        }
        else
        {
            MarketingInsightObject insight = GetInsightFromBand(currentBand);
			if (insight == null)
            {
				isProgrammaticLocationToggle = true;
				for (int i = 0; i < LocationTargetToggles.Count; i++)
				{
					if (currentBand.preReleaseLocation == burroughLookup[i])
					{
						LocationTargetToggles[i].isOn = true;
						LocationTargetToggles[i].interactable = true;
						HeatmapTooltip.ShowTooltip(burroughLookup[i]);
					}
					else
					{
						LocationTargetToggles[i].isOn = false;
						LocationTargetToggles[i].interactable = true;
					}
				}
				isProgrammaticLocationToggle = false;
            }
            else
            {

                for (int i = 0; i < LocationTargetToggles.Count; i++)
                {
                    LocationTargetToggles[i].interactable = false;
                    LocationTargetToggles[i].isOn = burroughLookup[i] == insight.location;
                }
				HeatmapTooltip.ShowTooltip(insight.location);
            }

            for (int i = 0; i < RecordingDots.Count; i++)
            {
                RecordingDots[i].color = colors.dotUnfilledColor;
                RecordingDots[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < RecordingBars.Count; i++)
            {
                RecordingBars[i].color = InactiveHitBarColor;
            }

            SongNameLabel.SetIText(defaultNameText);

			CheckRecordButtonInteractable();

            ReleaseSongButton.gameObject.SetActive(false);
            ReleaseFinishedSongButton.gameObject.SetActive(false);

			if (insight == null)
				Heatmap.UpdateSaturation(releaseView.targetedBoroughId); // burroughSaturation);

			if (GameRefs.I.m_gameController.GetRecordingSongs().Count == 3)
			{
				if (!recordingFullBox.activeInHierarchy)
				{
					StartCoroutine(Footilities.CoEaseBackInOut(parameters.recordingFullEaseTime, parameters.recordingFullEaseInitialScale, 1, s => {
						recordingFullBox.transform.localScale = new Vector3(s, s, s);
					}));
				}
				recordingFullBox.SetActive(true);
				RecordSongButton.gameObject.SetActive(false);
			}
		}

		if (currentBand.preReleaseName != null)
		{
			SongNameLabel.SetIText(currentBand.preReleaseName);
		}
    }

    public void OnMoodSelected(int i)
    {
		currentBand.preReleaseMood = MoodToggles[i].isOn ? moodLookup[i] : StatSubType.NONE;
		CheckRecordButtonInteractable();
	}

    public void OnTopicSelected(int i)
    {
		currentBand.preReleaseTopic = TopicToggles[i].isOn ? topicLookup[i] : StatSubType.NONE;
		CheckRecordButtonInteractable();
	}

    public void SetRandomSongTitle()
    {
		string randoName = SongNameGenerator.Instance.GetRandomSongTitle();
		currentBand.preReleaseName = randoName;
		SongNameLabel.SetIText(randoName);
    }

    public void OnLocationTargeted(int value)
    {
		if (currentBand == null)
			return;

		if (!isProgrammaticLocationToggle)
		{
			if (value >= 0)
			{
				currentBand.preReleaseLocation = burroughLookup[value];
			}
			else
			{
				currentBand.preReleaseLocation = StatSubType.NONE;
			}
		}

		CheckRecordButtonInteractable();
		HeatmapTooltip.ShowTooltip(currentBand.preReleaseLocation);
	}

	public void OnLocationHighlighted(int value)
	{
		if (value >= 0)
		{
			bool isUnlocked = GameRefs.I.m_marketingView.IsBoroughUnlocked(burroughLookup[value]);
			HeatmapTooltip.ShowTooltip(isUnlocked ? burroughLookup[value] : StatSubType.NONE);
		}
		else
			HeatmapTooltip.ShowTooltip(StatSubType.NONE);
	}

	public float lastClickedTime = 0f;
	public void RecordSong()
    {
		// Not a hack but quick solution to not allow multiple button clicks
		if(Time.time - lastClickedTime < 1f)
		{
			return;
		}
		else
		{
			lastClickedTime = Time.time;
		}
        //store dropdowns into stat list
        List<StatSubType> songStats = new List<StatSubType>();

		// Randomly pick any optional and unselected traits.
		StatSubType targetedBorough = releaseView.targetedBorough;
		if (targetedBorough != StatSubType.NONE)
		{
			List<StatSubType> traits = currentBand.GetKnownTraits();
			bool[] interests = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(targetedBorough);

			if (!interests[1] && currentBand.preReleaseMood == StatSubType.NONE)
			{
				currentBand.preReleaseMood = traits.Where(trait => trait.SuperType == StatType.MOOD).ToList().RandomElement();
			}

			if (!interests[2] && currentBand.preReleaseTopic == StatSubType.NONE)
			{
				currentBand.preReleaseTopic = traits.Where(trait => trait.SuperType == StatType.TOPIC).ToList().RandomElement();
			}
		}

		if (currentBand.preReleaseLocation == StatSubType.NONE ||
			currentBand.preReleaseMood == StatSubType.NONE ||
			currentBand.preReleaseTopic == StatSubType.NONE)
		{
			Debug.LogError("Band does not have a location, mood, and topic. Something went horribly wrong!");
			MarketingInsightObject insight = GetInsightFromBand(currentBand);

			if (currentBand.preReleaseLocation == StatSubType.NONE)
			{
				Debug.Log("Missing loc");
			}
			else if (currentBand.preReleaseMood == StatSubType.NONE)
			{
				Debug.Log("Missing mood");
				if(insight != null && insight.statType.SuperType == StatType.MOOD)
				{
					Debug.Log("Fixed missing mood from insight");
					currentBand.preReleaseMood = insight.statType;
				}
			}
			else if (currentBand.preReleaseTopic == StatSubType.NONE)
			{
				Debug.Log("Missing topic");
				if (insight != null && insight.statType.SuperType == StatType.TOPIC)
				{
					Debug.Log("Fixed missing topic from insight");
					currentBand.preReleaseTopic = insight.statType;
				}
			}
				

		}

        songStats.Add(currentBand.preReleaseMood);
        songStats.Add(currentBand.preReleaseTopic);
        songStats.Add(currentBand.GetGenre());

        if (SongNameLabel.text == defaultNameText)
        {
            SetRandomSongTitle();
        }

		Song nextSong = new Song()
		{
			Artist = currentBand,
			Name = SongNameLabel.text,
			Stats = songStats,
			TargetedLocation = currentBand.preReleaseLocation,
			TargetedLocationName = currentBand.preReleaseLocation.Name,
        };
		nextSong.SetMaxRecordingTurns();

		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ReturnToRecording);
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.StartRecording, TutorialController.TutorialAction.Confirm);

		currentBand.AssignRecordingSong(nextSong);
        Controller.OnSongCreated(currentBand.GetRecordingSong());
		Controller.MarketingInsights.ClearSelectedEntry();

		GameRefs.I.PostGameState(true, "clickedButton", "RecordButton");
		currentBand.preReleaseLocation = StatSubType.NONE;
		currentBand.preReleaseMood = StatSubType.NONE;
		currentBand.preReleaseTopic = StatSubType.NONE;
		currentBand.preReleaseName = null;
    }

    private void RefreshStats()
    {
        Song songToMimic = currentBand.GetRecordingSong();
        MarketingInsightObject insight = GetInsightFromBand(currentBand);

        if (songToMimic != null && insight == null)
            WhatsHotRoot.gameObject.SetActive(false);
        else
            WhatsHotRoot.gameObject.SetActive(true);

        List<StatSubType> bandKnownTraits = currentBand.GetKnownTraits();
		if (insight == null)
		{
			moodPredictionIcon.SetActive(false);
			topicPredictionIcon.SetActive(false);
			genrePredictionIcon.SetActive(false);
			moodLabel.color = Color.black;
			topicLabel.color = Color.black;
			moodIcon.color = Color.black;
			topicIcon.color = Color.black;
		}
		else
		{
			StatType stat = insight.statType.SuperType;
			moodPredictionIcon.SetActive(stat == StatType.MOOD);
			topicPredictionIcon.SetActive(stat == StatType.TOPIC);
			genrePredictionIcon.SetActive(stat == StatType.GENRE);

			if (stat == StatType.MOOD)
			{
				moodMostPopularIcon.SetActive(insight.insightType == MarketingInsights.InsightType.MostPopular);	
				moodTrendingUpIcon.SetActive(insight.insightType == MarketingInsights.InsightType.IsTrending);	
			}

			if (stat == StatType.TOPIC)
			{
				topicMostPopularIcon.SetActive(insight.insightType == MarketingInsights.InsightType.MostPopular);	
				topicTrendingUpIcon.SetActive(insight.insightType == MarketingInsights.InsightType.IsTrending);	
			}

			if (stat == StatType.GENRE)
			{
				genreMostPopularIcon.SetActive(insight.insightType == MarketingInsights.InsightType.MostPopular);	
				genreTrendingUpIcon.SetActive(insight.insightType == MarketingInsights.InsightType.IsTrending);	
			}

			moodLabel.color = stat == StatType.MOOD ? Color.white : Color.black;
			topicLabel.color = stat == StatType.TOPIC ? Color.white : Color.black;
			moodIcon.color = stat == StatType.MOOD ? Color.white : Color.black;
			topicIcon.color = stat == StatType.TOPIC ? Color.white : Color.black;
		}

        foreach (KeyValuePair<int, StatSubType> kvp in moodLookup)
        {
            MoodToggles[kvp.Key].gameObject.SetActive(true);
            MoodToggles[kvp.Key].GetComponent<Image>().color = Color.white;
            if (songToMimic == null)
            {
                bool bandKnowsMood = bandKnownTraits.Contains(kvp.Value);

                if (insight != null && insight.statType.SuperType == StatType.MOOD)
                {
					MoodToggles[kvp.Key].interactable = false;
                    if (kvp.Value == insight.statType)
                    {
                        MoodToggles[kvp.Key].isOn = true;
						moodToggleLabels[kvp.Key].color = colors.recordingTraitLabelPredicted;
                    }
                    else
                    {
                        if (bandKnowsMood)
						{
                            MoodToggles[kvp.Key].GetComponent<Image>().color = Color.gray;
							moodToggleLabels[kvp.Key].color = colors.recordingTraitLabelUnpredicted;
						}
						else
						{
							moodToggleLabels[kvp.Key].color = colors.recordingTraitLabelUnknown;
						}
                    }
                }
                else
                {
                    MoodToggles[kvp.Key].interactable = bandKnowsMood;
					MoodToggles[kvp.Key].isOn = currentBand.preReleaseMood == kvp.Value ? true : false;
					moodToggleLabels[kvp.Key].color = colors.recordingTraitLabelSelectable;
                }
            }
            else
            {
                MoodToggles[kvp.Key].gameObject.SetActive(songToMimic.Stats.Contains(kvp.Value));
                MoodToggles[kvp.Key].interactable = songToMimic.Stats.Contains(kvp.Value);
                MoodToggles[kvp.Key].isOn = songToMimic.Stats.Contains(kvp.Value);
				moodToggleLabels[kvp.Key].color = colors.recordingTraitLabelPredicted;
            }
        }

        foreach (KeyValuePair<int, StatSubType> kvp in topicLookup)
        {
            TopicToggles[kvp.Key].GetComponent<Image>().color = Color.white;
            TopicToggles[kvp.Key].gameObject.SetActive(true);
            if (songToMimic == null)
            {
                bool bandKnowsTopic = bandKnownTraits.Contains(kvp.Value);

                if (insight != null && insight.statType.SuperType == StatType.TOPIC)
                {
					TopicToggles[kvp.Key].interactable = false;
                    if (kvp.Value == insight.statType)
                    {
                        TopicToggles[kvp.Key].isOn = true;
						topicToggleLabels[kvp.Key].color = colors.recordingTraitLabelPredicted;
                    }
                    else
                    {
                        if (bandKnowsTopic)
						{
                            TopicToggles[kvp.Key].GetComponent<Image>().color = Color.gray;
							topicToggleLabels[kvp.Key].color = colors.recordingTraitLabelUnpredicted;
						}
						else
						{
							topicToggleLabels[kvp.Key].color = colors.recordingTraitLabelUnknown;
						}
                    }
                }
                else
                {
                    TopicToggles[kvp.Key].interactable = bandKnowsTopic;
					TopicToggles[kvp.Key].isOn = currentBand.preReleaseTopic == kvp.Value ? true : false;
					topicToggleLabels[kvp.Key].color = colors.recordingTraitLabelSelectable;
				}

            }
            else
            {
                TopicToggles[kvp.Key].gameObject.SetActive(songToMimic.Stats.Contains(kvp.Value));
                TopicToggles[kvp.Key].interactable = songToMimic.Stats.Contains(kvp.Value);
                TopicToggles[kvp.Key].isOn = songToMimic.Stats.Contains(kvp.Value);
            }
        }

        if (songToMimic == null)
        {
            SongTitleRoot.gameObject.SetActive(true);
            PlanYourSongText.text = "PLAN YOUR SONG";
        }
        else
        {
            SongTitleRoot.gameObject.SetActive(false);
            PlanYourSongText.text = string.Format("\"{0}\"", currentBand.GetRecordingSong().Name);
        }

        if (insight != null)
            WhatsHotText.text = "I PREDICT...";
        else
            WhatsHotText.text = "WHAT'S HOT?";
    }

	private void CheckRecordButtonInteractable()
	{
		if (Controller.CanRecordNewSong())
		{
			bool isInteractable = true;
			bool isTopicMandatory = true;
			bool isMoodMandatory = true;

			StatSubType targetedBorough = releaseView.targetedBorough;
			if (targetedBorough != StatSubType.NONE)
			{
				bool[] interests = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(targetedBorough);
				isMoodMandatory = interests[1];
				isTopicMandatory = interests[2];
			}

			string reasonText = "CHOOSE A BOROUGH";

			if (!LocationTargetToggles.Any(x => x.isOn))
			{
				reasonText = "CHOOSE A BOROUGH";
				isInteractable = false;
			}
			else if (!MoodToggles.Any(x => x.isOn) && isMoodMandatory)
			{
				reasonText = "CHOOSE A MOOD";
				isInteractable = false;
			}
			else if (!TopicToggles.Any(x => x.isOn) && isTopicMandatory)
			{
				reasonText = "CHOOSE A TOPIC";
				isInteractable = false;
			}

			RecordSongButton.interactable = isInteractable;
			SongRecordButtonLabel.text = isInteractable ? "START RECORDING" : reasonText;
		}
		else
		{
			RecordSongButton.interactable = false;
			SongRecordButtonLabel.text = "CHOOSE A BOROUGH";
		}
	}

    private MarketingInsightObject GetInsightFromBand(Band band)
    {
        MarketingInsightObject insight = null;

        for (int i = 0; i < MarketingInsights.unconfirmedInsights.Count; i++)
        {
            if (MarketingInsights.unconfirmedInsights[i].bandAttached == band)
            {
                insight = MarketingInsights.unconfirmedInsights[i];
                return insight;
            }
        }

        for (int i = 0; i < GameRefs.I.m_gameController.marketingInsightList.Count; i++)
        {
            if (GameRefs.I.m_gameController.marketingInsightList[i].bandAttached == band)
            {
                insight = GameRefs.I.m_gameController.marketingInsightList[i];
                return insight;
            }
        }

        return insight;
    }
}
