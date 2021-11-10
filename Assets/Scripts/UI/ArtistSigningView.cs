using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Utility;

/// <summary>
/// Connected to the Song Creation UI panel
/// </summary>
public class ArtistSigningView : MonoBehaviour
{
	public GameController Controller;
	public Strings strings;
	public GameObject signingView;
	public Button backButton;

	public Animator animator;
	public AnimationClip toOffClip;
	public ArtistSelector ArtistSelectorPrefab;
	public Transform ArtistSelectorRoot;
	public ArtistSigningPanel ArtistPanel;
	public ToggleGroup ArtistSelectorToggle;

	[Header("Widget Primitives")]
	public TextMeshProUGUI headerLabel;
	public TextMeshProUGUI tooltipLabel;
	public TextMeshProUGUI AvailableArtistIndicator;
	public Button NewArtistButton;
	public GameObject spaceHolder;
	public GameObject LeftArrow;
	public GameObject RightArrow;
	public Button signButton;
	public Button startUpgradingButton;
	public Button confirmUpgradeButton;
	public Button stopUpgradingButton;
	public Button endContractButton;
	public Scrollbar scrollBar;

	[Header("Confirmation Popup Items")]
	public GameObject ConfirmationPopup;
	public TextMeshProUGUI ConfirmationPopupText;
	public TextMeshProUGUI ConfirmationHeaderText;
	public TextMeshProUGUI ConfirmationButtonText;
	public GameObject ConfirmationButtonHeader;
	public GameObject EndContractButton;
	public GameObject NewArtistNotification;
	public TextMeshProUGUI NewArtistCountLabel;

	[Header("Swapping")]
	public Button swapCancelButton;
	public Button swapEndContractButton;
	public GameObject swapArtistHeader;
	public TextMeshProUGUI swapArtistNameLabel;

	[Header("Hitboxes")]
	public GameObject[] skillBoxes;
	public GameObject[] skillUpgradeBoxes;

	private List<ArtistSelector> artistSelectors;

	private Band currentBand;
	private List<Band> availableBands;
	private int viewedBand;
	private ArtistViewState viewState;
	private Band pendingBand = null;
	private bool[] hoverBeeenLogged = new bool[4];
	private bool firstTime = true;

	public void Init()
	{
		this.artistSelectors = new List<ArtistSelector>();
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		for (int i = 0; i < skillBoxes.Length; ++i)
		{
			RegisterSkillMouseEvents(skillBoxes[i], i);
			RegisterSkillMouseEvents(skillUpgradeBoxes[i], i);
		}

		startUpgradingButton.onClick.AddListener(() => {
			ArtistPanel.SetSigningPanelsate("upgrade");
			viewState = ArtistViewState.Upgrading;
			animator.SetTrigger("Header");
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.ArtistUpgradeMode);
			SyncButtons();
		});

