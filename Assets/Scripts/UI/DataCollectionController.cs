using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class DataCollectionController : MonoBehaviour
{
	public GameObject view;
	public DataCollectionParameters dataCollectionParameters;
	public HudController hudController;
	public GameObject graphTutorialMount;

	[Header("Primitives")]
	public Button backButton;
	public Button buySlotButton;

	public TextMeshProUGUI nextSlotCostLabel;
	public TextMeshProUGUI totalCostLabel;
	public TextMeshProUGUI[] headerLabels;

	[Header("Animation")]
	public Animator animator;

	[Header("Storage Slots")]
	public GameObject[] totalCapacitySlots;
	public GameObject[] totalFilledSlots;

	[System.Serializable]
	public enum TraitType {
		Mood,
		Topic,
		Genre
	};

	[System.Serializable]
	public class AllocationWidget {
		public GameObject[] capacitySlots;
		public GameObject[] filledSlots;
		public Slider slider;
		public Image fill;
		public Image frequencyLabelBackground;
		public TextMeshProUGUI frequencyLabel;
		public TextMeshProUGUI costLabel;
		public TraitType trait;
	}

	public AllocationWidget[] allocationWidgets;
	public bool beenOpened = false;

	private StatType[] traitTypeToStatType;
	private Coroutine pulseNextSlotTask;
	private Color slotColor;
	private int currentTotalCost;
	private string previousScreen;

	void Start()
	{
		traitTypeToStatType = new StatType[3];
		traitTypeToStatType[0] = StatType.MOOD;
		traitTypeToStatType[1] = StatType.TOPIC;
		traitTypeToStatType[2] = StatType.GENRE;

		slotColor = totalCapacitySlots[0].GetComponent<Image>().color;

		for (int i = 0; i < headerLabels.Length; ++i)
		{
			headerLabels[i].text = dataCollectionParameters.frequencies[i].header;
		}

		RegisterCallbacks();
		Synchronize();
	}

	void RegisterCallbacks()
	{
		buySlotButton.onClick.AddListener(() => {
			int cost = dataCollectionParameters.slotCosts[GameRefs.I.storageSlotCount];
			if (GameRefs.I.m_gameController.GetCash() < cost) {
				animator.SetTrigger("CostUIShake");
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.Error);
			} else {
				if (pulseNextSlotTask != null)
				{
					StopCoroutine(pulseNextSlotTask);
				}
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ButtonClickMajor);
				pulseNextSlotTask = null;
				totalCapacitySlots[GameRefs.I.storageSlotCount].GetComponent<Image>().color = slotColor;
				++GameRefs.I.storageSlotCount;
				GameRefs.I.m_gameController.RemoveCash(cost);
				Synchronize();
				GameRefs.I.PostGameState(true, "clickedButton", "BuyStorageButton");
				/* PulseNextSlot(); */
			}
		});

		EventTrigger eventTrigger = buySlotButton.gameObject.AddComponent<EventTrigger>();
		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => PulseNextSlot());
		eventTrigger.Register(EventTriggerType.PointerExit, (data) => {
			if (GameRefs.I.storageSlotCount < totalCapacitySlots.Length)
			{
				if (pulseNextSlotTask != null)
				{
					StopCoroutine(pulseNextSlotTask);
					pulseNextSlotTask = null;
				}
				totalCapacitySlots[GameRefs.I.storageSlotCount].SetActive(false);
			}
		});

		for (int i = 0; i < 3; ++i)
		{
			RegisterAllocationWidgetCallbacks(allocationWidgets[i], i);
		}
	}

	void RegisterAllocationWidgetCallbacks(AllocationWidget widget, int i)
	{
		StatType statType = traitTypeToStatType[(int) widget.trait];
		TraitSampling sampling = GameRefs.I.traitSamplings[statType];
		
		widget.slider.onValueChanged.AddListener((value) => {
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.DataToggle);
			int nSlotsRequested = Mathf.RoundToInt(value);
			sampling.slotCount = nSlotsRequested;
			Synchronize();
		});

		SliderRelease release = widget.slider.gameObject.AddComponent<SliderRelease>();
		release.callback = () => {
			int nSlotsRequested = Mathf.RoundToInt(widget.slider.value);
			int nSlotsFromOthers = (GameRefs.I.traitSamplings[StatType.GENRE].slotCount + GameRefs.I.traitSamplings[StatType.MOOD].slotCount + GameRefs.I.traitSamplings[StatType.TOPIC].slotCount) - GameRefs.I.traitSamplings[statType].slotCount;
			int nSlotsAvailable = GameRefs.I.storageSlotCount - nSlotsFromOthers;
			int nSlotsAllocated = Mathf.Min(nSlotsAvailable, nSlotsRequested);
			widget.slider.value = nSlotsAllocated;
			sampling.slotCount = nSlotsAllocated;
			sampling.iteration = nSlotsAllocated > 0 ? 0 : -1;
			GameRefs.I.PostGameState(false, "changedSlider", string.Format("{0}_toSampling_{1}", Utility.Utilities.InterceptText(statType.Name), sampling.slotCount));
			Synchronize(true);
			if (nSlotsRequested > nSlotsAvailable)
			{
				FlashFullSlots();
			}
		};
	}

	void PulseNextSlot()
	{
		if (buySlotButton.interactable)
		{
			totalCapacitySlots[GameRefs.I.storageSlotCount].SetActive(true);
			pulseNextSlotTask = StartCoroutine(CoPulseNextSlot());
		}
	}

	IEnumerator CoPulseNextSlot()
	{
		float startTime = Time.time;
		Image image = totalCapacitySlots[GameRefs.I.storageSlotCount].GetComponent<Image>();
		Color color = image.color;
		while (true)
		{
			color.a = Mathf.PingPong(Time.time - startTime, 0.4f) + 0.1f;
			image.color = color;
			yield return null;
		}
	}

	public void Show(UnityAction oldListener)
	{
		graphTutorialMount.SetActive(false);
		hudController.ToDataCollectionMode();
		view.SetActive(true);
		animator.SetBool("On", true);
		Synchronize();
		StartCoroutine(TutorialDelay());
		previousScreen = GameRefs.I.m_globalLastScreen;
		GameRefs.I.m_globalLastScreen = "dataCollect";
		beenOpened = true;
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.UnlockedNewBorough);

		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			graphTutorialMount.SetActive(true);
			GameRefs.I.PostGameState(true, "clickedButton", "closeDataManagement");
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ManageDataScreen);
			Hide();
			GameRefs.I.m_globalLastScreen = previousScreen;
			backButton.onClick.RemoveAllListeners();
			backButton.onClick.AddListener(oldListener);
		});
	}

	public int GetWeeklyCost()
	{
		int totalCost = 0;
		for (int i = 0; i < allocationWidgets.Length; ++i)
		{
			SynchronizeAllocationWidget(i);
			totalCost += dataCollectionParameters.frequencies[GameRefs.I.traitSamplings[traitTypeToStatType[(int)allocationWidgets[i].trait]].slotCount].costPerWeek;
		}

		return totalCost;
	}

	public void Synchronize(bool isAnimated = false)
	{
		// Show unlocked slots.
		for (int i = 0; i < GameRefs.I.storageSlotCount; ++i)
		{
			totalCapacitySlots[i].SetActive(true);
			totalFilledSlots[i].SetActive(i < GameRefs.I.filledSlotCount);
		}

		// Hide locked slots.
		for (int i = GameRefs.I.storageSlotCount; i < dataCollectionParameters.slotCosts.Length; ++i)
		{
			totalCapacitySlots[i].SetActive(false);
			totalFilledSlots[i].SetActive(false);
		}

		if (GameRefs.I.storageSlotCount == dataCollectionParameters.slotCosts.Length)
		{
			nextSlotCostLabel.text = dataCollectionParameters.noMoreSlotsText;
			buySlotButton.interactable = false;
		}
		else
		{
			buySlotButton.interactable = true;
			nextSlotCostLabel.text = string.Format("-${0:n0}", dataCollectionParameters.slotCosts[GameRefs.I.storageSlotCount]);
		}

		int totalCost = 0;
		for (int i = 0; i < allocationWidgets.Length; ++i)
		{
			SynchronizeAllocationWidget(i);
			totalCost += dataCollectionParameters.frequencies[GameRefs.I.traitSamplings[traitTypeToStatType[(int) allocationWidgets[i].trait]].slotCount].costPerWeek;
		}

		currentTotalCost = totalCost;
		totalCostLabel.text = string.Format(dataCollectionParameters.costPerWeekFormat, totalCost);

		if (isAnimated)
		{
			for (int i = 0; i < GameRefs.I.filledSlotCount; ++i)
			{
				Bulge(totalFilledSlots[i].GetComponent<RectTransform>());
			}

			for (int i = 0; i < allocationWidgets.Length; ++i)
			{
				AllocationWidget widget = allocationWidgets[i];
				Bulge(widget.frequencyLabel.GetComponent<RectTransform>());
				for (int iSlot = 0; iSlot < GameRefs.I.traitSamplings[traitTypeToStatType[(int) widget.trait]].slotCount; ++iSlot)
				{
					Bulge(widget.filledSlots[iSlot].GetComponent<RectTransform>());
				}
			}
		}
	}

	void Bulge(RectTransform xform)
	{
		StartCoroutine(CoBulge(xform));
	}

	void FlashFullSlots()
	{
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.Error);
		for (int i = 0; i < GameRefs.I.filledSlotCount; ++i)
		{
			Flash(totalFilledSlots[i].GetComponent<Image>());
		}

		for (int i = 0; i < allocationWidgets.Length; ++i)
		{
			AllocationWidget widget = allocationWidgets[i];
			for (int iSlot = 0; iSlot < GameRefs.I.traitSamplings[traitTypeToStatType[(int) widget.trait]].slotCount; ++iSlot)
			{
				Flash(widget.filledSlots[iSlot].GetComponent<Image>());
			}
		}
	}

	void Flash(Image image)
	{
		StartCoroutine(CoFlash(image));
	}

	IEnumerator CoFlash(Image image)
	{
		Color oldColor = image.color;
		for (int i = 0; i < 2; ++i)
		{
			image.color = Color.white;
			yield return new WaitForSeconds(0.15f);
			image.color = oldColor;
			yield return new WaitForSeconds(0.15f);
		}
		image.color = oldColor;
		
	}

	IEnumerator CoBulge(RectTransform xform)
	{
		yield return Footilities.CoLerp(0.15f, 1.0f, 1.1f, (factor) => xform.localScale = new Vector3(factor, factor, 1));
		yield return Footilities.CoLerp(0.15f, 1.1f, 1.0f, (factor) => xform.localScale = new Vector3(factor, factor, 1));
	}

	IEnumerator TutorialDelay()
	{
		yield return new WaitForSeconds(1f);
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.ManageDataScreen);
	}

	void SynchronizeAllocationWidget(int i)
	{
		AllocationWidget widget = allocationWidgets[i];
		TraitSampling sampling = GameRefs.I.traitSamplings[traitTypeToStatType[(int) widget.trait]];
		widget.frequencyLabel.text = dataCollectionParameters.frequencies[sampling.slotCount].label;
		widget.costLabel.text = string.Format(dataCollectionParameters.costPerWeekFormat, dataCollectionParameters.frequencies[sampling.slotCount].costPerWeek);
		widget.slider.value = sampling.slotCount;

		// Fill or disable slots.
		for (int iSlot = 0; iSlot < widget.capacitySlots.Length; ++iSlot)
		{
			bool isFilled = iSlot < sampling.slotCount;
			widget.capacitySlots[iSlot].SetActive(isFilled);
			widget.filledSlots[iSlot].SetActive(isFilled);
		}

		widget.fill.color = dataCollectionParameters.frequencies[sampling.slotCount].color;
		widget.frequencyLabelBackground.color = dataCollectionParameters.frequencies[sampling.slotCount].color;
	}

	public void Hide()
	{
		hudController.ToMostRecentTrendsMode();
		animator.SetBool("On", false);
		GameRefs.I.m_gameController.Graph.RefreshGraph();
	}
}

class SliderRelease : MonoBehaviour, IPointerUpHandler
{
	public void OnPointerUp(PointerEventData eventData)
	{
		callback();
	}

	public UnityAction callback { get; set; }
}
