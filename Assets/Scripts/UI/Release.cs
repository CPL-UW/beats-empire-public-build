using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using TMPro;
using Utility;

/// <summary>
/// Connected to the Song Creation UI panel
/// </summary>
public class Release : MonoBehaviour
{
	[Header("Widget Primitives")]
	public TextMeshProUGUI MarketingInsightButtonText;
	public Button marketingInsightsButton;
	public Button editPredictionButton;
	public Button marketingInsightsCloseButton;
	public Button predictionCloseButton;
	public Button backButton;
	public TextMeshProUGUI tooltipLabel;
	public GameObject sidebarScrollbar;
	public GameObject popularIcon;
	public GameObject trendingIcon;
	public TextMeshProUGUI maxHitLabel;
	public TextMeshProUGUI[] boroughNames;
	public CanvasGroup genreCaresAbout;
	public CanvasGroup moodHeader;
	public CanvasGroup topicHeader;
	public GameObject moodOptionalLabel;
	public GameObject topicOptionalLabel;
	public Toggle[] moodToggles;
	public Toggle[] topicToggles;

	[System.Serializable]
	public class HitThreshold {
		public TextMeshProUGUI label;
		public int barCount;
	}
	public HitThreshold[] hitThresholds;

	[Header("Things To Color")]
	public Image artistTopBackground;
	public Image artistLeftBackground;
	public Image artistBottomBackground;

	public Image audienceTooltipBackground;
	public Image audienceTooltipPopulationBackground;
	public Image audienceTooltipTraitsBackground;

	public Image songWritingSkillsBackground;
	public Image songTraitsBackgroundLeft;
	public Image songTraitsBackgroundRight;

	[Header("Animations")]
	public AnimationClip toOnOffClip;

	[Header("Hitboxes")]
	public GameObject[] skillBoxes;
	public GameObject[] boroughBoxes;
	public GameObject audienceBox;
	public GameObject predictionsBox;

	[Header("Widget Containers")]
	public GameObject predictionsRoot;
	public RectTransform audienceRoot;
	public GameObject findTrendRoot;
	public GameObject trendPredictRoot;
	public TextMeshProUGUI predictionLabel;
	public TextMeshProUGUI predictionSublabel;

	[Header("Others")]
	public GameController Controller;
	public GameObject noArtistYetView;
	public Animator animator;
	public ArtistSelector ArtistSelectorPrefab;
	public Transform ArtistSelectorRoot;
	public SongRecordingPanel ArtistPanel;
	public MarketingInsights MarketingInsights;
	public GameObject LeftArrow;
	public GameObject RightArrow;

	private StatSubType[] boroughs = {
		StatSubType.TURTLE_HILL,
		StatSubType.MADHATTER,
		StatSubType.IRONWOOD,
		StatSubType.THE_BRONZ,
		StatSubType.KINGS_ISLE,
		StatSubType.BOOKLINE
	};

	[Header("Parameters")]
	public Strings strings;
	public Colors colors;

	private List<ArtistSelector> artistSelectors;
	private Band currentBand;
	private int viewedBand;
	private int targetedBoroughIndex;
	public int targetedBoroughId {
		get
		{
			if (targetedBoroughIndex >= 0)
			{
				return boroughs[targetedBoroughIndex].ID;
			}
			else
			{
				return StatSubType.NONE_ID;
			}
		}
	}
	public StatSubType targetedBorough {
		get
		{
			if (targetedBoroughIndex >= 0)
			{
				return boroughs[targetedBoroughIndex];
			}
			else
			{
				return StatSubType.NONE;
			}
		}
	}
	private bool isCondensed;
	private bool[] hoverBeeenLogged = new bool[4];

	public void Init()
	{
		artistSelectors = new List<ArtistSelector>();
		ArtistPanel.Init(Controller);

		RegisterEvents();
	}

	private void RegisterEvents()
	{
		// Sorry, but I have mixed feelings about UnityEvents. Sure, they allow
		// us to wire about up callbacks in the Inspector, but I have three
		// complaints about them: 1) it becomes much more difficult to track
		// what's going on, 2) you can't search through the Inspector like you
		// can code, and 3) the actions are easily lost if the object gets
		// redone. I favor code.

		for (int i = 0; i < skillBoxes.Length; ++i)
		{
			RegisterSkillMouseEvents(skillBoxes[i], i);
		}

		for (int i = 0; i < boroughBoxes.Length; ++i)
		{
			RegisterBoroughEvents(boroughBoxes[i], i);
		}

		// Hover events for Target Audience.
		EventTrigger eventTrigger = audienceBox.AddComponent<EventTrigger>();
		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => animator.SetBool("Audience/Hover", true));
		eventTrigger.Register(EventTriggerType.PointerExit, (data) => animator.SetBool("Audience/Hover", false));

