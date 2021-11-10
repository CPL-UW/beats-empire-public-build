using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeauRoutine;

public class RecordingAudio : MonoBehaviour {
	public enum SongPlayingState
	{
		Idle,
		PlayingDrums,
		PlayingBeat,
		PlayingCombined,
		PlayingReleased
	}

	public class SongData
	{
		public int genre;
		public int mood;
		public int bpm;
		public bool isLoaded;
		public AudioClip drumClip;
		public AudioClip drumClipFiltered;
		public AudioClip beatClip;
		public AudioClip beatClipFiltered;
		public AudioClip releaseGood;
		public AudioClip releaseBest;
	}

	public AudioSource mainSource;
	public AudioSource altSource;
	public AudioClip defaultBackgroundClip;

	[Header("Streaming")]
	public string url;

	[Header("Locally")]

	public SongAudio[] popObjects;
	public SongAudio[] rockObjects;
	public SongAudio[] electronicObjects;
	public SongAudio[] rapObjects;
	public SongAudio[] hiphopObjects;
	public SongAudio[] rnbObjects;

	public AudioClip[] recordScratch;
	public AudioClip heyStinger;
	public AudioClip[] cheer;
	public List<SongData> songLoadList;

	private Song currSong = null;
	private SongData preRecordData = null;
	private SongData currSongObject = null;
	private SongPlayingState currState;

	private Routine checkAudioRoutine;
	private Routine finishedSongRoutine;
	private List<Song> songPlaylist;
	private bool inStudio = true;
	private Routine m_loadSongRoutine;
	private float lastUpdateTime = 0f;
	void Start()
	{
		songPlaylist = new List<Song>();
		checkAudioRoutine.Replace(this, CheckAudioRoutine());
		songLoadList = new List<SongData>();
		StartLoadingSong(StatSubType.POP_ID, StatSubType.MOOD1_ID);
		//Routine.Start(Load());
	}

	IEnumerator LoadAudioTracks(int genre, int mood)
	{
		SongAudio trackInfo = getClipFromType(genre, mood);
		preRecordData = new SongData();
		preRecordData.isLoaded = false;
		preRecordData.bpm = trackInfo.BPM;
		preRecordData.genre = trackInfo.GenreID;
		preRecordData.mood = trackInfo.MoodID;

		preRecordData.drumClip = trackInfo.drumsTrack;
		preRecordData.drumClipFiltered = trackInfo.drumsFiltered;
		songLoadList.Add(preRecordData);

		// Load the rest of the clips after it's been added to the list
		int songIndex = -1;
		for(int i = 0; i < songLoadList.Count; i++)
		{
			if (songLoadList[i].genre == genre && songLoadList[i].mood == mood)
				songIndex = i;
		}
		WWW request = new WWW(Application.streamingAssetsPath + "/Music/" + trackInfo.beatFiltered + ".mp3");
		yield return request;
		songLoadList[songIndex].beatClipFiltered = request.GetAudioClip(true, false);
		request.Dispose();

		request = new WWW(Application.streamingAssetsPath + "/Music/" + trackInfo.beatTrack + ".mp3");
		yield return request;
		songLoadList[songIndex].beatClip = request.GetAudioClip(true, false);
		request.Dispose();

		request = new WWW(Application.streamingAssetsPath + "/Music/" + trackInfo.releaseGood + ".mp3");
		yield return request;
		songLoadList[songIndex].releaseGood = request.GetAudioClip(true, false);
		request.Dispose();

		request = new WWW(Application.streamingAssetsPath + "/Music/" + trackInfo.releaseBest + ".mp3");
		yield return request;
		songLoadList[songIndex].releaseBest = request.GetAudioClip(true, false);
		request.Dispose();

		songLoadList[songIndex].isLoaded = true;
		yield return null;
	}

	public void StartLoadingSong(int genre, int mood)
	{
		Routine.Start(LoadAudioTracks(genre, mood));
	}

	public void RemoveSong(int genre, int mood)
	{
		for(int i = 0; i < songLoadList.Count; i++)
		{
			if(songLoadList[i].genre == genre && songLoadList[i].mood == mood)
			{
				songLoadList.RemoveAt(i);
				break;
			}
		}
	}

	public void AddSongToQueue(Song song)
	{
		StartLoadingSong(song.Artist.GetGenre().ID, song.mood.ID);
		if(!songPlaylist.Contains(song))
			songPlaylist.Add(song);
	}

	public void RequestSong(Song song)
	{
		if(songPlaylist.Contains(song))
			songPlaylist.Remove(song);

		GameRefs.I.currentBPM = getClipFromSong(song).BPM;
		songPlaylist.Insert(0, song);
	}

