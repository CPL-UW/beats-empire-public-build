using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Utility;

public class ArtistSelector : MonoBehaviour
{
	public Colors colors;
	public Image genreTexture;
	public GameObject activeBackground;
	public Image selectionBackground;

    public Toggle SelectorToggle;
    public TextMeshProUGUI BandNameLabel;
    public TextMeshProUGUI BandGenreLabel;
    public Image BandGenreBacker;
    public GameObject RecordingMarker;
    public GameObject ReadyMarker;
    public GameObject DoneMarker;
    /* public Sprite[] GenreImages; */
	public GameObject[] memberModels;

	public List<CanvasGroup> MoodMarkers;
    public List<CanvasGroup> TopicMarkers;

    public List<TextMeshProUGUI> MoodLabels;
    public List<TextMeshProUGUI> TopicLabels;

	public GameObject artistCard;
	public GameObject emptyCard;

    private Release songView;
    private ArtistSigningView signingView;
	private MarketingInsights insightsView;

    private Band band;
    private bool isRecording;
	private int myTrendsViewIndex;
	private Coroutine rotateTask;

    private void GenericInit(Band band)
    {
		this.SelectorToggle.group = this.GetComponentInParent<ToggleGroup>();

        this.band = band;

        this.BandNameLabel.text = band.Name;

        this.RefreshAppearance();
		this.RefreshTraits();

		// ArtistCard in SigningView doesn't have this object, but it does have this script. :(
		if (genreTexture != null)
		{
			genreTexture.color = colors.GenreToColorSet(band.GetGenre()).midtone;
		}

		BandGenreBacker.sprite = GameRefs.I.sharedParameters.SpriteSmallForGenre(band.GetGenre());
		band.Incarnate(memberModels);
        this.BandGenreLabel.text = this.band.GetGenre().Name.ToUpper();

		// Hover events.
		if (activeBackground != null && selectionBackground != null) {
			EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
			eventTrigger.Register(EventTriggerType.PointerEnter, (data) => {
				activeBackground.SetActive(true);
				selectionBackground.color = colors.hoverBackgroundColor;
				selectionBackground.CrossFadeAlpha(1, 0, false);
				Rotate();
			});
			eventTrigger.Register(EventTriggerType.PointerExit, (data) => {
				activeBackground.SetActive(IsSelected);
				selectionBackground.color = colors.selectedBackgroundColor;
				if (!IsSelected)
				{
					selectionBackground.CrossFadeAlpha(0, 0, false);
					if (rotateTask != null)
					{
						StopCoroutine(rotateTask);
						rotateTask = null;
					}
				}
			});
		}
    }

	void Rotate() {
		 if (gameObject.activeInHierarchy) 
		 { 
			if (rotateTask != null)
			{
				StopCoroutine(rotateTask);
			}
			rotateTask = StartCoroutine(CoRotate());
		 } 
	}

	IEnumerator CoRotate()
	{
		RectTransform[] xforms = {
			activeBackground.GetComponent<RectTransform>(),
			selectionBackground.GetComponent<RectTransform>(),
		};

		// This approach is subject to floating point cancellation issues, and
		// it should probably be improved.

		float startTime = Time.time;
		float amplitude = 1.5f;
		float speed = 3.0f;

		while (true)
		{
			float sine = Mathf.Sin((Time.time - startTime) * speed);
			Vector3 angles = Vector3.forward * sine * amplitude;
			xforms[0].eulerAngles = angles;
			xforms[1].eulerAngles = angles;
			yield return null;
		}
	}

    public void InitForRecording(Release songView, Band band)
    {
        this.songView = songView;
        this.GenericInit(band);
    }

    public void InitForSigning(ArtistSigningView signingView, Band band)
    {
        this.signingView = signingView;
        this.GenericInit(band);
    }

    public void InitForInsights(MarketingInsights insightsView, Band band, int index)
    {
		myTrendsViewIndex = index;
		this.insightsView = insightsView;
		this.GenericInit(band);
    }

    public void RefreshAppearance()
    {
        this.isRecording = band.IsRecordingSong();

        this.RecordingMarker.SetActive(this.isRecording);

        if (this.isRecording)
        {
            this.ReadyMarker.SetActive(this.band.GetRecordingSong().ReadyToRelease);
            this.DoneMarker.SetActive(this.band.GetRecordingSong().DoneRecording);
        }
        else
        {
            this.ReadyMarker.SetActive(false);
            this.DoneMarker.SetActive(false);
        }
    }

