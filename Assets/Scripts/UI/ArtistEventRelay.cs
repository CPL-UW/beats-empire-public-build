using System.Collections;
using UnityEngine;

public class ArtistEventRelay : MonoBehaviour {
	public ArtistSigningView target;

	public void ShowNextArtist()
	{
		target.ShowNextArtist();
	}

	public void ShowPreviousArtist()
	{
		target.ShowPreviousArtist();
	}

	public void SyncHeaderText()
	{
		target.SyncHeaderText();
	}
}
