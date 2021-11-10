using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utility;
using System.Linq;

[System.Serializable]
public class MarketingInsightObject
{
	[System.Serializable]
	public class LoggedData
	{
		public string songName;
		public string bandName;
		public string type;
		public string superType;
		public string statType;
		public string location;
		public bool successful;
		public int turnCreated;
	}

	public Song songAttached;
	public Band bandAttached;
    public MarketingInsights.InsightType insightType;
    public StatSubType statType;
    public StatSubType location;
	public int turnCreated;
	public bool successful;

    public MarketingInsightObject(MarketingInsights.InsightType insight, StatSubType stat, Band band, StatSubType loc)
    {
        statType = stat;
        insightType = insight;
		bandAttached = band;
		songAttached = band.GetRecordingSong();
        location = loc;
		successful = false;
		turnCreated = GameRefs.I.m_gameController.currentTurn;
    }

	public LoggedData GetDataForLogs()
	{
		LoggedData data = new LoggedData();
		data.bandName = bandAttached.Name;
		if (songAttached != null)
			data.songName = songAttached.Name;
		else
			data.songName = "";
		data.type = insightType == MarketingInsights.InsightType.IsTrending ? "trending" : "mostPopular";
		data.statType = Utilities.InterceptText(statType.Name);
		data.superType = statType.SuperType.Name;
		data.successful = this.successful;
		data.location = location.Name;
		data.turnCreated = turnCreated;
		return data;
	}
}

public class MarketingInsights : MonoBehaviour {

	public enum InsightType
	{
		MostPopular,
		IsTrending
	}

	public Button manageDataButton;
	public DataCollectionController dataCollectionController;
	public GraphManager graphs;
	public ArtistSelector currentArtistPanel;
	public ArtistSelector ArtistSelectorPrefab;
	public Transform ArtistSelectorRoot;

	public ToggleGroup toggleGroup;
	public MarketingInsightSelector marketingPopup;
	public LocationButtonHelper locationSelect;
	public Animator animGuy;
	public SongRecordingPanel recordingPanel;
	public RectTransform[] popupAnchors;
	public GameObject goInsights;
	public GameObject goTrends;
	public GameObject goAllLocationsText;
	public GameObject goBusyText;
	public GameObject goDoesntCareAlert;

	public RectTransform artistMenuArrow;
	public TextMeshProUGUI artistMenuText;
	public RectTransform artistMenu;
	public Release releaseView;
	public TextMeshProUGUI findLineText;

	public float yPosMenuVisible = 312.2f;
	public float yPosMenuHidden = 900f;
	public Image[] statBackgrounds;
	public TextMeshProUGUI[] statTexts;
	public TextMeshProUGUI artistNameText;
	public Image artistGenreImage;
	public GameObject[] memberModels;
	public Vector3 popupOffset;
	public Vector3 belowOffset;
	public float leftRightOffset;

	public TextMeshProUGUI cancelPredictionText;
	public RectTransform predictionButton;
	public CanvasGroup predictionButtonCG;
	public GameObject choices;
	public GameObject choicesResponsive;
	public GameObject cancelInsight;
	public GameObject chooseInsight;

	public float yValueCutoff;
	public float xCutoffRight;
	public float xCutoffLeft;
	public Image BonusPopGenre;
	public Image BonusTrendGenre;
	public Image BonusPopMood;
	public Image BonusTrendMood;
	public Image BonusPopTopic;
	public Image BonusTrendTopic;
	public Image BonusGraphIcon;
	public Sprite TrendSprite;
	public Sprite PopSprite;

	//public MarketingInsightObject lastSelectedInsight;
	public List<MarketingInsightObject> unconfirmedInsights;

	private List<Band> currentbands;
	private Routine m_showArtistRoutine;
	private Routine m_shakeAwayRoutine;
	private bool bMenuIsOpen = false;
	private bool bBlockInsightSelection;
	private int selectedEntry = -1;
	private StatType selectedType = StatType.NONE;
	private bool tooHigh = false;
	private MarketingInsightObject insightBeforeEdit;
	public bool graphAnimating = false;