		stopUpgradingButton.onClick.AddListener(() => {
			ArtistPanel.SetSigningPanelsate("existing");
			viewState = ArtistViewState.Managing;
			animator.SetTrigger("Header");
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ArtistUpgradeMode);
			SyncButtons();
		});
	}

	private void RegisterSkillMouseEvents(GameObject skillBox, int index)
	{
		EventTrigger eventTrigger = skillBox.AddComponent<EventTrigger>();

		eventTrigger.Register(EventTriggerType.PointerEnter, (data) => {
			animator.SetBool("SkillTooltip/On", true);
			animator.SetInteger("SkillTooltip/Arrow", index + 1);
			if (!hoverBeeenLogged[index])
			{
				GameRefs.I.PostGameState(false, "tooltipHovered", skillBox.name);
				hoverBeeenLogged[index] = true;
			}
				
			tooltipLabel.text = strings.skillTooltipLabels[index];
		});

		eventTrigger.Register(EventTriggerType.PointerExit, (data) => {
			animator.SetBool("SkillTooltip/On", false);
		});
	}

	public void Show(bool isShow)
	{
		signingView.SetActive(isShow);
		for (int i = 0; i < hoverBeeenLogged.Length; i++)
			hoverBeeenLogged[i] = false;
	}

	public void OnViewOpened(List<Band> signedBands, List<Band> availableBands, bool keepSelection)
	{
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			GameRefs.I.PostGameState(false, "clickedButton", "backButton");
			animator.SetBool("On", false);
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.SignArtist);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.ArtistUpgradeMode);
			Controller.OpenStudioView();
			Footilities.Schedule(this, toOffClip.length / 1.5f, () => gameObject.SetActive(false));
		});
		GameRefs.I.hudController.ToArtistsMode();

		this.availableBands = availableBands;
		//NewArtistButton.interactable = availableBands.Count > 0;
		NewArtistButton.gameObject.SetActive(availableBands.Count > 0);
		spaceHolder.SetActive(availableBands.Count > 0);

		foreach (ArtistSelector selector in this.artistSelectors)
		{
			GameObject.Destroy(selector.gameObject);
		}

		this.artistSelectors.Clear();

		if (signedBands.Count > 0)
		{
			foreach (Band band in signedBands)
			{
				ArtistSelector newSelector = ArtistSelector.Instantiate(this.ArtistSelectorPrefab);
				newSelector.gameObject.name = string.Format("ArtistSelector_{0}", band.Name);
				this.artistSelectors.Add(newSelector);
			}

			for (int i = 0; i < this.artistSelectors.Count; i++)
			{
				this.artistSelectors[i].InitForSigning(this, signedBands[i]);
			}

			artistSelectors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
		}
		else
		{
			viewState = ArtistViewState.Upgrading;
		}

		while (artistSelectors.Count < GameRefs.I.artistViewParameters.maxArtistCount)
		{
			ArtistSelector selector = ArtistSelector.Instantiate(this.ArtistSelectorPrefab);
			selector.ShowEmpty();
			artistSelectors.Add(selector);
		}

		for (int i = 0; i < this.artistSelectors.Count; i++)
		{
			this.artistSelectors[i].transform.SetParent(this.ArtistSelectorRoot);
			this.artistSelectors[i].transform.localScale = new Vector3(1, 1, 1);
			this.artistSelectors[i].transform.localPosition = new Vector3(this.artistSelectors[i].transform.localPosition.x, this.artistSelectors[i].transform.localPosition.y, 0);
		}

		if (viewState == ArtistViewState.Signing && this.availableBands.Count == 0)
		{
			if (signedBands.Count > 0)
			{
				viewState = ArtistViewState.Managing;
			}
			else
			{
				this.ArtistPanel.gameObject.SetActive(false);
				return;
			}
		}

		if (keepSelection)
		{
			if (viewState == ArtistViewState.Signing)
			{
				this.viewedBand = this.viewedBand % this.availableBands.Count;
				this.SelectBand(this.availableBands[this.viewedBand]);
			}
			else
			{
				this.viewedBand = this.viewedBand % this.artistSelectors.Count;
				this.SelectBand(this.artistSelectors[this.viewedBand].GetBand());
				this.artistSelectors[this.viewedBand].SelectorToggle.isOn = true;
			}
		}
		else
		{
			if (signedBands.Count > 0)
			{
				this.viewedBand = 0;
				viewState = ArtistViewState.Managing;
				this.SelectBand(this.artistSelectors[0].GetBand());
				this.artistSelectors[0].SelectorToggle.isOn = true;
			}
			else
			{
				this.viewedBand = 0;
				viewState = ArtistViewState.Signing;
				this.SelectBand(this.availableBands[0]);
			}
		}

		if (viewState == ArtistViewState.Signing)
		{
			headerLabel.text = strings.signArtistHeader;
		}
		else
		{
			headerLabel.text = strings.manageArtistHeader;
		}

		if(firstTime)
		{
			scrollBar.value = 1f;
			firstTime = false;
		}
		SyncButtons();
	}

	public void OnArtistSelected(ArtistSelector selected)
	{
		// Animate header change if needed.
		ArtistViewState oldViewState = viewState;
		if (viewState == ArtistViewState.Signing)
		{
			animator.SetTrigger("Header");
		}

		int newViewedBand = -1;
		for (int i = 0; i < this.artistSelectors.Count; i++)
		{
			if (selected != this.artistSelectors[i])
			{
				this.artistSelectors[i].IsSelected = false;
			}
			else
			{
				viewState = pendingBand == null ? ArtistViewState.Managing : ArtistViewState.Swapping;
				newViewedBand = i;
			}
		}

		if (newViewedBand >= 0 && (newViewedBand != viewedBand || oldViewState == ArtistViewState.Signing))
		{
			if (oldViewState == ArtistViewState.Signing || newViewedBand > viewedBand)
			{
				viewedBand = (newViewedBand - 1 + artistSelectors.Count) % artistSelectors.Count;
				OnNextArtist();
			}
			else
			{
				viewedBand = (newViewedBand + 1) % artistSelectors.Count;
				OnPreviousArtist();
			}
		}

		SyncButtons();
	}

	public void SyncHeaderText()
	{
		if (viewState == ArtistViewState.Signing)
		{
			headerLabel.text = strings.signArtistHeader;	
		}
		else if (viewState == ArtistViewState.Upgrading)
		{
			headerLabel.text = strings.upgradeArtistHeader;	
		}
		else if (viewState == ArtistViewState.Swapping)
		{
			headerLabel.text = strings.swapArtistHeader;	
		}
		else
		{
			headerLabel.text = strings.manageArtistHeader;	
		}
	}

	public Band GetActiveBand()
	{
		return this.currentBand;
	}

	private void SelectBand(Band band)
	{
		if (viewState != ArtistViewState.Signing)
		{
			ArtistPanel.SetSigningPanelsate("existing");
		}

		this.currentBand = band;

		this.ArtistSelectorToggle.allowSwitchOff = !band.IsSigned;

		if (!band.IsSigned)
		{
			foreach (ArtistSelector selector in this.artistSelectors)
			{
				selector.SelectorToggle.isOn = false;
			}
		}

		LeftArrow.SetActive(viewState == ArtistViewState.Signing && availableBands.Count > 1);
		RightArrow.SetActive(viewState == ArtistViewState.Signing && availableBands.Count > 1);

		this.RefreshNewArtistNotification();

		if (band != null)
		{
			this.ArtistPanel.gameObject.SetActive(true);
			this.ArtistPanel.AssignBand(band);
		}
		else
		{
			this.ArtistPanel.gameObject.SetActive(false);
		}

		this.AvailableArtistIndicator.gameObject.SetActive(viewState == ArtistViewState.Signing);
		this.AvailableArtistIndicator.text = string.Format("{0} OF {1}", this.viewedBand + 1, this.availableBands.Count);

	}

	public void ViewAvailableArtists()
	{
		if (this.availableBands.Count == 0)
		{
			NewArtistButton.gameObject.SetActive(false);
			//NewArtistButton.interactable = false;
			return;
		}

		if (viewState != ArtistViewState.Signing)
		{
			artistSelectors[this.viewedBand].SelectorToggle.isOn = false;
			animator.SetTrigger("Header");
			viewState = ArtistViewState.Signing;
			SyncButtons();
			viewedBand = availableBands.Count - 1;
			OnNextArtist();
		}
	}

	private void SyncButtons()
	{
		endContractButton.gameObject.SetActive(viewState == ArtistViewState.Managing);
		signButton.gameObject.SetActive(viewState == ArtistViewState.Signing);
		startUpgradingButton.gameObject.SetActive(viewState == ArtistViewState.Managing);
		confirmUpgradeButton.gameObject.SetActive(viewState == ArtistViewState.Upgrading);
		stopUpgradingButton.gameObject.SetActive(viewState == ArtistViewState.Upgrading);
	}

	void OnEnable()
	{
		animator.SetBool("On", true);
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
	}

	void EnterSwap()
	{
		Debug.Log("enter");
		viewState = ArtistViewState.Swapping;
		NewArtistButton.gameObject.SetActive(false);	
		swapCancelButton.gameObject.SetActive(true);
		swapEndContractButton.gameObject.SetActive(true);
		swapArtistHeader.SetActive(true);
		swapArtistNameLabel.text = pendingBand.Name;
		SyncButtons();
		SyncHeaderText();
		SelectBand(artistSelectors[0].GetBand());
		artistSelectors[0].SelectorToggle.isOn = true;
	}

	void ExitSwap()
	{
		viewState = ArtistViewState.Managing;
		NewArtistButton.gameObject.SetActive(availableBands.Count > 0);
		swapCancelButton.gameObject.SetActive(false);
		swapEndContractButton.gameObject.SetActive(false);
		swapArtistHeader.SetActive(false);
		pendingBand = null;
		SyncButtons();
		SyncHeaderText();
	}

	public void OnSigned()
	{
		if (!artistSelectors.Any(selector => selector.IsEmpty))
		{
			pendingBand = this.currentBand;
			EnterSwap();
		}
		else
		{
			this.Controller.HireBand(this.currentBand);
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ButtonClickMajor);
		}
		GameRefs.I.PostGameState(true, "clickedButton", "SignArtistButton");
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.NextWeekReady);
		GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.EnterArtists);
		GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.SignArtist, TutorialController.TutorialAction.Confirm);
	}

	public void OnContractEnd()
	{
		if(Controller.GetSignedBands().Count > 1)
		{
			EndContractButton.SetActive(true);
			ConfirmationHeaderText.text = "Are you sure?";
			ConfirmationButtonText.text = "Yeah, End Contract!";
			ConfirmationPopupText.text = "You will lose this artist and any songs the artist has not yet released.";
		}
		else
		{
			EndContractButton.SetActive(false);
			ConfirmationHeaderText.text = "Hold on!";
			ConfirmationButtonText.text = "Got it!";
			ConfirmationPopupText.text = "You cannot fire your only remaining artist! Sign a new group before firing. New bands will be available in the coming weeks.";
		}
		this.ConfirmationPopup.SetActive(true);
	}

	public void OnConfirmFired()
	{
		this.ConfirmationPopup.SetActive(false);
		if (viewState == ArtistViewState.Swapping)
		{
			Controller.FireBand(this.currentBand, false);
			Controller.HireBand(pendingBand);
			foreach (ArtistSelector selector in artistSelectors)
			{
				if (selector.GetBand() == pendingBand)
				{
					SelectBand(pendingBand);
					selector.SelectorToggle.isOn = true;
				}
			}
			ExitSwap();
		}
		else
		{
			this.Controller.FireBand(this.currentBand, true);
		}
		GameRefs.I.PostGameState(true, "clickedButton", "FiringConfirmationButton");
	}

	public void OnCancelFired()
	{
		this.ConfirmationPopup.SetActive(false);
	}

	public void OnSwapCanceled()
	{
		viewState = ArtistViewState.Managing;
		ExitSwap();
	}

	public void RefreshNewArtistNotification()
	{
		this.NewArtistNotification.SetActive(this.availableBands.Count(x => x.IsNew) > 0);
		this.NewArtistCountLabel.text = this.availableBands.Count(x => x.IsNew).ToString();
	}

	public void RefreshArtistList()
	{
		foreach (ArtistSelector selector in this.artistSelectors)
		{
			selector.RefreshTraits();
		}
	}

	public void ShowPreviousArtist()
	{
		if (viewState == ArtistViewState.Signing)
		{
			this.viewedBand = (this.viewedBand + this.availableBands.Count - 1) % this.availableBands.Count;
			this.SelectBand(this.availableBands[this.viewedBand]);
		}
		else
		{
			this.artistSelectors[this.viewedBand].SelectorToggle.isOn = false;
			this.viewedBand = (this.viewedBand + this.artistSelectors.Count - 1) % this.artistSelectors.Count;
			this.artistSelectors[this.viewedBand].SelectorToggle.isOn = true;
			this.SelectBand(this.artistSelectors[this.viewedBand].GetBand());
		}

		animator.SetTrigger("Artist/PreviousArtist");
	}

	public void ShowNextArtist()
	{
		if (viewState == ArtistViewState.Signing)
		{
			this.viewedBand = (this.viewedBand + 1) % this.availableBands.Count;
			this.SelectBand(this.availableBands[this.viewedBand]);
		}
		else
		{
			this.artistSelectors[this.viewedBand].SelectorToggle.isOn = false;
			this.viewedBand = (this.viewedBand + 1) % this.artistSelectors.Count;
			this.artistSelectors[this.viewedBand].SelectorToggle.isOn = true;
			this.SelectBand(this.artistSelectors[this.viewedBand].GetBand());
		}

		animator.SetTrigger("Artist/NextArtist");
	}

	public void OnNextArtist()
	{
		animator.SetTrigger("Artist/NextArtist");
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.SwapArtistsB);
	}

	public void OnPreviousArtist()
	{
		animator.SetTrigger("Artist/PreviousArtist");
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.SwapArtistsB);
	}
}

public enum ArtistViewState
{
	Signing,
	Managing,
	Upgrading,
	Swapping,
}