	// Interrupt whatever we're playing and play this song now
	public void PlaySong(Song song, bool releasingNow)
	{
		finishedSongRoutine.Replace(this, PlayFinishedSong(song));
	}

	public int GetSongBPM(Song song)
	{
		SongAudio songData = getClipFromSong(song);
		if (songData == null)
			return 100;
		else
			return songData.BPM;
	}

	public void SetInStudio(bool ena)
	{
		inStudio = ena;

		if(currSongObject != null && currState != SongPlayingState.PlayingReleased)
		{
			float mainSourceTime = mainSource.time;

			mainSource.Stop();
			altSource.Stop();
			if(ena)
			{
				mainSource.clip = currSongObject.drumClipFiltered;
				altSource.clip = currSongObject.beatClipFiltered;
			}
			else
			{
				mainSource.clip = currSongObject.drumClip;
				altSource.clip = currSongObject.beatClip;
			}
			mainSource.time = altSource.time = mainSourceTime;
			try {
				mainSource.Play();
				altSource.Play();
			} catch (System.Exception e) {
				Debug.LogError(e);
			}
		}
	
	}

	public void EndGameStopAudio()
	{
		checkAudioRoutine.Stop();
		mainSource.Stop();
		altSource.Stop();
	}

	private IEnumerator CheckAudioRoutine()
	{
		bool waitingOnLoad = false;
		while(true)
		{
			if (Time.time - lastUpdateTime > 5f)
			{
				lastUpdateTime = Time.time;
			}   

			if (currState != SongPlayingState.PlayingReleased)
			{
				if(songPlaylist.Count > 0)
				{
					if(waitingOnLoad)
					{
						currSong = songPlaylist[0];
						if (currSong != null)
						{
							currSongObject = getDataFromList(currSong);
							if (currSongObject.isLoaded)
							{
								if (currSong.TurnsRecorded < 2)
								{
									mainSource.clip = inStudio ? currSongObject.drumClipFiltered : currSongObject.drumClip;
									mainSource.loop = true;
									mainSource.Play();
								}
								// Just guitar
								else if (currSong.TurnsRecorded < 4)
								{
									altSource.clip = inStudio ? currSongObject.beatClipFiltered : currSongObject.beatClip;
									altSource.loop = true;
									altSource.time = mainSource.time;
									altSource.Play();
								}
								// Both together
								else
								{
									mainSource.clip = inStudio ? currSongObject.drumClipFiltered : currSongObject.drumClip;
									mainSource.loop = true;
									mainSource.Play();
									altSource.clip = inStudio ? currSongObject.beatClipFiltered : currSongObject.beatClip;
									altSource.loop = true;
									altSource.time = mainSource.time;
									altSource.Play();
								}

								
								waitingOnLoad = false;
							}
						}
					}
					else if (currSong == null || currSong != songPlaylist[0])
					{
						currSong = songPlaylist[0];
						currSongObject = getDataFromList(currSong);
						if (currSongObject == null)
						{
							yield return 0.2f;
							currSongObject = getDataFromList(currSong);
						}
						
						GameRefs.I.currentBPM = currSongObject.bpm;
						mainSource.clip = inStudio ? currSongObject.drumClipFiltered : currSongObject.drumClip;
						mainSource.loop = true;
						mainSource.Play();

						if(currSongObject.isLoaded)
						{
							altSource.clip = inStudio ? currSongObject.beatClipFiltered : currSongObject.beatClip;
							altSource.loop = true;
							altSource.time = mainSource.time;
							altSource.Play();
							waitingOnLoad = false;
						}
						else
						{
							waitingOnLoad = true;
						}
					}

					if (currSong.TurnsRecorded < 2)
					{
						mainSource.mute = false;
						altSource.mute = true;
					}
					// Just guitar
					else if (currSong.TurnsRecorded < 4)
					{
						mainSource.mute = true;
						altSource.mute = false;
					}
					// Both together
					else
					{
						mainSource.mute = false;
						altSource.mute = false;
					}
				}
				else if(mainSource.clip != defaultBackgroundClip)
				{
					mainSource.Stop();
					altSource.Stop();
					mainSource.loop = true;
					altSource.loop = true;
					mainSource.clip = defaultBackgroundClip;
					songLoadList.Clear();
					mainSource.Play();
				}
			}
			yield return 0.1f;
		}
	}

