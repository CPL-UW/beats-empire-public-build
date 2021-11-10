using System.Collections;
using UnityEngine;

public class EventRelay : MonoBehaviour {
	public Release release;

	public void ShowNextArtist()
	{
		release.ShowNextArtist();
	}

	public void ShowPreviousArtist()
	{
		release.ShowPreviousArtist();
	}

	public void ShowDone()
	{
		release.ShowDone();
	}
}