		// Hover events for What's Hot.
		eventTrigger = predictionsBox.AddComponent<EventTrigger>();
		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => animator.SetBool("Prediction/Hover", true));
		eventTrigger.Register(EventTriggerType.PointerExit, (data) => animator.SetBool("Prediction/Hover", false));

		// The Find a Trend button covers the What's Hot hitbox but is not
		// enclosed, so we must repeat the animation logic for it.
		eventTrigger = marketingInsightsButton.gameObject.AddComponent<EventTrigger>();
		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => animator.SetBool("Prediction/Hover", true));
		eventTrigger.Register(EventTriggerType.PointerExit, (data) => animator.SetBool("Prediction/Hover", false));

		eventTrigger = predictionCloseButton.gameObject.AddComponent<EventTrigger>();
		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => animator.SetBool("Prediction/Hover", true));
		eventTrigger.Register(EventTriggerType.PointerExit, (data) => animator.SetBool("Prediction/Hover", false));
	}

	[ContextMenu("Set BPM")]
	public void SetBPM(float bpm)
	{
		// Set to a base of 60;
		float multiplier = bpm / 60f;
		animator.SetFloat("Recording/BPMMultiplier", multiplier);
	}

	private void RegisterSkillMouseEvents(GameObject skillBox, int index)
	{
		EventTrigger eventTrigger = skillBox.AddComponent<EventTrigger>();

		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => {
			animator.SetBool("SkillTooltip/On", true);
			animator.SetInteger("SkillTooltip/Arrow", index + 1);
			tooltipLabel.text = strings.skillTooltipLabels[index];

			if(!hoverBeeenLogged[index])
			{
				GameRefs.I.PostGameState(false, "tooltipHovered", skillBox.name);
				hoverBeeenLogged[index] = true;
			}
		});

		eventTrigger.Register(EventTriggerType.PointerExit, (data) => {
			animator.SetBool("SkillTooltip/On", false);
		});
	}

	public void RegisterBoroughEvents(GameObject boroughBox, int index) {
		EventTrigger eventTrigger = boroughBox.AddComponent<EventTrigger>();
		Toggle toggle = boroughBox.GetComponent<Toggle>();

		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => {
			if (GameRefs.I.m_marketingView.IsBoroughUnlocked(boroughs[index])) {
				animator.SetBool("Audience/Hover", true);
				animator.SetInteger("Audience/Arrow", index + 1);

				if (!toggle.isOn) {
					audienceTooltipBackground.color = colors.hoverBackground;
					audienceTooltipPopulationBackground.color = colors.hoverPopulationBackground;
					audienceTooltipTraitsBackground.color = colors.hoverTraitsBackground;

					ArtistPanel.OnLocationHighlighted(index);
				}
			}
		});

		eventTrigger.Register(EventTriggerType.PointerExit, (data) => {
			if (GameRefs.I.m_marketingView.IsBoroughUnlocked(boroughs[index])) {
				animator.SetInteger("Audience/Arrow", 0);

				if (targetedBoroughIndex < 0) {
					ArtistPanel.OnLocationHighlighted(-1);
				} else if (index != targetedBoroughIndex) {
					audienceTooltipBackground.color = colors.selectedBackground;
					audienceTooltipPopulationBackground.color = colors.selectedPopulationBackground;
					audienceTooltipTraitsBackground.color = colors.selectedTraitsBackground;

					ArtistPanel.OnLocationTargeted(targetedBoroughIndex);
				}
			}
		});

		toggle.onValueChanged.AddListener((isOn) => {
			if (isOn) {
				audienceTooltipBackground.color = colors.selectedBackground;
				audienceTooltipPopulationBackground.color = colors.selectedPopulationBackground;
				audienceTooltipTraitsBackground.color = colors.selectedTraitsBackground;
				boroughNames[index].color = colors.selectedBoroughTextColor;
				targetedBoroughIndex = index;
				genreCaresAbout.gameObject.SetActive(true);

				bool[] interests = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(boroughs[targetedBoroughIndex]);
				genreCaresAbout.alpha = interests[0] ? 1.0f : 0.2f;

				if (interests[1])
				{
					moodHeader.alpha = 1;
					moodOptionalLabel.SetActive(false);
				}
				else
				{
					moodHeader.alpha = 0.6f;
					moodOptionalLabel.SetActive(true);
				}

				if (interests[2])
				{
					topicHeader.alpha = 1;
					topicOptionalLabel.SetActive(false);
				}
				else
				{
					topicHeader.alpha = 0.6f;
					topicOptionalLabel.SetActive(true);
				}

				SetToggleColors(interests[1], interests[2]);
			} else {
				boroughNames[index].color = colors.unselectedBoroughTextColor;
				targetedBoroughIndex = -1;
				ClearInterests();
			}

			ArtistPanel.OnLocationTargeted(targetedBoroughIndex);
		});
	}

	private void SetToggleColors(bool isMoodMandatory, bool isTopicMandatory)
	{
		ColorBlock mandatoryColors = ColorBlock.defaultColorBlock;
		mandatoryColors.disabledColor = colors.recordingTraitDisabledColor;
		mandatoryColors.pressedColor = colors.recordingTraitPressedColor;
		mandatoryColors.highlightedColor = colors.recordingTraitMandatorySelectedColor;
		mandatoryColors.normalColor = colors.recordingTraitMandatoryUnselectedColor;

		ColorBlock optionalColors = ColorBlock.defaultColorBlock;
		optionalColors.disabledColor = colors.recordingTraitDisabledColor;
		optionalColors.pressedColor = colors.recordingTraitPressedColor;
		optionalColors.highlightedColor = colors.recordingTraitOptionalSelectedColor;
		optionalColors.normalColor = colors.recordingTraitOptionalUnselectedColor;

		foreach (Toggle moodToggle in moodToggles)
		{
			moodToggle.colors = isMoodMandatory ? mandatoryColors : optionalColors;
		}

		foreach (Toggle topicToggle in topicToggles)
		{
			topicToggle.colors = isTopicMandatory ? mandatoryColors : optionalColors;
		}
	}

	private void ClearInterests()
	{
		genreCaresAbout.gameObject.SetActive(false);
		moodHeader.alpha = 1;
		moodOptionalLabel.SetActive(false);
		topicHeader.alpha = 1;
		topicOptionalLabel.SetActive(false);
		SetToggleColors(true, true);
	}

	private void RegisterBack()
	{
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			ClearUnrecordedInsights();
			GameRefs.I.PostGameState(false, "clickedButton", "backButton");
			animator.SetBool("On", false);
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.RecordingAfterRelease);
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.StudioFloorFinal);
			Controller.OpenStudioView();
			Footilities.Schedule(this, toOnOffClip.length, () => gameObject.SetActive(false));
		});
	}

	public void OnViewRestore()
	{
		RegisterBack();
		GameRefs.I.hudController.ToRecordingMode();
		if (currentBand != null)
		{
			ArtistPanel.AssignBand(currentBand);
			ShowRecordingStatus();
		}
	}

	public void OnViewOpened(List<Band> bandOptions, bool keepSelection, GameController.NextTurnState recordingState, bool updateRecordingStatus=false)
	{
		ArtistPanel.OnViewOpened(!keepSelection);
		for (int i = 0; i < hoverBeeenLogged.Length; i++)
			hoverBeeenLogged[i] = false;

		targetedBoroughIndex = -1;
		ClearInterests();
		animator.SetBool("Recording/HitMeterMaxed", false);
		RegisterBack();
		GameRefs.I.hudController.ToRecordingMode();
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.RecordingBooth);
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.RecordingAfterRelease, TutorialController.TutorialAction.Confirm);
		viewedBand = 0;

		foreach (ArtistSelector selector in artistSelectors)
		{
			GameObject.Destroy(selector.gameObject);
		}

		artistSelectors.Clear();

		if (bandOptions.Count > 0)
		{
			LeftArrow.SetActive(bandOptions.Count > 1);
			RightArrow.SetActive(bandOptions.Count > 1);

			foreach (Band band in bandOptions)
			{
				ArtistSelector newSelector = ArtistSelector.Instantiate(ArtistSelectorPrefab);
				newSelector.name = string.Format("ArtistSelector_{0}", band.Name);
				newSelector.transform.SetParent(ArtistSelectorRoot);
				newSelector.InitForRecording(this, band);
				artistSelectors.Add(newSelector);
			}

			artistSelectors.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			for (int i = 0; i < artistSelectors.Count; i++)
			{
				artistSelectors[i].transform.SetAsLastSibling();
				artistSelectors[i].transform.localScale = new Vector3(1, 1, 1);
				artistSelectors[i].transform.localPosition = new Vector3(artistSelectors[i].transform.localPosition.x, artistSelectors[i].transform.localPosition.y, 0);

				if (artistSelectors[i].GetBand() == currentBand)
				{
					viewedBand = i;
				}
			}

			if (!keepSelection)
			{
				viewedBand = 0;
			}

			Band bandToShow = artistSelectors[viewedBand].GetBand();

			// If we have a song ready to release, default to that band
			if (recordingState == GameController.NextTurnState.NeedToRelease)
			{
				for (int i = 0; i < artistSelectors.Count; i++)
				{
					if (artistSelectors[i].GetBand().GetRecordingSong().DoneRecording)
					{
						viewedBand = i;
						break;
					}
				}
			}

			// If a band needs to record, default to that band
			else if (recordingState == GameController.NextTurnState.NeedToRecord)
			{

				if (GameRefs.I.m_globalLastBand != null && !GameRefs.I.m_globalLastBand.IsRecordingSong())
				{
					for (int i = 0; i < artistSelectors.Count; i++)
					{
						if (artistSelectors[i].GetBand().Name == GameRefs.I.m_globalLastBand.Name)
						{
							viewedBand = i;
							break;
						}
					}
				}
				else
				{
					for (int i = 0; i < artistSelectors.Count; i++)
					{
						if (artistSelectors[i].GetBand().GetRecordingSong() == null)
						{
							viewedBand = i;
							break;
						}
					}
				}
			}



			SelectBand(artistSelectors[viewedBand].GetBand(), !keepSelection || updateRecordingStatus);
			artistSelectors[viewedBand].SelectorToggle.isOn = true;
		}
		else
		{
			SelectBand(null);
		}

		/* targetedBoroughIndex = -1; */
		ArtistPanel.OnLocationTargeted(targetedBoroughIndex);
	}

	void OnEnable()
	{
		animator.SetBool("On", true);
	}

	public void OnArtistSelected(ArtistSelector selected)
	{
		int newViewedBand = -1;

		for (int i = 0; i < artistSelectors.Count; ++i)
		{
			ArtistSelector selector = artistSelectors[i];
			if (selected != selector)
			{
				selector.SelectorToggle.isOn = false;
			}
			else
			{
				newViewedBand = i;
			}
		}

		if (newViewedBand >= 0 && newViewedBand != viewedBand)
		{
			if (newViewedBand > viewedBand)
			{
				viewedBand = (newViewedBand + 1) % artistSelectors.Count;
				HideCurrentArtist(-1);
			}
			else
			{
				viewedBand = (newViewedBand - 1 + artistSelectors.Count) % artistSelectors.Count;
				HideCurrentArtist(1);
			}
		}
	}

	public Band GetActiveBand()
	{
		return currentBand;
	}

	void ShowRecordingStatus()
	{
		if (currentBand != null &&
			currentBand.IsRecordingSong() &&
			currentBand.GetRecordingSong().DoneRecording && 
			!currentBand.GetRecordingSong().IsKnownDoneRecording) {
			animator.SetInteger("Recording/Status", 1);
		}

		if (currentBand != null && currentBand.IsRecordingSong())
		{
			marketingInsightsButton.interactable = false;
			editPredictionButton.gameObject.SetActive(false);

			Song song = currentBand.GetRecordingSong();

			if (song.IsKnownStartedRecording)
			{
				animator.SetInteger("Recording/On", 2);
			}
			else
			{
				song.IsKnownStartedRecording = true;
				animator.SetInteger("Recording/On", 1);
			}

			if (song.IsKnownDoneRecording)
			{
				animator.SetInteger("Recording/Status", 3);
			}
			else if (!song.DoneRecording)
			{
				animator.SetInteger("Recording/Status", 1);
			}
		}
		else
		{
			editPredictionButton.gameObject.SetActive(true);
			marketingInsightsButton.interactable = true;
			animator.SetInteger("Recording/On", 0);
			animator.SetInteger("Recording/Status", 0);
		}
	}

	public void SelectBand(Band band, bool isRecordingUpdate = true)
	{
		currentBand = band;

		// Synchronize band UI.
		bool hasBand = band != null;
		ArtistPanel.gameObject.SetActive(hasBand);
		noArtistYetView.SetActive(!hasBand);

		if (band != null)
		{
			ArtistPanel.AssignBand(band);
			Synchronize();

			if (band.IsRecordingSong())
			{
				GameRefs.I.m_gameController.RecordingBand = band;
			}
		}

		if (isRecordingUpdate)
		{
			ShowRecordingStatus();
		}
	}

	public void CancelCurrentInsight()
	{
		for (int i = 0; i < MarketingInsights.unconfirmedInsights.Count; i++)
		{
			MarketingInsightObject insight = MarketingInsights.unconfirmedInsights[i];
			if (insight.bandAttached == currentBand)
			{
				MarketingInsights.unconfirmedInsights.Remove(insight);
				SelectBand(currentBand);
				GameRefs.I.PostGameState(true, "clickedButton", "CancelCurrentInsight");
				/* RefreshStats(); */
				break;
			}
		}
	}

	public void OnSongRecordBegun()
	{
		if (currentBand != null)
		{
			ShowRecordingStatus();

			if (currentBand.IsRecordingSong())
			{
				ArtistPanel.AssignBand(currentBand);
				GameRefs.I.m_recordingAudio.AddSongToQueue(currentBand.GetRecordingSong());
			}

			foreach (ArtistSelector selector in artistSelectors)
			{
				selector.RefreshAppearance();
			}
		}
	}

	public void Synchronize() {
		MarketingInsightButtonText.text = string.Format("FIND TRENDS");

		songWritingSkillsBackground.color = colors.GenreToColorSet(currentBand.GetGenre()).dark;
		songTraitsBackgroundLeft.color = colors.GenreToColorSet(currentBand.GetGenre()).midtone;
		songTraitsBackgroundRight.color = colors.GenreToColorSet(currentBand.GetGenre()).midtone;

		// Check for insights that have not been confirmed.
		MarketingInsightObject predictedInsight = MarketingInsights.unconfirmedInsights.Find(insight => insight.bandAttached == currentBand);
		marketingInsightsCloseButton.gameObject.SetActive(predictedInsight != null);
		if (predictedInsight == null)
		{
			predictedInsight = GameRefs.I.m_gameController.marketingInsightList.Find(insight => insight.bandAttached == currentBand);
		}
		bool isPredicting = predictedInsight != null;

		if (isPredicting) {
			trendPredictRoot.SetActive(true);
			findTrendRoot.SetActive(false);
			predictionLabel.SetIText(predictedInsight.statType.Name);
			if (predictedInsight.insightType == MarketingInsights.InsightType.MostPopular)
			{
				predictionSublabel.SetIText(string.Format("MOST POPULAR {0}", predictedInsight.statType.SuperType.Name));
				popularIcon.SetActive(true);
				trendingIcon.SetActive(false);
			}
			else
			{
				predictionSublabel.SetIText(string.Format("{0} TRENDING UP", predictedInsight.statType.SuperType.Name));
				trendingIcon.SetActive(true);
				popularIcon.SetActive(false);
			}

			if (currentBand.IsRecordingSong())
			{
				predictionCloseButton.gameObject.SetActive(false);
			}
			else
			{
				predictionCloseButton.gameObject.SetActive(true);
			}
		}
		else
		{
			findTrendRoot.SetActive(true);
			trendPredictRoot.SetActive(false);
		}

		if (!isCondensed && currentBand.IsRecordingSong() && !isPredicting)
		{
			isCondensed = true;
			predictionsRoot.SetActive(false);
			StartCoroutine(Footilities.CoLerp(0.1f, Vector2.zero, new Vector2(290, 0), (position) => {
				audienceRoot.anchoredPosition = position;
			}));
		}
		else if (isCondensed && (!currentBand.IsRecordingSong() || isPredicting))
		{
			isCondensed = false;
			predictionsRoot.SetActive(true);
			StartCoroutine(Footilities.CoLerp(0.1f, audienceRoot.anchoredPosition, Vector2.zero, (position) => {
				audienceRoot.anchoredPosition = position;
			}));
		}

		if (!currentBand.IsRecordingSong())
		{
			artistTopBackground.color = colors.preRecordingColor;
			artistLeftBackground.color = colors.preRecordingColor;
			artistBottomBackground.color = colors.preRecordingColor;
		}
		else if (currentBand.IsRecordingSong() && !currentBand.GetRecordingSong().DoneRecording)
		{
			artistTopBackground.color = colors.midRecordingColor;
			artistLeftBackground.color = colors.midRecordingColor;
			artistBottomBackground.color = colors.midRecordingColor;
		}
		else
		{
			artistTopBackground.color = colors.postRecordingColor;
			artistLeftBackground.color = colors.postRecordingColor;
			artistBottomBackground.color = colors.postRecordingColor;
		}

		if (currentBand.IsRecordingSong())
		{
			int nBars = currentBand.GetRecordingSong().Quality + 1;
			for (int i = 0; i < hitThresholds.Length; ++i)
			{
				if (nBars >= hitThresholds[i].barCount && ((i == 0 && nBars < 15) || (i > 0 && nBars < hitThresholds[i - 1].barCount)))
				{
					hitThresholds[i].label.color = Color.white;
				} else
				{
					hitThresholds[i].label.color = new Color(1, 1, 1, 0.2f);
				}
			}
		}
	}

	public void HideCurrentArtist(int direction)
	{
		animator.SetBool("Recording/HitMeterMaxed", false);
		if (direction > 0)
		{
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.RecordingToggle);
			animator.SetTrigger("Artist/NextArtist");
		}
		else if (direction < 0)
		{
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.RecordingToggle);
			animator.SetTrigger("Artist/PreviousArtist");
		}
	}

	public void ShowNextArtist()
	{
		artistSelectors[viewedBand].SelectorToggle.isOn = false;
		viewedBand = (viewedBand + 1) % artistSelectors.Count;
		artistSelectors[viewedBand].SelectorToggle.isOn = true;
		SelectBand(artistSelectors[viewedBand].GetBand());
		animator.SetTrigger("Artist/NextArtist");
	}

	public void ShowPreviousArtist()
	{
		artistSelectors[viewedBand].SelectorToggle.isOn = false;
		viewedBand = (viewedBand + artistSelectors.Count - 1) % artistSelectors.Count;
		artistSelectors[viewedBand].SelectorToggle.isOn = true;
		SelectBand(artistSelectors[viewedBand].GetBand());
		animator.SetTrigger("Artist/PreviousArtist");
	}

	public void ShowRecording() {
		if (currentBand != null &&
			currentBand.IsRecordingSong() &&
			currentBand.GetRecordingSong().DoneRecording && 
			!currentBand.GetRecordingSong().IsKnownDoneRecording) {
			animator.SetInteger("Recording/Status", 1);
		}
	}

	public void ShowDone()
	{
		// We don't want the Recording to Done transition to show until the
		// artist carousel is properly seated. To that end, this method is
		// ultimately called by the three animation clips that do that seating:
		//
		// - RecordingView_ArtistSwapping_Next_toOn
		// - RecordingView_ArtistSwapping_Previous_toOn
		// - RecordingView_Base_toOn

		if (currentBand != null &&
			currentBand.IsRecordingSong())
		{
			Song song = currentBand.GetRecordingSong();
			if (song.DoneRecording && !song.IsKnownDoneRecording)
			{
				animator.SetInteger("Recording/Status", 2);
				currentBand.GetRecordingSong().IsKnownDoneRecording = true;
			}

			if (song.Quality >= 14)
			{
				maxHitLabel.text = "EPIC!";
				animator.SetBool("Recording/HitMeterMaxed", true);
			}
		}
	}
	
	private void ClearUnrecordedInsights()
	{
		foreach (Band band in GameRefs.I.m_gameController.GetSignedBands())
		{
			if (!band.IsRecordingSong())
			{
				MarketingInsights.RemoveInsightsFromBand(band);
			}
			band.preReleaseLocation = StatSubType.NONE;
			band.preReleaseMood = StatSubType.NONE;
			band.preReleaseTopic = StatSubType.NONE;
			band.preReleaseName = null;
		}
	}
}