	private int lastPlayedGenre= -1;
	private int lastPlayedMood = -1;
	private IEnumerator PlayFinishedSong(Song song)
	{
		currState = SongPlayingState.PlayingReleased;
		mainSource.Stop();
		altSource.Stop();
		mainSource.mute = false;
		altSource.mute = true;
		mainSource.loop = false;

		AudioClip stingerClip;
		if(song.Artist.GetGenre() == StatSubType.ROCK ||
			song.Artist.GetGenre() == StatSubType. RANDB)
		{
			stingerClip = recordScratch[Random.Range(0, recordScratch.Length)];
		}
		else
		{
			stingerClip = recordScratch[Random.Range(0, recordScratch.Length)];
		}

		mainSource.PlayOneShot(stingerClip);
		// Play Record scratch + cheer and the new clip all at the same time
		if (song.Quality > 7)
		{
			mainSource.PlayOneShot(cheer[1]);
			mainSource.clip = getDataFromList(song).releaseBest;
		}
		else
		{
			mainSource.PlayOneShot(cheer[0]);
			mainSource.clip = getDataFromList(song).releaseGood;
		}

		//if(lastPlayedGenre != song.Artist.GetGenre().ID && lastPlayedMood != song.mood.ID)
			//RemoveSong(lastPlayedGenre, lastPlayedMood);

		lastPlayedGenre = song.Artist.GetGenre().ID;
		lastPlayedMood = song.mood.ID;
		mainSource.time = 0;
		mainSource.Play();
		songPlaylist.Remove(song);
		currSong = null;
		yield return 0.5f;
		while(mainSource.isPlaying)
		{
			yield return 0.2f;
		}
		//RemoveSong(song.Artist.GetGenre().ID, song.mood.ID);
		currState = SongPlayingState.Idle;
	}

	private SongData getDataFromList(Song s)
	{
		for(int i = 0; i < songLoadList.Count; i++)
		{
			if(songLoadList[i].genre == s.Artist.GetGenre().ID && songLoadList[i].mood == s.mood.ID)
			{
				return songLoadList[i];
			}
		}
		return null;
	}
	private SongAudio getClipFromSong(Song song)
	{
		SongAudio[] songObjects = new SongAudio[6];
		if(song.Artist.GetGenre() == StatSubType.POP)
			songObjects = popObjects;
	
		else if (song.Artist.GetGenre() == StatSubType.ELECTRONIC)
			songObjects = electronicObjects;
		else if (song.Artist.GetGenre() == StatSubType.ROCK)
			songObjects = rockObjects;
		else if (song.Artist.GetGenre() == StatSubType.RAP)
			songObjects = rapObjects;
		else if (song.Artist.GetGenre() == StatSubType.HIP_HOP)
			songObjects = hiphopObjects;
		else if (song.Artist.GetGenre() == StatSubType.RANDB)
			songObjects = rnbObjects;

		for (int i = 0; i < songObjects.Length; i++)
		{
			if (song.Stats.Contains(StatSubType.MOOD1))
				return songObjects[0];
			else if (song.Stats.Contains(StatSubType.MOOD2))
				return songObjects[1];
			else if (song.Stats.Contains(StatSubType.MOOD3))
				return songObjects[2];
			else if (song.Stats.Contains(StatSubType.MOOD4))
				return songObjects[3];
			else if (song.Stats.Contains(StatSubType.MOOD5))
				return songObjects[4];
			else if (song.Stats.Contains(StatSubType.MOOD6))
				return songObjects[5];
		}

		return null;
	}

	private SongAudio getClipFromType(int genre, int mood)
	{
		SongAudio[] songObjects = new SongAudio[6];
		if (genre == StatSubType.POP_ID)
			songObjects = popObjects;
		else if (genre == StatSubType.ELECTRONIC_ID)
			songObjects = electronicObjects;
		else if (genre == StatSubType.ROCK_ID)
			songObjects = rockObjects;
		else if (genre == StatSubType.RAP_ID)
			songObjects = rapObjects;
		else if (genre == StatSubType.HIP_HOP_ID)
			songObjects = hiphopObjects;
		else if (genre == StatSubType.RANDB_ID)
			songObjects = rnbObjects;

		for (int i = 0; i < songObjects.Length; i++)
		{
			if (mood == StatSubType.MOOD1_ID)
				return songObjects[0];
			else if (mood == StatSubType.MOOD2_ID)
				return songObjects[1];
			else if (mood == StatSubType.MOOD3_ID)
				return songObjects[2];
			else if (mood == StatSubType.MOOD4_ID)
				return songObjects[3];
			else if (mood == StatSubType.MOOD5_ID)
				return songObjects[4];
			else if (mood == StatSubType.MOOD6_ID)
				return songObjects[5];
		}

		return null;
	}

}
