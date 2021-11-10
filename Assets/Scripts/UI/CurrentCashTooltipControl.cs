using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using System.Globalization;

public class CurrentCashTooltipControl : MonoBehaviour
{

	public CanvasGroup tooltipAlpha;
	public GameObject storage;

	public TextMeshProUGUI salesNumber;
	public TextMeshProUGUI listensNumber;
	public TextMeshProUGUI salariesNumber;
	public TextMeshProUGUI storageNumber;
	public TextMeshProUGUI totalNumber;
	public TextMeshProUGUI currentCashNumber;

	public float saveBandUpkeep;
	public float saveCashFromSongs;
	public float saveCashFromListens;
	public float saveStorageCosts;

	private Routine m_routine;
	private bool tooltipLock = false;
	
	public IEnumerator SetData(float bandUpkeep, float cashFromSongs, float cashFromListens, float oldCash, float storageCosts)
	{
		saveBandUpkeep = bandUpkeep;
		saveCashFromListens = cashFromListens;
		saveCashFromSongs = cashFromSongs;
		saveStorageCosts = storageCosts;

		yield return m_routine.Replace(this, CountUp(bandUpkeep, cashFromSongs, cashFromListens, oldCash, storageCosts));
	}

	public void SetDataInstant(float bandUpkeep, float cashFromSongs, float cashFromListens, float storageCosts)
	{
		saveBandUpkeep = bandUpkeep;
		saveCashFromListens = cashFromListens;
		saveCashFromSongs = cashFromSongs;
		saveStorageCosts = storageCosts;

		salesNumber.text = string.Format("{0}${1}", cashFromSongs >= 0f ? "+" : "-", Mathf.Abs(cashFromSongs).ToString("N0", CultureInfo.InvariantCulture));
		listensNumber.text = string.Format("{0}${1}", cashFromListens >= 0f ? "+" : "-", Mathf.Abs(cashFromListens).ToString("N0", CultureInfo.InvariantCulture));
		salariesNumber.text = string.Format("{0}${1}", bandUpkeep >= 0f ? "+" : "-", Mathf.Abs(bandUpkeep).ToString("N0", CultureInfo.InvariantCulture));
		storageNumber.text = string.Format("{0}${1}", storageCosts >= 0f ? "+" : "-", Mathf.Abs(storageCosts).ToString("N0", CultureInfo.InvariantCulture));

		float total = bandUpkeep + cashFromSongs + cashFromListens + storageCosts;
		totalNumber.text = string.Format("{0}${1}", total >= 0f ? "+" : "-", Mathf.Abs(total).ToString("N0", CultureInfo.InvariantCulture));
	}

	public void ShowTooltip(bool show)
	{
		if (tooltipLock)
			return;
		m_routine.Replace(this, FadeTooltip(show, true));
	}

	private IEnumerator FadeTooltip(bool fadeIn, bool postLog)
	{ 
		tooltipAlpha.blocksRaycasts = fadeIn;
		yield return tooltipAlpha.FadeTo(fadeIn ? 1f : 0f, 0.1f);
		if(fadeIn && postLog)
		{
			yield return 0.5f;
			GameRefs.I.PostGameState(false, "tooltipHovered", "cashFlow");
		}
	}

	private IEnumerator CountUp(float bandUpkeep, float cashFromSongs, float cashFromListens, float oldCash, float storageCosts)
	{
		if (bandUpkeep == 0f && cashFromSongs == 0f && cashFromListens == 0f && storageCosts == 0f)
			yield break;

		salesNumber.text = "+$0";
		listensNumber.text = "+$0";
		salariesNumber.text = "+$0";
		storageNumber.text = "+0";
		totalNumber.text = "+$0";

		tooltipLock = true;
		yield return FadeTooltip(true, false);

		int steps = 15;
		float fSteps = (float)steps;

		salesNumber.text = string.Format("{0}${1}", cashFromSongs >= 0f ? "+" : "-", Mathf.Abs(cashFromSongs).ToString("N0", CultureInfo.InvariantCulture));
		listensNumber.text = string.Format("{0}${1}", cashFromListens >= 0f ? "+" : "-", Mathf.Abs(cashFromListens).ToString("N0", CultureInfo.InvariantCulture));
		salariesNumber.text = string.Format("{0}${1}", bandUpkeep >= 0f ? "+" : "-", Mathf.Abs(bandUpkeep).ToString("N0", CultureInfo.InvariantCulture));
		storageNumber.text = string.Format("{0}${1}", storageCosts >= 0f ? "+" : "-", Mathf.Abs(storageCosts).ToString("N0", CultureInfo.InvariantCulture));

		float total = bandUpkeep + cashFromSongs + cashFromListens + storageCosts;

		GameRefs.I.m_sfxAudio.PlaySfxClip(total >= 0f ? SfxAudio.SfxType.DollarsUp : SfxAudio.SfxType.DollarsDown);
		for(int i = 0; i <= steps; i++)
		{
			totalNumber.text = string.Format("{0}${1}", total >= 0f ? "+" : "-", Mathf.Abs(total * i / fSteps).ToString("N0", CultureInfo.InvariantCulture));
			yield return 0.02f;
		}

		for(int i = 0; i <= steps; i++)
		{
			currentCashNumber.text = string.Format("${0:#,##0}", oldCash + (total * i / fSteps));
			yield return 0.02f;
		}

		currentCashNumber.text = string.Format("${0:#,##0}", GameRefs.I.m_gameController.GetCash());

		yield return .7f;

		yield return FadeTooltip(false, false);
		tooltipLock = false;
	}
}
