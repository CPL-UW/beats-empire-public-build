using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using System.Linq;

public class TutorialController : MonoBehaviour {

	public enum TutorialID
	{
		None = 0,
		ArriveAtStudio,
		EnterArtists,
		SignArtist,
		ReturnToStudio,
		EnterRecording,
		EnterFindTrends,
		ReturnToRecording,
		StartRecording,
		NextWeekReady,
		NextWeekClicked,
		FirstSongReady,
		RecordingWhenReady,
		RecordingAfterRelease,
		StudioFloorFinal,

		ForReferenceTrends,
		MarketingScreen,
		ManageDataScreen,
		ArtistUpgradeMode,
		TopChartsView,

		NoArtistUpgradesAfterXWeeks,
		CanUnlockBoroughAfterYWeeks,
		UnlockedNewBorough
	}

	public enum ComeFromDirection
	{
		Bottom,
		Left,
		Right,
		Top
	}

	[System.Serializable]
	public class TutorialEvent
	{
		public TutorialID id;
		public TutorialID prerequisiteId;
		public TutorialObject charTextInfo;
		public bool animateOut;
		public ComeFromDirection direction;
		public RectTransform tutorialMount;


		public bool triggered;

		public bool completed;

		public GameObject tutorialObj;
	}

	public enum TutorialAction {
		Idle,
		Speak,
		Confirm
	};

	public GameObject tutorialGuy;
	public TutorialEvent[] events;

	[Header("Unlocks")]
	public GameObject trendsButton;
	public GameObject nextWeekUI;
	public GameObject weekCounter;
	public CanvasGroup RecordSongButtonCG;

	private Routine m_routine;
	public void SpawnTutorial(TutorialID id, TutorialAction action = TutorialAction.Speak)
	{
		for(int i = 0; i < events.Length; i++)
		{
			if(events[i].id == id)
			{
				// If we've already shown this tutorial, skip it.
				if(events[i].triggered)
				{
					return;
				}
				else
				{
					if(events[i].prerequisiteId != TutorialID.None)
					{
						// Don't fire the event if the prerequisite hasn't been completed
						if (!getEventFromId(events[i].prerequisiteId).completed)
						{
							return;
						}
					}

					GameRefs.I.PostGameState(false, "spawnTutorial", string.Format("{0}_{1}", (int)id, id.ToString()));
					GameObject tutor = Instantiate(tutorialGuy, events[i].tutorialMount);
					events[i].tutorialMount.gameObject.SetActive(true);
					tutor.SetActive(true);
					events[i].tutorialObj = tutor;
					HelpGuyController tutorControl = tutor.GetComponent<HelpGuyController>();
					tutorControl.anim.enabled = true;
					tutorControl.anim.SetInteger("Tutorial", events[i].direction == ComeFromDirection.Bottom ? 1 : 2);
					if (events[i].direction == ComeFromDirection.Right)
						tutorControl.anim.SetBool("ReverseText", true);
					if (events[i].id == TutorialID.StudioFloorFinal)
						tutorControl.anim.SetBool("StudioGoals", true);

					tutorControl.anim.SetInteger("Action", (int) action);
					
					// Force animator to look normal when called from coroutines
					tutorControl.anim.Update(0f);
					tutorControl.helpText.text = events[i].charTextInfo.chracterText;
					events[i].triggered = true;
					return;
				}
			}
		}
	}

	public void SetTutorialCompleted(TutorialID id, bool oRide =false)
	{
		for (int i = 0; i < events.Length; i++)
		{
			if (events[i].id == id && (events[i].triggered || oRide))
			{
				// Don't set this complete if the prerequisite is not complete (out of order)

				if (events[i].completed || (events[i].prerequisiteId != TutorialID.None && !getEventFromId(events[i].prerequisiteId).triggered))
				{
					if(!oRide)
						return;
				}

				events[i].completed = true;
				events[i].triggered = true;
				CheckSpecialAction(id);
				if (events[i].tutorialObj == null)
					return;

				HelpGuyController tutorControl = events[i].tutorialObj.GetComponent<HelpGuyController>();
				
				if (events[i].animateOut)
				{
					m_routine.Replace(this, AnimateOutAndDestroy(tutorControl, events[i].tutorialObj));
				}
				else
				{
				//	tutorControl.anim.SetInteger("Tutorial", 0);
					Destroy(events[i].tutorialObj);
				}
			}
		}
	}

	public void SpawnFirstIncompleteTutorial()
	{
		// We have 14 "Linear" tutorials
		for(int i = 0; i < 14; i++)
		{
			if (!events[i].completed)
				SpawnTutorial((TutorialID)i);
		}
	}

	private IEnumerator AnimateOutAndDestroy(HelpGuyController helperGuy, GameObject go)
	{
		helperGuy.anim.SetInteger("Tutorial", 0);
		yield return 0.5f;
		Destroy(go);
	}

	private TutorialEvent getEventFromId(TutorialID id)
	{
		foreach(TutorialEvent tut in events)
		{
			if (tut.id == id)
				return tut;
		}
		return null;
	}

	private void CheckSpecialAction(TutorialID id)
	{
		switch(id)
		{
			case TutorialID.ArriveAtStudio:
				trendsButton.SetActive(true);
				// Show trends button
				break;

			case TutorialID.EnterFindTrends:
				RecordSongButtonCG.interactable = true;
				RecordSongButtonCG.alpha = 1f;
				break;

			case TutorialID.SignArtist:
				// Show next week UI
				nextWeekUI.SetActive(true);
				weekCounter.SetActive(true);
				GameRefs.I.m_gameController.UnlockRecording();
				break;

			case TutorialID.NextWeekReady:
				GameRefs.I.m_gameController.UnlockMarketing();
				break;
		}
	}

	public void MarkAllCompleted()
	{
		foreach (TutorialID id in System.Enum.GetValues(typeof(TutorialID)).Cast<TutorialID>())
		{
			SetTutorialCompleted(id, true);
		}
	}
}
