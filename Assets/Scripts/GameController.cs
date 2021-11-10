#define ENABLE_PROFILER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;
using Utility;
using BeauRoutine;
using UnityEngine.Profiling;

public class GameController : MonoBehaviour
{
	[Header("Misc Data")]
	public TextAsset TextKVPJSON;
	public DataSimulationManager DataManager;
	public GameOverControl GameOverControl;
	public SongReleaseVariables SongReleaseVariables;
	public BandTierList BandTierList;
	public GraphicRaycaster MainCanvasRaycaster;
	public List<MarketingInsightObject> marketingInsightList;
	public List<MarketingInsightObject> previousMarketingInsights;
	public SaveGameController Saves;
	public TutorialController Tutorial;
	public int currentTurn;

	[Header("UI Views")]
	public Release SongManager;
	public GraphManager Graph;
	public SongResultsView SongResultsView;
	public ArtistSigningView signingManager;
	public MarketingView MarketingView;
	public MarketingInsights MarketingInsights;
	public GameInitializationVariables GameInitializationVariables;
	public CurrentCashTooltipControl CashTooltip;
	public RecordingSlotsTooltipControl RecordingTooltip;
	public TopChartsView TopCharts;
	public TopChartsHudControl TopChartsHud;

	[Header("Office Stuff")]
	public GameObject loadingIndicator;
	public Transform SongViewRoot;
	public GameObject Furniture;
	public NotificationButton NewArtistNotification;
	public TextMeshProUGUI NewArtistCountLabel;
	public NotificationButton SongsReadyNotification;
	public TextMeshProUGUI SongsReadyCountLabel;
	public Image nextTurnBlocker;
	public Image nextTurnTextImage;
	public Image recordSongTextImage;
	public Image releaseSongTextImage;
	public Button recordingRoom;
	public Button marketingRoom;
	public GameObject[] platinumRoot;
	public GameObject[] goldRoot;
	public GameObject studioViewTutorialMount;

	[System.Serializable]
	public class StaffMember {
		public Animator animator;
		public SkinnedMeshRenderer renderer;
	}

	[Header("Office Models")]
	public StaffMember[] staffMembers;
	public OfficeParameters parameters;
	public GameObject[] recordingModels;
	public Renderer floor;
	public GameObject marketingTutorialGuyNormal;

	[Header("HUD Stuff")]
	public TextMeshProUGUI CashCounter;
	public TextMeshProUGUI CashFlowCounter;
	public TextMeshProUGUI RecordingCounter;
	public TextMeshProUGUI TurnCounter;
	public TextMeshProUGUI FollowerCounter;
	public GameObject ScreenNameBackground;
	public Button GoToTrendsButton;
	public Button NextTurnButton;
	public GameObject NextTurnTextImage;
	public Image NextTurnButtonColor;
	public Color NextTurnNormal;
	public Color NextTurnRelease;
	public Color NextTurnRecord;
	public Image muteStatus;
	public Sprite muteIsMuted;
	public Sprite muteIsPlaying;
	public GameObject lostConnectionOverlay;

	public RectTransform HeaderSwing;
	public RectTransform TooltipSwing;
	public float hudSwingSpeed = 0.2f;
	public float hudSwingDistance = -252f;
	public Curve hudSwingCurve = Curve.BackInOut;

	private bool muted = false;
	private List<Band> signedBands;
	private List<Song> songs = new List<Song>();
	private float cash;
	private float lastCashChange;
	private float thisTurnListens;
	private float thisTurnIncome;
	private float thisTurnExpenses;
	private List<Song> recordingsInProgress;
	private List<Band> availableBands;
	private Action lastViewFunction;
	private ViewMode viewMode;
	private Routine swingRoutine;
	private Routine nextTurnRoutine;
	private Coroutine[] twitchTasks;
	private Routine loadRoutine;

	public enum NextTurnState
	{
		Normal,
		NeedToRelease,
		NeedToRecord,
		NeedToSign
	}

	public enum ViewMode
	{
		Graph,
		Songs,
		Studio,
		Signing,
		Marketing,
		TopCharts,
		Results
	}

	[System.Serializable]
	private class JSONKVP
	{
		public string Key; 
		public string Value;

