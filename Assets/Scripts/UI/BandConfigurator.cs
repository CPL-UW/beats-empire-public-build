using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BandConfigurator : MonoBehaviour {
	public GameObject[] persons;
	public BandGenerators bandGenerators;
	public Renderer plane;
	public Band.PoseContext poseContext;

	public TextMeshProUGUI nameLabel;
	public Slider bandSlider;
	public Button reloadButton;

	private Coroutine[] twitchTasks;

	public void Start()
	{
		twitchTasks = new Coroutine[5];
		bandSlider.onValueChanged.AddListener(i => LoadBand((int) i));
		reloadButton.onClick.AddListener(() => LoadBand((int) bandSlider.value));
		LoadBand(0);
	}

	private Band band;
	private void LoadBand(int bandIndex)
	{
		foreach (GameObject person in persons)
		{
			Footilities.DestroyChildren(person);
		}

		for (int i = 0; i < 5; ++i)
		{
			if (twitchTasks[i] != null) {
				StopCoroutine(twitchTasks[i]);
				twitchTasks[i] = null;
			}
		}

		band = new Band(bandGenerators.generators[bandIndex], 5, bandGenerators);	
		nameLabel.text = band.Name;
		for (int i = 0; i < band.members.Count; i++)
		{
			band.members[i].Incarnate(persons[i], poseContext, plane, Color.black);
			if (poseContext == Band.PoseContext.Jam &&
				(band.members[i].instrumentId == Band.Instrument.Guitar ||
				 band.members[i].instrumentId == Band.Instrument.Drums ||
				 band.members[i].instrumentId == Band.Instrument.Vocals))
			{
				twitchTasks[i] = StartCoroutine(CoTwitch(i));
			}
		}
	}

	private bool HasBandMember(int i)
	{
		return band != null && i < band.members.Count;
	}

	IEnumerator CoTwitch(int i)
	{
		while (HasBandMember(i))
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(5.0f, 15.0f));
			if (HasBandMember(i))
			{
				band.members[i].Twitch();
			}
		}
	}
}
