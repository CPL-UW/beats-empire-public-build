using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using System.Globalization;
using UnityEngine.UI; 

public class RecordingSlotsTooltipControl : MonoBehaviour
{

	public CanvasGroup tooltipAlpha;
	public GameObject[] inUseSlots;
	public GameObject[] bands;
	public GameObject[] availableSlots;

	public Image[] genreIcon;
	public TextMeshProUGUI[] genreLabel;
	public TextMeshProUGUI[] artistName;
	public TextMeshProUGUI[] songTitle;

	public Image[] songStatus;
	public TextMeshProUGUI[] songStatusText;

	public CanvasGroup[] hitBars; // 0-14 song1, 15-29 song2, 30-44 song3
	public RectTransform[] hitBarsTrans;

	public Sprite[] genreSprites;
	public Vector3 hitBarOffset;
	public float hitBarSpeed;
	public Curve hitBarCurve;

	private Routine m_routine;
	private Vector3[] hitBarOrigPositions;

	private void Awake()
	{
		hitBarOrigPositions = new Vector3[hitBarsTrans.Length];
		for(int i = 0; i < hitBarOrigPositions.Length; i++)
		{
			hitBarOrigPositions[i] = hitBarsTrans[i].localPosition;
		}
	}

	public void ShowTooltip(bool show)
	{
		m_routine.Replace(this, FadeTooltip(show, true));
		List<Song> recordingSongs = GameRefs.I.m_gameController.GetRecordingSongs();
		UpdateTooltipSlots();
	}

	public void UpdateSlotsInstant()
	{
		m_routine.Replace(this, GrowTooltip(false));
	}

	public void UpdateTooltipSlots()
	{
		List<Song> recordingSongs = GameRefs.I.m_gameController.GetRecordingSongs();
		for (int i = 0; i < 3; i++)
		{
			if (recordingSongs.Count > i)
			{
				inUseSlots[i].SetActive(true);
				availableSlots[i].SetActive(false);

				SetTextData(recordingSongs, i);

				for (int j = 0; j < GameRefs.I.m_gameInitVars.MaxSongQuality; j++)
				{
					int hIndex = i * GameRefs.I.m_gameInitVars.MaxSongQuality + j;
					if (recordingSongs[i].Quality >= j)
					{
						hitBarsTrans[hIndex].SetPosition(hitBarOrigPositions[hIndex], Axis.XYZ, Space.Self);
						hitBars[hIndex].alpha = 1f;
					}
					else
					{
						hitBarsTrans[hIndex].SetPosition(hitBarOrigPositions[hIndex] + hitBarOffset, Axis.XYZ, Space.Self);
						hitBars[hIndex].alpha = 0f;
						
					}
				}
			}
			else
			{
				inUseSlots[i].SetActive(false);
				availableSlots[i].SetActive(true);
				for (int j = 0; j < GameRefs.I.m_gameInitVars.MaxSongQuality; j++)
				{
					int hIndex = i * GameRefs.I.m_gameInitVars.MaxSongQuality + j;
					hitBarsTrans[hIndex].SetPosition(hitBarOrigPositions[hIndex] + hitBarOffset, Axis.XYZ, Space.Self);
					hitBars[hIndex].alpha = 0f;
				}

			}
		}
	}

	public IEnumerator AnimateTooltip()
	{
		yield return m_routine.Replace(this, GrowTooltip());
	}

	private IEnumerator GrowTooltip(bool show=true)
	{
		List<Song> recordingSongs = GameRefs.I.m_gameController.GetRecordingSongs();
		// Set band data before routine so we don't get jitter
		for (int i = 0; i < 3; i++)
		{
			if (recordingSongs.Count > i)
			{
				inUseSlots[i].SetActive(true);
				availableSlots[i].SetActive(false);
				SetTextData(recordingSongs, i);
			}
		}

		if(show)
		{
			yield return FadeTooltip(true, false);
			yield return 0.1f;
		}
		

		for (int i = 0; i < 3; i++)
		{
			if (recordingSongs.Count > i)
			{
				inUseSlots[i].SetActive(true);
				availableSlots[i].SetActive(false);

				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.HitMeterGrow);
				for (int j = 0; j < GameRefs.I.m_gameInitVars.MaxSongQuality; j++)
				{
					int hIndex = i * GameRefs.I.m_gameInitVars.MaxSongQuality + j;
					if (recordingSongs[i].Quality >= j && hitBarsTrans[hIndex].localPosition != hitBarOrigPositions[hIndex])
					{
						yield return Routine.Combine(hitBarsTrans[hIndex].MoveTo(hitBarOrigPositions[hIndex], hitBarSpeed, Axis.XYZ, Space.Self).Ease(hitBarCurve),
							hitBars[hIndex].FadeTo(1f, hitBarSpeed));
					}
				}
			}
			else
			{
				inUseSlots[i].SetActive(false);
				availableSlots[i].SetActive(true);
				for (int j = 0; j < GameRefs.I.m_gameInitVars.MaxSongQuality; j++)
				{
					int hIndex = i * GameRefs.I.m_gameInitVars.MaxSongQuality + j;
					hitBarsTrans[hIndex].SetPosition(hitBarOrigPositions[hIndex] + hitBarOffset, Axis.XYZ, Space.Self);
					hitBars[hIndex].alpha = 0f;
				}
			}
		}

		if (show)
		{
			yield return 0.7f;
			yield return FadeTooltip(false, false);
		}
	}

	private void SetTextData(List<Song> recordingSongs, int i)
	{
		genreIcon[i].sprite = genreSprites[GenreToSprite(recordingSongs[i].Artist.GetGenre())];
		genreLabel[i].text = recordingSongs[i].Artist.GetGenre().Name;
		artistName[i].text = recordingSongs[i].Artist.Name;
		songTitle[i].text = recordingSongs[i].Name;

		// Hack alert. Hierarchy must be active for band to be properly posed.
		bool isBandSlotActive = bands[i].activeSelf;
		bands[i].SetActive(true);
		bands[i].GetComponent<RecordingSlotBandController>().band = recordingSongs[i].Artist;
		bands[i].SetActive(isBandSlotActive);

		if (recordingSongs[i].TurnsRecorded < GameRefs.I.m_songReleaseVariables.MinimumTurns)
		{
			songStatusText[i].text = "RECORDING";
			songStatus[i].color = new Color(227f / 255f, 112f / 255f, 152f / 255f);
		}
		else if (recordingSongs[i].DoneRecording)
		{
			songStatusText[i].text = "READY TO RELEASE";
			songStatus[i].color = new Color(251f / 255f, 216f / 255f, 63f / 255f);
		}
		else
		{
			songStatusText[i].text = "RELEASE SONG EARLY?";
			songStatus[i].color = new Color(127f / 255f, 216f / 255f, 193f / 255f);
		}
	}

	private IEnumerator FadeTooltip(bool fadeIn, bool log)
	{
		if (fadeIn)
		{
			for (int i = 0; i < bands.Length; i++)
			{
				bands[i].gameObject.SetActive(true);
			}
		}

		tooltipAlpha.blocksRaycasts = fadeIn;

		if (!fadeIn)
		{
			for (int i = 0; i < bands.Length; i++)
				bands[i].gameObject.SetActive(false);
		}

		yield return tooltipAlpha.FadeTo(fadeIn ? 1f : 0f, 0.1f);

		if (fadeIn && log)
		{
			yield return 0.5f;
			GameRefs.I.PostGameState(false, "tooltipHovered", "recordingSlots");
		}

	}

	private int GenreToSprite(StatSubType genre)
	{
		return genre.ID - StatSubType.ROCK_ID;
	}
}
