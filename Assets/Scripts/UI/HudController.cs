using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour {
	public Colors colors;
	public GameObject headerBackground;
	public GameObject headerSlideRoot;
	public GameObject screenHeaderBackground;
	public Image headerTextBackgroundImage;
	public Image muteButtonImage;
	public GameObject studioLabel;
	public GameObject backLabel;
	public Animator saveAnimator;
	public GameObject cashWidget;
	public GameObject fansWidget;
	public GameObject recordingSlotsWidget;
	public GameObject topChartsWidget;
	public GameObject topChartsImage;
	public GameObject recordingImage;
	public GameObject artistsImage;
	public GameObject manageDataImage;
	public GameObject marketingImage;
	public GameObject resultsImage;
	public GameObject lockOverlay;

	public enum TrendsLayout {
		Back,
		Studio,
		Predict
	};

	public TrendsLayout lastTrendsLayout;

	public void Lock()
	{
		lockOverlay.SetActive(true);
	}

	public void Unlock()
	{
		lockOverlay.SetActive(false);
	}

	private void DisableAllHeaderImages()
	{
		screenHeaderBackground.SetActive(false);
		topChartsImage.SetActive(false);
		recordingImage.SetActive(false);
		artistsImage.SetActive(false);
		manageDataImage.SetActive(false);
		marketingImage.SetActive(false);
		resultsImage.SetActive(false);
	}

	public void IndicateSave()
	{
		saveAnimator.SetTrigger("Saved");
	}

	public void ToOverMode()
	{
		DisableAllHeaderImages();
		GameRefs.I.m_gameController.SetBackToStudioButton(false);
		headerBackground.SetActive(true);
		headerSlideRoot.SetActive(true);
		muteButtonImage.color = colors.hudDefaultMuteBackgroundColor;
		EnableWidgets(true, true);
	}

	public void ToArtistsMode()
	{
		SetHeader(artistsImage);
		headerTextBackgroundImage.color = colors.hudArtistsBackgroundColor;
		muteButtonImage.color = colors.hudArtistsBackgroundColor;
		backLabel.SetActive(false);
		studioLabel.SetActive(true);
		EnableWidgets(true, true);
	}

	public void ToMarketingMode()
	{
		SetHeader(marketingImage);
		headerTextBackgroundImage.color = colors.hudMarketingBackgroundColor;
		muteButtonImage.color = colors.hudMarketingBackgroundColor;
		backLabel.SetActive(false);
		studioLabel.SetActive(true);
		EnableWidgets(true, true);
	}

	public void ToRecordingMode()
	{
		SetHeader(recordingImage);
		GameRefs.I.m_gameController.SetBackToStudioButton(true);
		headerBackground.SetActive(true);
		headerSlideRoot.SetActive(true);
		headerTextBackgroundImage.color = colors.hudRecordingBackgroundColor;
		muteButtonImage.color = colors.hudRecordingBackgroundColor;
		backLabel.SetActive(false);
		studioLabel.SetActive(true);
		EnableWidgets(true, true);
	}

	public void ToResultsMode()
	{
		SetHeader(resultsImage);
		GameRefs.I.m_gameController.EnableTrendsButton(true);
		GameRefs.I.m_gameController.SetBackToStudioButton(false);
		headerTextBackgroundImage.color = colors.hudResultsBackgroundColor;
		muteButtonImage.color = colors.hudResultsBackgroundColor;
		backLabel.gameObject.SetActive(false);
		studioLabel.gameObject.SetActive(false);
		EnableWidgets(true, true);
	}

	private void SetHeader(GameObject image)
	{
		DisableAllHeaderImages();
		image.SetActive(true);
		headerBackground.SetActive(true);
		headerSlideRoot.SetActive(true);
		screenHeaderBackground.SetActive(true);
	}

	public void ToThisWeekMode()
	{
		SetHeader(topChartsImage);
		headerTextBackgroundImage.color = colors.hudResultsBackgroundColor;
		muteButtonImage.color = colors.hudResultsBackgroundColor;
		backLabel.gameObject.SetActive(false);
		studioLabel.gameObject.SetActive(false);
		GameRefs.I.m_gameController.SetBackToStudioButton(false);
		EnableWidgets(true, true);
	}

	public void ToAllTimeHitsMode(bool isBackToStudio)
	{
		SetHeader(topChartsImage);
		headerTextBackgroundImage.color = colors.hudResultsBackgroundColor;
		muteButtonImage.color = colors.hudResultsBackgroundColor;
		backLabel.gameObject.SetActive(!isBackToStudio);
		studioLabel.gameObject.SetActive(isBackToStudio);
		GameRefs.I.m_gameController.SetBackToStudioButton(true);
		EnableWidgets(true, true);
	}

	private void EnableWidgets(bool isEnabled, bool isTopChartsEnabled = true)
	{
		topChartsWidget.SetActive(isTopChartsEnabled);
		fansWidget.SetActive(isEnabled);
		cashWidget.SetActive(isEnabled);
		recordingSlotsWidget.SetActive(isEnabled);
	}

	private void ToTrendsMode()
	{
		DisableAllHeaderImages();
		headerBackground.SetActive(false);
		headerSlideRoot.SetActive(false);
		muteButtonImage.color = colors.hudDefaultMuteBackgroundColor;
		EnableWidgets(false, false);
	}

	public void ToTrendsPredictMode()
	{
		ToTrendsMode();
		GameRefs.I.m_gameController.SetBackToStudioButton(false);
		backLabel.SetActive(false);
		studioLabel.SetActive(false);
		lastTrendsLayout = TrendsLayout.Predict;
	}

	public void ToTrendsStudioMode()
	{
		ToTrendsMode();
		GameRefs.I.m_gameController.SetBackToStudioButton(true);
		backLabel.SetActive(false);
		studioLabel.SetActive(true);
		headerSlideRoot.SetActive(true);
		lastTrendsLayout = TrendsLayout.Studio;
	}

	public void ToTrendsBackMode()
	{
		ToTrendsMode();
		GameRefs.I.m_gameController.SetBackToStudioButton(true);
		backLabel.SetActive(true);
		studioLabel.SetActive(false);
		headerSlideRoot.SetActive(true);
		lastTrendsLayout = TrendsLayout.Back;
	}

	public void ToMostRecentTrendsMode()
	{
		if (lastTrendsLayout == TrendsLayout.Studio)
		{
			ToTrendsStudioMode();
		}
		else if (lastTrendsLayout == TrendsLayout.Predict)
		{
			ToTrendsPredictMode();
		}
		else
		{
			ToTrendsBackMode();
		}
	}

	public void ToDataCollectionMode()
	{
		SetHeader(manageDataImage);
		headerTextBackgroundImage.color = colors.hudResultsBackgroundColor;
		GameRefs.I.m_gameController.SetBackToStudioButton(true);
		muteButtonImage.color = colors.hudResultsBackgroundColor;
		backLabel.SetActive(true);
		studioLabel.SetActive(false);
		EnableWidgets(true, false);
	}

	public void ToOfficeMode()
	{
		DisableAllHeaderImages();
		GameRefs.I.m_gameController.SetBackToStudioButton(false);
		muteButtonImage.color = colors.hudDefaultMuteBackgroundColor;
		headerBackground.SetActive(true);
		backLabel.SetActive(false);
		studioLabel.SetActive(false);
		EnableWidgets(true, true);
	}
}
