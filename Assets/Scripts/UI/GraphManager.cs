using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Utility;
using BeauRoutine;

public class GraphManager : MonoBehaviour
{
    public class GraphEntryData
    {
        public class Subset
        {
            public List<float?> Values;
            public StatSubType SubType;

            public Subset(StatSubType subType)
            {
                this.Values = new List<float?>();
                this.SubType = subType;
            }
        }

        public string Name;
        public bool Glow;
		public bool EntryUnlocked;
        public float Thickness;
        public int Offset;
        public List<Subset> Subsets;
    }

	public HudController hudController;
    public DataSimulationManager DataManager;
	public DataCollectionParameters dataCollectionParams;
    public LineGraph LineGraph;
    public BarGraph BarGraph;
	public HeatGraph HeatGraph;
    public RectTransform GraphZoneTransform;
    public Animator Animator;
    public List<RectTransform> YValueLabels;
    public List<RectTransform> XValueLabels;
    public GameObject XAxisLabel;
    public GameObject TimeDropdownRoot;
    public GameObject WeekPanRoot;
	public GameObject WeekPanHeader;
    public TextMeshProUGUI WeekPanInput;
    public Button WeekPanBackButton;
    public Button WeekPanForwardButton;
    public Text NoQueryLabel;
	public LocationButtonHelper locButtons;
	public MarketingInsights marketingInsights;
	public TextMeshProUGUI graphHeader;
	public Button backButton;
	public Button cancelButton;
	public Image[] sortButtonBgs;
	public TextMeshProUGUI[] sortButtonText;
	public Image[] sortButtonIcon;
	public LayoutElement[] sortButtonIconLE;
	public Image[] visualizationIcons;
	public TextMeshProUGUI sortSongsHeader;
	public GameObject beforeYouStarted;

	public Color selectedColor;
	public Color deselectedColor;
	public GameObject doesntCareRoot;
	public TextMeshProUGUI doesntCareText;
	public TextMeshProUGUI collectionText;
	public GameObject lineBar;
	public GameObject locationToggles;
	public ToggleGroup genreToggles;
	public ToggleGroup moodToggles;
	public ToggleGroup topicToggles;
	public TextMeshProUGUI traitCollectionText;
	public GameObject moodCollectionIcon;
	public GameObject topicCollectionIcon;
	public GameObject genreCollectionIcon;
	public TextMeshProUGUI toggleSubheader;
	public TextMeshProUGUI toggleHeader;

    private StatSubType locationToGraph = StatSubType.NONE;
    private StatType typeToGraph = StatType.MOOD;
    private StatType secondaryTypeToGraph = StatType.GENRE;
	private StatSubType secondaryFilterToGraph = StatSubType.NONE; // Unused?

	private StatSubType topicToGraph = StatSubType.TOPIC1;
    private StatSubType moodToGraph = StatSubType.MOOD1;
    private StatSubType genreToGraph = StatSubType.ROCK;
	private RectTransform lineGraphArea;
	private RectTransform barGraphArea;
    private Band bandFilter;

	private Routine animRoutine;
	private int lastSelectedEntry;
	private int timeRange;
    private int minKey;
    private int maxKey;

    private int pannedWeek;
	private bool isTraitCaredAbout;
	public UnityAction backListener;

    public enum ViewMode
    {
        Waiting = 0,
        Query = 1,
        Data = 2,
    }

    public enum VisualizationMode
    {
		Bar,
		Line,
        StackedBar,
        Heatmap,
    }

    public enum GraphMode
    {
        Preferences,
        IndustryListens,
        Sales,
        CumulativeSales,
        Listens,
        ArtistFollowers,
        SongPreference,
    }

    private VisualizationMode visualizationMode;
	private GraphMode graphMode = GraphMode.Listens;
    private int currentTurn;
    private ViewMode viewMode;
    private Vector2 cachedResolution;
    private int xValuesDisplayed;

    private void Start()
    {
        this.timeRange = 16;
        this.viewMode = ViewMode.Query;

        this.cachedResolution = new Vector2();

        this.graphMode = GraphMode.IndustryListens;

		lineGraphArea = LineGraph.GetComponent<RectTransform>();
		barGraphArea = BarGraph.GetComponent<RectTransform>();
		/* gameObject.SetActive(false); // TODO */
	}

	private void Update()
    {
		if (this.cachedResolution.x != Screen.width || this.cachedResolution.y != Screen.height)
        {
            RectTransform thisRectTransform = this.GraphZoneTransform;
            Vector3[] worldCorners = new Vector3[4];
            thisRectTransform.GetWorldCorners(worldCorners);
            float thisBottom = worldCorners[0].y;
            float thisLeft = worldCorners[0].x;
            float rectHeight = worldCorners[1].y - worldCorners[0].y;
            float rectWidth = worldCorners[2].x - worldCorners[0].x;

            for (int i = 0; i < this.YValueLabels.Count; i++)
            {
                this.YValueLabels[i].position = new Vector3(this.YValueLabels[i].position.x, thisBottom + rectHeight * (((float)i) / (this.YValueLabels.Count - 1)));
            }

            for (int i = 0; i < this.XValueLabels.Count; i++)
            {
                if (this.visualizationMode == VisualizationMode.Line)
                {
                    this.XValueLabels[i].position = new Vector3(thisLeft + rectWidth * (((float)i) / (this.xValuesDisplayed - 1)), this.XValueLabels[i].position.y);
                }
                else
                {
                    this.XValueLabels[i].position = new Vector3(thisLeft + rectWidth * (((float)i + 1) / (this.xValuesDisplayed + 1)), this.XValueLabels[i].position.y);
                }
            }

            this.cachedResolution.x = Screen.width;
            this.cachedResolution.y = Screen.height;
        }
    }

