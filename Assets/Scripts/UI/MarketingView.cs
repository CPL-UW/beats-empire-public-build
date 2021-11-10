using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using BeauRoutine;
using Utility;

public class MarketingView : MonoBehaviour {
	public enum GenreUnlockType
	{
		ExtraSales=0,
		AttractArtists,
		CreatingHit,
	}
	public enum BoroughUnlockType
	{
		UnlockBorough=0,
		BonusSurgeLength,
		BonusInsight,
	}

	[Header("Shared Stuff")]
	public Button backButton;
	public Animator mainAnimator;
	public UpgradeCardControl[] upgradeCards;
	public TextMeshProUGUI[] upgradePriceText;
	public GameObject card1Arrows;
	public GameObject card1Profiles;
	public GameObject staticTutorialGuy;
	public float largeFontSize = 35f;
	public float smallFontSize = 25f;

	[Header ("Borough Stuff")]
	public Animator[] locks;
	public TextMeshProUGUI[] boroughNameTexts;
	public RectTransform[] boroughChoices;
	public Button[] boroughButtons;

	public TextMeshProUGUI boroughNameText;
	public TextMeshProUGUI populationText;
	public TextMeshProUGUI boroughCaresAboutText;
	public TextMeshProUGUI boroughTrendSpeedText;

	public Image[] boroughSprites;
	public Color selectedColor;
	public Color unlockedColor;
	public Color selectedLockedColor;
	public Color deselectedLockedColor;
	public Color lockedBoroughCardColor;
	public Color unlockedBoroughCardColor;
	public Color lockedBoroughChoiceBar;
	public Color unlockedBoroughChoiceBar;


	[Header("Genre Stuff")]
	public TextMeshProUGUI[] genreNameTexts;
	public Image choiceBarBackground;
	public RectTransform[] genreChoices;
	public Button[] genreButtons;
	public Color[] genreUpgradeCardColors;
	public Color[] genreChoiceBarColors;
	public float deselectedIconSize = 0.65f;
	public float selectedIconSize = 1f;

	private int lastAnimScreen = 1;
	private bool tutorialSeen = false;
	private StatSubType currBorough = StatSubType.TURTLE_HILL;
	private StatSubType currGenre = StatSubType.ROCK;
	private Routine lockRoutine;
	private Routine slideRoutine;
	private Routine screenRoutine;
	private Routine choiceBarColorRoutine;
	private Dictionary<StatSubType, CardUnlocks[]> genreUnlocks;
	private Dictionary<StatSubType, CardUnlocks[]> boroughsUnlocks;
	private Dictionary<StatSubType, string> boroughPopulations;
	private Dictionary<StatSubType, float> upgradeCost;

	private void OnDisable()
	{
		lastAnimScreen = 1;
	}

	[System.Serializable]
	public class CardUnlocks
	{
		public bool[] unlockLevel = new bool[] { false, false, false };

		public CardUnlocks(bool init1=false, bool init2=false, bool init3=false)
		{
			unlockLevel[0] = init1;
			unlockLevel[1] = init2;
			unlockLevel[2] = init3;
		}

		public int getNumUnlocks()
		{
			if (unlockLevel[2])
				return 3;
			else if (unlockLevel[1])
				return 2;
			else if (unlockLevel[0])
				return 1;
			else
				return 0;
		}

