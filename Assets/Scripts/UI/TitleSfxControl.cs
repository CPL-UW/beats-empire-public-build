using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeauRoutine;

public class TitleSfxControl : MonoBehaviour {

	public AudioSource source;
	public AudioClip spotlight;
	public AudioClip creditsClick;

	private Routine routine;

	public void Start()
	{
		routine.Replace(this, PlaySounds());
	}

	private IEnumerator PlaySounds()
	{
		yield return 1.0f;
		source.PlayOneShot(spotlight);
	}

	public void PlayClick()
	{
		source.PlayOneShot(creditsClick);
	}
}
