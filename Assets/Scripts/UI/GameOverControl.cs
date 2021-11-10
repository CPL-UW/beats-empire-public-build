using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BeauRoutine;

public class GameOverControl : MonoBehaviour {

	public TextMeshProUGUI reasonText;
	public TextMeshProUGUI headerText;
	public GameObject gameEndView;
	public Animator gameEndAnimation;
	public Image genreImage;
	public GameObject recordingView;
	public HelpGuyEndController helper;

	public Sprite popSprite;
	public Sprite hiphopSprite;
	public Sprite elecSprite;
	public Sprite rbSprite;
	public Sprite rockSprite;
	public Sprite rapSprite;

	private bool hasBeenShown = false;
	private Routine endGameRoutine;

	public TopChartsView.WinCondition testWin;

	[ContextMenu("WINNER")]
	public void TriggerEndGame()
	{
		EndGame(testWin);
	}

	void Start()
	{
		gameEndAnimation.keepAnimatorControllerStateOnDisable = true;
	}

	public void ReturnToTitle()
	{
		// Save to flag that we need a new game next time (delete their save?)
		endGameRoutine.Replace(this, EndGameRoutine());
	}


	private IEnumerator EndGameRoutine()
	{
		yield return 0.5f;
		SessionController.S.ClearListeners();
		SceneManager.LoadScene("Pregame");
	}

	public void EndGame(TopChartsView.WinCondition reason)
	{
		gameEndView.SetActive(true);
		if (!hasBeenShown)
		{
			GameRefs.I.m_gameController.Saves.SaveGameState(true);
			GameRefs.I.m_recordingAudio.EndGameStopAudio();

			if(reason == TopChartsView.WinCondition.NoCash)
			{
				GameRefs.I.PostGameState(true, "autoEvent", "gameLose_noCash");
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.LoseGame);
			}
			else if(reason != TopChartsView.WinCondition.NotWinner)
			{
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.WinGame);
				helper.Confirm();
			}

			gameEndAnimation.Update(0);
			headerText.text = "Nice Work, Boss!";
			recordingView.SetActive(false);
			int weeksFinished = GameRefs.I.m_gameController.currentTurn - GameRefs.I.m_gameInitVars.StartingWeeks;
			switch (reason)
			{
				case TopChartsView.WinCondition.Generalization:
					gameEndAnimation.SetInteger("ScreenState", 1);
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_generalization");
					reasonText.text = string.Format("We've released a Gold Record for each genre in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenereElectronic:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = elecSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_electronic");
					reasonText.text = string.Format("We've released 3 Platinum Electronic Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenreHipHop:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = hiphopSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_hiphop");
					reasonText.text = string.Format("We've released 3 Platinum Hip Hop Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenrePop:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = popSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_pop");
					reasonText.text = string.Format("We've released 3 Platinum Pop Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenreRAndB:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = rbSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_rnb");
					reasonText.text = string.Format("We've released 3 Platinum R&B Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenreRap:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = rapSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_rap");
					reasonText.text = string.Format("We've released 3 Platinum Rap Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.GenreRock:
					gameEndAnimation.SetInteger("ScreenState", 2);
					genreImage.sprite = rockSprite;
					GameRefs.I.PostGameState(true, "autoEvent", "gameWin_rock");
					reasonText.text = string.Format("We've released 3 Platinum Rock Records in {0} weeks! Thanks for putting our studio on the map!", weeksFinished);
					break;
				case TopChartsView.WinCondition.NoCash:
					headerText.text = "Studio Closed!";
					reasonText.text = "You ran out of money before you could reach your goals. Try again and I bet you can do it!";
					gameEndAnimation.SetInteger("ScreenState", 3);
					break;
			}
			gameEndAnimation.SetBool("On", true);

			hasBeenShown = true;
		}

		GameRefs.I.hudController.ToOverMode();
	}

	public void Hide()
	{
		gameEndView.SetActive(false);
	}
}