		public void setNumUnlocks(int unlocks)
		{
			for (int i = 0; i < 3; i++)
				unlockLevel[i] = unlocks > i;
		}
	}
	// Use this for initialization
	public MarketingView() {

		boroughsUnlocks = new Dictionary<StatSubType, CardUnlocks[]>();
		genreUnlocks = new Dictionary<StatSubType, CardUnlocks[]>();
		boroughPopulations = new Dictionary<StatSubType, string>();
		
		boroughsUnlocks.Add(StatSubType.TURTLE_HILL, new CardUnlocks[3] { new CardUnlocks(true, false, false), new CardUnlocks(), new CardUnlocks() });
		boroughsUnlocks.Add(StatSubType.BOOKLINE, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		boroughsUnlocks.Add(StatSubType.MADHATTER, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		boroughsUnlocks.Add(StatSubType.KINGS_ISLE, new CardUnlocks[3] { new CardUnlocks(true, false, false), new CardUnlocks(), new CardUnlocks() });
		boroughsUnlocks.Add(StatSubType.IRONWOOD, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		boroughsUnlocks.Add(StatSubType.THE_BRONZ, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });

		genreUnlocks.Add(StatSubType.ROCK, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() } );
		genreUnlocks.Add(StatSubType.POP, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		genreUnlocks.Add(StatSubType.RANDB, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		genreUnlocks.Add(StatSubType.HIP_HOP, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		genreUnlocks.Add(StatSubType.RAP, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
		genreUnlocks.Add(StatSubType.ELECTRONIC, new CardUnlocks[3] { new CardUnlocks(), new CardUnlocks(), new CardUnlocks() });
	}

	void Awake()
	{
		upgradeCost = new Dictionary<StatSubType, float>();
		upgradeCost.Add(StatSubType.TURTLE_HILL, 0f);
		upgradeCost.Add(StatSubType.THE_BRONZ, GameRefs.I.m_upgradeVariables.TheBronzUnlockCost);
		upgradeCost.Add(StatSubType.IRONWOOD, GameRefs.I.m_upgradeVariables.IronwoodUnlockCost);
		upgradeCost.Add(StatSubType.KINGS_ISLE, GameRefs.I.m_upgradeVariables.KingsIsleUnlockCost);
		upgradeCost.Add(StatSubType.BOOKLINE, GameRefs.I.m_upgradeVariables.BooklineUnlockCost);
		upgradeCost.Add(StatSubType.MADHATTER, GameRefs.I.m_upgradeVariables.MadhatterUnlockCost);
	}

	void Start()
	{ 
		List<PopulationData> data = GameRefs.I.m_gameController.DataManager.GetPopulationData();

		for (int i = 0; i < data.Count; i++)
		{
			boroughPopulations.Add(data[i].Location, data[i].GetPopulation().ToString("N0", CultureInfo.InvariantCulture));
		}
	}

	// Animation control

	public void OnViewOpened()
	{
		backButton.onClick.RemoveAllListeners();
		backButton.onClick.AddListener(() => {
			GameRefs.I.PostGameState(false, "clickedButton", "backButton");
			GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
			GameRefs.I.m_gameController.OpenStudioView();
			GameRefs.I.m_tutorialController.SetTutorialCompleted(TutorialController.TutorialID.MarketingScreen);
		});
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UIEnterExit);
		GameRefs.I.hudController.ToMarketingMode();

		if (!tutorialSeen)
		{
			GameRefs.I.m_tutorialController.SpawnTutorial(TutorialController.TutorialID.MarketingScreen);
			tutorialSeen = true;
		}
		else
		{
			staticTutorialGuy.gameObject.SetActive(true);
		}
		
		RefreshBoroughs();
		
		mainAnimator.SetBool("On", true);
		if (lastAnimScreen == 1)
		{
			//SetScreenAndCards(1);
			SetBorough(boroughLookupReverse(currBorough));
		}	
		else
		{
			//SetScreenAndCards(2);
			SetGenre(genreLookupReverse(currGenre));
		}
			
	}

	public void OnViewClosed()
	{
		mainAnimator.SetBool("On", false);
	}

	private void SetScreenAndCards(int screen)
	{
		screenRoutine.Replace(this, SetNewScreen(screen));
		lastAnimScreen = screen;
	}

	// End animation control
	public float selectOptionCooldown = 0f;
	public void SelectOption(bool isBorough)
	{
		if (!CheckButtonCooldown())
			return;

		if (isBorough)
			SetScreenAndCards(1);
		else
			SetScreenAndCards(2);
	}

	public void SetBorough(int borough)
	{
		if (!CheckButtonCooldown())
			return;

		for (int i = 0; i < boroughChoices.Length; i++)
		{
			if (i == borough)
				boroughChoices[i].SetScale(selectedIconSize, Axis.XY);
			else
				boroughChoices[i].SetScale(deselectedIconSize, Axis.XY);
		}
		slideRoutine.Replace(this, SetNewBorough(borough, false));
	}

	public void SetGenre(int genre)
	{
		if (!CheckButtonCooldown())
			return;

		for (int i = 0; i < genreChoices.Length; i++)
		{
			if (i == genre)
				genreChoices[i].SetScale(selectedIconSize, Axis.XY);
			else
				genreChoices[i].SetScale(deselectedIconSize, Axis.XY);
		}
		slideRoutine.Replace(this, SetNewGenre(genre, false));
	}

	private bool CheckButtonCooldown()
	{
		if (Time.time - selectOptionCooldown < 0.65f)
		{
			return false;
		}
		else
		{
			selectOptionCooldown = Time.time;
		}
		return true;
	}

	public bool IsBoroughUnlocked(StatSubType borough)
	{
		if (borough.SuperType != StatType.LOCATION)
			return false;

		return boroughsUnlocks[borough][(int)BoroughUnlockType.UnlockBorough].unlockLevel[0];
	}

	public Dictionary<StatSubType, CardUnlocks[]> GetBoroughUnlocks()
	{
		return boroughsUnlocks;
	}
	public Dictionary<StatSubType, CardUnlocks[]> GetGenreUnlocks()
	{
		return genreUnlocks;
	} 

	public int GetNumUnlocks(StatSubType genre, GenreUnlockType type)
	{
		if (genre.SuperType != StatType.GENRE)
			return 0;
		return genreUnlocks[genre][(int)type].getNumUnlocks();
	}

	public int GetNumUnlocks(StatSubType borough, BoroughUnlockType type)
	{
		if (borough.SuperType != StatType.LOCATION)
			return 0;
		return boroughsUnlocks[borough][(int)type].getNumUnlocks();
	}

	public int GetNumBoroughsUnlocked()
	{
		int boroughsUnlocked = 0;
		if (IsBoroughUnlocked(StatSubType.TURTLE_HILL)) boroughsUnlocked++;
		if (IsBoroughUnlocked(StatSubType.IRONWOOD)) boroughsUnlocked++;
		if (IsBoroughUnlocked(StatSubType.MADHATTER)) boroughsUnlocked++;
		if (IsBoroughUnlocked(StatSubType.BOOKLINE)) boroughsUnlocked++;
		if (IsBoroughUnlocked(StatSubType.KINGS_ISLE)) boroughsUnlocked++;
		if (IsBoroughUnlocked(StatSubType.THE_BRONZ)) boroughsUnlocked++;
		return boroughsUnlocked;
	}

	public void Debug_UnlockAllBoroughs()
	{
		List<StatSubType> boroughList = new List<StatSubType> { StatSubType.TURTLE_HILL, StatSubType.KINGS_ISLE, StatSubType.THE_BRONZ, StatSubType.IRONWOOD, StatSubType.BOOKLINE, StatSubType.MADHATTER };
		foreach(StatSubType borough in boroughList)
		{
			for (int i = 0; i < 3; i++)
				boroughsUnlocks[borough][(int)BoroughUnlockType.UnlockBorough].unlockLevel[i] = true;
		}
	}

	public bool AttemptUnlock(int cardID)
	{
		float unlockCost = 0f;

		if(lastAnimScreen == 1) // Borough Unlocks
		{
			if(cardID == 0)
			{
				switch (currBorough.ID)
				{
					case StatSubType.TURTLE_HILL_ID: unlockCost = 0f; break;
					case StatSubType.KINGS_ISLE_ID: unlockCost = GameRefs.I.m_upgradeVariables.KingsIsleUnlockCost; break;
					case StatSubType.THE_BRONZ_ID: unlockCost = GameRefs.I.m_upgradeVariables.TheBronzUnlockCost; break;
					case StatSubType.IRONWOOD_ID: unlockCost = GameRefs.I.m_upgradeVariables.IronwoodUnlockCost; break;
					case StatSubType.BOOKLINE_ID: unlockCost = GameRefs.I.m_upgradeVariables.BooklineUnlockCost; break;
					case StatSubType.MADHATTER_ID: unlockCost = GameRefs.I.m_upgradeVariables.MadhatterUnlockCost; break;
				}

				if (!IsBoroughUnlocked(currBorough) && GameRefs.I.m_gameController.GetCash() >= unlockCost)
				{
					boroughSprites[boroughLookupReverse(currBorough)].color = selectedColor;
					for(int i = 0; i < 3; i++)
						boroughsUnlocks[currBorough][(int)BoroughUnlockType.UnlockBorough].unlockLevel[i] = true;

					GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.AllUpgradesInCatResearched);

					GameRefs.I.m_gameController.RemoveCash(unlockCost);
					locks[boroughLookupReverse(currBorough)].SetInteger("Unlocked", 2);
					mainAnimator.SetInteger("UpgradeCards", 2);
					RefreshBoroughs();
					GameRefs.I.m_gameController.UpdateFollowerCount();
					GameRefs.I.PostGameState(true, "autoEvent", "unlockedMarketingUpgrade");
					return true;
				}
			}
			else // Second two cards for borough unlocks behave as usual
			{
				int currUnlockLevel = boroughsUnlocks[currBorough][cardID].getNumUnlocks();

				if (currUnlockLevel > 2)
					unlockCost = -1f;
				switch (cardID)
				{
					case 0: unlockCost = GameRefs.I.m_upgradeVariables.boroughCard1UnlockCosts[currUnlockLevel]; break;
					case 1: unlockCost = GameRefs.I.m_upgradeVariables.boroughCard2UnlockCosts[currUnlockLevel]; break;
					case 2: unlockCost = GameRefs.I.m_upgradeVariables.boroughCard3UnlockCosts[currUnlockLevel]; break;
				}

				if (unlockCost < 0f)
				{
					upgradePriceText[cardID].text = "Done!";
				}
				else if (GameRefs.I.m_gameController.GetCash() >= unlockCost)
				{
					GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ButtonClickMajor);
					if (currUnlockLevel >= 2)
						GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.AllUpgradesInCatResearched);

					boroughsUnlocks[currBorough][cardID].unlockLevel[currUnlockLevel] = true;
					SetCardPrices(cardID, StatType.LOCATION);
					GameRefs.I.m_gameController.RemoveCash(unlockCost);
					RefreshBoroughs();
					GameRefs.I.PostGameState(true, "autoEvent", "unlockedMarketingUpgrade");
					return true;
				}
			}
		}
		else // Genre unlocks
		{
			int currUnlockLevel = genreUnlocks[currGenre][cardID].getNumUnlocks();

			if (currUnlockLevel > 2)
				unlockCost = -1f;
			else
			{
				switch(cardID)
				{
					case 0: unlockCost = GameRefs.I.m_upgradeVariables.genreCard1UnlockCosts[currUnlockLevel]; break;
					case 1: unlockCost = GameRefs.I.m_upgradeVariables.genreCard2UnlockCosts[currUnlockLevel]; break;
					case 2: unlockCost = GameRefs.I.m_upgradeVariables.genreCard3UnlockCosts[currUnlockLevel]; break;
				}
			}
				

			if (unlockCost < 0f)
			{
				upgradePriceText[cardID].text = "Done!";
			}
			else if (GameRefs.I.m_gameController.GetCash() >= unlockCost)
			{
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.ButtonClickMajor);
				if (currUnlockLevel >= 2)
					GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.AllUpgradesInCatResearched);

				genreUnlocks[currGenre][cardID].unlockLevel[currUnlockLevel] = true;
				SetCardPrices(cardID, StatType.GENRE);
				GameRefs.I.m_gameController.RemoveCash(unlockCost);
				RefreshGenres();
				GameRefs.I.PostGameState(true, "autoEvent", "unlockedMarketingUpgrade");
				return true;
			}

		}

