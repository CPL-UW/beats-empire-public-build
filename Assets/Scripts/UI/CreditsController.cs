using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeauRoutine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour {

	public RectTransform creditsContainer;
	public GameObject creditsBackground;
	public GameObject bandRoot;

	public float scrollTo = 4000f;
	public float inTime = 3f;

	private Vector3 startPos;
	private Routine scrollRoutine;
	private bool paused = false;
	// Use this for initialization

	private void Awake()
	{
		startPos = creditsContainer.localPosition;
	}

	private void Update()
	{
		if(Input.GetMouseButton(0))
		{
			scrollRoutine.Pause();
			paused = true;
		}
		else if(paused && !Input.GetMouseButton(0))
		{
			scrollRoutine.Resume();
			paused = false;
		}
	}

	public void StartCredits()
	{
		bandRoot.SetActive(false);
		creditsBackground.SetActive(true);
		scrollRoutine.Replace(this, ScrollCredits());
	}

	public void StopCredits()
	{
		scrollRoutine.Stop();
		creditsBackground.SetActive(false);
		creditsContainer.localPosition = startPos;
		bandRoot.SetActive(true);
	}

	private IEnumerator ScrollCredits()
	{
		while(true)
		{
			creditsContainer.localPosition = startPos;
			yield return creditsContainer.MoveTo(scrollTo, inTime, Axis.Y, Space.Self);
		}

	}
}