	void Awake()
	{
		unconfirmedInsights = new List<MarketingInsightObject>();
		currentbands = new List<Band>();
		manageDataButton.onClick.AddListener(() => {
			dataCollectionController.Show(graphs.backListener);
		});
	}

	void Update()
	{
		if (goInsights.activeSelf &&                                 // Only active when insights active
			graphs.GetCurrentLocation() != StatSubType.NONE &&      // If they have all locations selected
			Input.GetButtonDown("Fire1") &&                         // On click
			!GameRefs.I.m_globalLastBand.IsRecordingSong() &&           // Not currently recording       
			!goDoesntCareAlert.activeSelf)
		{
			// Check if we're clicking on an artist or the weeks dropdown (probably a better way to do this)
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = Input.mousePosition;

			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointer, raycastResults);
			// Skip this function if we're not in the graph view, or pressing a button that overlaps with the view
			if (raycastResults.Count > 0)
			{
				bool inGraph = false;
				foreach (var go in raycastResults)
				{
					if (go.gameObject.name.Contains("GraphBG"))
					{
						inGraph = true;
					}
					else if (go.gameObject.name.Contains("BarBack") ||
						go.gameObject.name.Contains("LineBack") ||
						go.gameObject.name.Contains("HeatBack") ||
						go.gameObject.name.Contains("Dropdown") ||
						go.gameObject.name.Contains("Viewport") ||
						go.gameObject.name.Contains("InsightBG") ||
						go.gameObject.name.Contains("ColorAdd") ||
						go.gameObject.name.Contains("Trending") ||
						go.gameObject.name.Contains("ColorAdd") ||
						go.gameObject.name.Contains("MostPop"))
					{
						return;
					}
				}

				if (!inGraph)
					return;
			}

			if (choices.gameObject.activeSelf || choicesResponsive.gameObject.activeSelf)
				return;

			// Find the closest line, highlight it, and display the marketing insights popup
			Vector3 clickPoint;
			int thisSelectedEntry = graphs.GetClosestTrait(out clickPoint, true);

			if (thisSelectedEntry >= 0)
			{
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ToggleBetweenOptions);
				GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
				if (thisSelectedEntry == selectedEntry && graphs.GetPrimaryStatFilter() == selectedType)
				{
					cancelPredictionText.text = BandHasInsight(GameRefs.I.m_globalLastBand) ? "DELETE PREDICTION" : "CANCEL PREDICTION";
					if (BandHasInsight(GameRefs.I.m_globalLastBand))
					{
						OpenCancelSelector(true);
					}
					else
					{
						OpenInsightSelector(clickPoint);
					}
				}
				else
				{
					selectedEntry = thisSelectedEntry;
					selectedType = graphs.GetPrimaryStatFilter();
					graphs.SetEntrySelected(selectedEntry);
					/*
					for(int i = 0; i < unconfirmedInsights.Count; i++)
					{
						if(unconfirmedInsights[i].bandAttached == GameRefs.I.m_globalLastBand)
						{
							if (unconfirmedInsights[i].statType.SuperType == graphs.GetPrimaryStatFilter())
								RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);
						}
					}
					*/

					//Vector3 predictionIconLocation = graphs.GetPredictionLocation();
					if (graphs.GetPrimaryStatFilter() == StatType.MOOD)
					{
						if (BonusTrendMood.gameObject.activeSelf || BonusPopMood.gameObject.activeSelf)
						{
							RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);
							ClearAllInsightIcons();
							ClearPredictionGraphIcon(false);
						}

						string logAction = "clickedBarOrLine";
						GameRefs.I.PostGameState(false, logAction, string.Format("{0}", StatSubType.List[StatSubType.MOOD1.ID + selectedEntry].Name));

						StatSubType selectedMood = StatSubType.List[StatSubType.MOOD1.ID + selectedEntry];
						GameRefs.I.m_globalLastBand.preReleaseMood = selectedMood;
						statTexts[0].text = Utilities.InterceptText(selectedMood.Name);

					}
					else if (graphs.GetPrimaryStatFilter() == StatType.TOPIC)
					{
						if (BonusTrendTopic.gameObject.activeSelf || BonusPopTopic.gameObject.activeSelf)
						{
							RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);
							ClearAllInsightIcons();
							ClearPredictionGraphIcon(false);
						}

						string logAction = "clickedBarOrLine";
						GameRefs.I.PostGameState(false, logAction, string.Format("{0}", StatSubType.List[StatSubType.TOPIC1.ID + selectedEntry].Name));

						StatSubType selectedTopic = StatSubType.List[StatSubType.TOPIC1.ID + selectedEntry];
						GameRefs.I.m_globalLastBand.preReleaseTopic = selectedTopic;
						statTexts[1].text = Utilities.InterceptText(selectedTopic.Name);
					}

