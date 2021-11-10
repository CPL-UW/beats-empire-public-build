using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using UnityEngine.UI;
using System.Linq;

public class TopChartsView : MonoBehaviour {

	public enum WinCondition
	{
		NotWinner=0,
		Generalization,
		GenreRock,
		GenrePop,
		GenreHipHop,
		GenereElectronic,
		GenreRAndB,
		GenreRap,
		NoCash
	}

	public Image[] trophyGenreIcons;
	public TopChartsParameters parameters;
	public Animator animator;
	public Button backButton;
	public GameObject songReleaseContainer;
	public GameObject topChartsContainer;
	public Color highlightedColor, normalColor;
	public TextMeshProUGUI titleHeader;
	public TextMeshProUGUI genreHeader;

	[Header("Song Release Chart")]
	public TopChartItem[] songReleaseChartItems;
	public CanvasGroup[] itemAlphas;
	public SongNameGenerator nameGenerator;
	public ArtistNameGenerator artistNameGenerator;
	public Curve AnimateInCurve;

	[Header("Top Songs Chart")]
	public TopChartItem[] topSongs;
	public Toggle[] genreToggles;
	public RectTransform[] genreIconScalers;
	public TextMeshProUGUI[] genreLabels;
	public Image backgroundImage;

	public List<SaveGameController.PreviousSong> songSalesList = new List<SaveGameController.PreviousSong>();
	public List<Song> songObjectSalesList = new List<Song>();

	// Private variables
	private Vector3[] initialPos;
	private Vector3[] initialScale;
	private Routine m_routine;
	private float moveSpeed = 0.15f;
	private float fadeSpeed = 0.2f;
	private float delayInterval = 0.1f;
	private StatSubType lastGenreOpened = StatSubType.ROCK;
	private int[] mostRecentTrophys = new int[6] { 0, 0, 0, 0, 0, 0 };
	private bool[] mostRecentHonorable = new bool[6] { false, false, false, false, false, false };