		return false;
	}
	// Private methods

	private void SetUpgradeCardColor(Color c)
	{
		foreach(UpgradeCardControl card in upgradeCards)
		{
			card.SetColor(c);
		}
	}

	private void SetChoiceBarColor(Color c)
	{
		choiceBarColorRoutine.Replace(this, ChoiceBarColorFade(c));
	}
	private IEnumerator ChoiceBarColorFade(Color c)
	{
		yield return choiceBarBackground.ColorTo(c, 0.25f);
	}

	private void RefreshBoroughs()
	{
		if (IsBoroughUnlocked(currBorough))
		{
			mainAnimator.SetInteger("UpgradeCards", 2);

			UpgradeVariables upgradeVars = GameRefs.I.m_upgradeVariables;
			for (int i = 0; i < 3; i++)
			{
				// Upgrade cards [0] just shows borough profile
				upgradeCards[1].SetArrowText(i, string.Format("{0} week{1}", upgradeVars.BonusSurgeLengthWeeks[i], upgradeVars.BonusSurgeLengthWeeks[i] > 0 ? "s" : ""));
				upgradeCards[2].SetArrowText(i, string.Format("+{0}%", upgradeVars.BonusInsightPercentFromUpgrades[i]));

				SetCardPrices(i, StatType.LOCATION);
			}

			SetUpgradeCardColor(unlockedBoroughCardColor);
			SetChoiceBarColor(unlockedBoroughChoiceBar);

			upgradeCards[0].SetUpperCardText(string.Format("<color=#{1}>{0} is available!</color> You can release songs to this borough.", currBorough.Name.ToUpper(), ColorUtility.ToHtmlStringRGB(unlockedBoroughChoiceBar)));
			upgradeCards[0].SetLowerCardText("Upgrade level:");

			upgradeCards[1].SetUpperCardText(string.Format("<color=#{1}>Make trends last longer</color> by understanding what affects the interests in <color=#{1}>{0}</color>.", currBorough.Name.ToUpper(), ColorUtility.ToHtmlStringRGB(unlockedBoroughChoiceBar)));
			upgradeCards[1].SetLowerCardText(string.Format("Trends in {0} last an extra:", Utilities.InterceptText(currBorough.Name).ToUpper()));

			upgradeCards[2].SetUpperCardText(string.Format("<color=#{1}>Earn a bigger bonus</color> for a successfully predicted trend in <color=#{1}>{0}</color>.", currBorough.Name.ToUpper(), ColorUtility.ToHtmlStringRGB(unlockedBoroughChoiceBar)));
			upgradeCards[2].SetLowerCardText("Additional prediction bonus:");
		}
		else
		{
			mainAnimator.SetInteger("UpgradeCards", 1);
			SetUpgradeCardColor(lockedBoroughCardColor);
			SetChoiceBarColor(lockedBoroughChoiceBar);
			upgradeCards[0].SetUpperCardText(string.Format("Expand your operations to <color=#{1}>{0}</color> so you can release songs there.", currBorough.Name.ToUpper(), ColorUtility.ToHtmlStringRGB(unlockedBoroughChoiceBar)));
			upgradeCards[0].SetLowerCardText("Upgrade level:");
		}
	}

	private void EvaluateLocks()
	{
		for (int i = 0; i < locks.Length; i++)
		{
			if (IsBoroughUnlocked(boroughLookup(i)))
				locks[i].gameObject.SetActive(false);
			else
				locks[i].gameObject.SetActive(true);
		}
	}

	private void RefreshGenres()
	{
		UpgradeVariables upgradeVars = GameRefs.I.m_upgradeVariables;
		for (int i = 0; i < 3; i++)
		{
			upgradeCards[0].SetArrowText(i, string.Format("{0} week{1}", upgradeVars.BonusWeeksForSongSales[i], upgradeVars.BonusWeeksForSongSales[i] > 0 ? "s" : ""));
			upgradeCards[1].SetArrowText(i, string.Format("+{0}%", upgradeVars.BonusGetGenreArtistPercent[i]));
			upgradeCards[2].SetArrowText(i, string.Format("{0}%", upgradeVars.BonusChanceToMakeHit[i]));

			SetCardPrices(i, StatType.GENRE);
		}

		upgradeCards[0].SetUpperCardText(string.Format("<color=#{1}>Earn more residual sales</color> by keeping the interest in your <color=#{1}>{0}</color> songs going longer.", Utilities.InterceptText(currGenre.Name).ToUpper(), ColorUtility.ToHtmlStringRGB(genreChoiceBarColors[genreLookupReverse(currGenre)])));
		upgradeCards[0].SetLowerCardText("Song sales last an extra:");

		upgradeCards[1].SetUpperCardText(string.Format("<color=#{1}>Attract more {0} artists to sign</color> by advertising your studio's <color=#{1}>{0}</color> focus.", Utilities.InterceptText(currGenre.Name).ToUpper(), ColorUtility.ToHtmlStringRGB(genreChoiceBarColors[genreLookupReverse(currGenre)])));
		upgradeCards[1].SetLowerCardText(string.Format("Extra chance of {0} applicants:", Utilities.InterceptText(currGenre.Name).ToUpper()));

		upgradeCards[2].SetUpperCardText(string.Format("<color=#{1}>Increase your chances of a hit {0} song</color> after one week of recording.", Utilities.InterceptText(currGenre.Name).ToUpper(), ColorUtility.ToHtmlStringRGB(genreChoiceBarColors[genreLookupReverse(currGenre)])));
		upgradeCards[2].SetLowerCardText("Chance of an instantly epic song:");
	}

	private IEnumerator SetNewScreen(int screen)
	{
		if (screen == 1)
		{
			mainAnimator.SetTrigger("SwitchLeft");
			slideRoutine.Replace(this, SetNewBorough(boroughLookupReverse(currBorough), true));
			yield return 0.15f;
			mainAnimator.SetInteger("Research", 1);
			mainAnimator.SetInteger("UpgradeCards", IsBoroughUnlocked(currBorough) ? 2 : 1);
			mainAnimator.SetTrigger("SwitchLeft");
			yield return 0.2f;
			EvaluateLocks();
		}
		else
		{
			mainAnimator.SetTrigger("SwitchRight");
			slideRoutine.Replace(this, SetNewGenre(genreLookupReverse(currGenre), true));
			mainAnimator.SetInteger("UpgradeCards", 2);
			yield return 0.15f;
			mainAnimator.SetInteger("Research", 2);
			
			mainAnimator.SetTrigger("SwitchRight");
		}
	}

	private IEnumerator SetNewBorough(int borough, bool skipAnim)
	{
		int dir = 0;

		if (borough < boroughLookupReverse(currBorough))
			dir = -1;
		else if (borough > boroughLookupReverse(currBorough))
			dir = 1;

		if(!skipAnim)
		{
			if (dir < 0)
				mainAnimator.SetTrigger("SwitchLeft");
			else if (dir > 0)
				mainAnimator.SetTrigger("SwitchRight");
		}

		for(int i = 0; i < boroughButtons.Length; i++)
		{
			if (i == borough)
				boroughButtons[i].interactable = false;
			else
				boroughButtons[i].interactable = true;
		}

		currBorough = boroughLookup(borough);

		EvaluateLocks();

		for (int i = 0; i < boroughNameTexts.Length; i++)
		{
			if (borough == i)
			{
				SetUpgradeCardColor(IsBoroughUnlocked(boroughLookup(i)) ? unlockedBoroughCardColor : lockedBoroughCardColor);
				boroughNameTexts[i].fontSize = largeFontSize;
				boroughSprites[i].color = IsBoroughUnlocked(boroughLookup(i)) ? selectedColor : selectedLockedColor;
			}
			else
			{
				boroughNameTexts[i].fontSize = smallFontSize;
				boroughSprites[i].color = IsBoroughUnlocked(boroughLookup(i)) ? unlockedColor : deselectedLockedColor;
			}
		}

		card1Arrows.SetActive(false);
		card1Profiles.SetActive(true);

		boroughNameText.text = currBorough.Name;
		populationText.text = boroughPopulations[currBorough];
		bool[] boroughInterests = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(currBorough);
		List<string> caresAboutStrings = new List<string>();
		if (boroughInterests[0]) caresAboutStrings.Add("Genre");
		if (boroughInterests[1]) caresAboutStrings.Add("Mood");
		if (boroughInterests[2]) caresAboutStrings.Add("Topic");

		string caresAbout = "";
		for (int i = 0; i < caresAboutStrings.Count; i++)
		{
			caresAbout += caresAboutStrings[i];
			if (i != caresAboutStrings.Count - 1)
				caresAbout += ", ";
		}

		boroughCaresAboutText.text = caresAbout;
		boroughTrendSpeedText.text = getTrendString(currBorough);

		for (int i = 0; i < boroughNameTexts.Length; i++)
		{
			if (borough == i)
			{
				upgradePriceText[0].text = string.Format("-${0}", upgradeCost[boroughLookup(i)].ToString("N0", CultureInfo.InvariantCulture));
			}
		}

		if (IsBoroughUnlocked(currBorough))
		{
			upgradeCards[0].UpgradeImmediately(3);
		}
		else
		{
			upgradeCards[0].UpgradeImmediately(2);
		}

		yield return 0.3f;
		RefreshBoroughs();

		// Start at 1 because special case for left-most card
		for (int i = 1; i < upgradeCards.Length; i++)
			upgradeCards[i].UpgradeImmediately(boroughsUnlocks[currBorough][i].getNumUnlocks());

		if (!skipAnim)
		{
			if (dir < 0)
				mainAnimator.SetTrigger("SwitchLeft");
			else if (dir > 0)
				mainAnimator.SetTrigger("SwitchRight");
		}
	}

	private IEnumerator SetNewGenre(int genre, bool skipAnim)
	{
		int dir = 0;
		StatSubType genreType = genreLookup(genre);

		if (genre < genreLookupReverse(currGenre))
			dir = -1;
		else if (genre > genreLookupReverse(currGenre))
			dir = 1;

		if (!skipAnim)
		{
			if (dir < 0)
				mainAnimator.SetTrigger("SwitchLeft");
			else if (dir > 0)
				mainAnimator.SetTrigger("SwitchRight");
		}

		for (int i = 0; i < genreButtons.Length; i++)
		{
			if (i == genre)
				genreButtons[i].interactable = false;
			else
				genreButtons[i].interactable = true;
		}

		currGenre = genreType;
		for (int i = 0; i < genreNameTexts.Length; i++)
		{
			if (genre == i)
			{
				SetUpgradeCardColor(genreUpgradeCardColors[i]);
				SetChoiceBarColor(genreChoiceBarColors[i]);
				genreNameTexts[i].fontSize = largeFontSize;
			}
			else
			{
				genreNameTexts[i].fontSize = smallFontSize;
			}
		}
		yield return 0.3f;
		card1Arrows.SetActive(true);
		card1Profiles.SetActive(false);

		RefreshGenres();

		for(int i = 0; i < upgradeCards.Length; i++)
			upgradeCards[i].UpgradeImmediately(genreUnlocks[genreType][i].getNumUnlocks());

		if (!skipAnim)
		{
			if (dir < 0)
				mainAnimator.SetTrigger("SwitchLeft");
			else if (dir > 0)
				mainAnimator.SetTrigger("SwitchRight");
		}
	}

	private void SetCardPrices(int cardID, StatType type)
	{
		UpgradeVariables upgradeVars = GameRefs.I.m_upgradeVariables;
		if (type == StatType.GENRE)
		{
			if (genreUnlocks[currGenre][cardID].getNumUnlocks() > 2)
			{
				upgradePriceText[cardID].text = "Done!";
			}
			else
			{
				switch (cardID)
				{
					case 0: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.genreCard1UnlockCosts[genreUnlocks[currGenre][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					case 1: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.genreCard2UnlockCosts[genreUnlocks[currGenre][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					case 2: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.genreCard3UnlockCosts[genreUnlocks[currGenre][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					default: upgradePriceText[cardID].text = "Done!"; break;
				}
			}
		}
		else // Borough
		{
			if (boroughsUnlocks[currBorough][cardID].getNumUnlocks() > 2)
			{
				upgradePriceText[cardID].text = "Done!";
			}
			else
			{
				switch (cardID)
				{
					case 0: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.boroughCard1UnlockCosts[boroughsUnlocks[currBorough][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					case 1: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.boroughCard2UnlockCosts[boroughsUnlocks[currBorough][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					case 2: upgradePriceText[cardID].text = string.Format("-${0}", upgradeVars.boroughCard3UnlockCosts[boroughsUnlocks[currBorough][cardID].getNumUnlocks()].ToString("N0", CultureInfo.InvariantCulture)); break;
					default: upgradePriceText[cardID].text = "Done!"; break;
				}
			}
		}
	}

	private StatSubType boroughLookup(int i)
	{
		switch(i)
		{
			case 0: return StatSubType.TURTLE_HILL; 
			case 1: return StatSubType.KINGS_ISLE; 
			case 2: return StatSubType.THE_BRONZ; 
			case 3: return StatSubType.IRONWOOD; 
			case 4: return StatSubType.BOOKLINE; 
			case 5: return StatSubType.MADHATTER; 
			default: return StatSubType.NONE;
		}
	}

	private int boroughLookupReverse(StatSubType loc)
	{
		switch (loc.ID)
		{
			case StatSubType.TURTLE_HILL_ID: return 0;
			case StatSubType.KINGS_ISLE_ID: return 1;
			case StatSubType.THE_BRONZ_ID: return 2;
			case StatSubType.IRONWOOD_ID: return 3;
			case StatSubType.BOOKLINE_ID: return 4;
			case StatSubType.MADHATTER_ID: return 5;
			default: return -1;
		}
	}

	private StatSubType genreLookup(int i)
	{
		switch (i)
		{
			case 0: return StatSubType.ROCK;
			case 1: return StatSubType.POP;
			case 2: return StatSubType.RANDB;
			case 3: return StatSubType.HIP_HOP;
			case 4: return StatSubType.RAP;
			case 5: return StatSubType.ELECTRONIC;
			default: return StatSubType.NONE;
		}
	}

	private int genreLookupReverse(StatSubType genre)
	{
		return genre.ID - StatSubType.ROCK_ID;
	}

	private string getTrendString(StatSubType borough)
	{
		switch (borough.ID)
		{
			case StatSubType.TURTLE_HILL_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.TurtleHillTrendString;
			case StatSubType.KINGS_ISLE_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.KingsIsleTrendString;
			case StatSubType.THE_BRONZ_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.TheBronzTrendString;
			case StatSubType.IRONWOOD_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.IronwoodTrendString;
			case StatSubType.BOOKLINE_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.BooklineTrendString;
			case StatSubType.MADHATTER_ID: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.MadhatterTrendString;
			default: return GameRefs.I.m_dataSimulationManager.DataSimulationVariables.TurtleHillTrendString;
		}
	}
}