					OpenInsightSelector(clickPoint);
				}
			}
			else
			{
				if (marketingPopup.gameObject.activeSelf)
					return;

				if (graphs.GetPrimaryStatFilter() == StatType.MOOD)
				{
					if (BonusTrendMood.gameObject.activeSelf || BonusPopMood.gameObject.activeSelf)
						return;

					statTexts[0].text = "";
					GameRefs.I.m_globalLastBand.preReleaseMood = StatSubType.NONE;
					graphs.SetEntrySelected(-1);
				}
				else if (graphs.GetPrimaryStatFilter() == StatType.TOPIC)
				{
					if (BonusTrendTopic.gameObject.activeSelf || BonusPopTopic.gameObject.activeSelf)
						return;

					statTexts[1].text = "";
					GameRefs.I.m_globalLastBand.preReleaseTopic = StatSubType.NONE;
					graphs.SetEntrySelected(-1);
				}
				else // Genre
				{
					graphs.SetEntrySelected(GameRefs.I.m_globalLastBand.GetGenre().ID - StatSubType.ROCK_ID);
				}
				marketingPopup.gameObject.SetActive(false);
			}
		}
		else if (goInsights.activeSelf &&                                 // Only active when insights active
			!graphAnimating &&
			graphs.GetCurrentLocation() != StatSubType.NONE &&      // If they have all locations selected
			!GameRefs.I.m_globalLastBand.IsRecordingSong())          // Not currently recording       
		{
			// Check if we're clicking on an artist or the weeks dropdown (probably a better way to do this)
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = Input.mousePosition;

			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointer, raycastResults);
			// Skip this function if we're not in the graph view, or pressing a button that overlaps with the view
			if (raycastResults.Count > 0)
			{
				bool inGraph = false;
				foreach (var go in raycastResults)
				{
					if (go.gameObject.name.Contains("GraphBG"))
					{
						inGraph = true;
					}
					else if (go.gameObject.name.Contains("BarBack") ||
						go.gameObject.name.Contains("LineBack") ||
						go.gameObject.name.Contains("HeatBack") ||
						go.gameObject.name.Contains("Dropdown") ||
						go.gameObject.name.Contains("Viewport") ||
						go.gameObject.name.Contains("InsightBG") ||
						go.gameObject.name.Contains("ColorAdd") ||
						go.gameObject.name.Contains("Trending") ||
						go.gameObject.name.Contains("ColorAdd") ||
						go.gameObject.name.Contains("MostPop"))
					{
						return;
					}
				}

				if (!inGraph)
					return;
			}

			if (choices.gameObject.activeSelf)
				return;

			// Find the closest line, highlight it, and display the marketing insights popup
			Vector3 clickPoint;
			int thisSelectedEntry = graphs.GetClosestTrait(out clickPoint, false); 
			graphs.SetEntrySelected(thisSelectedEntry, true);
		}
	}

		public void OpenInsightSelector(Vector3 clickPoint)
	{
		// If line has no insight, open button to ask
		RectTransform popupPosition = marketingPopup.GetComponent<RectTransform>();
		if (graphs.GetVisualizationMode() == GraphManager.VisualizationMode.Line)
		{
			float xValue, yValue = 0f;
			if (clickPoint.x <= xCutoffLeft)
				xValue = xCutoffLeft;
			else if (clickPoint.x > xCutoffRight)
				xValue = xCutoffRight;
			else
				xValue = clickPoint.x;
			xValue += popupOffset.x;

			if (clickPoint.y > yValueCutoff)
			{
				yValue = yValueCutoff;
				tooHigh = true;
			}
			else
			{
				yValue = clickPoint.y;
				tooHigh = false;
			}
			yValue += popupOffset.y;

			popupPosition.SetAnchorPos(new Vector3(xValue, yValue));	
		}
		else
		{
			popupPosition.SetAnchorPos(popupAnchors[selectedEntry].localPosition);
		}
		cancelPredictionText.text = "CANCEL PREDICTION";
		marketingPopup.gameObject.SetActive(true);
		chooseInsight.gameObject.SetActive(true);
		// If line has insight, do nothing? They must click the icon to edit the insight
	}

	public void OpenCancelSelector(bool alsoOpenChoices=false)
	{
		OpenInsightChoice();
		choicesResponsive.SetActive(false);
		choices.SetActive(alsoOpenChoices);
		chooseInsight.gameObject.SetActive(false);
		choicesResponsive.SetActive(false);
		cancelInsight.gameObject.SetActive(true);
		marketingPopup.gameObject.SetActive(true);
	}

	public void SetInsightsInactive()
	{
		goInsights.SetActive(false);
		goTrends.SetActive(true);
	}

	private bool BandHasInsight(Band band)
	{
		foreach (MarketingInsightObject obj in unconfirmedInsights)
		{
			if (obj.bandAttached == band)
			{
				return true;
			}
		}
		return false;
	}

	public void CloseInsightSelector()
	{
		choicesResponsive.SetActive(false);
		choices.SetActive(false);
		chooseInsight.gameObject.SetActive(true);
		marketingPopup.gameObject.SetActive(false);
		cancelInsight.gameObject.SetActive(false);
	}

	public void OpenInsightChoice()
	{
		bool hasInsight = false;
		if(!hasInsight)
		{
			hasInsight = BandHasInsight(GameRefs.I.m_globalLastBand);
			choices.gameObject.SetActive(false);
			choicesResponsive.gameObject.SetActive(false);
			if (tooHigh)
				choicesResponsive.gameObject.SetActive(true);
			else
				choices.gameObject.SetActive(true);
		}

		chooseInsight.gameObject.SetActive(!hasInsight);
		cancelInsight.gameObject.SetActive(hasInsight);
	}

	public void CancelInsights()
	{
		bool sendFullLog = false;
		if (BandHasInsight(GameRefs.I.m_globalLastBand))
			sendFullLog = true;

		CloseInsightSelector();
		RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);

		if (cancelPredictionText.text.Contains("CANCEL") &&  insightBeforeEdit != null) // Don't re add the insight if we're clicking delete
		{
			unconfirmedInsights.Add(insightBeforeEdit);
			insightBeforeEdit = null;
		}

		ClearAllInsightIcons(true);
		ClearPredictionGraphIcon(true);
		selectedEntry = -1;
		selectedType = StatType.NONE;
		if(sendFullLog)
			GameRefs.I.PostGameState(true, "clickedButton", "deleteInsights");
		else
			GameRefs.I.PostGameState(false, "clickedButton", "cancelInsights");
	}

	public void ClearSelectedEntry()
	{
		selectedEntry = -1;
		selectedType = StatType.NONE;
	}

    private bool lastAllLocations = false;
    public void SetAnimGuy(bool enabled, bool allLocations)
    {
		// Typo in the animator :)
		if (!this.gameObject.activeSelf)
			return;
        animGuy.SetBool("DisabedAlert", enabled);
        goAllLocationsText.SetActive(allLocations);
        goBusyText.SetActive(!allLocations);
        lastAllLocations = allLocations;
    }
    
    public void SetAnimGuy(bool enabled)
    {
		if (!this.gameObject.activeSelf)
			return;
		animGuy.SetBool("DisabedAlert", enabled);
        goAllLocationsText.SetActive(lastAllLocations);
        goBusyText.SetActive(!lastAllLocations);
    }

	public void OpenInsights()
    {
		/* CloseInsightSelector(); */
		if (GameRefs.I.m_globalLastBand.IsRecordingSong())
			return;

		selectedEntry = -1; // TODO: This seems to fix an insidious bug.

		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterRecording);
		goInsights.SetActive(true);
        goTrends.SetActive(false);
        currentbands = GameRefs.I.m_gameController.GetSignedBands();

		GameRefs.I.preserveLastSelected = true;
		Band currBand = GameRefs.I.m_globalLastBand;
		insightBeforeEdit = GetCurrentInsight(currBand);
		graphs.SetBandFilter(currBand);
		artistNameText.text = currBand.Name;
		artistGenreImage.sprite = GameRefs.I.sharedParameters.SpriteSmallForGenre(currBand.GetGenre());
		statTexts[0].text = currBand.preReleaseMood == StatSubType.NONE ? "" : Utilities.InterceptText(currBand.preReleaseMood.Name);
		statTexts[1].text = currBand.preReleaseTopic == StatSubType.NONE ? "" : Utilities.InterceptText(currBand.preReleaseTopic.Name);
		statTexts[2].text = currBand.GetGenre().Name;
		releaseView.gameObject.SetActive(false);

		// Hack alert! The hierarchy needs to be active for Incarnate to set
		// animator parameters to pose the band members.
		bool isActive = gameObject.activeSelf;
		gameObject.SetActive(true);
		currBand.Incarnate(memberModels);
		gameObject.SetActive(isActive);

		RefreshInsightIcons();
		graphs.OnOpen(currBand.preReleaseLocation.ID, GameController.ViewMode.Songs);
		GameRefs.I.m_globalLastScreen = "insightsScreen";
		SetSelectLineText();
		StatSubType loc = currBand.preReleaseLocation;
		switch (loc.ID)
		{
			case StatSubType.BOOKLINE_ID: locationSelect.OnBouroughSelected(1); break;
			case StatSubType.MADHATTER_ID: locationSelect.OnBouroughSelected(5); break;
			case StatSubType.TURTLE_HILL_ID: locationSelect.OnBouroughSelected(0); break;
			case StatSubType.KINGS_ISLE_ID: locationSelect.OnBouroughSelected(4); break;
			case StatSubType.THE_BRONZ_ID: locationSelect.OnBouroughSelected(2); break;
			case StatSubType.IRONWOOD_ID: locationSelect.OnBouroughSelected(3); break;
			default: locationSelect.SelectRandomUnlockedBorough(); break;
		}
		CheckGraphHighlights();
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.EnterFindTrends);
	}

	public void RefreshInsightIcons()
	{
		ClearAllInsightIcons();
		ClearPredictionGraphIcon(true);
		foreach (MarketingInsightObject insight in unconfirmedInsights)
		{
			if (insight.bandAttached == GameRefs.I.m_globalLastBand)
			{
				SetConfiguratorIcon(insight.insightType == InsightType.MostPopular ? true : false, insight.statType.SuperType);
				locationSelect.EnableTrendIcon(insight.location, insight.insightType == InsightType.MostPopular);
			}
		}
	}

	public MarketingInsightObject GetCurrentInsight(Band band)
	{
		foreach (MarketingInsightObject insight in unconfirmedInsights)
		{
			if (insight.bandAttached == band)
			{
				return insight;
			}
		}
		return null;
	}

	public void RemoveInsightsFromBand(Band band)
	{
		// Check if we're overwriting an insight for the current band
		MarketingInsightObject toRemove = null;
		for (int i = 0; i < unconfirmedInsights.Count; i++)
		{
			if (unconfirmedInsights[i].bandAttached == band)
			{
				toRemove = unconfirmedInsights[i];
				break;
			}
		}
		if (toRemove != null)
			unconfirmedInsights.Remove(toRemove);
	}

	public void SelectInsight(bool isMostPopular)
	{
		RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);

		// Save the insight data so far, and apply when we hit record
		unconfirmedInsights.Add(new MarketingInsightObject(isMostPopular ? InsightType.MostPopular : InsightType.IsTrending, 
            graphs.GetLastClosestType(),
            GameRefs.I.m_globalLastBand,
            graphs.GetCurrentLocation()));

		marketingPopup.gameObject.SetActive(false);

		SetConfiguratorIcon(isMostPopular, graphs.GetPrimaryStatFilter());
		locationSelect.EnableTrendIcon(graphs.GetCurrentLocation(), isMostPopular);
		CloseInsightSelector();
		GameRefs.I.PostGameState(true, "clickedButton", string.Format("{0}", isMostPopular ? "MostPopular" : "TrendingUp"));
	}

	private void SetConfiguratorIcon(bool isMostPopular, StatType type)
	{
		ClearAllInsightIcons();

		if (type == StatType.GENRE)
		{
			if (isMostPopular)
				BonusPopGenre.gameObject.SetActive(true);
			else
				BonusTrendGenre.gameObject.SetActive(true);
		}
		else if (type == StatType.TOPIC)
		{
			if (isMostPopular)
				BonusPopTopic.gameObject.SetActive(true);
			else
				BonusTrendTopic.gameObject.SetActive(true);
		}
		else if (type == StatType.MOOD)
		{
			if (isMostPopular)
				BonusPopMood.gameObject.SetActive(true);
			else
				BonusTrendMood.gameObject.SetActive(true);
		}

		if (graphs.GetPrimaryStatFilter() == type)
		{
			predictionButton.localPosition = graphs.GetPredictionMarkerPosition(graphs.entrySelectedViaHighlight);
			BonusGraphIcon.sprite = isMostPopular ? PopSprite : TrendSprite;
			predictionButton.gameObject.SetActive(true);
			predictionButtonCG.alpha = 1f;
		}

	}

	public void ClearPredictionGraphIcon(bool immediate)
	{
		if (immediate)
			predictionButton.gameObject.SetActive(false);
		else
			m_shakeAwayRoutine.Replace(this, ShakeAndDisappear());
		//predictionButton.gameObject.SetActive(false);
	}

	private IEnumerator ShakeAndDisappear()
	{
		yield return predictionButton.AnchorPosTo(predictionButton.anchoredPosition.x + 5f, 0.4f, Axis.X).Wave(Wave.Function.SinFade, 5);
		yield return Routine.Combine(predictionButton.AnchorPosTo(predictionButton.anchoredPosition.y + 20f, 0.4f, Axis.Y),
			predictionButtonCG.FadeTo(0f, 0.4f));
		predictionButton.gameObject.SetActive(false);
	}

	void ClearAllInsightIcons(bool isImmediate = false)
	{
		BonusPopGenre.gameObject.SetActive(false);
		BonusTrendGenre.gameObject.SetActive(false);
		BonusPopMood.gameObject.SetActive(false);
		BonusTrendMood.gameObject.SetActive(false);
		BonusPopTopic.gameObject.SetActive(false);
		BonusTrendTopic.gameObject.SetActive(false);

		MarketingInsightObject currInsight = null;
		foreach (MarketingInsightObject insight in unconfirmedInsights)
		{
			if (insight.bandAttached == GameRefs.I.m_globalLastBand)
			{
				currInsight = insight;
			}
		}
		locationSelect.DisableAllTrendIcons(currInsight);
	}

    public void SetCurrentBand(int band)
    {
		if (band < 0)
			return;

		SetCurrentBand(currentbands[band]);
    }

	public void GraphTypeChanged()
	{
		if (!goInsights.activeSelf)
			return;

		int type = 0;

		if (graphs.GetPrimaryStatFilter() == StatType.MOOD)
			type = 0;
		else if (graphs.GetPrimaryStatFilter() == StatType.TOPIC)
			type = 1;
		else
			type = 2;

		for(int i = 0; i < 3; i++)
		{
			if (i == type)
				statBackgrounds[i].color = new Color(0.5f, 0.5f, 0.5f);
			else
				statBackgrounds[i].color = Color.white;
		}
		graphs.SetSortButtonsToggle(type);

		SetSelectLineText();

		CheckGraphHighlights();
		CloseInsightSelector();
		RefreshInsightIcons();
	}

	private void SetSelectLineText()
	{
		if (graphs.GetVisualizationMode() == GraphManager.VisualizationMode.Line)
		{
			findLineText.text = "Select a Line";
		}
		else if (graphs.GetVisualizationMode() == GraphManager.VisualizationMode.Bar)
		{
			findLineText.text = "Select a Bar";
		}
		else if (graphs.GetVisualizationMode() == GraphManager.VisualizationMode.Heatmap)
		{
			findLineText.text = "Select a Location";
		}
	}

	public void GraphLocationChanged()
	{
		if (!goInsights.activeSelf)
			return;
		else
		{
			StatSubType loc = graphs.GetCurrentLocation();
			if (loc != GameRefs.I.m_globalLastBand.preReleaseLocation)
			{
				RemoveInsightsFromBand(GameRefs.I.m_globalLastBand);
				RefreshInsightIcons();
				CloseInsightSelector();
				GameRefs.I.m_globalLastBand.preReleaseLocation = loc;
			}
		}
		
	}

	private void CheckGraphHighlights()
	{
		if (!goInsights.activeSelf)
		{
			graphs.SetEntrySelected(-1);
		}
		else
		{
			if (graphs.GetPrimaryStatFilter() == StatType.MOOD)
			{
				if (GameRefs.I.m_globalLastBand.preReleaseMood != StatSubType.NONE)
					graphs.SetEntrySelected(GameRefs.I.m_globalLastBand.preReleaseMood.ID - StatSubType.MOOD1_ID);
				else
					graphs.SetEntrySelected(-1);
			}
			else if (graphs.GetPrimaryStatFilter() == StatType.TOPIC)
			{
				if (GameRefs.I.m_globalLastBand.preReleaseTopic != StatSubType.NONE)
					graphs.SetEntrySelected(GameRefs.I.m_globalLastBand.preReleaseTopic.ID - StatSubType.TOPIC1_ID);
				else
					graphs.SetEntrySelected(-1);
			}
			else if (graphs.GetPrimaryStatFilter() == StatType.GENRE)
			{
				graphs.SetEntrySelected(GameRefs.I.m_globalLastBand.GetGenre().ID - StatSubType.ROCK_ID);
			}
		}

	}

	public void SetCurrentBand(Band band)
	{
		currentArtistPanel.InitForInsights(this, band, -1);
		GameRefs.I.m_globalLastBand = band;
        graphs.SetBandFilter(GameRefs.I.m_globalLastBand);
        graphs.SetEntrySelected(-1);
        graphs.RefreshGraph();
	}

	public void OnArtistSelected(ArtistSelector selected)
	{
        graphs.SetEntrySelected(-1);
        marketingPopup.gameObject.SetActive(false);

	}

	public void CloseInsights()
    {
		GameRefs.I.hudController.ToRecordingMode();
        animGuy.SetBool("DisabedAlert", false);
        graphs.OnClose();
		CloseInsightSelector();
		releaseView.gameObject.SetActive(true);
		SetInsightsInactive();
		releaseView.SelectBand(GameRefs.I.m_globalLastBand);
        graphs.SetBandFilter(null);
		GameRefs.I.PostGameState(true, "clickedButton", "PlanSongButton");
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.ReturnToRecording);
		GameRefs.I.m_globalLastScreen = "recordingScreen";
	}

    public void ToggleArtistMenu()
    {
        m_showArtistRoutine.Replace(this, DisplayArtistMenu(!bMenuIsOpen));
    }

    private IEnumerator DisplayArtistMenu(bool open)
    {
        if (open)
        {
            bMenuIsOpen = true;
            artistMenuText.text = "CLOSE MENU";
        }
        else
        {
            bMenuIsOpen = false;
            artistMenuText.text = "CHOOSE ARTIST";
        }
        yield return Routine.Combine(artistMenuArrow.RotateTo(open ? 180f : 0f, 0.3f, Axis.Z).Ease(Curve.BackOut),
            artistMenu.MoveTo(open ? yPosMenuVisible : yPosMenuHidden, 0.4f, Axis.Y, Space.Self).Ease(Curve.QuartInOut));
    }
}