	void Start()
	{
		initialPos = new Vector3[5];
		initialScale = new Vector3[5];
		for (int i = 0; i < initialPos.Length; i++)
		{
			initialPos[i] = songReleaseChartItems[i].gameObject.transform.position;
			initialScale[i] = songReleaseChartItems[i].gameObject.transform.position;
		}

		genreToggles[0].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.ROCK, isOn));
		genreToggles[1].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.POP, isOn));
		genreToggles[2].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.RANDB, isOn));
		genreToggles[3].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.HIP_HOP, isOn));
		genreToggles[4].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.RAP, isOn));
		genreToggles[5].onValueChanged.AddListener(isOn => OnGenreToggled(StatSubType.ELECTRONIC, isOn));
	}

	private void OnGenreToggled(StatSubType genre, bool isOn)
	{
		int i = genre.ID - StatSubType.ROCK_ID;
		if (isOn)
		{
			if (genre != lastGenreOpened)
			{
				if (lastGenreOpened == StatSubType.NONE)
				{
					FetchTopSongs(genre);
				}
				else
				{
					if (lastGenreOpened.ID < genre.ID)
					{
						animator.SetTrigger("SwitchRight");
					}
					else
					{
						animator.SetTrigger("SwitchLeft");
					}
					lastGenreOpened = genre;
				}
			}
		}
		else
		{
			genreIconScalers[i].localScale = new Vector3(0.65f, 0.65f, 1);
			genreLabels[i].fontSize = 25;
		}
	}

	public void OnRightFinished()
	{
		FetchTopSongs(lastGenreOpened);
		animator.SetTrigger("SwitchRight");
	}

	public void OnLeftFinished()
	{
		FetchTopSongs(lastGenreOpened);
		animator.SetTrigger("SwitchLeft");
	}

	private void RegisterEvents()
	{
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			GameRefs.I.PostGameState(false, "clickedButton", "backButton");
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.TopChartsView);

			// If the player got here through This Week's Releases, Back should
			// return to This Week's Releases.
			if (isThisWeek)
			{
				ShowThisWeek();
			}
			else
			{
				gameObject.SetActive(false);
				GameRefs.I.m_gameController.OpenLastView();
			}
		});
	}

	public void OnViewOpened(bool isBackToStudio = false)
	{
		isThisWeek = false;
		RegisterEvents();

		// This view can be reached from the end game. Don't show the tutorial then.
		if (IsGameOver() == TopChartsView.WinCondition.NotWinner)
		{
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.TopChartsView);
		}

		UpdateGenreCounts();
		genreToggles[lastGenreOpened.ID - StatSubType.ROCK_ID].isOn = true;

		ShowAllTimeHits(isBackToStudio);
	}

	public void FetchTopSongs(StatSubType genre)
	{
		int genreIndex = genre.ID - StatSubType.ROCK_ID;
		backgroundImage.color = GameRefs.I.colors.GenreToColorSet(genre).dark;
		genreIconScalers[genreIndex].localScale = new Vector3(1, 1, 1);
		genreLabels[genreIndex].fontSize = 35;

		lastGenreOpened = genre;
		genreHeader.text = string.Format("{0} SONGS", genre.Name.ToUpper());

		// Why was this in the loop?
		if (songSalesList == null)
			return;

		List<SaveGameController.PreviousSong> hits = songSalesList.Where(song => song.genre == genre.Name).OrderBy(song => song.chartPosition).ThenByDescending(song => song.sales).Take(3).ToList();

		for (int i = 0; i < hits.Count; i++)
		{
			topSongs[i].ShowSong(hits[i], normalColor, false);
			if (hits[i].chartPosition <= 5)
			{
				topSongs[i].ShowTrophy(hits[i].chartPosition, genre);
			}
		}

		for (int i = hits.Count; i < topSongs.Length; i++)
		{
			topSongs[i].ShowPlaceholder(genre);
		}
	}

	public void UpdateGenreCounts()
	{
		StatSubType[] genreList = new StatSubType[6] { StatSubType.ROCK, StatSubType.POP, StatSubType.RANDB, StatSubType.HIP_HOP, StatSubType.RAP, StatSubType.ELECTRONIC };

		for(int i = 0; i < genreList.Length; i++)
		{
			int genreTopCount = 0;
			int genreHonorableCount = 0;
			foreach (SaveGameController.PreviousSong song in songSalesList)
			{
				if (song.genre == genreList[i].Name)
				{
					if (song.chartPosition == 1)
						genreTopCount++;
					else if (song.chartPosition <= 5)
						genreHonorableCount++;
				}
			}
			mostRecentTrophys[i] = genreTopCount;
			mostRecentHonorable[i] = genreTopCount + genreHonorableCount > 0;
		}
	}

	public int GetGenreCompletionStatus(StatSubType genre)
	{
		if (mostRecentTrophys == null)
			return 0;

		UpdateGenreCounts();

		int genreInt = genre.ID - StatSubType.ROCK_ID;
		if (mostRecentTrophys[genreInt] >= 3)
			return 4;
		else if (mostRecentTrophys[genreInt] >= 2)
			return 3;
		else if (mostRecentTrophys[genreInt] >= 1)
			return 2;
		else if (mostRecentHonorable[genreInt])
			return 1;
		else
			return 0;
	}

	// Returns 0 if false, returns 1 if won by specialization, returns 2 if won by generalization
	public WinCondition IsGameOver()
	{
		StatSubType[] genreList = new StatSubType[6] { StatSubType.ROCK, StatSubType.POP, StatSubType.RANDB, StatSubType.HIP_HOP, StatSubType.RAP, StatSubType.ELECTRONIC };
		int[] genreTopCounts = new int[6] { 0, 0, 0, 0, 0, 0 };
		int[] genreHonorableCounts = new int[6] { 0, 0, 0, 0, 0, 0 };
		
		// Add up top and honorable counts
		if(songSalesList != null)
		{
			for (int i = 0; i < genreList.Length; i++)
			{
				foreach (SaveGameController.PreviousSong song in songSalesList)
				{
					if (song.genre == genreList[i].Name)
					{
						if (song.chartPosition == 1)
							genreTopCounts[i]++;
						else if (song.chartPosition <= 5)
							genreHonorableCounts[i]++;
					}
				}
			}
		}

		// Assume we're winning by generalization right now
		bool winnerByGeneralization = true;

		for(int i = 0; i < 6; i++)
		{
			// Check for 3 number one spots in one genre
			if (genreTopCounts[i] >= 3)
			{
				if (genreList[i] == StatSubType.ROCK) return WinCondition.GenreRock;
				else if (genreList[i] == StatSubType.POP) return WinCondition.GenrePop;
				else if (genreList[i] == StatSubType.RANDB) return WinCondition.GenreRAndB;
				else if (genreList[i] == StatSubType.HIP_HOP) return WinCondition.GenreHipHop;
				else if (genreList[i] == StatSubType.RAP) return WinCondition.GenreRap;
				else if (genreList[i] == StatSubType.ELECTRONIC) return WinCondition.GenereElectronic;
			}

			// Check if they ever don't have an honorable mention or #1 spot in a genre
			if(genreTopCounts[i] + genreHonorableCounts[i] == 0)
				winnerByGeneralization = false;
		}

		if (winnerByGeneralization)
			return WinCondition.Generalization;

		if (GameRefs.I.m_gameController.GetCash() < 0)
			return WinCondition.NoCash;

		return WinCondition.NotWinner;
	}

	public void CalculateChartPosition(Song song)
	{
		RegisterEvents();
		isThisWeek = true;
		isBackToStudio = false;
		ShowThisWeek();

		int awardParameter = 0;

		SongReleaseVariables releaseVars = GameRefs.I.m_gameController.SongReleaseVariables;
		genreHeader.text = string.Format("{0} songs", song.Artist.GetGenre()).ToUpper();

		int totalSales = getTotalSales(song);

		if (totalSales >= releaseVars.SpotSalesThresholds[0])	// Song is number 1
		{
			int previousSales = 0;
			SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, 1, song.Quality, totalSales, song.TurnOfCreation);
			songReleaseChartItems[0].ShowSong(songStats, highlightedColor, true);
			songReleaseChartItems[0].ShowTrophy(1, song.Artist.GetGenre());
			songSalesList.Add(songStats);
			songObjectSalesList.Add(song);
			trophyGenreIcons[0].sprite = parameters.PlatinumIconForGenre(song.Artist.GetGenre());
			awardParameter = 1;
			previousSales = totalSales;
			for (int i = 1; i < 5; i++)
			{
				string songName = nameGenerator.GetRandomSongTitle();
				string artistName = artistNameGenerator.GetRandom(); // Eventually we should use an artist name generator
				Song fakeSong = new Song();
				fakeSong.Name = songName;
				fakeSong.Quality = Random.Range(2, GameRefs.I.m_gameInitVars.MaxSongQuality+1);

				int songBelowSales = previousSales > releaseVars.SalesToNextPos ? releaseVars.SalesToNextPos : previousSales / 3;
				int thisSales = previousSales - Random.Range(500, songBelowSales);
				SaveGameController.PreviousSong fakeSongStats = new SaveGameController.PreviousSong(fakeSong.Name, song.Artist.GetGenre().Name, artistName, i + 1, fakeSong.Quality, thisSales, GameRefs.I.m_gameController.currentTurn - 1);
				songReleaseChartItems[i].ShowSong(fakeSongStats, normalColor);
				previousSales = thisSales;
			}
		}

		else if(totalSales >= releaseVars.SpotSalesThresholds[1])	// Song is number 2 spot
		{
			int previousSales = 0;
			SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, 2, song.Quality, totalSales, song.TurnOfCreation);
			songReleaseChartItems[1].ShowSong(songStats, highlightedColor, true);
			songReleaseChartItems[1].ShowTrophy(2, song.Artist.GetGenre());
			songSalesList.Add(songStats);
			songObjectSalesList.Add(song);
			/* trophyGenreIcons[1].sprite = parameters.GoldIconForGenre(song.Artist.GetGenre()); */
			awardParameter = 2;
			previousSales = totalSales;
			for (int i = 0; i < 5; i++)
			{
				string songName = nameGenerator.GetRandomSongTitle();
				string artistName = artistNameGenerator.GetRandom(); // Eventually we should use an artist name generator
				Song fakeSong = new Song();
				fakeSong.Name = songName;
				fakeSong.Quality = Random.Range(2, GameRefs.I.m_gameInitVars.MaxSongQuality+1);

				int thisSales = 0;
				if(i == 0)
				{
					thisSales = totalSales + Random.Range(totalSales + 1000, releaseVars.SalesToNextPos);
					SaveGameController.PreviousSong fakeSongStats = new SaveGameController.PreviousSong(fakeSong.Name, song.Artist.GetGenre().Name, artistName, 1, fakeSong.Quality, thisSales, GameRefs.I.m_gameController.currentTurn - 1);
					songReleaseChartItems[i].ShowSong(fakeSongStats, normalColor);
				}
				else if(i == 1)
				{
					previousSales = totalSales;
					continue;
				}
				else
				{
					int songBelowSales = previousSales > releaseVars.SalesToNextPos ? releaseVars.SalesToNextPos : previousSales / 3;
					thisSales = previousSales - Random.Range(500, songBelowSales);
					SaveGameController.PreviousSong fakeSongStats = new SaveGameController.PreviousSong(fakeSong.Name, song.Artist.GetGenre().Name, artistName, i + 1, fakeSong.Quality, thisSales, GameRefs.I.m_gameController.currentTurn - 1);
					songReleaseChartItems[i].ShowSong(fakeSongStats, normalColor);
				}
				
				previousSales = thisSales;
			}
		}

		else // Pick a random chart pos between 2 numbers if not number 3 spot
		{
			int ourPosition;
			if (totalSales >= releaseVars.SpotSalesThresholds[2])
			{
				ourPosition = 3;
				/* trophyGenreIcons[2].sprite = parameters.GoldIconForGenre(song.Artist.GetGenre()); */
				awardParameter = 3;
				SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, 3, song.Quality, totalSales, song.TurnOfCreation);
				songSalesList.Add(songStats);
				songObjectSalesList.Add(song);
			}
			else if(totalSales >= releaseVars.SpotSalesThresholds[3])
			{
				ourPosition = 4;
				/* trophyGenreIcons[2].sprite = parameters.GoldIconForGenre(song.Artist.GetGenre()); */
				awardParameter = 3;
				SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, 4, song.Quality, totalSales, song.TurnOfCreation);
				songSalesList.Add(songStats);
				songObjectSalesList.Add(song);
			}
			else if(totalSales >= releaseVars.SpotSalesThresholds[4])
			{
				ourPosition = 5;
				/* trophyGenreIcons[2].sprite = parameters.GoldIconForGenre(song.Artist.GetGenre()); */
				awardParameter = 3;
				SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, 5, song.Quality, totalSales, song.TurnOfCreation);
				songSalesList.Add(songStats);
				songObjectSalesList.Add(song);
			}
			else
			{
				float percent = (float)totalSales / (float)releaseVars.SpotSalesThresholds[4];
				ourPosition = releaseVars.LowEndChartPos + (int)((1f-percent) * (releaseVars.HighEndChartPos - releaseVars.LowEndChartPos));
				SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, ourPosition, song.Quality, totalSales, song.TurnOfCreation);
				songSalesList.Add(songStats);
				songObjectSalesList.Add(song);
			}
				
			for (int i = 0; i < 5; i++)
			{
				if (i == 2) // Our song
				{
					SaveGameController.PreviousSong songStats = new SaveGameController.PreviousSong(song.Name, song.Artist.GetGenre().Name, song.Artist.Name, ourPosition, song.Quality, totalSales, song.TurnOfCreation);
					songReleaseChartItems[i].ShowSong(songStats, highlightedColor, true);
					if (ourPosition <= 5)
					{
						songReleaseChartItems[i].ShowTrophy(ourPosition, song.Artist.GetGenre());
					}
				}
				else
				{
					string songName = nameGenerator.GetRandomSongTitle();
					string artistName = artistNameGenerator.GetRandom(); // Eventually we should use an artist name generator
					Song fakeSong = new Song();
					fakeSong.Name = songName;
					fakeSong.Quality = Random.Range(2, GameRefs.I.m_gameInitVars.MaxSongQuality+1);
					int thisSales = 0;
					int songBelowSales = totalSales > releaseVars.SalesToNextPos ? releaseVars.SalesToNextPos : totalSales / 3;
					int song2BelowSales = totalSales > releaseVars.SalelsToNextNextPos ? releaseVars.SalelsToNextNextPos : totalSales * 2 / 3;
					switch (i)
					{
						case 0: thisSales = totalSales + Random.Range(releaseVars.SalesToNextPos, releaseVars.SalelsToNextNextPos); break;
						case 1: thisSales = totalSales + Random.Range(1000, releaseVars.SalesToNextPos); break;
						case 3: thisSales = totalSales - Random.Range(1000, songBelowSales); break;
						case 4: thisSales = totalSales - Random.Range(songBelowSales, song2BelowSales); break;
					}
					SaveGameController.PreviousSong fakeSongStats = new SaveGameController.PreviousSong(songName,song.Artist.GetGenre().Name, artistName, ourPosition + i - 2, fakeSong.Quality, thisSales, GameRefs.I.m_gameController.currentTurn - 1);
					songReleaseChartItems[i].ShowSong(fakeSongStats, normalColor);
				}
			}
		}

		m_routine.Replace(this, AnimateIn(totalSales > releaseVars.SpotSalesThresholds[4], awardParameter));
		GameRefs.I.m_gameController.OnSongCharted();
	}

	private int getTotalSales(Song s)
	{
		int totalSales = 0;
		// Get total sales for our song
		foreach (StatSubType location in StatSubType.GetFilteredList(StatType.LOCATION_ID))
		{
			if (location.ID == StatSubType.RANDOM_ID || location.ID == StatSubType.NONE_ID)
				continue;

			int locationSales = s.TurnData[0][location].Sales;
			totalSales += locationSales;
		}
		return totalSales;
	}

	private IEnumerator AnimateIn(bool topHit, int awardParameter)
	{
		for(int i = 0; i < songReleaseChartItems.Length; i++)
		{
			songReleaseChartItems[i].gameObject.transform.localScale = new Vector3(3f, 3f, 3f);
			itemAlphas[i].alpha = 0;
		}

		yield return Routine.Combine(songReleaseChartItems[0].transform.ScaleTo(1f, moveSpeed, Axis.XYZ).DelayBy(delayInterval * 5).Ease(AnimateInCurve),
			songReleaseChartItems[1].transform.ScaleTo(1f, moveSpeed, Axis.XYZ).DelayBy(delayInterval * 4).Ease(AnimateInCurve),
			songReleaseChartItems[2].transform.ScaleTo(1f, moveSpeed, Axis.XYZ).DelayBy(delayInterval * 3).Ease(AnimateInCurve),
			songReleaseChartItems[3].transform.ScaleTo(1f, moveSpeed, Axis.XYZ).DelayBy(delayInterval * 2).Ease(AnimateInCurve),
			songReleaseChartItems[4].transform.ScaleTo(1f, moveSpeed, Axis.XYZ).DelayBy(delayInterval).Ease(AnimateInCurve),
			itemAlphas[0].FadeTo(1f, fadeSpeed).DelayBy(delayInterval * 5).Ease(AnimateInCurve),
			itemAlphas[1].FadeTo(1f, fadeSpeed).DelayBy(delayInterval * 4).Ease(AnimateInCurve),
			itemAlphas[2].FadeTo(1f, fadeSpeed).DelayBy(delayInterval * 3).Ease(AnimateInCurve),
			itemAlphas[3].FadeTo(1f, fadeSpeed).DelayBy(delayInterval * 2).Ease(AnimateInCurve),
			itemAlphas[4].FadeTo(1f, fadeSpeed).DelayBy(delayInterval).Ease(AnimateInCurve),
			PlaySfxSyncd(delayInterval, topHit)
			);

		animator.SetInteger("Award", awardParameter);
		GameRefs.I.m_gameController.TopChartsHud.UpdateCircles();
	}

	private IEnumerator PlaySfxSyncd(float delayInterval, bool tophit)
	{
		for(int i = 0; i < 5; i++)
		{
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.SongOnTopCharts);
			yield return delayInterval;
		}
		if (tophit)
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.GoldOrPlatAchieved);
	}

	public void ShowAllTimeHits(bool isBackToStudio = true)
	{
		GameRefs.I.m_globalLastScreen = "allTimeHits"; // Hi, Rob. Like this?
		GameRefs.I.hudController.ToAllTimeHitsMode(isBackToStudio);
		songReleaseContainer.SetActive(false);
		topChartsContainer.SetActive(true);
		titleHeader.text = "ALL-TIME HITS";
		FetchTopSongs(lastGenreOpened);
	}

	public void ShowThisWeek()
	{
		GameRefs.I.m_globalLastScreen = "thisWeeksReleases"; // Hi, Rob. Like this?
		GameRefs.I.hudController.ToThisWeekMode();
		songReleaseContainer.SetActive(true);
		topChartsContainer.SetActive(false);
		titleHeader.text = "THIS WEEK'S RELEASES";
	}

	private bool isBackToStudio;
	public void RestoreLayout()
	{
		GameRefs.I.m_gameController.EnableTrendsButton();
		if (isThisWeek)
		{
			ShowThisWeek();
		}
		else
		{
			ShowAllTimeHits(isBackToStudio);
		}
		RegisterEvents();
	}

	public bool isThisWeek { get; set; }
}
