using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SongAudio", menuName = "SongAudio", order = 1)]
public class SongAudio : ScriptableObject
{
	public int BPM;
	public AudioClip drumsTrack;
	public AudioClip drumsFiltered;
	public string beatTrack;
	public string beatFiltered;
	public string releaseGood;
	public string releaseBest;

	public int GenreID;
	public int MoodID;
}