		public static JSONKVP CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<JSONKVP>(jsonString);
		}
	}

	[System.Serializable]
	private class JSONKVPList
	{
		public JSONKVP[] KVPs;

		public static JSONKVPList CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<JSONKVPList>(jsonString);
		}
	}

	private void Awake()
	{
		this.BandTierList.Init();
		twitchTasks = new Coroutine[5];

		marketingInsightList = new List<MarketingInsightObject>();
		//Band newBand = this.GenerateBand(i);

		this.availableBands = new List<Band>();

		this.RefreshAvailableBands();
		this.SongManager.Init();
		this.signingManager.Init();

		this.Graph.gameObject.SetActive(true);

		JSONKVPList kvps = JsonUtility.FromJson<JSONKVPList>("{\"KVPs\" : " + this.TextKVPJSON.text + "}");

		InterceptedText.TextKVPs = new Dictionary<string, string>();

		foreach (JSONKVP kvp in kvps.KVPs)
		{
			InterceptedText.TextKVPs["[" + kvp.Key + "]"] = kvp.Value;
		}

		foreach (TextMeshProUGUI textElement in FindObjectsOfType<TextMeshProUGUI>())
		{
			textElement.SetIText(textElement.text);
		}
	}

	private void Start()
	{
		this.DataManager.Init();
		this.recordingsInProgress = new List<Song>();
		this.signedBands = new List<Band>();
		GameRefs.I.m_globalLastScreen = "mainScreen";

		foreach (int bandPoints in this.GameInitializationVariables.DebugBandPoints)
		{
			Band newBand = new Band(this.BandTierList.GetRandomBandTemplate(StatSubType.NONE), bandPoints, GameRefs.I.bandGenerators);
			//newBand.Name += string.Format("-{0} Points", bandPoints);
			newBand.BaseCost = BandTierList.BaseBandCost;
			newBand.CostPerPoint = BandTierList.AdditionalBandCostPerPoint;
			newBand.CostMultiplierPerPoint = BandTierList.AdditionalBandCostMultiplierPerPoint;
			newBand.IsSigned = true;
			this.signedBands.Add(newBand);
		}

		this.DataManager.GeneratePeople(50);
		this.DataManager.CollapsePopulationData();

		this.cash = this.GameInitializationVariables.StartingCash;

		//this.OnTurnAdvanced();

		RecordingBand = null;
		this.OpenStudioView();

		AdvanceXTurns(GameInitializationVariables.StartingWeeks);

		for (int i = 0; i < staffMembers.Length; ++i)
		{
			FidgetStaff(i);
			staffMembers[i].animator.gameObject.SetActive(i < 2);
			staffMembers[i].renderer.material.SetTexture("_MainTex", parameters.staffMemberConfigurators[i].skin);
		}

		UpdateWall();
		GameRefs.I.PostGameState(true, "autoEvent", "startedGame");

		if (GameRefs.I.sharedParameters.isMuted)
		{
			ToggleMute();
		}

		if (GameRefs.I.sharedParameters.skipTutorials)
		{
			GameRefs.I.m_tutorialController.MarkAllCompleted();
		}

		if(!PlayerInformation.isNewGame)
		{
			Saves.LoadGameState();
		}
		else
		{
			Saves.SaveGameState(false);
		}

		if(PlayerInformation.isMuted)
		{
			ToggleMute();
		}

		StartCoroutine(CoLoadSession());
	}

	private IEnumerator CoLoadSession()
	{
		yield return Footilities.CoLoadSessionManager();

		// If the connection went down in the title screen, we won't have
		// received an alert from Javascript and thus won't see the lost
		// connection dialog. So, we manually check.
		if (!SessionController.S.isConnected)
		{
			IndicateDisconnect();
		}

		SessionController.S.AddListener(isConnected => {
			if (!isConnected)
			{
				IndicateDisconnect();
			}
		});
	}

	public void UpdateLoadedStuffToHud()
	{
		loadRoutine.Replace(this, LoadInSavedData());
	}

	private IEnumerator LoadInSavedData()
	{
		yield return 0.1f;
		for (int i = 0; i < signedBands.Count; i++)
		{
			if(signedBands[i].IsRecordingSong())
			{
				RecordingBand = signedBands[i];
				GameRefs.I.m_recordingAudio.AddSongToQueue(signedBands[i].GetRecordingSong());
			}
		}
		this.RecordingCounter.text = string.Format("{0}/3", this.recordingsInProgress.Count);
		this.CashCounter.text = string.Format("${0:#,##0}", this.cash);
		GameRefs.I.cashTooltipValues.SetDataInstant(GameRefs.I.cashTooltipValues.saveBandUpkeep, GameRefs.I.cashTooltipValues.saveCashFromSongs,
													GameRefs.I.cashTooltipValues.saveCashFromListens, GameRefs.I.cashTooltipValues.saveStorageCosts);
		GameRefs.I.recordingTooltipValues.UpdateSlotsInstant();

		this.TurnCounter.text = string.Format("{0}", this.currentTurn - 30);
		UpdateFollowerCount();
		CheckNextTurnButtonState();
		RefreshNewArtistNotification(); 
		RefreshSongReadyNotification();
		TopChartsHud.UpdateCircles();
		Tutorial.SpawnFirstIncompleteTutorial();
		GameRefs.I.m_gameController.MainCanvasRaycaster.enabled = true;
	}

	public void UnlockRecording()
	{
		recordingRoom.interactable = true;				
		staffMembers[2].animator.gameObject.SetActive(true);
		staffMembers[2].animator.SetBool("IsBored", true);
	}

	public void UnlockMarketing()
	{
		marketingRoom.interactable = true;
		staffMembers[3].animator.gameObject.SetActive(true);
	}

	private void FidgetStaff(int i)
	{
		staffMembers[i].animator.keepAnimatorControllerStateOnDisable = true;
		StartCoroutine(CoFidgetStaff(i));
	}

	private IEnumerator CoFidgetStaff(int i)
	{
		OfficeParameters.StaffMemberConfigurator configurator = parameters.staffMemberConfigurators[i];
		while (true)
		{
			float delay = UnityEngine.Random.Range(configurator.minTimeBetweenFidgets, configurator.maxTimeBetweenFidgets);
			yield return new WaitForSeconds(delay);
			staffMembers[i].animator.SetTrigger("Fidget");
		}
	}

	public void OnNextTurnButtonClicked()
	{
		if (GetNextTurnState() == NextTurnState.NeedToRelease)
		{
			GameRefs.I.PostGameState(false, "clickedButton", "nextWeekRecording");
			OpenSongView();
		}
		else if (GetNextTurnState() == NextTurnState.NeedToRecord)
		{
			GameRefs.I.PostGameState(false, "clickedButton", "nextWeekRecording");
			OpenSongView();
		}
		else
		{
			EnableTrendsButton(false);
			NextTurnButton.interactable = false;
			NextTurnTextImage.SetActive(false);
			loadingIndicator.SetActive(true);
			GameRefs.I.PostGameState(true, "clickedButton", "NextWeekButton");
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.NextWeekReady);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.NextWeekClicked);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.StudioFloorFinal);
			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.NoArtistUpgradesAfterXWeeks);
			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.CanUnlockBoroughAfterYWeeks);
			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.UnlockedNewBorough);
			this.OnTurnAdvanced();
		}
	}

	public void AdvanceXTurns(int numTurns)
	{
		for (int i = 0; i < numTurns; i++)
			OnTurnAdvanced(true);

		this.availableBands.Clear();

		List<StatSubType> initGenres = new List<StatSubType>();
		initGenres.Add(StatSubType.ROCK); initGenres.Add(StatSubType.RAP); initGenres.Add(StatSubType.RANDB);
		initGenres.Add(StatSubType.HIP_HOP); initGenres.Add(StatSubType.POP); initGenres.Add(StatSubType.ELECTRONIC);
		for(int i = 0; i < BandTierList.NumStartingBands; i++)
		{
			bool bandAdded = false;
			while(!bandAdded)
			{
				// Attempt to get at least one band from each genre before repeats
				StatSubType initBandGenre = StatSubType.NONE;
				if (initGenres.Count > 0)
					initBandGenre = initGenres[UnityEngine.Random.Range(0, initGenres.Count)];

				Band initBand = GenerateBand(0, initBandGenre);
				if (!availableBands.Contains(initBand))
				{
					initGenres.Remove(initBandGenre);
					bandAdded = true;
					initBand.IsNew = true;
					availableBands.Add(initBand);
				}
			}
		}
		this.RefreshNewArtistNotification();
	}

	public void CheckNextTurnButtonState()
	{
		NextTurnButton.interactable = true;
		loadingIndicator.SetActive(false);

		if (GetNextTurnState() == NextTurnState.NeedToRelease)
		{
			NextTurnTextImage.SetActive(false);
			releaseSongTextImage.gameObject.SetActive(true);
			recordSongTextImage.gameObject.SetActive(false);
			NextTurnButtonColor.color = NextTurnRelease;
		}
		else if(GetNextTurnState() == NextTurnState.NeedToRecord)
		{
			NextTurnTextImage.SetActive(false);
			releaseSongTextImage.gameObject.SetActive(false);
			recordSongTextImage.gameObject.SetActive(true);
			NextTurnButtonColor.color = NextTurnRecord;
		}
		else
		{
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.StartRecording);
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.NextWeekReady);
			NextTurnTextImage.SetActive(true);
			releaseSongTextImage.gameObject.SetActive(false);
			recordSongTextImage.gameObject.SetActive(false);
			NextTurnButtonColor.color = NextTurnNormal;
		}
	}

	public Color GetNextTurnColor()
	{
		if (GetNextTurnState() == NextTurnState.NeedToRelease)
		{
			return NextTurnRelease;
		}
		else if (GetNextTurnState() == NextTurnState.NeedToRecord)
		{
			return NextTurnRecord;
		}
		else
		{
			return NextTurnNormal;
		}
	}

	private NextTurnState GetNextTurnState()
	{
		foreach (Song s in GetRecordingSongs())
		{
			if (s.DoneRecording)
			{
				return NextTurnState.NeedToRelease;
			}
		}

		if (signedBands.Count == 0)
			return NextTurnState.NeedToSign;

		if(GetNumRecordings() == Math.Min(3, signedBands.Count))
			return NextTurnState.Normal;

		return NextTurnState.NeedToRecord;
	}

	public void OnTurnAdvanced(bool bypassCashReduction = false)
	{
		if (!bypassCashReduction && signedBands.Count == 0)
		{
			return;
		}

		bool skipHudUpdate = false;

		this.currentTurn += 1;
		this.TurnCounter.text = string.Format("{0}", this.currentTurn - 30);

		int totalSales;
		int totalListens;
		float oldCash = cash;

		RecordingTooltip.UpdateTooltipSlots();

		this.ProcessNextSongListens(out totalSales, out totalListens);
		thisTurnListens = totalListens;
		int songIncome = Mathf.RoundToInt((float)totalSales * this.DataManager.DataSimulationVariables.CashPerSale + (float)totalListens * this.DataManager.DataSimulationVariables.CashPerListen);


		if (!bypassCashReduction)
		{
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.AdvanceWeek);

			if (this.currentTurn > GameInitializationVariables.StartingWeeks)
			{
				this.cash += this.ProcessAndGetCashChange(songIncome);
			}
			else
			{
				this.ProcessAndGetCashChange(songIncome);
				this.RecordingCounter.text = string.Format("{0}/3", this.recordingsInProgress.Count);
				this.CashCounter.text = string.Format("${0:#,##0}", this.cash);
			}
		}
		else
		{
			this.RecordingCounter.text = string.Format("{0}/3", this.recordingsInProgress.Count);
			this.CashCounter.text = string.Format("${0:#,##0}", this.cash);
			skipHudUpdate = true;
		}

		this.DataManager.IncrementTurn(1);

		foreach (Song song in this.recordingsInProgress)
		{
			song.TurnsRecorded += 1;

			if (song.TurnsRecorded > song.GetMaximumRecordingTurns())
			{
				song.TurnsRecorded = song.GetMaximumRecordingTurns();
			}
			else
			{
				int rollAttempts = Mathf.FloorToInt(song.TurnsRecorded * (1 + this.SongReleaseVariables.ExtraStatRollsPerSpeed * song.Artist.SpeedScore)) - song.RollsMade;

				for (int i = 0; i < rollAttempts; i++)
				{
					song.RollForStats(this.SongReleaseVariables);
				}
			}

			int minimumRecordingTurns = this.SongReleaseVariables.MinimumTurns;

			if (song.TurnsRecorded >= minimumRecordingTurns)
			{
				song.ReadyToRelease = true;
			}

			if (song.TurnsRecorded >= song.GetMaximumRecordingTurns() || song.Quality >= 14)
			{
				song.DoneRecording = true;
			}
		}

		this.DataManager.CommitFollowers();

		this.Graph.UpdateGraphTurn(this.currentTurn);

		if (SongViewRoot.gameObject.activeInHierarchy)
		{
			this.SongManager.OnSongRecordBegun();
		}

		foreach (Band band in this.availableBands)
		{
			band.TurnsLeft -= 1;

			if (band.TurnsLeft <= 0 && !this.signedBands.Contains(band))
			{
				band.IsSigned = false;
			}
		}

		this.availableBands.RemoveAll(x => x.TurnsLeft <= 0);
		this.RefreshAvailableBands();
		this.RefreshSongReadyNotification();


		if (MarketingInsights.unconfirmedInsights.Count > 0)
		{
			// toRemove: remove this from the list after we're done iterating over it if we found something
			List<MarketingInsightObject> toRemove = new List<MarketingInsightObject>();
			for (int i = 0; i < MarketingInsights.unconfirmedInsights.Count; i++)
			{
				if (MarketingInsights.unconfirmedInsights[i].songAttached == null || MarketingInsights.unconfirmedInsights[i].songAttached.Name == null)
					toRemove.Add(MarketingInsights.unconfirmedInsights[i]);
			}
			if (toRemove.Count > 0)
			{
				foreach(MarketingInsightObject obj in toRemove)
						MarketingInsights.unconfirmedInsights.Remove(obj);
			}
		}

		//CheckGameOver();
		if (CheckGameOver())
			skipHudUpdate = true;

		if(!skipHudUpdate)
			nextTurnRoutine.Replace(this, HUDUpdateRoutine(oldCash, songIncome));
		else
			CheckNextTurnButtonState();

		this.UpdateFollowerCount();
	}

	private IEnumerator HUDUpdateRoutine(float oldCash, float cashFromSongs)
	{
		MainCanvasRaycaster.enabled = false;
		NewArtistNotification.gameObject.SetActive(false);
		SongsReadyNotification.gameObject.SetActive(false);
		yield return nextTurnBlocker.FadeTo(0.6f, 0.15f);

		// Show Cash tooltip and update
		float totalUpkeep = 0f;
		foreach (Band band in this.signedBands)
		{
			totalUpkeep -= band.UpkeepCost;
			this.lastCashChange -= band.UpkeepCost;
		}
		this.lastCashChange += cashFromSongs;
		float cashFromListens = thisTurnListens * this.DataManager.DataSimulationVariables.CashPerListen;
		
		yield return CashTooltip.SetData(totalUpkeep, cashFromSongs - cashFromListens, cashFromListens, oldCash, -GameRefs.I.m_dataCollect.GetWeeklyCost());
		yield return 0.1f;

		// Show recording tooltip and update
		yield return RecordingTooltip.AnimateTooltip();
		yield return 0.1f;

		yield return nextTurnBlocker.FadeTo(0f, 0.2f);
		// Animate in new artist icons
		NewArtistNotification.gameObject.SetActive(this.availableBands.Count(x => x.IsNew) > 0);
		if (this.availableBands.Count(x => x.IsNew) > 0)
		{
			NewArtistNotification.PopIn();
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.NotificationAppear);
		}
		yield return 0.15f;

		SongsReadyNotification.gameObject.SetActive(recordingsInProgress.Count(x => x.DoneRecording) > 0);
		if(recordingsInProgress.Count(x => x.DoneRecording) > 0)
		{
			SongsReadyNotification.PopIn();
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.NotificationDisappear);
		}
		

		// Tutorial stuff
		Tutorial.SpawnTutorial(TutorialController.TutorialID.NextWeekClicked);
		if(GetNextTurnState() == NextTurnState.NeedToRelease)
		{
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.FirstSongReady, TutorialController.TutorialAction.Confirm);
		}

		if (!signingManager.ArtistPanel.HasPlayerPurchasedUpgrade() &&  currentTurn > GameInitializationVariables.weeksBeforeRemindingUpgrades)
			Tutorial.SpawnTutorial(TutorialController.TutorialID.NoArtistUpgradesAfterXWeeks);
		if (MarketingView.GetNumBoroughsUnlocked() <= 2 && currentTurn > GameInitializationVariables.weeksBeforeRemindingBoroughs)
			Tutorial.SpawnTutorial(TutorialController.TutorialID.CanUnlockBoroughAfterYWeeks);
		if (!GameRefs.I.m_dataCollect.beenOpened && currentTurn > GameInitializationVariables.weeksBeforeRemindingDataCollect)
			Tutorial.SpawnTutorial(TutorialController.TutorialID.UnlockedNewBorough);

		// Save Game Date
		GameRefs.I.m_gameController.Saves.SaveGameState(false);
		EnableTrendsButton(true);

		isHudDone = true;
		CheckNextTurnButtonState();
		yield return 0.1f;
		MainCanvasRaycaster.enabled = true;
	}

	public void RecordAndRelease()
	{
		StartCoroutine(CoRecordAndRelease());
	}

	int iSong = 0;
	private IEnumerator CoRecordAndRelease()
	{
		OpenStudioView();
		yield return null;

		for (int i = 0; i < 3; ++i)
		{
			Band band = signedBands[i];
			if (!band.IsRecordingSong())
			{
				List<StatSubType> stats = new List<StatSubType>();
				stats.Add(band.GetGenre());
				stats.Add(StatSubType.MOOD1);
				stats.Add(StatSubType.TOPIC1);

				Song song = new Song()
				{
					Artist = band,
					Name = string.Format("Song {0}", iSong++),
					Stats = stats,
					TargetedLocation = StatSubType.TURTLE_HILL,
					TargetedLocationName = "Turtle Hill"
				};
				song.SetMaxRecordingTurns();
				band.AssignRecordingSong(song);
				OnSongCreated(band.GetRecordingSong());
				MarketingInsights.ClearSelectedEntry();
			}
		}

		while (signedBands.All(band => band.IsRecordingSong() && !band.GetRecordingSong().DoneRecording))
		{
			isHudDone = false;
			OnTurnAdvanced();
			yield return new WaitUntil(() => {
				return isHudDone;
			});
		}

		Band doneBand = signedBands.Find(band => band.IsRecordingSong() && band.GetRecordingSong().DoneRecording);
		SongManager.SelectBand(doneBand, false);
		OnReleaseSongButtonClicked();
	}

	public void ReleaseRandom()
	{
		StartCoroutine(CoReleaseRandom());
	}

	private IEnumerator CoReleaseRandom()
	{
		List<StatSubType> boroughs = StatSubType.GetFilteredList(StatType.LOCATION, false);
		foreach (StatSubType borough in boroughs)
		{
			foreach (MarketingInsights.InsightType type in Enum.GetValues(typeof(MarketingInsights.InsightType)))
			{
				foreach (StatSubType topic in StatSubType.GetFilteredList(StatType.TOPIC, false))
				{
					foreach (StatSubType mood in StatSubType.GetFilteredList(StatType.MOOD, false))
					{
						foreach (StatSubType genre in StatSubType.GetFilteredList(StatType.GENRE, false))
						{
							ReleaseFakeSong(genre, mood, topic, borough, type, mood);
							yield return null;
							ReleaseFakeSong(genre, mood, topic, borough, type, topic);
							yield return null;
							ReleaseFakeSong(genre, mood, topic, borough, type, genre);
							yield return null;
						}
					}
				}
			}
		}
	}

	private void ReleaseFakeSong(StatSubType genre, StatSubType mood, StatSubType topic, StatSubType borough, MarketingInsights.InsightType insightType, StatSubType insightStat)
	{
		Band band = new Band("Yeepers", genre);

		Song song = new Song()
		{
			Artist = band,
			Instrument = Band.Instrument.Guitar,
			Name = string.Format("Song {0}", 17),
			Stats = new List<StatSubType> { genre, mood, topic },
			TargetedLocation = borough,
			TargetedLocationName = borough.Name,
			Quality = 10,
			StarRating = Mathf.Max(1, 10 / 2),
		};

		recordingsInProgress.Add(song);

		while (!song.DoneRecording)
		{
			OnTurnAdvanced(true);
		}

		MarketingInsightObject insight = new MarketingInsightObject(insightType, insightStat, band, borough);
		insight.successful = IsInsightCorrect(insight);

		string message = SongResultsView.GetResponseSentence(song, borough, insight);
		Debug.LogFormat("{0}: {1}", "message", message);

		recordingsInProgress.Clear();
		MarketingInsights.unconfirmedInsights.Clear();
	}

	public void PerfectSignedBands()
	{
		foreach (Band band in signedBands)
		{
			band.Perfect();
		}
	}

	public void PrimeSongs()
	{
		Band band = new Band("Yeepers", StatSubType.ROCK);

		for (int i = 0; i < 50; ++i)
		{
			Song song = new Song()
			{
				Artist = band,
				Instrument = Band.Instrument.Guitar,
				Name = string.Format("Song {0}", i),
				Stats = new List<StatSubType> { StatSubType.ROCK, StatSubType.MOOD1, StatSubType.TOPIC1 },
				TargetedLocation = StatSubType.TURTLE_HILL,
				TargetedLocationName = "Turtle Hill",
				Quality = 10,
				StarRating = Mathf.Max(1, 10 / 2),
			};

			songs.Add(song);
		}
	}

	public void AutoNext()
	{
		StartCoroutine(CoAutoNext());
	}

	private bool isHudDone;
	private IEnumerator CoAutoNext()
	{
		while (true)
		{
			float startTime = Time.time;
			isHudDone = false;
			OnTurnAdvanced();
			yield return new WaitUntil(() => {
				return isHudDone;
			});
			float endTime = Time.time;
		}
	}

	public bool CheckGameOver()
	{
		TopChartsView.WinCondition gameOver = TopCharts.IsGameOver();
		if (gameOver != TopChartsView.WinCondition.NotWinner)
		{
			GameOverControl.EndGame(gameOver);
			return true;
		}
		else
			return false;
	}

	public List<Band> GetAvailableBands()
	{
		return availableBands;
	}

	private void ProcessNextSongListens(out int totalSales, out int totalListens)
	{
		totalSales = 0;
		totalListens = 0;
		DataManager.ProcessNextAllSongsSales(songs, ref totalSales, ref totalListens);
	}

	public void AddReleasedSongsToGame(List<Song.LoggedData> loadedSongs)
	{
		foreach (Song.LoggedData song in loadedSongs)
		{
			// band might have been fired after release is why this function exists
			Band thisBand = new Band(song.artistName, StatSubType.List[song.genreID]);

			Song newSong = new Song()
			{
				Artist = thisBand,
				Instrument = Band.Instrument.Guitar,
				Name = song.name,
				Stats = new List<StatSubType> { StatSubType.List[song.genreID], StatSubType.List[song.moodID], StatSubType.List[song.topicID] },
				TargetedLocation = StatSubType.GetTypeFromString(song.targetedLocation),
				TargetedLocationName = song.targetedLocation,
				Quality = song.Quality,
				TurnReleased = song.TurnReleased,
				StarRating = Mathf.Max(1, song.Quality / 2),
			};
			this.songs.Add(newSong);
		}
	}

	public void AddLoadedSongsToGame(List<Song.LoggedData> loadedSongs)
	{
		foreach(Song.LoggedData song in loadedSongs)
		{
			Band thisBand = null;

			foreach (Band b in signedBands)
			{
				if (b.Name == song.artistName)
				{
					thisBand = b;
					Song thisSong = new Song()
					{
						Artist = thisBand,
						Instrument = Band.Instrument.Guitar,
						Name = song.name,
						Stats = new List<StatSubType> { StatSubType.List[song.genreID], StatSubType.List[song.moodID], StatSubType.List[song.topicID] },
						TargetedLocation = StatSubType.GetTypeFromString(song.targetedLocation),
						TargetedLocationName = song.targetedLocation,
						TurnsRecorded = song.TurnsRecorded,
						NumRecordingTurns = song.totalRecordingTurns,
						DoneRecording = song.DoneRecording,
						TurnReleased = song.TurnReleased,
						Quality = song.Quality,
					};
					b.AssignRecordingSong(thisSong);
					recordingsInProgress.Add(thisSong);
				}
			}

			if(thisBand == null)
			{
				thisBand = new Band(song.artistName, StatSubType.List[song.genreID]);
			}

			Song newSong = new Song()
			{
				Artist = thisBand,
				Instrument = Band.Instrument.Guitar,
				Name = song.name,
				Stats = new List<StatSubType> { StatSubType.List[song.genreID], StatSubType.List[song.moodID], StatSubType.List[song.topicID] },
				TargetedLocation = StatSubType.GetTypeFromString(song.targetedLocation),
				TargetedLocationName = song.targetedLocation,
				TurnsRecorded = song.TurnsRecorded,
				TurnReleased = song.TurnReleased,
				NumRecordingTurns = song.totalRecordingTurns,
			};
		}
	}

	private float ProcessAndGetCashChange(float cashFromSongs)
	{
		this.lastCashChange = 0;
		this.thisTurnExpenses = 0;
		this.thisTurnIncome = 0;

		foreach (Band band in this.signedBands)
			this.lastCashChange -= band.UpkeepCost;

		this.lastCashChange -= GameRefs.I.m_dataCollect.GetWeeklyCost();
		this.lastCashChange += cashFromSongs;

		return this.lastCashChange;
	}

	private void RefreshHUDLabels()
	{
		this.RecordingCounter.text = string.Format("{0}/3", this.recordingsInProgress.Count);
		this.CashCounter.text = string.Format("${0:#,##0}", this.cash);
		//this.CashFlowCounter.SetIText(string.Format("{0}", Utilities.FormatNumberForDisplay(this.lastCashChange - this.thisTurnExpenses + this.thisTurnIncome)));

	}

	public void OpenStudioView()
	{
		SetBackToStudioButton(false);
		studioViewTutorialMount.SetActive(true);
		if (lastViewFunction == this.OpenMarketingView)
		{
			MarketingView.OnViewClosed();
		}
		TopChartsHud.gameObject.SetActive(true);
		TopChartsHud.UpdateCircles();
		TopCharts.gameObject.SetActive(false);
		CheckNextTurnButtonState();
		/* SongViewRoot.gameObject.SetActive(false); */
		SongResultsView.gameObject.SetActive(false);
		GoToTrendsButton.gameObject.SetActive(true);
		viewMode = ViewMode.Studio;
		GameRefs.I.m_recordingAudio.SetInStudio(true);
		ScreenNameBackground.SetActive(false);
		lastViewFunction = this.OpenStudioView;
		RefreshNewArtistNotification();
		RefreshSongReadyNotification();
		GameRefs.I.m_globalLastScreen = "mainScreen";
		Furniture.SetActive(true);
		Tutorial.SpawnTutorial(TutorialController.TutorialID.ArriveAtStudio);
		Tutorial.SpawnTutorial(TutorialController.TutorialID.ReturnToStudio);
		GameRefs.I.hudController.ToOfficeMode();
	}

	public void IndicateSave()
	{
		GameRefs.I.hudController.IndicateSave();
	}

	public void IndicateDisconnect()
	{
		lostConnectionOverlay.SetActive(true);
	}

	public void RefreshNewArtistNotification()
	{
		NewArtistNotification.gameObject.SetActive(this.availableBands.Count(x => x.IsNew) > 0);
		NewArtistCountLabel.text = this.availableBands.Count(x => x.IsNew).ToString();
	}

	public void RefreshSongReadyNotification()
	{
		SongsReadyNotification.gameObject.SetActive(this.recordingsInProgress.Count(x => x.DoneRecording) > 0);
		this.SongsReadyCountLabel.text = this.recordingsInProgress.Count(x => x.DoneRecording).ToString();
	}

	public void OpenGraphView()
	{
		ViewMode virtualViewMode = viewMode;
		if (TopCharts.gameObject.activeInHierarchy)
		{
			virtualViewMode = ViewMode.TopCharts;
		}
		studioViewTutorialMount.SetActive(false);
		this.Graph.OnOpen(GameRefs.I.m_lastLocationGraphed == null ? -1 : GameRefs.I.m_lastLocationGraphed.ID, virtualViewMode);
		Tutorial.SpawnTutorial(TutorialController.TutorialID.ForReferenceTrends);
		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.MarketingScreen);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.TopChartsView);
		this.GoToTrendsButton.gameObject.SetActive(false);
		marketingTutorialGuyNormal.SetActive(false);
		if (lastViewFunction == this.OpenSongView)
		{
			SongViewRoot.gameObject.SetActive(false);
		}
		else if(lastViewFunction == this.OpenSigningView)
		{
			signingManager.gameObject.SetActive(false);
		}
		this.ScreenNameBackground.SetActive(false);
		Furniture.SetActive(false);
		this.viewMode = ViewMode.Graph;
	}

	public void OpenSongView()
	{
		signingManager.Show(false);
		MarketingView.gameObject.SetActive(false);
		TopCharts.gameObject.SetActive(false);
		studioViewTutorialMount.SetActive(false);
		this.SongViewRoot.gameObject.SetActive(true);
		SetBackToStudioButton(true);
		GameRefs.I.m_recordingAudio.SetInStudio(false);
		this.SongResultsView.gameObject.SetActive(false);
		this.GoToTrendsButton.gameObject.SetActive(false);
		GameRefs.I.preserveLastSelected = false;

		// If top charts is closing, we can't do the whole slash-and-burn
		// redisplay of the recording view thing without obliterating any
		// prediction that hasn't been committed yet.
		if (viewMode == ViewMode.TopCharts)
		{
			SongViewRoot.gameObject.SetActive(true);
			SongManager.OnViewRestore();
		}
		else
		{
			this.SongManager.OnViewOpened(this.signedBands, GameRefs.I.preserveLastSelected, GetNextTurnState(), true);
		}

		if (!CheckGameOver())
		{
			this.NextTurnButton.gameObject.SetActive(true);
			this.ScreenNameBackground.SetActive(true);
			Furniture.SetActive(false);
			this.viewMode = ViewMode.Songs;
			this.lastViewFunction = this.OpenSongView;
			GameRefs.I.m_globalLastScreen = "recordingScreen";

			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.ReturnToStudio);
			Tutorial.SpawnTutorial(TutorialController.TutorialID.EnterRecording);

			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.FirstSongReady);
			Tutorial.SpawnTutorial(TutorialController.TutorialID.RecordingWhenReady);

			Tutorial.SetTutorialCompleted(TutorialController.TutorialID.StudioFloorFinal);
		}
	}

	public void OpenSigningView()
	{
		signingManager.Show(true);
		MarketingView.gameObject.SetActive(false);
		SetBackToStudioButton(true);
		TopCharts.gameObject.SetActive(false);
		this.SongViewRoot.gameObject.SetActive(false);
		this.SongResultsView.gameObject.SetActive(false);
		this.GoToTrendsButton.gameObject.SetActive(true);

		// TODO
		// Why the conditional?
		/* if (this.viewMode != ViewMode.Graph) */
		/* { */
			this.signingManager.OnViewOpened(this.signedBands, this.availableBands.Where(x => !x.IsSigned).ToList(), false);
		/* } */

		if(!NextTurnButton.gameObject.activeSelf)
		{
			if(GetNextTurnState() != NextTurnState.NeedToSign)
			{
				this.NextTurnButton.gameObject.SetActive(true);
				CheckNextTurnButtonState();
			}
		}
		studioViewTutorialMount.SetActive(false);
		this.ScreenNameBackground.SetActive(true);
		Furniture.SetActive(false);
		this.viewMode = ViewMode.Signing;
		this.lastViewFunction = this.OpenSigningView;
		GameRefs.I.m_globalLastScreen = "signingScreen";

		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.ArriveAtStudio);
		Tutorial.SpawnTutorial(TutorialController.TutorialID.EnterArtists);
		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.NoArtistUpgradesAfterXWeeks);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.StudioFloorFinal);
	}

	public void OpenMarketingView()
	{
		MarketingView.gameObject.SetActive(true);
		studioViewTutorialMount.SetActive(false);
		MarketingView.OnViewOpened();
		TopCharts.gameObject.SetActive(false);
		signingManager.Show(false);
		SongViewRoot.gameObject.SetActive(false);
		SongResultsView.gameObject.SetActive(false);
		SetBackToStudioButton(true);
		GoToTrendsButton.gameObject.SetActive(true);
		this.ScreenNameBackground.SetActive(true);
		this.viewMode = ViewMode.Marketing;
		this.lastViewFunction = this.OpenMarketingView;
		GameRefs.I.m_globalLastScreen = "marketingScreen";
		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.CanUnlockBoroughAfterYWeeks);
		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.NextWeekClicked);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.StudioFloorFinal);
	}

	public void OpenResultsView()
	{
		signingManager.Show(false);
		MarketingView.gameObject.SetActive(false);
		this.SongViewRoot.gameObject.SetActive(false);
		SetBackToStudioButton(false);
		TopCharts.gameObject.SetActive(false);
		this.SongResultsView.Show();
		this.GoToTrendsButton.gameObject.SetActive(true);
		this.NextTurnButton.gameObject.SetActive(false);
		this.ScreenNameBackground.SetActive(true);
		this.lastViewFunction = this.OpenResultsView;
		GameRefs.I.m_globalLastScreen = "resultsScreen";
		viewMode = ViewMode.Results;
	}

	public void OpenTopChartsView()
	{
		if (TopCharts.gameObject.activeInHierarchy)
		{
			if (TopCharts.isThisWeek)
			{
				TopCharts.ShowAllTimeHits(false);
			}
		}
		else if (CheckGameOver())
		{
			GameOverControl.Hide();
			TopCharts.gameObject.SetActive(true);
			TopCharts.OnViewOpened(true);
		}
		else
		{
			studioViewTutorialMount.SetActive(false);
			TopCharts.gameObject.SetActive(true);
			TopCharts.OnViewOpened(viewMode == ViewMode.Studio);
			MarketingView.gameObject.SetActive(false);
			signingManager.Show(false);
			SongViewRoot.gameObject.SetActive(false);
			SongResultsView.gameObject.SetActive(false);
			SetBackToStudioButton(true);
			GoToTrendsButton.gameObject.SetActive(true);
			this.ScreenNameBackground.SetActive(true);
			this.viewMode = ViewMode.TopCharts;
			GameRefs.I.m_globalLastScreen = "topCharts";
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.StudioFloorFinal);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.MarketingScreen);
		}
	}

	public void ToggleMute()
	{
		AudioListener.volume = muted ? 1f : 0f;
		muteStatus.sprite = muted ? muteIsPlaying : muteIsMuted;
		muted = !muted;
	}

	public void EnableTrendsButton(bool isEnabled = true)
	{
		GoToTrendsButton.gameObject.SetActive(isEnabled);
	}

	public void OpenLastView()
	{
		if (!CheckGameOver())
		{
			this.Graph.OnClose();
			this.lastViewFunction();
		}
		else
		{
			TopCharts.gameObject.SetActive(false);
		}
	}

	public void OnSongCreated(Song newSong)
	{
		this.recordingsInProgress.Add(newSong);

		this.SongManager.OnSongRecordBegun();

		if (MarketingInsights.unconfirmedInsights.Count > 0)
		{
			// toRemove: remove this from the list after we're done iterating over it if we found something
			MarketingInsightObject toRemove = null;
			for (int i = 0; i < MarketingInsights.unconfirmedInsights.Count; i++)
			{
				if (MarketingInsights.unconfirmedInsights[i].bandAttached == newSong.Artist)
				{
					MarketingInsights.unconfirmedInsights[i].songAttached = newSong;
					GameRefs.I.m_gameController.marketingInsightList.Add(MarketingInsights.unconfirmedInsights[i]);
					toRemove = MarketingInsights.unconfirmedInsights[i];
				}
			}
			if (toRemove != null)
			{
				MarketingInsights.unconfirmedInsights.Remove(toRemove);
			}
		}

		this.RefreshHUDLabels();

		this.SongManager.OnViewOpened(this.signedBands, true, NextTurnState.Normal);
		/* SongManager.SynchronizeIslands(); */
	}

	public void OnReleaseSongButtonClicked()
	{
		Song recordingSong = this.SongManager.GetActiveBand().GetRecordingSong();
		recordingSong.TurnReleased = currentTurn;

		if (recordingSong.TurnsRecorded < recordingSong.NumRecordingTurns)
		{
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.FirstSongReady);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.FirstSongReady);
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.RecordingWhenReady);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.RecordingWhenReady);
		}

		Band releasingBand = this.SongManager.GetActiveBand();
		Tutorial.SetTutorialCompleted(TutorialController.TutorialID.RecordingWhenReady);
		Song releasingSong = releasingBand.GetRecordingSong();
		releasingBand.OnSongReleased();

		if (releasingBand == recordingBand)
		{
			RecordingBand = null;
		}

		this.ReleaseSong(releasingSong);
	}

	public void ReleaseSong(Song newSong)
	{
		newSong.CalculateStarRating();

		this.songs.Add(newSong);
		this.recordingsInProgress.Remove(newSong);

		newSong.TurnOfCreation = this.currentTurn - 1;

		this.RefreshHUDLabels();

		this.OpenResultsView();
		GameRefs.I.m_recordingAudio.PlaySong(newSong, true);
		// Determine whether there is an insight for this song 
		MarketingInsightObject insight = GetMarketingInsightForSong(newSong);
		if (insight != null)
		{
			if (IsInsightCorrect(insight))
			{
				insight.successful = true;
				Debug.Log("Trend was correct!");
			}
			else
			{
				insight.successful = false;
				Debug.Log("Trend was not correct");
			}
		}
		else // No marketing insight
		{
			Debug.Log("You missed an insight opportunity");
		}
		this.SongResultsView.ProcessNewSongAndShowResults(newSong, this, this.DataManager, insight);
	}

	public MarketingInsightObject GetMarketingInsightForSong(Song song)
	{
		for (int i = 0; i < marketingInsightList.Count; i++)
		{
			if (marketingInsightList[i].bandAttached == song.Artist)
				return marketingInsightList[i];
		}
		return null;
	}

	public bool IsInsightCorrect(MarketingInsightObject insight)
	{
		Dictionary<StatSubType, List<float>> data;
		//data = DataSimulationManager.Instance.GetCachedIndustryPreferenceData();
		data = DataSimulationManager.Instance.GetCachedIndustryLocationData(insight.location);

		if (insight.insightType == MarketingInsights.InsightType.MostPopular)
		{
			StatSubType highestType = insight.statType;
			float highestValue = 0f;
			foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
			{
				// Need to check all of the types of this super to see whose is the highest
				if (kvp.Key.SuperType == insight.statType.SuperType)
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
			Debug.LogFormat("HighestType == {0}", highestType.Name);

			if (highestType == insight.statType)
				return true;
			else
				return false;
		}
		else if (insight.insightType == MarketingInsights.InsightType.IsTrending)
		{
			foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
			{
				// Need to check all of the types of this super to see whose is the highest
				if (kvp.Key == insight.statType)
				{
					List<float?> values = kvp.Value.Cast<float?>().ToList();
					float? predictedValue = values[30 - (currentTurn - insight.turnCreated)];
					float? currentValue = values[values.Count - 1];

					Debug.LogFormat("Value on Prediction: {0}   Value Now: {1}", predictedValue.Value, currentValue.Value);
					if (currentValue > predictedValue)
						return true;
					else
						return false;
				}
			}

		}

		return false;
	}

	public void OnNewSongRelease(Song song, float initialCashIncome)
	{
		this.cash += initialCashIncome;
		this.thisTurnIncome += initialCashIncome;
		this.RefreshHUDLabels();
	}

	public void OnSongCharted()
	{
		UpdateWall();
	}

	public void UpdateWall()
	{
		StatSubType[] genreList = new StatSubType[6] {
			StatSubType.ROCK,
			StatSubType.POP,
			StatSubType.RANDB,
			StatSubType.HIP_HOP,
			StatSubType.RAP,
			StatSubType.ELECTRONIC
		};

		for (int i = 0; i < genreList.Length; i++)
		{
			int status = GameRefs.I.m_topCharts.GetGenreCompletionStatus(genreList[i]);

			if (status >= 2)
			{
				platinumRoot[i].SetActive(true);
				for (int ci = 0; ci < status - 1; ++ci)
				{
					platinumRoot[i].transform.GetChild(ci).gameObject.SetActive(true);
				}
				goldRoot[i].SetActive(false);
			}
			else if (status == 1)
			{
				goldRoot[i].SetActive(true);
			}
		}
	}

	public List<Song> GetRecordingSongs()
	{
		return recordingsInProgress;
	}

	public bool IsBandRecording(Band band)
	{
		return this.recordingsInProgress.Where(x => x.Artist == band).Count() > 0;
	}

	public bool CanRecordNewSong()
	{
		return this.recordingsInProgress.Count < 3;
	}

	public int GetNumRecordings()
	{
		return this.recordingsInProgress.Count;
	}

	public List<Song> GetSongs()
	{
		return this.songs;
	}

	public List<Band> GetSignedBands()
	{
		return this.signedBands;
	}

	public void ClearAndSetBands(List<Band.SaveData> bands, List<Band.SaveData> availBands)
	{
		signedBands.Clear();
		foreach (Band.SaveData b in bands)
			signedBands.Add(new Band(b, true));

		availableBands.Clear();
		foreach (Band.SaveData b in availBands)
			availableBands.Add(new Band(b, false));
	}

	private void RefreshAvailableBands()
	{
		for (int n = 0; n < 3; n++)
		{
			int currentBandTier = 0;

			for (int i = 0; i < this.BandTierList.Tiers.Count; i++)
			{
				if (this.DataManager.GetTotalFollowerCount() >= this.BandTierList.Tiers[i].MinimumFollowers)
					currentBandTier = i;
				else
					break;
			}

			bool useHigherTier = UnityEngine.Random.value <= this.BandTierList.BetterTierRate;
			bool useLowerTier = UnityEngine.Random.value <= this.BandTierList.WorseTierRate && !useHigherTier;

			int turnsToKeep = UnityEngine.Random.Range(this.BandTierList.CommonBandAvailabilityMinimumDuration, this.BandTierList.CommonBandAvailabilityMaximumDuration);

			if (useHigherTier && currentBandTier < this.BandTierList.Tiers.Count - 1)
			{
				currentBandTier += 1;
				turnsToKeep = UnityEngine.Random.Range(this.BandTierList.RareBandAvailabilityMinimumDuration, this.BandTierList.RareBandAvailabilityMaximumDuration);
			}

			if (useLowerTier && currentBandTier > 0)
			{
				currentBandTier -= 1;
				turnsToKeep = UnityEngine.Random.Range(this.BandTierList.RareBandAvailabilityMinimumDuration, this.BandTierList.RareBandAvailabilityMaximumDuration);
			}
			Band newBand = null;
			if (currentTurn % 2 == 0)
			{
				if (n == 0 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.POP) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.POP);
				else if (n == 1 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.ROCK) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.ROCK);
				else if (n == 2 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.HIP_HOP) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.HIP_HOP);
			}
			else if(currentTurn % 2 == 1)
			{
				if (n == 0 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.ELECTRONIC) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.ELECTRONIC);
				else if (n == 1 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.RAP) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.RAP);
				else if (n == 2 && UnityEngine.Random.Range(0f, 1f) < (GameInitializationVariables.BaseChanceForArtist + DataManager.GetBonusArtistPercent(StatSubType.RANDB) / 100f))
					newBand = GenerateBand(currentBandTier, StatSubType.RANDB);
			}

			if (newBand != null && !newBand.IsSigned && !this.availableBands.Contains(newBand))
			{
				bool alreadyIn = false;
				if(signedBands != null)
				{
					foreach (Band b in signedBands)
					{
						if (b.Name == newBand.Name)
							alreadyIn = true;
					}
				}
		
				if(availableBands != null)
				{
					foreach (Band b in availableBands)
					{
						if (b.Name == newBand.Name)
							alreadyIn = true;
					}
				}
			

				if (!alreadyIn)
				{
					newBand.IsNew = true;
					this.availableBands.Add(newBand);
					newBand.TurnsLeft = turnsToKeep;
				}
			}
		}
        this.RefreshNewArtistNotification();
    }

    public Band GenerateBand(int tier, StatSubType forceGenre)
    {
        int bandPoints = UnityEngine.Random.Range(this.BandTierList.Tiers[tier].MinimumPoints, this.BandTierList.Tiers[tier].MaximumPoints);
        Band newBand = new Band(this.BandTierList.GetRandomBandTemplate(forceGenre), bandPoints, GameRefs.I.bandGenerators);
		newBand.BaseCost = BandTierList.BaseBandCost;
		newBand.CostPerPoint = BandTierList.AdditionalBandCostPerPoint;
		newBand.CostMultiplierPerPoint = BandTierList.AdditionalBandCostMultiplierPerPoint;
        return newBand;
    }

    public void HireBand(Band band)
    {
		this.availableBands.Remove(band);
        this.signedBands.Add(band);
        band.IsSigned = true;
        this.signingManager.OnViewOpened(this.signedBands, this.availableBands.Where(x => !x.IsSigned).ToList(), true);

        this.thisTurnExpenses += band.UpkeepCost;
        this.RefreshHUDLabels();
    }

    public void FireBand(Band band, bool isReload = true)
    {
        this.signedBands.Remove(band);
		band.IsSigned = false;
        if (band.IsRecordingSong())
        {
            this.recordingsInProgress.Remove(band.GetRecordingSong());
            band.AssignRecordingSong(null);
        }

        this.thisTurnIncome += band.UpkeepCost;
        this.RefreshHUDLabels();

		if (isReload)
		{
			this.signingManager.OnViewOpened(this.signedBands, this.availableBands.Where(x => !x.IsSigned).ToList(), false);
		}
    }

    public void UpdateFollowerCount()
    {
        this.FollowerCounter.text = Utilities.FormatNumberForDisplay(this.DataManager.GetTotalFollowerCount());
    }

    public Dictionary<StatSubType, List<float>> GetFollowerData()
    {
        Dictionary<StatSubType, List<float>> followerData = new Dictionary<StatSubType, List<float>>();

        foreach (PopulationData data in this.DataManager.GetPopulationData())
        {
            followerData[data.Location] = data.GetFollowerData();
        }

        return followerData;
    }

    public float GetCash()
    {
        return this.cash;
    }
	public void SetCash(float newCash)
	{
		this.cash = newCash;
	}

	public float GetFollowersAtLocation(StatSubType loc)
	{
		List<PopulationData> popData = DataManager.GetPopulationData();
		foreach(PopulationData pop in popData)
		{
			if (pop.Location == loc)
				return pop.GetFollowers();
		}
		return 0f;
	}

	public void SetFollowersAtLocation(StatSubType loc, float followers)
	{
		foreach (PopulationData data in this.DataManager.GetPopulationData())
		{
			if (data.Location == loc)
			{
				data.ReplacePendingFollowers(followers);
			}
		}
	}

	public void RemoveCash(float cashToRemove)
    {
        if(cash >= cashToRemove)
        {
            cash -= cashToRemove;
        }
        RefreshHUDLabels();
    }

    public List<float> GetBurroughSaturation()
    {
        List<float> saturations = new List<float>();

        saturations = this.DataManager.GetPopulationData().OrderBy(x => x.Location.ID).Select(x => x.GetPendingFollowers() / x.GetPopulation()).ToList();

        return saturations;
    }

    public void Debug_GetMoney(float x)
    {
        this.cash += x;
        this.RefreshHUDLabels();
    }

    public void Debug_DoubleFans(StatSubType location)
    {
        foreach (PopulationData data in this.DataManager.GetPopulationData())
        {
            if (data.Location == location || location == StatSubType.NONE)
            {
                data.AddPendingFollowers(data.GetPendingFollowers());
            }
        }

        this.UpdateFollowerCount();
    }

    public void Debug_RefreshAvailableArtists()
    {
        foreach (Band band in this.availableBands)
        {
            band.TurnsLeft = 0;

            if (!this.signedBands.Contains(band))
            {
                band.IsSigned = false;
            }
        }

        this.availableBands.Clear();
        this.RefreshAvailableBands();
        this.signingManager.OnViewOpened(this.signedBands, this.availableBands, false);
    }

	public void SetBackToStudioButton(bool on)
	{
		swingRoutine.Replace(this, StudioHeaderSwing(on));
	}

	private IEnumerator StudioHeaderSwing(bool on)
	{
		yield return Routine.Combine(HeaderSwing.AnchorPosTo(on ? 0f : hudSwingDistance, hudSwingSpeed, Axis.X).Ease(hudSwingCurve),
			TooltipSwing.AnchorPosTo(on ? 0f : hudSwingDistance+450, hudSwingSpeed, Axis.X).Ease(hudSwingCurve));
	}

	public void DebugRecordingBand()
	{
		recordingRoom.interactable = true;				
		RecordingBand = GenerateBand(1, StatSubType.ROCK);
	}

	private bool HasBandMember(int i)
	{
		return recordingBand != null && i < recordingBand.members.Count;
	}

	IEnumerator CoTwitch(int i)
	{
		while (HasBandMember(i))
		{
			float delay = UnityEngine.Random.Range(GameRefs.I.bandGenerators.minTimeBetweenTwitches, GameRefs.I.bandGenerators.maxTimeBetweenTwitches);
			yield return new WaitForSeconds(delay);
			if (HasBandMember(i))
			{
				recordingBand.members[i].Twitch();
			}
		}
		twitchTasks[i] = null;
	}

	private Band recordingBand;
	public Band RecordingBand
	{
		get
		{
			return recordingBand;
		}
		set {
			for (int i = 0; i < twitchTasks.Length; ++i)
			{
				if (twitchTasks[i] != null) {
					StopCoroutine(twitchTasks[i]);
					twitchTasks[i] = null;
				}
			}

			staffMembers[2].animator.SetBool("IsBored", value == null);

			recordingBand = value;
			if (recordingBand == null)
			{
				for (int i = 0; i < recordingModels.Length; ++i)
				{
					Footilities.DestroyChildren(recordingModels[i]);
					recordingModels[i].SetActive(false);
				}
			}
			else
			{
				// If the meshes aren't active, then the animators that are
				// generated by Incarnate won't play. So, we force the parent
				// to be active, perhaps momentarily.
				bool isRootActive = Furniture.activeInHierarchy;
				Furniture.SetActive(true);

				for (int i = 0; i < recordingBand.members.Count; i++)
				{
					recordingBand.members[i].Incarnate(recordingModels[i], Band.PoseContext.Jam, floor, GameRefs.I.colors.recordingShadowColor);
					if (recordingBand.members[i].instrumentId == Band.Instrument.Guitar ||
						recordingBand.members[i].instrumentId == Band.Instrument.Drums ||
						recordingBand.members[i].instrumentId == Band.Instrument.Vocals)
					{
						twitchTasks[i] = StartCoroutine(CoTwitch(i));
					}
				}

				// Disable unused members.
				for (int i = recordingBand.members.Count; i < recordingModels.Length; ++i)
				{
					recordingModels[i].SetActive(false);
				}

				Furniture.SetActive(isRootActive);
			}
		}
	}
}