	public void RefreshTraits()
	{
		if (this.band == null)
			return;

		List<StatSubType> bandKnownTraits = this.band.GetKnownTraits();

		this.MoodLabels[0].text = StatSubType.MOOD1.Name.InterceptText()[0].ToString();
		this.MoodLabels[1].text = StatSubType.MOOD2.Name.InterceptText()[0].ToString();
		this.MoodLabels[2].text = StatSubType.MOOD3.Name.InterceptText()[0].ToString();
		this.MoodLabels[3].text = StatSubType.MOOD4.Name.InterceptText()[0].ToString();
		this.MoodLabels[4].text = StatSubType.MOOD5.Name.InterceptText()[0].ToString();
		this.MoodLabels[5].text = StatSubType.MOOD6.Name.InterceptText()[0].ToString();

		this.TopicLabels[0].text = StatSubType.TOPIC1.Name.InterceptText()[0].ToString();
		this.TopicLabels[1].text = StatSubType.TOPIC2.Name.InterceptText()[0].ToString();
		this.TopicLabels[2].text = StatSubType.TOPIC3.Name.InterceptText()[0].ToString();
		this.TopicLabels[3].text = StatSubType.TOPIC4.Name.InterceptText()[0].ToString();
		this.TopicLabels[4].text = StatSubType.TOPIC5.Name.InterceptText()[0].ToString();
		this.TopicLabels[5].text = StatSubType.TOPIC6.Name.InterceptText()[0].ToString();

		this.MoodMarkers[0].alpha = bandKnownTraits.Contains(StatSubType.MOOD1) ? 1 : 0.1f;
		this.MoodMarkers[1].alpha = bandKnownTraits.Contains(StatSubType.MOOD2) ? 1 : 0.1f;
		this.MoodMarkers[2].alpha = bandKnownTraits.Contains(StatSubType.MOOD3) ? 1 : 0.1f;
		this.MoodMarkers[3].alpha = bandKnownTraits.Contains(StatSubType.MOOD4) ? 1 : 0.1f;
		this.MoodMarkers[4].alpha = bandKnownTraits.Contains(StatSubType.MOOD5) ? 1 : 0.1f;
		this.MoodMarkers[5].alpha = bandKnownTraits.Contains(StatSubType.MOOD6) ? 1 : 0.1f;

		this.TopicMarkers[0].alpha = bandKnownTraits.Contains(StatSubType.TOPIC1) ? 1 : 0.1f;
		this.TopicMarkers[1].alpha = bandKnownTraits.Contains(StatSubType.TOPIC2) ? 1 : 0.1f;
		this.TopicMarkers[2].alpha = bandKnownTraits.Contains(StatSubType.TOPIC3) ? 1 : 0.1f;
		this.TopicMarkers[3].alpha = bandKnownTraits.Contains(StatSubType.TOPIC4) ? 1 : 0.1f;
		this.TopicMarkers[4].alpha = bandKnownTraits.Contains(StatSubType.TOPIC5) ? 1 : 0.1f;
		this.TopicMarkers[5].alpha = bandKnownTraits.Contains(StatSubType.TOPIC6) ? 1 : 0.1f;
	}

	public Band Band
	{
		get {
			return band;
		}
	}

    public Band GetBand()
    {
        return this.band;
    }

    public bool IsBandRecording()
    {
        return this.isRecording;
    }

    public void OnToggleClicked(bool isOn)
    {
		activeBackground.SetActive(isOn);

        if (isOn)
        {
			Rotate();
            if (this.songView != null)
            {
                this.songView.OnArtistSelected(this);
            }

            if (this.signingView != null)
            {
                this.signingView.OnArtistSelected(this);
			}

			if(this.insightsView != null)
			{
				this.insightsView.OnArtistSelected(this);
				insightsView.SetCurrentBand(myTrendsViewIndex);
			}
        }
		else
		{
			if (rotateTask != null)
			{
				StopCoroutine(rotateTask);
				rotateTask = null;
			}
		}

    }

	public void ShowEmpty()
	{
		artistCard.SetActive(false);
		emptyCard.SetActive(true);
	}

	public bool IsEmpty
	{
		get
		{
			return emptyCard.activeInHierarchy;
		}
	}

	public bool IsSelected {
		get
		{
			return SelectorToggle.isOn;
		}
		set
		{
			SelectorToggle.isOn = value;
		}
	}

	public int Priority
	{
		get
		{
			if (band.IsRecordingSong())
			{
				Song song = band.GetRecordingSong();
				if (song.DoneRecording)
				{
					return 0;
				}
				else if (song.ReadyToRelease)
				{
					return 1;
				}
				else
				{
					return 2;
				}
			}
			else
			{
				return 3;
			}
		}
	}
}
