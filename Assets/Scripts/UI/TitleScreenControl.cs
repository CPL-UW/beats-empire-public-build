using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeauRoutine;
using TMPro;
using UnityEngine.UI;

public class TitleScreenControl : MonoBehaviour {
	public Button newButton;
	public Button continueButton;
	public Button creditsButton;

	public RectTransform continueOut;
	public RectTransform newGameOut;
	public RectTransform creditsOut;
	public Curve slideOutCurve;
	public float slideOutSeconds;
	public GameInitializer init;
	public GameObject continueObject;
	public TextMeshProUGUI continueText;
	public TextMeshProUGUI newGameText;
	public AudioSource bgMusicSource;
	public Image muteImage;
	public Sprite muteSprite;
	public Sprite playingSprite;
	public GameObject confirmationObject;
	public FadeOutTitleMusic fadeMusic;
	public TextMeshProUGUI versionText;

	public AudioSource sfxAudioSource;
	public AudioClip recordScratch;

	private Routine slideRoutine;
	private bool isMuted = false;

	void Awake()
	{
		versionText.text = PlayerInformation.versionNum;
	}

	public void StartSlideOutAndLoad(bool isNew)
	{
		DisableButtons();
		PlayerInformation.isNewGame = isNew;
		slideRoutine.Replace(this, SlideOut());
	}

	public void DisableButtons()
	{
		newButton.interactable = false;
		continueButton.interactable = false;
		creditsButton.interactable = false;
	}

	public void NewGameClicked()
	{
		// If they are starting a new game when they have the option to continue
		if (continueObject.activeSelf)
		{
			sfxAudioSource.PlayOneShot(recordScratch);
			confirmationObject.SetActive(true);
		}
		else
		{
			StartSlideOutAndLoad(true);
			fadeMusic.FadeOut();
		}
	}

	private IEnumerator SlideOut()
	{
		if (PlayerInformation.isNewGame)
		{
			newGameText.text = "LOADING...";
			yield return Routine.Combine(
				continueOut.AnchorPosTo(new Vector2(4096, continueOut.GetAnchorPos().y), slideOutSeconds).Ease(slideOutCurve).DelayBy(0.25f),
				creditsOut.AnchorPosTo(new Vector2(-2048, creditsOut.GetAnchorPos().y), slideOutSeconds).Ease(slideOutCurve).DelayBy(0.15f)
				);
		}
		else
		{
			continueText.text = "LOADING...";
			yield return Routine.Combine(
				newGameOut.AnchorPosTo(new Vector2(4096, newGameOut.GetAnchorPos().y), slideOutSeconds).Ease(slideOutCurve).DelayBy(0.25f),
				creditsOut.AnchorPosTo(new Vector2(-2048, creditsOut.GetAnchorPos().y), slideOutSeconds).Ease(slideOutCurve).DelayBy(0.15f)
				);
		}

		PlayerInformation.isMuted = isMuted;
		init.StartGame();
	}

	public void ToggleMute()
	{
		isMuted = !isMuted;
		muteImage.sprite = isMuted ? muteSprite : playingSprite;
		bgMusicSource.mute = isMuted;
		sfxAudioSource.mute = isMuted;
	}
}