    public void SetBandFilter(Band band)
    {
        bandFilter = band;
    }

    public void OnOpen(int locId=-1, GameController.ViewMode viewMode = GameController.ViewMode.Studio)
    {
		this.gameObject.SetActive(true);
		animRoutine.Replace(this, OpenGraph());

		StatSubType preReleaseMood = StatSubType.NONE;
		StatSubType preReleaseTopic = StatSubType.NONE;
		StatSubType preReleaseLocation = StatSubType.NONE;

		if (GameRefs.I.m_globalLastBand != null)
		{
			preReleaseMood = GameRefs.I.m_globalLastBand.preReleaseMood;
			preReleaseTopic = GameRefs.I.m_globalLastBand.preReleaseTopic;
			preReleaseLocation = GameRefs.I.m_globalLastBand.preReleaseLocation;
		}

		if (viewMode == GameController.ViewMode.Songs)
		{
			GameRefs.I.hudController.ToTrendsPredictMode();
			GameRefs.I.m_gameController.SetBackToStudioButton(false);
			cancelButton.gameObject.SetActive(true);
			cancelButton.onClick.RemoveAllListeners();
			cancelButton.onClick.AddListener(() => {
				// Restore former prerelease settings so that recording panel
				// doesn't adopt canceled changes.
				if (GameRefs.I.m_globalLastBand != null)
				{
					GameRefs.I.m_globalLastBand.preReleaseMood = preReleaseMood;
					GameRefs.I.m_globalLastBand.preReleaseTopic = preReleaseTopic;
					GameRefs.I.m_globalLastBand.preReleaseLocation = preReleaseLocation;
				}

				marketingInsights.CancelInsights();
				if (GameRefs.I.m_globalLastScreen != "insightsScreen")
				{
					GameRefs.I.PostGameState(false, "clickedButton", "cancelButton");
				}
				marketingInsights.SetInsightsInactive();
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
				GameRefs.I.m_gameController.OpenLastView();
				GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
				GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.ReturnToRecording);
			});
		}
		else
		{
			cancelButton.gameObject.SetActive(false);
			backButton.onClick.RemoveAllListeners();
			backListener = () => {
				GameRefs.I.PostGameState(false, "clickedButton", "backButton");
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
				OnClose();
				if (viewMode == GameController.ViewMode.Studio)
				{
					GameRefs.I.m_globalLastScreen = "mainScreen";
					GameRefs.I.m_gameController.OpenStudioView();
				}
				else if (viewMode == GameController.ViewMode.Results)
				{
					GameRefs.I.m_globalLastScreen = "resultsScreen";
					GameRefs.I.hudController.ToResultsMode();
				}
				else if (viewMode == GameController.ViewMode.TopCharts)
				{
					GameRefs.I.m_globalLastScreen = "topCharts";
					GameRefs.I.m_topCharts.RestoreLayout();
				}
				else
				{
					if(GameRefs.I.m_tutorialController.events[15].completed)
						GameRefs.I.m_gameController.marketingTutorialGuyNormal.SetActive(true);
					GameRefs.I.m_gameController.OpenLastView();
				}
			};

			backButton.onClick.AddListener(backListener);

			if (viewMode == GameController.ViewMode.Studio)
			{
				GameRefs.I.hudController.ToTrendsStudioMode();
			}
			else
			{
				GameRefs.I.hudController.ToTrendsBackMode();
			}
		}

		SetEntrySelected(-1);
		this.UpdateKeyRange();
        this.RefreshGraph();

        this.viewMode = ViewMode.Query;

