using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHook : MonoBehaviour {

	public AudioSource musicSource;
	public AudioClip stampClip;

	public void PlayStampIn()
	{
		musicSource.PlayOneShot(stampClip);
	}

}
