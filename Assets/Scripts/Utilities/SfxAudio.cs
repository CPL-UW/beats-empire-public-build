using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SfxAudio : MonoBehaviour {

	public AudioSource source;

	public AudioClip hover;
	public AudioClip toggleClip;
	public AudioClip swapArtistsA;
	public AudioClip swapArtistsB;
	public AudioClip dataToggle;
	public AudioClip recordingBooth;
	public AudioClip recordingToggle;
	public AudioClip buttonClickMajor;
	public AudioClip buttonClickNeutral;
	public AudioClip buttonClickNegative;
	public AudioClip error;
	public AudioClip upgrade;
	public AudioClip advanceWeek;
	public AudioClip UIEnterExit;
	public AudioClip notificationAppear;
	public AudioClip notificationDisappear;
	public AudioClip songOnTopCharts;
	public AudioClip dollarsUp;
	public AudioClip dollarsDown;
	public AudioClip hitMeterGrow;
	public AudioClip stingerHey;
	public AudioClip stingerSpin;
	public AudioClip bonusInsightFound;
	public AudioClip goldOrPlatAchieved;
	public AudioClip allUpgradesInCatResearched;
	private AudioClip winGame; // Loaded async
	public AudioClip loseGame;
	public AudioClip boothClick;

	public enum SfxType
	{
		Hover,
		ToggleBetweenOptions,
		SwapArtistsA,
		SwapArtistsB,
		DataToggle,
		RecordingBooth,
		RecordingToggle,
		ButtonClickMajor,
		ButtonClickNeutral,
		ButtonClickNegtaive,
		Error,
		UpgradeArtist,
		AdvanceWeek,
		UIEnterExit,
		NotificationAppear,
		NotificationDisappear,
		SongOnTopCharts,
		DollarsUp,
		DollarsDown,
		HitMeterGrow,
		StingerHey,
		StingerSpin,
		BonusInsightFound,
		GoldOrPlatAchieved,
		AllUpgradesInCatResearched,
		WinGame,
		LoseGame,
		BoothClick,
		None
	}

	private void Start()
	{
		StartCoroutine(LoadWinTrack());
	}
	public void PlaySfxClip(SfxType type)
	{
		switch(type)
		{
			case SfxType.Hover: source.PlayOneShot(hover); break;
			case SfxType.ToggleBetweenOptions: source.PlayOneShot(toggleClip); break;
			case SfxType.SwapArtistsA: source.PlayOneShot(swapArtistsA); break;
			case SfxType.SwapArtistsB: source.PlayOneShot(swapArtistsB); break;
			case SfxType.DataToggle: source.PlayOneShot(dataToggle); break;
			case SfxType.RecordingBooth: source.PlayOneShot(recordingBooth); break;
			case SfxType.RecordingToggle: source.PlayOneShot(recordingToggle); break;
			case SfxType.ButtonClickMajor: source.PlayOneShot(buttonClickMajor); break;
			case SfxType.ButtonClickNeutral: source.PlayOneShot(buttonClickNeutral); break;
			case SfxType.ButtonClickNegtaive: source.PlayOneShot(buttonClickNegative); break;
			case SfxType.Error: source.PlayOneShot(error); break;
			case SfxType.UpgradeArtist: source.PlayOneShot(upgrade); break;
			case SfxType.AdvanceWeek: source.PlayOneShot(advanceWeek); break;
			case SfxType.UIEnterExit: source.PlayOneShot(UIEnterExit); break;
			case SfxType.NotificationAppear: source.PlayOneShot(notificationAppear); break;
			case SfxType.NotificationDisappear: source.PlayOneShot(notificationDisappear); break;
			case SfxType.SongOnTopCharts: source.PlayOneShot(songOnTopCharts); break;
			case SfxType.DollarsUp: source.PlayOneShot(dollarsUp); break;
			case SfxType.DollarsDown: source.PlayOneShot(dollarsDown); break;
			case SfxType.HitMeterGrow: source.PlayOneShot(hitMeterGrow); break;
			case SfxType.StingerHey: source.PlayOneShot(stingerHey); break;
			case SfxType.StingerSpin: source.PlayOneShot(stingerSpin); break;
			case SfxType.BonusInsightFound: source.PlayOneShot(bonusInsightFound); break;
			case SfxType.GoldOrPlatAchieved: source.PlayOneShot(goldOrPlatAchieved); break;
			case SfxType.AllUpgradesInCatResearched: source.PlayOneShot(allUpgradesInCatResearched); break;
			case SfxType.WinGame: source.PlayOneShot(winGame); break;
			case SfxType.LoseGame: source.PlayOneShot(loseGame); break;
			case SfxType.BoothClick: source.PlayOneShot(boothClick); break;
		}
	}

	IEnumerator LoadWinTrack()
	{
		yield return 5f; // Some delay before loading it
		WWW request = new WWW(Application.streamingAssetsPath + "/Music/success.mp3");
		yield return request;
		winGame = request.GetAudioClip(true, false);
		request.Dispose();
		Debug.Log("WinClip loaded");
	}
}