		if (locId < 0)
		{
			locButtons.SelectRandomUnlockedBorough();
		}
		else
		{
			switch (locId)
			{
				case StatSubType.BOOKLINE_ID: locButtons.OnBouroughSelected(1); break;
				case StatSubType.MADHATTER_ID: locButtons.OnBouroughSelected(5); break;
				case StatSubType.TURTLE_HILL_ID: locButtons.OnBouroughSelected(0); break;
				case StatSubType.KINGS_ISLE_ID: locButtons.OnBouroughSelected(4); break;
				case StatSubType.THE_BRONZ_ID: locButtons.OnBouroughSelected(2); break;
				case StatSubType.IRONWOOD_ID: locButtons.OnBouroughSelected(3); break;
				default: locButtons.SelectRandomUnlockedBorough(); break;
			}
		}
		GameRefs.I.m_globalLastScreen = "trendsScreen";
	}

    public void OnClose()
    {
		bandFilter = null;
		animRoutine.Replace(this, CloseGraph());
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ForReferenceTrends);
    }
	
	private IEnumerator OpenGraph()
	{
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
		Animator.SetBool("On", true);
		marketingInsights.graphAnimating = true;
		yield return 0.4f;
		marketingInsights.graphAnimating = false;
	}

	private IEnumerator CloseGraph()
	{
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
		Animator.SetBool("On", false);
		marketingInsights.graphAnimating = true;
		yield return 0.4f;
		marketingInsights.graphAnimating = false;
		this.gameObject.SetActive(false);
	}

    public void OnCreateNewQueryButtonClicked()
    {
        this.viewMode = ViewMode.Query;
    }

    public void OnDataButtonClicked()
    {
        if (this.viewMode == ViewMode.Data)
        {
            this.OnOpen();
        }
        else
        {
            this.viewMode = ViewMode.Data;
            //this.Animator.SetInteger("Screen", (int)this.viewMode);
        }
    }

    public StatSubType GetCurrentLocation()
    {
        return this.locationToGraph;
    }

	public int GetClosestTrait(out Vector3 clickPoint, bool setLastSelected)
	{
		Vector2 worldPoint;
		int selectedEntry = -1;
		if (visualizationMode == VisualizationMode.Line)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(lineGraphArea, Input.mousePosition, GameRefs.I.m_mainCamera, out worldPoint);
			if (Mathf.Abs(worldPoint.x) < 650f && Mathf.Abs(worldPoint.y) < 450f)
				selectedEntry = LineGraph.GetClosestLine(minKey, maxKey, worldPoint);
		}
		else
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(barGraphArea, Input.mousePosition, GameRefs.I.m_mainCamera, out worldPoint);
			if (Mathf.Abs(worldPoint.x) < 650f && Mathf.Abs(worldPoint.y) < 450f)
				selectedEntry = BarGraph.GetClosestBar(worldPoint);
		}
		clickPoint = worldPoint;
		if(setLastSelected && selectedEntry >= 0)
		{
			lastSelectedEntry = selectedEntry;
		}
			
		return selectedEntry;
	}

	public Vector3 GetPredictionMarkerPosition(int entry)
	{
		if (visualizationMode == VisualizationMode.Line)
			return LineGraph.GetPredictionMarkerPosition(minKey, maxKey, entry) + new Vector3(-90f, -45f, 0);
		else if (visualizationMode == VisualizationMode.Bar)
			return BarGraph.GetPredictionMarkerPosition(entry) + new Vector3(-140f, 10f, 0);
		else return Vector3.zero;
	}

	public StatSubType GetLastClosestType()
	{
		if (visualizationMode == VisualizationMode.Line)
			return LineGraph.currTypes[lastSelectedEntry];
		else
			return BarGraph.currTypes[lastSelectedEntry];
	}

	public int GetLastSelectedEntry()
	{
		return lastSelectedEntry;
	}

    public void RefreshGraph()
    {
		this.UpdateDropdowns();

		if (this.pannedWeek >= 30)
		{
			WeekPanHeader.SetActive(false);
			this.WeekPanInput.text = "THIS WEEK";
		}
		else
		{
			WeekPanHeader.SetActive(true);
			this.WeekPanInput.text = (30 - this.pannedWeek).ToString();
		}

        switch (this.graphMode)
        {
            case GraphMode.Preferences:
                this.OpenPreferenceGraph(false);
                break;
            case GraphMode.IndustryListens:
				this.OpenPreferenceGraph(true);
				break;
            case GraphMode.Sales:
                this.OpenSalesGraph(GameRefs.I.m_gameController.GetSongs());
                break;
            case GraphMode.CumulativeSales:
                this.OpenCumulativeSalesGraph(GameRefs.I.m_gameController.GetSongs());
                break;
            case GraphMode.Listens:
                this.OpenListensGraph(GameRefs.I.m_gameController.GetSongs());
                break;
            case GraphMode.ArtistFollowers:
                this.OpenFollowersGraph(GameRefs.I.m_gameController.GetFollowerData());
                break;
            case GraphMode.SongPreference:
                this.OpenAppealGraph(GameRefs.I.m_gameController.GetSongs());
                break;
        }

		if (visualizationMode == VisualizationMode.Heatmap)
		{
			string graphTitle = ""; 
			if(typeToGraph == StatType.TOPIC)
			{
				graphTitle += "Percent Listening to Songs About ";
				graphTitle += Utilities.InterceptText(topicToGraph.Name);
			}
				
			else
			{
				graphTitle += "Percent Listening to ";
				if (typeToGraph == StatType.GENRE)
					graphTitle += Utilities.InterceptText(genreToGraph.Name) + " Songs";
				else
					graphTitle += Utilities.InterceptText(moodToGraph.Name) + " Songs";
			}
			graphHeader.text = graphTitle;

			toggleHeader.text = string.Format("BY {0}", typeToGraph.Name.ToUpper());
			marketingInsights.ClearPredictionGraphIcon(true);
		}
		else
		{
			string graphTitle = "Song Listens by ";
			graphTitle += Utilities.InterceptText(typeToGraph.Name) + " in ";
			graphTitle += locationToGraph.Name;
			graphHeader.text = graphTitle;

			toggleHeader.text = "TARGET AUDIENCE";
		}

		TraitSampling sampling = GameRefs.I.traitSamplings[typeToGraph];
		collectionText.text = string.Format("{0}{1}", sampling.slotCount > 0 ? "Every " : "", dataCollectionParams.frequencies[sampling.slotCount].label);

		if (isTraitCaredAbout || visualizationMode == VisualizationMode.Heatmap)
		{
			doesntCareRoot.SetActive(false);
		}
		else
		{
			doesntCareRoot.SetActive(true);
			doesntCareText.text = string.Format("{0} doesn't care about {1}", locationToGraph.Name, typeToGraph.Name);
		}

		beforeYouStarted.SetActive(visualizationMode == VisualizationMode.Line);
		BarGraph.gameObject.SetActive(HasSample(pannedWeek) && visualizationMode == VisualizationMode.Bar && isTraitCaredAbout);
		LineGraph.gameObject.SetActive(visualizationMode == VisualizationMode.Line && isTraitCaredAbout);
        WeekPanRoot.SetActive(visualizationMode == VisualizationMode.Bar || visualizationMode == VisualizationMode.Heatmap);
		if (!HasSample(pannedWeek) || !(isTraitCaredAbout || visualizationMode == VisualizationMode.Heatmap))
		{
			WeekPanInput.text = "N/A";
		}
	}

	private void SyncCaresAbout()
	{
		// This mapping seems to be specified in a pretty ad hoc way. What can I do?
		int i = -1;
		if (typeToGraph == StatType.GENRE)
		{
			i = 0;
		}
		else if (typeToGraph == StatType.MOOD)
		{
			i = 1;
		}	
		else if (typeToGraph == StatType.TOPIC)
		{
			i = 2;
		}

		isTraitCaredAbout = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(locationToGraph)[i];
	}

    public void UpdateGraphTurn(int turn)
    {
        this.currentTurn = turn;

		// Don't update the graph graphics if we aren't even open!
		if (!this.gameObject.activeSelf)
			return;

		this.UpdateKeyRange();

        this.RefreshGraph();
    }

	public void OnVisualizationChanged(Toggle change)
	{
		if(change.isOn)
		{
			if (change.name.Contains("Bar"))
			{
				this.visualizationMode = VisualizationMode.Bar;
				for(int i = 0; i < visualizationIcons.Length; i++)
					visualizationIcons[i].color = i == 0 ? Color.black : Color.white;
			}
			else if (change.name.Contains("Line"))
			{
				this.visualizationMode = VisualizationMode.Line;
				for (int i = 0; i < visualizationIcons.Length; i++)
					visualizationIcons[i].color = i == 1 ? Color.black : Color.white;
			}
			else if (change.name.Contains("Heat"))
			{
				this.visualizationMode = VisualizationMode.Heatmap;
				for (int i = 0; i < visualizationIcons.Length; i++)
					visualizationIcons[i].color = i == 2 ? Color.black : Color.white;
			}
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ForReferenceTrends);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
			marketingInsights.GraphTypeChanged();
			ChangePannedWeek(pannedWeek);
		}
	}
        
	public VisualizationMode GetVisualizationMode()
	{
		return visualizationMode;
	}

    public void OnGraphTypeDropdownChanged(Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                this.graphMode = GraphMode.IndustryListens;
                break;
            case 1:
                this.graphMode = GraphMode.Preferences;
                break;
            case 2:
                this.graphMode = GraphMode.Sales;
                break;
            case 3:
                this.graphMode = GraphMode.CumulativeSales;
                break;
            case 4:
                this.graphMode = GraphMode.Listens;
                break;
            case 5:
                this.graphMode = GraphMode.ArtistFollowers;
                break;
            case 6:
                this.graphMode = GraphMode.SongPreference;
                break;
        }

		marketingInsights.GraphTypeChanged();
		this.UpdateKeyRange();

        this.RefreshGraph();
    }

	public void OnGraphStatSubTypeChanged(int type)
	{
		StatSubType newType = StatSubType.NONE;
		switch(type)
		{
			case StatSubType.HIP_HOP_ID: newType = StatSubType.HIP_HOP; break;
			case StatSubType.ROCK_ID: newType = StatSubType.ROCK; break;
			case StatSubType.RAP_ID: newType = StatSubType.RAP; break;
			case StatSubType.RANDB_ID: newType = StatSubType.RANDB; break;
			case StatSubType.ELECTRONIC_ID: newType = StatSubType.ELECTRONIC; break;
			case StatSubType.POP_ID: newType = StatSubType.POP; break;

			case StatSubType.MOOD1_ID: newType = StatSubType.MOOD1; break;
			case StatSubType.MOOD2_ID: newType = StatSubType.MOOD2; break;
			case StatSubType.MOOD3_ID: newType = StatSubType.MOOD3; break;
			case StatSubType.MOOD4_ID: newType = StatSubType.MOOD4; break;
			case StatSubType.MOOD5_ID: newType = StatSubType.MOOD5; break;
			case StatSubType.MOOD6_ID: newType = StatSubType.MOOD6; break;

			case StatSubType.TOPIC1_ID: newType = StatSubType.TOPIC1; break;
			case StatSubType.TOPIC2_ID: newType = StatSubType.TOPIC2; break;
			case StatSubType.TOPIC3_ID: newType = StatSubType.TOPIC3; break;
			case StatSubType.TOPIC4_ID: newType = StatSubType.TOPIC4; break;
			case StatSubType.TOPIC5_ID: newType = StatSubType.TOPIC5; break;
			case StatSubType.TOPIC6_ID: newType = StatSubType.TOPIC6; break;
		}

		typeToGraph = newType.SuperType;

		if (typeToGraph == StatType.GENRE)
			genreToGraph = newType;
		else if (typeToGraph == StatType.MOOD)
			moodToGraph = newType;
		else if (typeToGraph == StatType.TOPIC)
			topicToGraph = newType;

		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ForReferenceTrends);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
		marketingInsights.GraphTypeChanged();
		this.UpdateKeyRange();
		RefreshGraph();
	}

    public void OnGraphStatTypeChanged(int type)
    {
        StatType newStatToGraph = this.typeToGraph;

        switch (type)
        {
            case 2:
                newStatToGraph = StatType.GENRE;
				traitCollectionText.text = "Genre";
				genreCollectionIcon.SetActive(true);
				moodCollectionIcon.SetActive(false);
				topicCollectionIcon.SetActive(false);
				break;
            case 1:
                newStatToGraph = StatType.TOPIC;
				traitCollectionText.text = "TOPIC";
				genreCollectionIcon.SetActive(false);
				moodCollectionIcon.SetActive(false);
				topicCollectionIcon.SetActive(true);
				break;
            case 0:
                newStatToGraph = StatType.MOOD;
				traitCollectionText.text = "MOOD";
				genreCollectionIcon.SetActive(false);
				moodCollectionIcon.SetActive(true);
				topicCollectionIcon.SetActive(false);
				break;
            case 3:
                newStatToGraph = StatType.LOCATION;
                break;
        }

        typeToGraph = newStatToGraph;
		marketingInsights.GraphTypeChanged();
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ForReferenceTrends);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
		SyncCaresAbout();
		// If the current week doesn't have data available, jump to the last
		// one that does.
		if (!HasSample(pannedWeek))
		{
			ChangePannedWeek(GetPreviousSampledWeek(pannedWeek));
		}
		else
		{
			ChangePannedWeek(pannedWeek);
		}
		this.UpdateKeyRange();
		RefreshGraph();
    }

	private bool HasSample(int week)
	{
		return GameRefs.I.isSampledAt[typeToGraph][week].HasValue && GameRefs.I.isSampledAt[typeToGraph][week].Value;
	}

    public void OnLocationChanged(LocationButtonHelper.BouroughName change)
    {
        StatSubType newLocationToGraph = this.locationToGraph;

        switch (change)
        {
            case LocationButtonHelper.BouroughName.Bookline:
				newLocationToGraph = StatSubType.BOOKLINE;
				break;
            case LocationButtonHelper.BouroughName.Bronze:
                newLocationToGraph = StatSubType.THE_BRONZ;
                break;
            case LocationButtonHelper.BouroughName.Ironwood:
                newLocationToGraph = StatSubType.IRONWOOD;
                break;
            case LocationButtonHelper.BouroughName.Kings:
                newLocationToGraph = StatSubType.KINGS_ISLE;
                break;
            case LocationButtonHelper.BouroughName.Madhatter:
                newLocationToGraph = StatSubType.MADHATTER;
                break;
            case LocationButtonHelper.BouroughName.Turtle:
                newLocationToGraph = StatSubType.TURTLE_HILL;
                break;
			case LocationButtonHelper.BouroughName.All:
				newLocationToGraph = StatSubType.NONE;
				break;
			default:
				newLocationToGraph = StatSubType.NONE;
				break;
        }

		toggleSubheader.text = newLocationToGraph.Name;
		this.locationToGraph = newLocationToGraph;
		marketingInsights.GraphLocationChanged();

		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterFindTrends);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ForReferenceTrends);

		SyncCaresAbout();
		ChangePannedWeek(pannedWeek);
	}

	public void SetSortButtonsToggle(int type)
	{
		for(int i = 0; i < sortButtonBgs.Length; i++)
		{
			if (i == type)
			{
				sortButtonBgs[i].color = selectedColor;
				sortButtonText[i].color = Color.black;
				sortButtonText[i].fontSize = 45f;
				sortButtonIcon[i].color = Color.black;
				sortButtonIconLE[i].preferredWidth = 55f;
				sortButtonIconLE[i].preferredHeight = 55f;
				// increase size
			}	
			else
			{
				sortButtonBgs[i].color = deselectedColor;
				sortButtonText[i].color = Color.white;
				sortButtonText[i].fontSize = 35f;
				sortButtonIcon[i].color = Color.white;
				sortButtonIconLE[i].preferredWidth = 45f;
				sortButtonIconLE[i].preferredHeight = 45f;
			}
			
		}
	}

	public int entrySelectedViaHighlight;
	public void SetEntrySelected(int entry, bool altGlowColor=false)
	{
		if(visualizationMode == VisualizationMode.Line)
		{
			if (entry >= 0)
			{
				for (int i = 0; i < LineGraph.Entries.Count / 2; i++)
				{
					LineGraph.SetLineGlow(i, i == entry, altGlowColor);
				}
			}
			else
			{
				for (int i = 0; i < LineGraph.Entries.Count / 2; i++)
					LineGraph.SetLineGlow(i, false, altGlowColor);
			}
			LineGraph.SetLineGlowEntry(entry*2 + 1, altGlowColor);
		}
		else
		{
			BarGraph.SetBarGlow(entry, altGlowColor);
		}

		if(!altGlowColor && entry >= 0)
			entrySelectedViaHighlight = entry;
		RefreshGraph();
	}

    private void OpenPreferenceGraph(bool industry)
    {
		if(visualizationMode == VisualizationMode.Heatmap)
		{
			sortSongsHeader.text = "FILTER SONGS BY";
			this.SetupNewHeatGraphView(DataManager);
		}
		else
		{
			sortSongsHeader.text = "SORT SONGS BY";
			List<GraphManager.GraphEntryData> dataToGraph = new List<GraphManager.GraphEntryData>();

			Dictionary<StatSubType, List<float>> data;

			if (this.locationToGraph == StatSubType.NONE)
			{
				data = industry ? this.DataManager.GetCachedIndustryPreferenceData() : this.DataManager.GetCachedPreferenceData();
			}
			else
			{
				data = industry ? this.DataManager.GetCachedIndustryLocationData(locationToGraph) : this.DataManager.GetCachedLocationData(locationToGraph);
			}

			GameRefs.I.m_lastLocationGraphed = locationToGraph;

			foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
			{
				float thicknessToSet = GameRefs.I.trendsLineThickness;
				bool bandHasEntry = false;

				if (bandFilter != null)
				{
					if (bandFilter.GetKnownTraits().Contains(kvp.Key) || bandFilter.GetGenre() == kvp.Key)
					{
						bandHasEntry = true;
						thicknessToSet = marketingInsights.goInsights.activeSelf ? GameRefs.I.insightSelectableLineThickness : GameRefs.I.trendsLineThickness;
					}
					else
					{
						thicknessToSet = GameRefs.I.unselectableLineThickness;
					}
				}
				else
					bandHasEntry = true;

				if (kvp.Key.SuperType == this.typeToGraph)
				{
					dataToGraph.Add(new GraphManager.GraphEntryData
					{
						Name = kvp.Key.Name,
						Glow = false,
						EntryUnlocked = bandHasEntry,
						Thickness = thicknessToSet,
						Subsets = new List<GraphManager.GraphEntryData.Subset> {
						new GraphManager.GraphEntryData.Subset(kvp.Key) {
							Values = (this.visualizationMode == VisualizationMode.Line ? kvp.Value.Cast<float?>().ToList() : new List<float?> { kvp.Value[this.pannedWeek] }),
						}
					},
					});

					if (this.visualizationMode == VisualizationMode.Line)
					{
						dataToGraph.Add(new GraphManager.GraphEntryData
						{
							Name = kvp.Key.Name + "_glow",
							Glow = true,
							EntryUnlocked = false,
							Subsets = new List<GraphManager.GraphEntryData.Subset> {
						new GraphManager.GraphEntryData.Subset(kvp.Key) {
							Values = (this.visualizationMode == VisualizationMode.Line ? kvp.Value.Cast<float?>().ToList() : new List<float?> { kvp.Value[this.pannedWeek] }),
						}
					},
						});
					}
				}
			}

			float axisMultiplier = GetAxisMultiplier(locationToGraph);
			this.SetupNewGraphView(dataToGraph, axisMultiplier);
		}

    }

	private float GetAxisMultiplier(StatSubType location)
	{
		if (location == StatSubType.NONE)
		{
			return 1f;
		}
		else
		{
			List<PopulationData> data = GameRefs.I.m_gameController.DataManager.GetPopulationData();

			for (int i = 0; i < data.Count; i++)
			{
				if (location == data[i].Location)
					return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.GraphYAxisMultiplier * data[i].GetPopulation() / 1000000f;
			}
		}
		return 1f;
	}

    private void OpenRecentSongGraph(List<Song> songs, Func<Song, int, StatSubType, float> functionToGraph, bool cumulative)
    {
        if (cumulative)
        {
            this.minKey = 0;
            this.maxKey = this.timeRange;
        }

        Dictionary<Song, GraphManager.GraphEntryData.Subset> dataDict = new Dictionary<Song, GraphManager.GraphEntryData.Subset>();

        foreach (Song song in songs.Where(x =>
            (x.Artist.GetGenre() == this.genreToGraph || this.genreToGraph == StatSubType.NONE) &&
            (x.Stats.Contains(this.secondaryFilterToGraph) || this.secondaryFilterToGraph == StatSubType.NONE))
            .OrderByDescending(x => x.TurnOfCreation).Take(6))
        {
            dataDict[song] = new GraphEntryData.Subset(song.Artist.GetGenre());
        }

        if (dataDict.Count == 0)
        {
            this.OnNoApplicableSongs();
            return;
        }

        for (int i = this.GetMinKey(); i < this.GetMaxKey(); i++)
        {
            foreach (KeyValuePair<Song, GraphManager.GraphEntryData.Subset> kvp in dataDict)
            {
                StatSubType location = this.locationToGraph;

                bool indexIsValid;

                if (cumulative)
                {
                    indexIsValid = i < kvp.Key.TurnData.Count;
                }
                else
                {
                    indexIsValid = i - kvp.Key.TurnOfCreation >= 0 && i - kvp.Key.TurnOfCreation < kvp.Key.TurnData.Count;
                }

                if (indexIsValid)
                {
                    kvp.Value.Values.Add(functionToGraph(kvp.Key, cumulative ? i : i - kvp.Key.TurnOfCreation, location));
                }
                else
                {
                    kvp.Value.Values.Add(null);
                }
            }
        }

        List<GraphManager.GraphEntryData> dataToGraph = new List<GraphManager.GraphEntryData>();

        foreach (KeyValuePair<Song, GraphManager.GraphEntryData.Subset> kvp in dataDict)
        {
            if (kvp.Value.Values.Count > 0)
            {
				dataToGraph.Add(new GraphManager.GraphEntryData
				{
					Name = kvp.Key.Name,
					Glow = false,
					Thickness = 3,
					Subsets = new List<GraphManager.GraphEntryData.Subset> { kvp.Value },
                    Offset = this.graphMode == GraphMode.CumulativeSales ? 0 : this.GetMinKey(),
                });
            }
        }

        this.SetupNewGraphView(dataToGraph);
    }

    private void OpenSongGraph(List<Song> songs, Func<Song, int, StatSubType, float> functionToGraph)
    {
        StatType secondaryStatFilter = this.GetSecondaryStatFilter();

        StatType primaryStatFilter = this.typeToGraph;

        Dictionary<StatSubType, Dictionary<StatSubType, GraphManager.GraphEntryData.Subset>> dataDict = new Dictionary<StatSubType, Dictionary<StatSubType, GraphEntryData.Subset>>();

        foreach (StatSubType subType in StatSubType.GetFilteredList(primaryStatFilter.ID, false))
        {
            dataDict[subType] = new Dictionary<StatSubType, GraphEntryData.Subset>();

            if (secondaryStatFilter == StatType.NONE || primaryStatFilter == secondaryStatFilter)
            {
                dataDict[subType][StatSubType.NONE] = new GraphEntryData.Subset(subType);
            }
            else
            {
                foreach (StatSubType secondarySubType in StatSubType.GetFilteredList(secondaryStatFilter.ID, false))
                {
                    dataDict[subType][secondarySubType] = new GraphEntryData.Subset(secondarySubType);
                }
            }
        }

        bool valueFound = false;

        for (int i = this.GetMinKey(); i < this.GetMaxKey(); i++)
        {
            foreach (KeyValuePair<StatSubType, Dictionary<StatSubType, GraphManager.GraphEntryData.Subset>> kvp in dataDict)
            {
                foreach (KeyValuePair<StatSubType, GraphManager.GraphEntryData.Subset> kvp2 in kvp.Value)
                {
                    StatSubType location = this.locationToGraph;

                    if (kvp.Key.SuperType == StatType.LOCATION)
                    {
                        location = kvp.Key;
                    }
                    else if (kvp2.Key.SuperType == StatType.LOCATION)
                    {
                        location = kvp2.Key;
                    }

                    float? value = this.GetAggregateSongData(songs,
                        (Song s) =>
                        {
                        return
                            (s.Stats.Contains(kvp.Key) || kvp.Key.SuperType == StatType.LOCATION) &&
                            (s.Stats.Contains(kvp2.Key) || kvp2.Key == StatSubType.NONE || kvp2.Key.SuperType == StatType.LOCATION) &&
                            (s.TurnOfCreation <= i && i - s.TurnOfCreation < s.TurnData.Count);
                        },  
                        (Song s) => { return functionToGraph(s, i - s.TurnOfCreation, location); });

                    valueFound |= value.HasValue;

                    kvp2.Value.Values.Add(value);
                }
            }
        }

        if (!valueFound)
        {
            this.OnNoApplicableSongs();
            return;
        }

        List<GraphManager.GraphEntryData> dataToGraph = new List<GraphManager.GraphEntryData>();

        foreach (KeyValuePair<StatSubType, Dictionary<StatSubType, GraphManager.GraphEntryData.Subset>> kvp in dataDict)
        {
            if (kvp.Value.Values.Count > 0 && kvp.Value.Values.Any(x => x.Values.Count > 0))
            {
                dataToGraph.Add(new GraphManager.GraphEntryData
                {
                    Name = kvp.Key.Name,
                    Subsets = kvp.Value.Values.Where(x => x.Values.Count > 0).ToList(),
                    Offset = this.GetMinKey(),
                });
            }
        }

        this.SetupNewGraphView(dataToGraph);
    }

    private void OpenAppealGraph(List<Song> songs)
    {
        this.OpenSongGraph(songs, (Song s, int x, StatSubType loc) => { return s.GetTurnAppeal(x, loc); });
    }

    private void OpenSalesGraph(List<Song> songs)
    {
        this.OpenRecentSongGraph(songs, (Song s, int x, StatSubType loc) => { return s.GetTurnSales(x, loc); }, false);
    }

    private void OpenListensGraph(List<Song> songs)
    {
        this.OpenRecentSongGraph(songs, (Song s, int x, StatSubType loc) => { return s.GetTurnListens(x, loc); }, false);
    }

    private void OpenCumulativeSalesGraph(List<Song> songs)
    {
        this.OpenRecentSongGraph(songs, (Song s, int x, StatSubType loc) => { return s.GetCumulativeTurnSales(x, loc); }, true);
    }

    private void OpenFollowersGraph(Dictionary<StatSubType, List<float>> followerData)
    {
        List<GraphManager.GraphEntryData> dataToGraph = new List<GraphManager.GraphEntryData>();

        foreach (KeyValuePair<StatSubType, List<float>> data in followerData)
        {
            dataToGraph.Add(new GraphManager.GraphEntryData
            {
                Name = data.Key.Name,
                Subsets = new List<GraphManager.GraphEntryData.Subset> {
                        new GraphManager.GraphEntryData.Subset(data.Key) {
                            Values = data.Value.Cast<float?>().ToList(),
                        }
                    },
            });
        }

        this.SetupNewGraphView(dataToGraph);
    }

    private float? GetAggregateSongData(List<Song> songs, Func<Song, bool> qualifier, Func<Song, float> value)
    {
        float sum = 0;
        int qualifyingSongs = 0;

        foreach (Song song in songs.Where(x => qualifier(x)))
        {
            qualifyingSongs += 1;
            sum += value(song);
        }

        if (qualifyingSongs == 0)
        {
            return null;
        }
        else
        {
            return sum / qualifyingSongs;
        }
    }

    public void OnTimeDropdownChanged(Dropdown change)
    {
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ToggleBetweenOptions);
		switch (change.value)
        {
            case 0:
                this.timeRange = 6;
				GameRefs.I.PostGameState(false, "setTimeDropdown", "5Weeks");
				break;
            case 1:
                this.timeRange = 16;
				GameRefs.I.PostGameState(false, "setTimeDropdown", "15Weeks");
				break;
            case 2:
				GameRefs.I.PostGameState(false, "setTimeDropdown", "30Weeks");
				this.timeRange = 31;
                break;
        }

        this.UpdateKeyRange();

        this.RefreshGraph();
    }

	private int GetNextSampledWeek(int from)
	{
		int week = from + 1;
		while (week <= Mathf.Min(currentTurn-1, 30) && !HasSample(week))
		{
			++week;
		}
		return week;
	}

	private int GetPreviousSampledWeek(int from)
	{
		int week = from - 1;
		while (week >= 0 && !HasSample(week)) {
			--week;
		}
		return week;
	}

    private void UpdateKeyRange()
    {
        this.minKey = Mathf.Max(Mathf.Min(this.currentTurn, 31) - this.timeRange, 0);
        this.maxKey = Mathf.Max(Mathf.Min(this.currentTurn, 31), this.timeRange);
        this.ChangePannedWeek(GetPreviousSampledWeek(currentTurn > 31 ? 31 : currentTurn));
    }

    private void UpdateDropdowns()
    {
        this.TimeDropdownRoot.gameObject.SetActive(this.visualizationMode == VisualizationMode.Line);
        this.WeekPanRoot.gameObject.SetActive(this.visualizationMode != VisualizationMode.Line);
        this.XAxisLabel.SetActive(this.visualizationMode == VisualizationMode.Line);
    }

	public void SetupNewHeatGraphView(DataSimulationManager data)
	{
		lineBar.SetActive(false);
		this.LineGraph.gameObject.SetActive(false);
		this.BarGraph.gameObject.SetActive(false);
		this.HeatGraph.gameObject.SetActive(true);
		toggleSubheader.gameObject.SetActive(false);
		locationToggles.SetActive(false);
		StatSubType subTypeToGraph = StatSubType.NONE;
		if (typeToGraph == StatType.GENRE)
		{
			genreToggles.gameObject.SetActive(true);
			moodToggles.gameObject.SetActive(false);
			topicToggles.gameObject.SetActive(false);
			subTypeToGraph = genreToGraph;
		}
		else if (typeToGraph == StatType.MOOD)
		{
			genreToggles.gameObject.SetActive(false);
			moodToggles.gameObject.SetActive(true);
			topicToggles.gameObject.SetActive(false);
			subTypeToGraph = moodToGraph;
		}
		else
		{
			genreToggles.gameObject.SetActive(false);
			moodToggles.gameObject.SetActive(false);
			topicToggles.gameObject.SetActive(true);
			subTypeToGraph = topicToGraph;
		}

		this.HeatGraph.SetupNewGraphView(data, subTypeToGraph, locationToGraph, this.pannedWeek);
	}
    public void SetupNewGraphView(List<GraphManager.GraphEntryData> values, float axisMultiplier=1f)
    {
        this.OnValidGraph();
		toggleSubheader.gameObject.SetActive(true);
		switch (this.visualizationMode)
        {
            case VisualizationMode.Line:
				lineBar.SetActive(true);
				this.LineGraph.gameObject.SetActive(true);
                this.BarGraph.gameObject.SetActive(false);
				this.HeatGraph.gameObject.SetActive(false);
				locationToggles.SetActive(true);
				genreToggles.gameObject.SetActive(false);
				moodToggles.gameObject.SetActive(false);
				topicToggles.gameObject.SetActive(false);
				this.xValuesDisplayed = Mathf.Min(this.timeRange, this.XValueLabels.Count);
                this.LineGraph.SetupNewGraphView(values, this.minKey, this.maxKey, this.graphMode == GraphMode.Preferences || this.graphMode == GraphMode.SongPreference, null, axisMultiplier);
                break;
            case VisualizationMode.Bar:
				lineBar.SetActive(true);
				this.LineGraph.gameObject.SetActive(false);
                this.BarGraph.gameObject.SetActive(true);
				this.HeatGraph.gameObject.SetActive(false);
				locationToggles.SetActive(true);
				genreToggles.gameObject.SetActive(false);
				moodToggles.gameObject.SetActive(false);
				topicToggles.gameObject.SetActive(false);
				this.xValuesDisplayed = values.Count;
                this.BarGraph.SetupNewGraphView(values, this.pannedWeek, this.graphMode == GraphMode.Preferences || this.graphMode == GraphMode.SongPreference, axisMultiplier);
                break;
        }

        this.cachedResolution = Vector2.zero;
    }

    private int GetMinKey()
    {
        if (this.visualizationMode == VisualizationMode.Line)
        {
            return this.minKey;
        }
        else
        {
            return this.pannedWeek;
        }
    }

    private int GetMaxKey()
    {
        if (this.visualizationMode == VisualizationMode.Line)
        {
            return this.maxKey;
        }
        else
        {
            return this.pannedWeek + 1;
        }
    }

    public void OnWeekPanBack()
    {
        this.ChangePannedWeek(GetPreviousSampledWeek(pannedWeek));
    }

    public void OnWeekPanForward()
    {
        this.ChangePannedWeek(GetNextSampledWeek(pannedWeek));
    }

    public void PanToFront()
    {
        this.ChangePannedWeek(30);
    }

    public void OnWeekInputChanged(InputField input)
    {
        int inputValue;

        if (int.TryParse(input.text, out inputValue))
        {
            inputValue -= 1;
            inputValue = Mathf.Clamp(inputValue, 0, this.currentTurn - 1);
            input.text = inputValue.ToString();
            this.ChangePannedWeek(inputValue);
        }
        else
        {
            input.text = this.pannedWeek.ToString();
        }
    }

    private void ChangePannedWeek(int value)
    {
        value = Mathf.Clamp(value, 0, 30);

        this.WeekPanBackButton.gameObject.SetActive(GetPreviousSampledWeek(value) >= 0 && (isTraitCaredAbout || visualizationMode == VisualizationMode.Heatmap));
        this.WeekPanForwardButton.gameObject.SetActive(GetNextSampledWeek(value) <= Mathf.Min(currentTurn - 1, 30) && (isTraitCaredAbout || visualizationMode == VisualizationMode.Heatmap));

        this.pannedWeek = value;

        this.RefreshGraph();
		marketingInsights.RefreshInsightIcons();
	}

    private StatSubType GetLocationFilter()
    {
        if (this.visualizationMode == VisualizationMode.StackedBar)
        {
            return StatSubType.NONE;
        }
        else
        {
            return this.locationToGraph;
        }
    }

    public StatType GetPrimaryStatFilter()
    {
		return this.typeToGraph;
    }

    private StatType GetSecondaryStatFilter()
    {
        if (this.visualizationMode == VisualizationMode.StackedBar)
        {
            return this.secondaryTypeToGraph;
        }
        else
        {
            return StatType.NONE;
        }
    }

    private void OnValidGraph()
    {
        if (this.viewMode == ViewMode.Query)
        {
            //this.Animator.SetInteger("Screen", (int)this.viewMode);
            this.NoQueryLabel.text = "Create a query";
        }
    }

    private void OnNoApplicableSongs()
    {
        //this.Animator.SetInteger("Screen", (int)ViewMode.Waiting);
        this.NoQueryLabel.text = "Try recording a song to see data";

        bool missingGenre = this.genreToGraph != StatSubType.NONE;
        bool missingLocation = this.locationToGraph != StatSubType.NONE;

        switch (this.graphMode)
        {
            case GraphMode.Sales:
            case GraphMode.CumulativeSales:
            case GraphMode.SongPreference:
                this.NoQueryLabel.text = string.Format("Try recording {0} song {1}to see data", missingGenre ? "a " + this.genreToGraph.Name : "a", missingLocation ? "in " + this.locationToGraph.Name + " " : "");
                break;
            case GraphMode.ArtistFollowers:
                break;
        }
    }
}
